using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class MediaController : ApiController
    {
        [HttpPost]
        [Route("api/media/upload")]
        public HttpResponseMessage UploadImage()
        {
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var file = httpRequest.Files[0];

                var folderPath = HttpContext.Current.Server.MapPath("~/Images");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(folderPath, fileName);
                file.SaveAs(filePath);

                return Request.CreateResponse(HttpStatusCode.OK, new { fileName = fileName });
            }

            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Файл не знайдено");
        }
    }
}