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

        users.Add(user);
        SaveUsers(); // зберігаємо відразу після реєстрації
    }

    public bool Login(string login, string password)
    {
        return users.Any(u => u.Login == login && u.Password == password);
    }
}