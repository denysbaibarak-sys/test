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
            var loadedOrders = (List<Order>)serializer.ReadObject(fs);

            // Якщо старі замовлення не мають дати оновлення, ставимо поточну
            foreach (var order in loadedOrders)
            {
                if (order.UpdatedAt == default(DateTime))
                {
                    order.UpdatedAt = DateTime.Now;
                }
            }

            return loadedOrders;
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

        // Фіксуємо час створення/оновлення
        newOrder.UpdatedAt = DateTime.Now;

        orders.Add(newOrder);
        SaveOrders();
    }

    public List<Order> GetAllOrders()
    {
        return orders;
    }

    public List<Order> GetOrdersAfter(DateTime lastUpdate)
    {
        return orders
            .Where(o => o.UpdatedAt > lastUpdate)
            .ToList();
    }
    public void TestUpdateOrderStatus(string orderId)
    {
        // Знаходимо замовлення в оперативній пам'яті сервера
        var order = orders.FirstOrDefault(o => o.OrderId == orderId);

        if (order != null)
        {
            order.Status = "Доставлено"; // Змінюємо статус
            order.UpdatedAt = DateTime.Now;

            SaveOrders(); // Зберігаємо нові дані у файл
        }
    }
    public void ClearAllOrders()
    {
        orders.Clear();
        SaveOrders();
    }
}