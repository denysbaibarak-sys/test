using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Web.Hosting;

public class AuthService
{
    private string path = HostingEnvironment.MapPath("~/App_Data/users.json");
    private List<User> users;

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
            throw new ArgumentException("Login is empty");

        if (users.Any(u => u.Login == user.Login))
            throw new ArgumentException("User already exists");

        user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        SaveUsers(); // зберігаємо відразу після реєстрації
    }

    // Змінюємо тип повернення з bool на User
    public User Authenticate(string login, string password)
    {
        return users.FirstOrDefault(u => u.Login == login && u.Password == password);
    }
    public bool UpdateUserProfile(User updatedUser)
    {
        // Шукаємо користувача. 
        var existingUser = users.FirstOrDefault(u => u.Id == updatedUser.Id);

        if (existingUser != null)
        {
            // 1. Перевіряємо, чи новий логін часом вже не зайнятий кимось ІНШИМ
            if (existingUser.Login != updatedUser.Login && users.Any(u => u.Login == updatedUser.Login))
            {
                // Логін зайнятий
                return false;
            }

            // 2. Оновлюємо дані
            existingUser.Login = updatedUser.Login;

            existingUser.Phone = updatedUser.Phone; 

            // 3. Оновлюємо пароль ТІЛЬКИ якщо юзер ввів новий (не порожній)
            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                existingUser.Password = updatedUser.Password;
            }

            // 4. Зберігаємо оновлений список у users.json
            SaveUsers();

            return true;
        }

        return false;
    }
}