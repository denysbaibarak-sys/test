using System;
using System.Linq;
using System.Web.Http;
using WebApplication1.Models;

public class OrderController : ApiController
{
    private OrderService orderService = new OrderService();
    private Logger logger = new Logger();

    [HttpPost]
    [Route("api/orders/create")]
    public IHttpActionResult CreateOrder(Order order)
    {
        try
        {
            var token = Request.Headers.Authorization?.Parameter;

            logger.Log($"[ОТРИМАНО] Запит на створення замовлення. Токен: {token}");

            var user = new AuthService().GetUserByToken(token);

            if (user == null)
            {
                logger.Log("[ПОМИЛКА] Неавторизований доступ");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                logger.Log($"[БЕЗПЕКА] Відхилено замовлення від {user.Login}: відсутній номер телефону.");
                return BadRequest("Для оформлення замовлення необхідно вказати номер телефону в профілі.");
            }

            order.UserId = user.Id;
            order.UpdatedAt = DateTime.Now;

            if (string.IsNullOrEmpty(order.OrderId))
            {
                order.OrderId = "ORD-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            }

            TaskQueueManager.EnqueueTask(() =>
            {
                try
                {
                    var backgroundOrderService = new OrderService();
                    backgroundOrderService.AddOrder(order);

                    logger.Log($"[ОБРОБЛЕНО ВОРКЕРОМ] Замовлення {order.OrderId} успішно збережено в БД.");
                }
                catch (Exception ex)
                {
                    logger.Log($"[ПОМИЛКА ВОРКЕРА] Замовлення {order.OrderId} не збережено: {ex.Message}");
                }
            });

            logger.Log($"[ВІДПРАВЛЕНО] Відповідь клієнту: Замовлення {order.OrderId} додано в чергу.");

            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            logger.Log("Order creation failed: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Log("Server error during order creation: " + ex.Message);
            return InternalServerError(ex);
        }
    }

    [HttpGet]
    [Route("api/orders")]
    public IHttpActionResult GetOrders()
    {
        try
        {
            var allOrders = orderService.GetAllOrders();

            logger.Log("GetOrders called. Total orders: " + allOrders.Count);

            return Ok(allOrders);
        }
        catch (Exception ex)
        {
            logger.Log("Error in GetOrders: " + ex.Message);
            return InternalServerError(ex);
        }
    }

    [HttpGet]
    [Route("api/orders/poll")]
    public IHttpActionResult PollOrders(string lastUpdate)
    {
        try
        {
            DateTime parsedDate;

            if (string.IsNullOrEmpty(lastUpdate) || !DateTime.TryParse(lastUpdate, out parsedDate))
            {
                var allOrders = orderService.GetAllOrders();
                logger.Log("[POLLING] Немає дати, повертаємо всі замовлення.");
                return Ok(allOrders);
            }

            var newOrders = orderService.GetOrdersAfter(parsedDate);

            if (newOrders == null || !newOrders.Any())
            {
                return StatusCode(System.Net.HttpStatusCode.NoContent);
            }

            logger.Log($"[POLLING] Знайдено {newOrders.Count} оновлених замовлень.");
            return Ok(newOrders);
        }
        catch (Exception ex)
        {
            logger.Log("[ПОМИЛКА POLLING]: " + ex.Message);
            return InternalServerError(ex);
        }
    }

    [HttpGet]
    [Route("api/orders/simulate-delivery")]
    public IHttpActionResult SimulateDelivery(string orderId)
    {
        orderService.TestUpdateOrderStatus(orderId);
        logger.Log($"[ТЕСТ] Статус замовлення {orderId} змінено на 'Доставлено'");
        return Ok($"Замовлення {orderId} оновлено!");
    }

    [HttpDelete]
    [Route("api/orders/clear")]
    public IHttpActionResult ClearOrders()
    {
        orderService.ClearAllOrders();
        logger.Log("[ОЧИЩЕННЯ] Базу замовлень повністю видалено.");
        return Ok("Базу успішно очищено!");
    }
}