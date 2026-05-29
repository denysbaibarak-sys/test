using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Web.Hosting;

public class PlainPasswordRecord
{
    public int Id { get; set; }
    public string Password { get; set; }
}
public class PasswordStorageService
{
    private string path = HostingEnvironment.MapPath("~/App_Data/passwords.json");
    private static List<PlainPasswordRecord> passwords;
    private static readonly object _lock = new object();

    public PasswordStorageService()
    {
        if (passwords == null)
        {
            lock (_lock)
            {
                if (passwords == null)
                {
                    passwords = Load();
                }
            }
        }
    }

    private List<PlainPasswordRecord> Load()
    {
        if (!File.Exists(path))
            return new List<PlainPasswordRecord>();

        var serializer = new DataContractJsonSerializer(typeof(List<PlainPasswordRecord>));

        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            return (List<PlainPasswordRecord>)serializer.ReadObject(fs);
        }
    }

    private void Save()
    {
        var serializer = new DataContractJsonSerializer(typeof(List<PlainPasswordRecord>));

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            serializer.WriteObject(fs, passwords);
        }
    }

    public void SavePassword(int userId, string password)
    {
        lock (_lock)
        {
            passwords.RemoveAll(p => p.Id == userId);

            passwords.Add(new PlainPasswordRecord
            {
                Id = userId,
                Password = password
            });

            Save();
        }
    }

    public string GetPassword(int userId)
    {
        return passwords.FirstOrDefault(p => p.Id == userId)?.Password;
    }
}