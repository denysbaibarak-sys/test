using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Web.Http;

public class ApplicationController : ApiController
{
    private string appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "applications.json");
    private Logger logger = new Logger();

    private List<PartnerApplication> LoadApplications()
    {
        if (!File.Exists(appPath)) return new List<PartnerApplication>();
        var serializer = new DataContractJsonSerializer(typeof(List<PartnerApplication>));
        using (FileStream fs = new FileStream(appPath, FileMode.Open))
        {
            return (List<PartnerApplication>)serializer.ReadObject(fs);
        }
    }

    private void SaveApplications(List<PartnerApplication> apps)
    {
        var serializer = new DataContractJsonSerializer(typeof(List<PartnerApplication>));
        using (FileStream fs = new FileStream(appPath, FileMode.Create))
        {
            serializer.WriteObject(fs, apps);
        }
    }

    [HttpPost]
    [Route("api/applications/submit")]
    public IHttpActionResult SubmitApplication(PartnerApplication app)
    {
        try
        {
            var token = Request.Headers.Authorization?.Parameter;
            var user = new AuthService().GetUserByToken(token);

            if (user == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(app.FullName) || string.IsNullOrWhiteSpace(app.Phone) || string.IsNullOrWhiteSpace(app.Email))
            {
                return BadRequest("Заповніть всі обов'язкові поля!");
            }

            var apps = LoadApplications();

            if (apps.Any(a => a.UserId == user.Id && a.Status == "Pending"))
            {
                return BadRequest("Ваша попередня заявка вже знаходиться на розгляді.");
            }

            app.UserId = user.Id;
            app.Status = "Pending";
            apps.Add(app);

            SaveApplications(apps);
            logger.Log($"[ЗАЯВКА] Користувач {user.Login} подав заявку на партнерство.");

            return Ok(new { Message = "Заявку успішно відправлено!" });
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpGet]
    [Route("api/applications/my-status")]
    public IHttpActionResult GetMyApplicationStatus()
    {
        var token = Request.Headers.Authorization?.Parameter;
        var user = new AuthService().GetUserByToken(token);
        if (user == null) return Unauthorized();

        var apps = LoadApplications();
        var lastApp = apps.Where(a => a.UserId == user.Id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();

        if (lastApp == null) return Ok(new { Status = "None" });
        return Ok(new { Status = lastApp.Status });
    }

    // Для обробки через Postman: POST api/applications/review?id=ORD123&action=Approve (або Reject)
    [HttpPost]
    [Route("api/applications/review")]
    public IHttpActionResult ReviewApplication(string id, string action)
    {
        var apps = LoadApplications();
        var app = apps.FirstOrDefault(a => a.Id == id);

        if (app == null) return NotFound();
        if (app.Status != "Pending") return BadRequest("Ця заявка вже була оброблена.");

        string usersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "users.json");

        if (action.Equals("Approve", StringComparison.OrdinalIgnoreCase))
        {
            app.Status = "Approved";

            new AuthService().UpdateUserRole(app.UserId, "Owner");

            logger.Log($"[АДМІН] Заявку {id} СХВАЛЕНО. Користувач ID {app.UserId} тепер Owner.");
        }
        else if (action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
        {
            app.Status = "Rejected";
            logger.Log($"[АДМІН] Заявку {id} ВІДХИЛЕНО.");
        }
        else
        {
            return BadRequest("Невідома дія. Використовуйте Approve або Reject.");
        }

        SaveApplications(apps);
        return Ok($"Заявку {id} успішно оновлено статус на: {app.Status}");
    }
}