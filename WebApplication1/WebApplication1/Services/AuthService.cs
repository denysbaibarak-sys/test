using System;
using System.Linq;
using System.Text.RegularExpressions;
using WebApplication1.Models;

public class AuthService
{
    private PasswordStorageService _passwordStorage = new PasswordStorageService();

    // Додаємо "світлофор" тільки для реєстрації, щоб уникнути подвійних кліків
    private static readonly object _regLock = new object();

    private string GenerateToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    public void Register(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Login) || user.Login.Length < 3 || user.Login.Length > 20)
            throw new ArgumentException("Логін має бути від 3 до 20 символів.");

        var loginRegex = new Regex(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ]{3}[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9_\-]{0,17}$");
        if (!loginRegex.IsMatch(user.Login))
            throw new ArgumentException("Формат логіну невірний. Має починатися з 3 літер.");

        // Блокуємо двері: одночасно може реєструватися лише один потік
        lock (_regLock)
        {
            using (var db = new AppDbContext())
            {
                bool isDuplicate = db.Users.Any(u =>
                    (!string.IsNullOrEmpty(u.Login) && u.Login.ToLower() == user.Login.ToLower()) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower() == user.Email.ToLower())
                );

                if (isDuplicate)
                    throw new ArgumentException("Користувач з таким логіном або поштою вже існує!");

                user.Role = "Customer";

                string rawPassword = user.Password;
                user.Password = PasswordHasher.HashPassword(rawPassword);

                db.Users.Add(user);
                db.SaveChanges();

                _passwordStorage.SavePassword(user.Id, rawPassword);
            }
        }
    }

    public void UpdateUserRole(int userId, string newRole)
    {
        using (var db = new AppDbContext())
        {
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Role = newRole;
                db.SaveChanges();
            }
        }
    }

    public User Authenticate(string login, string password)
    {
        using (var db = new AppDbContext())
        {
            var user = db.Users.FirstOrDefault(u => u.Login == login);

            // Тільки якщо юзера знайдено, перевіряємо його хеш
            if (user != null && PasswordHasher.VerifyPassword(password, user.Password))
            {
                user.Token = GenerateToken();
                db.SaveChanges();
                return user;
            }

            return null;
        }
    }

    public User GetUserByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        using (var db = new AppDbContext())
        {
            return db.Users.FirstOrDefault(u => u.Token == token);
        }
    }

    public bool UpdateUserProfile(User updatedUser)
    {
        if (!string.IsNullOrEmpty(updatedUser.Phone))
        {
            var phoneRegex = new Regex(@"^\+?[0-9]{10,13}$");
            if (!phoneRegex.IsMatch(updatedUser.Phone)) return false;
        }

        if (!string.IsNullOrWhiteSpace(updatedUser.Login))
        {
            if (updatedUser.Login.Length < 3 || updatedUser.Login.Length > 20) return false;
            var loginRegex = new Regex(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ]{3}[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9_\-]{0,17}$");
            if (!loginRegex.IsMatch(updatedUser.Login)) return false;
        }

        using (var db = new AppDbContext())
        {
            var existingUser = db.Users.FirstOrDefault(u => u.Token == updatedUser.Token);

            if (existingUser != null)
            {
                bool isDuplicate = db.Users.Any(u =>
                    u.Id != existingUser.Id &&
                    (
                        (!string.IsNullOrEmpty(u.Login) && u.Login.ToLower() == updatedUser.Login.ToLower()) ||
                        (!string.IsNullOrEmpty(updatedUser.Phone) && !string.IsNullOrEmpty(u.Phone) && u.Phone == updatedUser.Phone)
                    ));

                if (isDuplicate) return false;

                if (!string.IsNullOrWhiteSpace(updatedUser.Login)) existingUser.Login = updatedUser.Login;
                if (!string.IsNullOrWhiteSpace(updatedUser.Phone)) existingUser.Phone = updatedUser.Phone;

                if (!string.IsNullOrWhiteSpace(updatedUser.Password) && updatedUser.Password.Length <= 16)
                {
                    _passwordStorage.SavePassword(existingUser.Id, updatedUser.Password);
                    existingUser.Password = PasswordHasher.HashPassword(updatedUser.Password);
                }

                if (!string.IsNullOrWhiteSpace(updatedUser.Address)) existingUser.Address = updatedUser.Address;

                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}