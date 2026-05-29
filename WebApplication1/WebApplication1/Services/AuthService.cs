using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Web.Hosting;

public class AuthService
{
    private PasswordStorageService _passwordStorage = new PasswordStorageService(); // без хеша пароль 1
    private string path = HostingEnvironment.MapPath("~/App_Data/users.json");
    private static List<User> users;
    private static readonly object _lock = new object();
    public AuthService()
    {
        if (users == null)
        {
            lock (_lock)
            {
                if (users == null)
                {
                    users = LoadUsers();
                }
            }
        }
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
        lock (_lock)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<User>));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                serializer.WriteObject(fs, users);
            }
        }
    }

    public void Register(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Login) || user.Login.Length < 3 || user.Login.Length > 20)
        {
            throw new ArgumentException("Логін має бути від 3 до 20 символів.");
        }

        var loginRegex = new Regex(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ]{3}[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9_\-]{0,17}$");
        if (!loginRegex.IsMatch(user.Login))
        {
            throw new ArgumentException("Формат логіну невірний. Має починатися з 3 літер.");
        }

        bool isDuplicate = users.Any(u =>
            (!string.IsNullOrWhiteSpace(u.Login) && u.Login.Equals(user.Login, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(u.Email) && u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
        );

        if (isDuplicate)
            throw new ArgumentException("Користувач з таким логіном або поштою вже існує!");

        user.Role = "Customer";
        
        lock (_lock)
        {
            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            _passwordStorage.SavePassword(user.Id, user.Password); // без хеша 3 

            user.Password = PasswordHasher.HashPassword(user.Password); // Хеширование
            users.Add(user);
            SaveUsers();
        }
    }
    public void UpdateUserRole(int userId, string newRole)
    {
        var user = users.FirstOrDefault(u => u.Id == userId);

        if (user != null)
        {
            user.Role = newRole;
            SaveUsers();
        }
    }
    public User Authenticate(string login, string password)
    {
        //var user = users.FirstOrDefault(u => u.Login == login && u.Password == password); // До хеширования 

        var user = users.FirstOrDefault(u => u.Login == login);

        if (user != null && PasswordHasher.VerifyPassword(password, user.Password))
        {
            user.Token = GenerateToken();
            SaveUsers();
            return user;
        }
        // 101 - 108 стр хеширование
        return null;

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
            var phoneRegex = new Regex(@"^\+?[0-9]{10,13}$");
            if (!phoneRegex.IsMatch(updatedUser.Phone))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(updatedUser.Login))
        {
            if (updatedUser.Login.Length < 3 || updatedUser.Login.Length > 20)
            {
                return false;
            }

            var loginRegex = new Regex(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ]{3}[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9_\-]{0,17}$");
            if (!loginRegex.IsMatch(updatedUser.Login))
            {
                return false;
            }
        }

        var existingUser = users.FirstOrDefault(u => u.Token == updatedUser.Token);

        if (existingUser != null)
        {
            bool isDuplicate = users.Any(u =>
                u.Id != existingUser.Id &&
                (
                    (!string.IsNullOrWhiteSpace(u.Login) && u.Login.Equals(updatedUser.Login, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(updatedUser.Phone) && !string.IsNullOrWhiteSpace(u.Phone) && u.Phone == updatedUser.Phone)
                ));

            if (isDuplicate)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.Login))
                existingUser.Login = updatedUser.Login;

            if (!string.IsNullOrWhiteSpace(updatedUser.Phone))
                existingUser.Phone = updatedUser.Phone;

            /* if (!string.IsNullOrWhiteSpace(updatedUser.Password)) до хеша 
                 existingUser.Password = updatedUser.Password;
            */
            
           

            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
                _passwordStorage.SavePassword(existingUser.Id, updatedUser.Password); // без хеша 
            existingUser.Password = PasswordHasher.HashPassword(updatedUser.Password);//хеш 

            if (!string.IsNullOrWhiteSpace(updatedUser.Address))
                existingUser.Address = updatedUser.Address;

            SaveUsers();

            return true;
        }

        return false;
    }
}