using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting; 
using System.Web.Http;

public class MediaController : ApiController
{
    private Logger logger = new Logger();

    [HttpPost]
    [Route("api/media/upload")]
    public async Task<IHttpActionResult> UploadImage()
    {
        if (!Request.Content.IsMimeMultipartContent())
        {
            return StatusCode(HttpStatusCode.UnsupportedMediaType);
        }

        try
        {
            string root = HostingEnvironment.MapPath("~/Images");

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            var provider = new MultipartFormDataStreamProvider(root);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FileData.Count == 0)
            {
                return BadRequest("Файл не знайдено у запиті.");
            }

            var fileData = provider.FileData[0];
            string originalFileName = fileData.Headers.ContentDisposition.FileName.Trim('"');
            string uniqueFileName = Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + originalFileName;
            string newPath = Path.Combine(root, uniqueFileName);

            File.Move(fileData.LocalFileName, newPath);

            logger.Log($"[МЕДІА] Успішно завантажено файл: {uniqueFileName}");

            return Ok(new { fileName = uniqueFileName });
        }
        catch (Exception ex)
        {
            logger.Log($"[ПОМИЛКА МЕДІА] Помилка завантаження файлу: {ex.Message}");
            return InternalServerError(ex);
        }
    }
}