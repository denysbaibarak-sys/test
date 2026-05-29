using System;
using System.Web.Http;
using WebApplication1.Models;

public class UserController : ApiController
{
    private AuthService authService = new AuthService();
    private Logger logger = new Logger();

    [HttpPost]
    [Route("api/users/register")]
    public IHttpActionResult Register(User user)
    {
        try
        {
            logger.Log($"[ОТРИМАНО] Запит на реєстрацію: {user.Login}");

            authService.Register(user);

            logger.Log($"[ВІДПРАВЛЕНО] Успішна реєстрація: {user.Login}");
            return Ok();
        }
        catch (ArgumentException ex)
        {
            logger.Log($"[ПОМИЛКА] Помилка реєстрації {user.Login}: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("api/users/login")]
    public IHttpActionResult Login(User user)
    {
        logger.Log($"[ОТРИМАНО] Запит на авторизацію: {user.Login}");

        // Синхронний логін
        var loggedInUser = authService.Authenticate(user.Login, user.Password);

        if (loggedInUser != null)
        {
            logger.Log($"[ВІДПРАВЛЕНО] Успішна авторизація: {user.Login}");
            return Ok(loggedInUser);
        }

        logger.Log($"[ПОМИЛКА] Невдала спроба авторизації: {user.Login}");
        return Unauthorized();
    }

    [HttpPut]
    [Route("api/users/update")]
    public IHttpActionResult UpdateProfile([FromBody] User updatedUser)
    {
        var token = Request.Headers.Authorization?.Parameter;
        logger.Log($"[ОТРИМАНО] Запит на оновлення профілю. Токен: {token}");

        var user = authService.GetUserByToken(token);

        if (user == null)
        {
            logger.Log("[ПОМИЛКА] Оновлення профілю відхилено. Невірний токен.");
            return Unauthorized();
        }

        updatedUser.Token = token;

        try
        {
            bool isUpdated = authService.UpdateUserProfile(updatedUser);

            if (isUpdated)
            {
                logger.Log($"[УСПІХ] Профіль {updatedUser.Login} успішно оновлено в базі.");
                return Ok();
            }
            else
            {
                logger.Log($"[ПОМИЛКА] Не вдалося оновити профіль {updatedUser.Login}. Можливо, конфлікт даних або помилка валідації.");
                return BadRequest("Не вдалося оновити профіль. Перевірте правильність введених даних (формат логіну/телефону).");
            }
        }
        catch (Exception ex)
        {
            logger.Log($"[ПОМИЛКА] Оновлення профілю {updatedUser.Login}: {ex.Message}");
            return InternalServerError(ex);
        }
    }
    [HttpGet]
    [Route("api/users/me")]
    public IHttpActionResult GetCurrentUser()
    {
        try
        {
            var token = Request.Headers.Authorization?.Parameter;
            var user = authService.GetUserByToken(token);

            if (user != null)
            {
                return Ok(user);
            }
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}