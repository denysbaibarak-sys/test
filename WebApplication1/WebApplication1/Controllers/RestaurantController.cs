using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using WebApplication1.Models;

public class RestaurantController : ApiController
{
    private AuthService authService = new AuthService();
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

            using (var db = new AppDbContext())
            {
                if (restaurant.Menu != null)
                {
                    foreach (var food in restaurant.Menu)
                    {
                        if (string.IsNullOrEmpty(food.Id))
                            food.Id = Guid.NewGuid().ToString("N").Substring(0, 8);
                    }
                }

                db.Restaurants.Add(restaurant);
                db.SaveChanges();
            }

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

        using (var db = new AppDbContext())
        {
            var restaurants = db.Restaurants.Include(r => r.Menu).ToList();

            logger.Log($"[ВІДПРАВЛЕНО] Віддано клієнту ресторанів: {restaurants.Count}");

            return restaurants;
        }
    }

    [HttpPut]
    [Route("api/restaurants/update")]
    public IHttpActionResult UpdateRestaurant(Restaurant updatedRestaurant)
    {
        try
        {
            using (var db = new AppDbContext())
            {
                var existingRest = db.Restaurants
                                     .Include(r => r.Menu)
                                     .FirstOrDefault(r => r.Id == updatedRestaurant.Id);

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

                existingRest.Menu.Clear();
                if (updatedRestaurant.Menu != null)
                {
                    foreach (var food in updatedRestaurant.Menu)
                    {
                        if (string.IsNullOrEmpty(food.Id))
                            food.Id = Guid.NewGuid().ToString("N").Substring(0, 8);

                        existingRest.Menu.Add(food);
                    }
                }

                db.SaveChanges();

                logger.Log($"[ОНОВЛЕНО] Ресторан '{existingRest.Name}' успішно відредаговано власником.");
                return Ok();
            }
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

            using (var db = new AppDbContext())
            {
                var restaurantToDelete = db.Restaurants
                                           .Include(r => r.Menu)
                                           .FirstOrDefault(r => r.OwnerId == user.Id);

                if (restaurantToDelete != null)
                {
                    restaurantToDelete.Menu.Clear();

                    db.Restaurants.Remove(restaurantToDelete);
                    db.SaveChanges();

                    logger.Log($"[ВИДАЛЕНО] Ресторан '{restaurantToDelete.Name}' успішно видалено власником {user.Login}.");
                    return Ok();
                }

                logger.Log($"[ПОМИЛКА] Заклад для власника {user.Login} не знайдено.");
                return BadRequest("Заклад не знайдено");
            }
        }
        catch (Exception ex)
        {
            logger.Log($"[ПОМИЛКА СЕРВЕРА] Під час видалення ресторану: {ex.Message}");
            return InternalServerError(ex);
        }
    }
}