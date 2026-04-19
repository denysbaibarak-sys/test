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
        var loggedInUser = authService.Authenticate(user.Login, user.Password);

        if (loggedInUser != null)
        {
            logger.Log("Login success: " + user.Login);

            // Повертаємо не просто Ok(), а Ok(дані_користувача)!
            // Клієнт отримає JSON з усіма полями юзера і збереже їх у себе.
            return Ok(loggedInUser);
        }

        logger.Log("Login failed: " + user.Login);
        return Unauthorized();
    }
    [HttpPut]
    [Route("api/users/update")]
    public IHttpActionResult UpdateProfile([FromBody] User updatedUser)
    {
        if (updatedUser == null)
            return BadRequest("Неправильні дані");

        // ВИПРАВЛЕНО: Використовуємо правильну назву змінної (authService замість _authService)
        bool isUpdated = authService.UpdateUserProfile(updatedUser);

        if (isUpdated)
        {
            logger.Log("User updated: " + updatedUser.Login);
            return Ok(new { message = "Профіль успішно оновлено!" });
        }

        return BadRequest("Не вдалося оновити профіль. Або користувача не знайдено, або такий логін вже існує.");
    }
}