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
}