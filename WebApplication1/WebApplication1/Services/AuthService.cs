using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

public class AuthService
{
    private string path;
    private List<User> users;

    // Додаємо необов'язковий параметр testPath. 
    public AuthService(string testPath = null)
    {
        if (string.IsNullOrEmpty(testPath))
        {
            // Універсальний спосіб знайти папку App_Data
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string appDataPath = Path.Combine(baseDir, "App_Data");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            path = Path.Combine(appDataPath, "users.json");
        }
        else
        {
            path = testPath;
        }

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

        if (users.Any(u => u.Login == user.Login || u.Email == user.Email || u.Phone == user.Phone))
            throw new ArgumentException("Користувач з таким логіном, поштою або номером телефону вже існує!");

        user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        SaveUsers();
    }

    public User Authenticate(string login, string password)
    {
        var user = users.FirstOrDefault(u => u.Login == login && u.Password == password);

        if (user != null)
        {
            user.Token = GenerateToken();
            SaveUsers();
        }

        return user;
    }

    public User GetUserByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return users.FirstOrDefault(u => u.Token == token);
    }

    public bool UpdateUserProfile(User updatedUser)
    {
        if (!string.IsNullOrEmpty(updatedUser.Phone))
        {
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