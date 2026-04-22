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

        var user = new AuthService().GetUserByToken(token);

        if (user == null)
            return Unauthorized();

        
        order.UserId = user.Id;

        
        orderService.AddOrder(order);

        logger.Log("Order created successfully: " + order.OrderId);

        return Ok(order);
    }
    catch (ArgumentException ex)
    {
        logger.Log("Order creation failed (Bad Request): " + ex.Message);
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

            // якщо клієнт нічого не передав → повертаємо всі
            if (string.IsNullOrEmpty(lastUpdate) || !DateTime.TryParse(lastUpdate, out parsedDate))
            {
                var allOrders = orderService.GetAllOrders();
                logger.Log("Polling: no lastUpdate, returning all orders");
                return Ok(allOrders);
            }

            var newOrders = orderService.GetOrdersAfter(parsedDate);

            if (newOrders == null || !newOrders.Any())
            {
                logger.Log("Polling: no new orders");
                return StatusCode(System.Net.HttpStatusCode.NoContent);
            }

            logger.Log($"Polling: returned {newOrders.Count} new orders");

            return Ok(newOrders);
        }
        catch (Exception ex)
        {
            logger.Log("Polling error: " + ex.Message);
            return InternalServerError(ex);
        }
    }
    [HttpGet]
    [Route("api/orders/updates")]
    public IHttpActionResult GetUpdates(DateTime? lastUpdate = null)
    {
        try
        {
            var orders = orderService.GetAllOrders();

            if (lastUpdate.HasValue)
            {
                orders = orders
                    .Where(o => o.UpdatedAt > lastUpdate.Value)
                    .ToList();
            }

            logger.Log("Polling updates: " + orders.Count);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            logger.Log("Polling error: " + ex.Message);
            return InternalServerError(ex);
        }
    }

}