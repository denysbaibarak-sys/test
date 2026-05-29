using System;
using System.Linq;
using System.Web.Http;
using WebApplication1.Models;

public class ApplicationController : ApiController
{
    private Logger logger = new Logger();

    [HttpPost]
    [Route("api/applications/submit")]
    public IHttpActionResult SubmitApplication(PartnerApplication app)
    {
        try
        {
            logger.Log("[ОТРИМАНО] Запит на створення заявки...");

            var token = Request.Headers.Authorization?.Parameter;
            var user = new AuthService().GetUserByToken(token);

            if (user == null)
            {
                logger.Log("[ПОМИЛКА] Користувач не знайдений або токен недійсний.");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(app.FullName) || string.IsNullOrWhiteSpace(app.Phone) || string.IsNullOrWhiteSpace(app.Email))
            {
                return BadRequest("Заповніть всі обов'язкові поля!");
            }

            using (var db = new AppDbContext())
            {
                if (db.PartnerApplications.Any(a => a.UserId == user.Id && a.Status == "Pending"))
                {
                    return BadRequest("Ваша попередня заявка вже знаходиться на розгляді.");
                }

                if (string.IsNullOrEmpty(app.Id))
                {
                    app.Id = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                }

                app.UserId = user.Id;
                app.Status = "Pending";
                app.CreatedAt = DateTime.Now;

                db.PartnerApplications.Add(app);
                db.SaveChanges();
            }

            logger.Log($"[ВІДПРАВЛЕНО] Користувач {user.Login} успішно подав заявку (ID: {app.Id}).");

            return Ok(new { Message = "Заявку успішно відправлено!" });
        }
        catch (Exception ex)
        {
            logger.Log($"[КРИТИЧНА ПОМИЛКА] {ex.Message}");
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

        using (var db = new AppDbContext())
        {
            var lastApp = db.PartnerApplications
                            .Where(a => a.UserId == user.Id)
                            .OrderByDescending(a => a.CreatedAt)
                            .FirstOrDefault();

            if (lastApp == null) return Ok(new { Status = "None" });

            return Ok(new { Status = lastApp.Status });
        }
    }

    [HttpGet]
    [Route("api/applications/pending")]
    public IHttpActionResult GetPendingApplications()
    {
        var token = Request.Headers.Authorization?.Parameter;
        var currentUser = new AuthService().GetUserByToken(token);

        if (currentUser == null || currentUser.Role != "Admin")
        {
            return Unauthorized();
        }

        using (var db = new AppDbContext())
        {
            var pendingApps = db.PartnerApplications.Where(a => a.Status == "Pending").ToList();
            return Ok(pendingApps);
        }
    }

    [HttpPost]
    [Route("api/applications/update-status")]
    public IHttpActionResult UpdateApplicationStatus([FromBody] ApplicationStatusUpdateRequest request)
    {
        var token = Request.Headers.Authorization?.Parameter;
        var currentUser = new AuthService().GetUserByToken(token);

        if (currentUser == null || currentUser.Role != "Admin")
        {
            return Unauthorized();
        }

        using (var db = new AppDbContext())
        {
            var application = db.PartnerApplications.FirstOrDefault(a => a.Id == request.Id);

            if (application == null)
            {
                return BadRequest("Заявку не знайдено.");
            }

            application.Status = request.Status;
            db.SaveChanges();

            if (request.Status == "Approved")
            {
                new AuthService().UpdateUserRole(application.UserId, "Owner");
                logger.Log($"[АДМІН] Заявку {application.Id} СХВАЛЕНО. Користувач ID {application.UserId} тепер Owner.");
            }
            else if (request.Status == "Rejected")
            {
                logger.Log($"[АДМІН] Заявку {application.Id} ВІДХИЛЕНО.");
            }

            return Ok();
        }
    }

    public class ApplicationStatusUpdateRequest
    {
        public string Id { get; set; }
        public string Status { get; set; }
    }
}