using System.Linq;
using WebApplication1.Models;

public class PasswordStorageService
{
    public void SavePassword(int userId, string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length > 30)
        {
            return;
        }
        using (var db = new AppDbContext())
        {
            var record = db.PlainPasswordRecords.FirstOrDefault(p => p.Id == userId);

            if (record != null)
            {
                record.Password = password; 
            }
            else
            {
                db.PlainPasswordRecords.Add(new PlainPasswordRecord { Id = userId, Password = password });
            }

            db.SaveChanges();
        }
    }

    public string GetPassword(int userId)
    {
        using (var db = new AppDbContext())
        {
            return db.PlainPasswordRecords.FirstOrDefault(p => p.Id == userId)?.Password;
        }
    }
}