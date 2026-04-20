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

        var loggedInUser = authService.Authenticate(user.Login, user.Password);

        if (loggedInUser != null)
        {
            logger.Log("Login success: " + user.Login);

            // Віддаємо клієнту ВЕСЬ об'єкт юзера, як того очікує Avalonia!
            return Ok(loggedInUser);
        }

        logger.Log("Login failed: " + user.Login);
        return Unauthorized();
    }

    [HttpPut]
    [Route("api/users/update")]
    public IHttpActionResult UpdateProfile([FromBody] User updatedUser)
    {
        var token = Request.Headers.Authorization?.Parameter;

        var user = authService.GetUserByToken(token);

        if (user == null)
            return Unauthorized();

        updatedUser.Token = token;

        bool isUpdated = authService.UpdateUserProfile(updatedUser);

        if (isUpdated)
            return Ok();

        return BadRequest("Update failed");
    }
}