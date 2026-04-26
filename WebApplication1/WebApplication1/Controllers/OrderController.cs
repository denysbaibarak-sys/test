using System;
using System.Linq;
using System.Web.Http;

public class OrderController : ApiController
{
    private static OrderService orderService = new OrderService();
    private Logger logger = new Logger(); // Використовуємо існуючий логер проекту

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

            order.UserId = user.Id;
            order.UpdatedAt = DateTime.Now;

            if (string.IsNullOrEmpty(order.OrderId))
            {
                order.OrderId = "ORD-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            }

            // MESSAGE QUEUE
            TaskQueueManager.EnqueueTask(() =>
            {
                try
                {
                    // Блок що виконується фоново воркером
                    orderService.AddOrder(order);

                    logger.Log($"[ОБРОБЛЕНО ВОРКЕРОМ] Замовлення {order.OrderId} успішно збережено в БД.");
                }
                catch (Exception ex)
                {
                    logger.Log($"[ПОМИЛКА ВОРКЕРА] Замовлення {order.OrderId} не збережено: {ex.Message}");
                }
            });

            // МИТТЄВА ВІДПОВІДЬ (Основа для Short Polling)
            logger.Log($"[ВІДПРАВЛЕНО] Відповідь клієнту: Замовлення {order.OrderId} додано в чергу.");

            // Повертаємо клієнту об'єкт із згенерованим OrderId
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

            // Якщо клієнт нічого не передав - повертаємо всі
            if (string.IsNullOrEmpty(lastUpdate) || !DateTime.TryParse(lastUpdate, out parsedDate))
            {
                var allOrders = orderService.GetAllOrders();
                logger.Log("[POLLING] Немає дати, повертаємо всі замовлення.");
                return Ok(allOrders);
            }

            // Беремо лише нові
            var newOrders = orderService.GetOrdersAfter(parsedDate);

            // Якщо нових немає - повертаємо 204 NoContent
            if (newOrders == null || !newOrders.Any())
            {
                return StatusCode(System.Net.HttpStatusCode.NoContent);
            }

            // Якщо є оновлення - пишемо в лог і віддаємо клієнту
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
        return Ok($"Замовлення {orderId} оновлено! Перевір вікно клієнта.");
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