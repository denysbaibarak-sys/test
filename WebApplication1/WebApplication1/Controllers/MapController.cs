using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
//в чем проблема ? В том что токен не передается в localstorage, из-за этого вылазит ошибка, фикс хз какой 
namespace WebApplication1.Controllers
{
    public class MapController : ApiController
    {
        private readonly MapService _mapService = new MapService();
        private readonly AuthService _authService = new AuthService();

        public class CoordinatesDto
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        [HttpPost]
        [Route("api/map/save")]
        public async Task<IHttpActionResult> SaveCoordinates([FromBody] CoordinatesDto data)
        {
            if (data == null)
                return BadRequest("No data");

           
            var token = Request.Headers
                .GetValues("Authorization")
                .FirstOrDefault();

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var user = _authService.GetUserByToken(token);

            if (user == null)
                return Unauthorized();

            int userId = user.Id;

            bool result = await _mapService.SaveCoordinatesAsync(
                userId,
                data.Latitude,
                data.Longitude
            );

            return Ok(new
            {
                message = "Saved",
                userId = userId
            });
        }
    }
}