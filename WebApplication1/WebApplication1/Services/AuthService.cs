using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Web.Hosting;

public class AuthService
{
    private string path = HostingEnvironment.MapPath("~/App_Data/users.json");
    private List<User> users;

    // Словник tokens більше не потрібен, зберігаємо токени прямо в users.json!

    public AuthService()
    {
        users = LoadUsers();
    }

    private List<User> LoadUsers()
    {
        if (!File.Exists(path))
            return new List<User>();

        var serializer = new DataContractJsonSerializer(typeof(List<User>));
        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            return (List<User>)serializer.ReadObject(fs);
        }
    }

    private string GenerateToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    private void SaveUsers()
    {
        var serializer = new DataContractJsonSerializer(typeof(List<User>));
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            serializer.WriteObject(fs, users);
        }
    }

    public void Register(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Login))
            throw new ArgumentException("Логін порожній");

        if (users.Any(u => u.Login == user.Login || u.Email == user.Email))
            throw new ArgumentException("Користувач з таким логіном, поштою або номером телефону вже існує!");
        
        user.Role = "Customer";
        user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        SaveUsers();
    }

    public User Authenticate(string login, string password)
    {
        var user = users.FirstOrDefault(u => u.Login == login && u.Password == password);

        if (user != null)
        {
            // Генеруємо токен і записуємо його прямо в об'єкт юзера
            user.Token = GenerateToken();

            SaveUsers();
        }

        return user; // Віддаємо клієнту об'єкт
    }

    public User GetUserByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        // Шукаємо юзера прямо в нашій базі за його токеном
        return users.FirstOrDefault(u => u.Token == token);
    }

    public bool UpdateUserProfile(User updatedUser)
    {
        if (!string.IsNullOrEmpty(updatedUser.Phone))
        {
            // Перевірка формату номера
            var phoneRegex = new Regex(@"^\+?[0-9]{10,12}$");
            if (!phoneRegex.IsMatch(updatedUser.Phone))
                return false;
        }

        var existingUser = users.FirstOrDefault(u => u.Token == updatedUser.Token);

        if (existingUser != null)
        {
            bool isDuplicate = users.Any(u =>
                u.Id != existingUser.Id &&
                (u.Login == updatedUser.Login || u.Email == updatedUser.Email || u.Phone == updatedUser.Phone));

            if (isDuplicate)
            {
                return false;
            }

            existingUser.Login = updatedUser.Login;
            existingUser.Phone = updatedUser.Phone;
            existingUser.Email = updatedUser.Email;

            // Оновлюємо пароль тільки якщо юзер ввів новий
            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                existingUser.Password = updatedUser.Password;
            }

            SaveUsers();

            return true;
        }

        return false;
    }
}