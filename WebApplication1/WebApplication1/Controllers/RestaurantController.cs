using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

public class RestaurantController : ApiController
{
    private FileService fileService = new FileService();
    private Logger logger = new Logger();
    private Validator validator = new Validator();

    [HttpPost]
    [Route("api/restaurants")]
    public IHttpActionResult AddRestaurant(Restaurant restaurant)
    {
        try
        {
            validator.ValidateRestaurant(restaurant);

            var restaurants = fileService.LoadRestaurants();
            restaurants.Add(restaurant);
            fileService.SaveRestaurants(restaurants);

            logger.Log("Added restaurant: " + restaurant.Name);

            return StatusCode(HttpStatusCode.Created);
        }
        catch (ArgumentException ex)
        {
            logger.Log("Error: " + ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Route("api/restaurants")]
    public List<Restaurant> GetRestaurants()
    {
        logger.Log("Get all restaurants");
        return fileService.LoadRestaurants();
    }
}