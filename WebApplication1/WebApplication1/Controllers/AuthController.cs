using System;
using System.Web.Http;

public class UserController : ApiController
{
    private static AuthService authService = new AuthService();
    private Logger logger = new Logger();

    [HttpPost]
    [Route("api/users/register")]
    public IHttpActionResult Register(User user)
    {
        try
        {
            authService.Register(user);
            logger.Log("User registered: " + user.Login);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            logger.Log("Register error: " + ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("api/users/login")]
    public IHttpActionResult Login(User user)
    {
        if (authService.Login(user.Login, user.Password))
        {
            logger.Log("Login success: " + user.Login);
            return Ok();
        }

        logger.Log("Login failed: " + user.Login);
        return Unauthorized();
    }
}