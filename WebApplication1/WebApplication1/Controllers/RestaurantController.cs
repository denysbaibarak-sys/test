using System.Collections.Generic;
using System.Web.Http;

public class RestaurantController : ApiController
{
    private static List<Restaurant> restaurants = new List<Restaurant>();
    private FileService fileService = new FileService();

    [HttpPost]
    [Route("api/restaurants")]
    public IHttpActionResult AddRestaurant(Restaurant restaurant)
    {
        var restaurants = fileService.LoadRestaurants();

        restaurants.Add(restaurant);

        fileService.SaveRestaurants(restaurants);

        return Ok(restaurants);
    }

    [HttpGet]
    [Route("api/restaurants")]
    public List<Restaurant> GetRestaurants()
    {
        return restaurants;
    }
}