using System;
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
            // Передаємо замовлення з кошика в сервіс
            orderService.AddOrder(order);

            logger.Log("Order created successfully: " + order.OrderId);

            // Повертаємо 200 OK і саме замовлення (щоб клієнт побачив свій ID та Статус)
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            logger.Log("Order creation failed (Bad Request): " + ex.Message);
            return BadRequest(ex.Message); // Повертаємо помилку 400, якщо кошик порожній
        }
        catch (Exception ex)
        {
            logger.Log("Server error during order creation: " + ex.Message);
            return InternalServerError(ex); // Повертаємо помилку 500, якщо зламався файл
        }
    }

    [HttpGet]
    [Route("api/orders")]
    public IHttpActionResult GetOrders()
    {
        var allOrders = orderService.GetAllOrders();
        return Ok(allOrders);
    }
}