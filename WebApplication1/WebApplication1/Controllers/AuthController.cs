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
            return Ok(loggedInUser); // Віддаємо клієнту ВЕСЬ об'єкт юзера
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
            logger.Log("[ПОМИЛКА] Оновлення профілю відхилено");
            return Unauthorized();
        }

        updatedUser.Token = token;

        // Кладемо оновлення у фонову чергу
        TaskQueueManager.EnqueueTask(() =>
        {
            try
            {
                // Блок виконується воркером у фоновому режимі
                bool isUpdated = authService.UpdateUserProfile(updatedUser);

                if (isUpdated)
                    logger.Log($"[ОБРОБЛЕНО ВОРКЕРОМ] Профіль {updatedUser.Login} успішно оновлено в базі.");
                else
                    logger.Log($"[ПОМИЛКА ВОРКЕРА] Не вдалося оновити профіль {updatedUser.Login} (можливо, логін вже зайнятий).");
            }
            catch (Exception ex)
            {
                logger.Log($"[ПОМИЛКА ВОРКЕРА] Оновлення профілю {updatedUser.Login}: {ex.Message}");
            }
        });

        logger.Log($"[ВІДПРАВЛЕНО] Відповідь клієнту: Запит на оновлення профілю додано в чергу.");

        return Ok();
    }
}