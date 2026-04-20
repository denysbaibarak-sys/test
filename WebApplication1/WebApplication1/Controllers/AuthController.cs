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
        // Припустимо, що AuthService після успішного входу повертає повного юзера (з базою, email тощо), 
        // або null, якщо пароль неправильний.
        var token = authService.Authenticate(user.Login, user.Password);

        if (token != null)
        {
            logger.Log("Login success: " + user.Login);

            return Ok(new { token = token });
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

        updatedUser.Id = user.Id;

        bool isUpdated = authService.UpdateUserProfile(updatedUser);

        if (isUpdated)
            return Ok();

        return BadRequest("Update failed");
    }
}