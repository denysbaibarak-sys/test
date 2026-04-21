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
            logger.Log($"[ОТРИМАНО] Запит на додавання ресторану: {restaurant?.Name}");

            validator.ValidateRestaurant(restaurant);

            var restaurants = fileService.LoadRestaurants();
            restaurants.Add(restaurant);
            fileService.SaveRestaurants(restaurants);

            logger.Log($"[ВІДПРАВЛЕНО] Ресторан '{restaurant.Name}' успішно додано до бази.");

            return StatusCode(HttpStatusCode.Created);
        }
        catch (ArgumentException ex)
        {
            logger.Log($"[ПОМИЛКА] Валідація ресторану {restaurant?.Name} не пройдена: {ex.Message}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Log($"[ПОМИЛКА СЕРВЕРА] Під час додавання ресторану: {ex.Message}");
            return InternalServerError(ex);
        }
    }

    [HttpGet]
    [Route("api/restaurants")]
    public List<Restaurant> GetRestaurants()
    {
        logger.Log("[ОТРИМАНО] Запит на отримання списку всіх ресторанів.");

        var restaurants = fileService.LoadRestaurants();

        logger.Log($"[ВІДПРАВЛЕНО] Віддано клієнту ресторанів: {restaurants.Count}");

        return restaurants;
    }
}