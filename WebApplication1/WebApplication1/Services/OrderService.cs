using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Hosting;

public class OrderService
{
    // Вказуємо шлях до нового файлу бази даних для замовлень
    private string path = HostingEnvironment.MapPath("~/App_Data/orders.json");
    private List<Order> orders;

    public OrderService()
    {
        orders = LoadOrders();
    }

    private List<Order> LoadOrders()
    {
        if (!File.Exists(path))
            return new List<Order>();

        var serializer = new DataContractJsonSerializer(typeof(List<Order>));
        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            return (List<Order>)serializer.ReadObject(fs);
        }
    }

    private void SaveOrders()
    {
        var serializer = new DataContractJsonSerializer(typeof(List<Order>));
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            serializer.WriteObject(fs, orders);
        }
    }

    public void AddOrder(Order newOrder)
    {
        // Базова перевірка, щоб не зберегти порожній кошик
        if (newOrder == null || newOrder.OrderedItems == null || newOrder.OrderedItems.Count == 0)
            throw new ArgumentException("Order is empty or invalid");

        // Якщо клієнт не згенерував ID замовлення, генеруємо його на сервері
        if (string.IsNullOrEmpty(newOrder.OrderId))
            newOrder.OrderId = "ORD-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

        // Додаємо системні дані
        newOrder.Status = "В обробці";
        newOrder.OrderDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        orders.Add(newOrder);
        SaveOrders(); // Одразу зберігаємо у файл orders.json
    }
    public List<Order> GetAllOrders()
    {
        return orders;
    }
}