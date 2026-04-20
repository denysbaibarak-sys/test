using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Web.Hosting;

public class OrderService
{
    // Вказуємо шлях до нового файлу бази даних для замовлень
    private string path = HostingEnvironment.MapPath("~/App_Data/orders.json");
    private FileService fileService = new FileService();
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
        // Базова перевірка
        if (newOrder == null || newOrder.OrderedItems == null || newOrder.OrderedItems.Count == 0)
            throw new ArgumentException("Order is empty or invalid");

        var restaurants = fileService.LoadRestaurants();

        var targetRestaurant = restaurants.FirstOrDefault(r => r.Id == newOrder.RestaurantId);

        if (targetRestaurant == null)
        {
            throw new ArgumentException($"Ресторан з ID {newOrder.RestaurantId} не знайдено.");
        }

        // Перевіряємо кожну замовлену страву
        foreach (var orderedItem in newOrder.OrderedItems)
        {
            bool dishExists = targetRestaurant.Menu.Any(m => m.Name == orderedItem.Name);
            if (!dishExists)
            {
                throw new ArgumentException($"Страви '{orderedItem.Name}' немає в меню ресторану '{targetRestaurant.Name}'.");
            }
        }

        // Автоматично підтягуємо правильну назву ресторану
        newOrder.RestaurantName = targetRestaurant.Name;

        // генерація ID, дата, статус
        if (string.IsNullOrEmpty(newOrder.OrderId))
            newOrder.OrderId = "ORD-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

        newOrder.Status = "В обробці";
        newOrder.OrderDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        // Зберігаємо
        orders.Add(newOrder);
        SaveOrders();
    }
    public List<Order> GetAllOrders()
    {
        return orders;
    }
}