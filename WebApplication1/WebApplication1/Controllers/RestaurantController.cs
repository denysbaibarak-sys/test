using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

public class RestaurantController : ApiController
{
    private static AuthService authService = new AuthService();
    private FileService fileService = new FileService();
    private Logger logger = new Logger();
    private Validator validator = new Validator();

    [HttpPost]
    [Route("api/restaurants/add")]
    public IHttpActionResult AddRestaurant(Restaurant restaurant)
    {
        try
        {
            logger.Log($"[ОТРИМАНО] Запит на додавання ресторану: {restaurant?.Name}");

            validator.ValidateRestaurant(restaurant);

            var restaurants = fileService.LoadRestaurants();

            if (restaurants.Any())
            {
                restaurant.Id = restaurants.Max(r => r.Id) + 1;
            }
            else
            {
                restaurant.Id = 1;
            }

            restaurants.Add(restaurant);
            fileService.SaveRestaurants(restaurants);

            logger.Log($"[ВІДПРАВЛЕНО] Ресторан '{restaurant.Name}' успішно додано до бази під Id {restaurant.Id}.");

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

    [HttpPut]
    [Route("api/restaurants/update")]
    public IHttpActionResult UpdateRestaurant(Restaurant updatedRestaurant)
    {
        try
        {
            var restaurants = fileService.LoadRestaurants();

            var existingRest = restaurants.FirstOrDefault(r => r.Id == updatedRestaurant.Id);

            if (existingRest == null)
            {
                return NotFound();
            }

            if (existingRest.OwnerId != updatedRestaurant.OwnerId)
            {
                return Unauthorized();
            }

            existingRest.Name = updatedRestaurant.Name;
            existingRest.Category = updatedRestaurant.Category;
            existingRest.DeliveryTime = updatedRestaurant.DeliveryTime;
            existingRest.Description = updatedRestaurant.Description;
            existingRest.ImagePath = updatedRestaurant.ImagePath;
            existingRest.Address = updatedRestaurant.Address;
            existingRest.Menu = updatedRestaurant.Menu;

            fileService.SaveRestaurants(restaurants);

            logger.Log($"[ОНОВЛЕНО] Ресторан '{existingRest.Name}' успішно відредаговано власником.");
            return Ok();
        }
        catch (Exception ex)
        {
            logger.Log($"[ПОМИЛКА] Під час оновлення ресторану: {ex.Message}");
            return InternalServerError(ex);
        }
    }

    [HttpDelete]
    [Route("api/restaurant/delete")]
    public IHttpActionResult DeleteRestaurant()
    {
        try
        {
            var token = Request.Headers.Authorization?.Parameter;
            var user = authService.GetUserByToken(token);

            if (user == null || user.Role != "Owner")
            {
                logger.Log("[ПОМИЛКА] Спроба видалення закладу без прав доступу.");
                return Unauthorized();
            }

            var restaurants = fileService.LoadRestaurants();
            var restaurantToDelete = restaurants.FirstOrDefault(r => r.OwnerId == user.Id);

            if (restaurantToDelete != null)
            {
                restaurants.Remove(restaurantToDelete);
                fileService.SaveRestaurants(restaurants);

                logger.Log($"[ВИДАЛЕНО] Ресторан '{restaurantToDelete.Name}' успішно видалено власником {user.Login}.");
                return Ok();
            }

            logger.Log($"[ПОМИЛКА] Заклад для власника {user.Login} не знайдено.");
            return BadRequest("Заклад не знайдено");
        }
        catch (Exception ex)
        {
            logger.Log($"[ПОМИЛКА СЕРВЕРА] Під час видалення ресторану: {ex.Message}");
            return InternalServerError(ex);
        }
    }
}