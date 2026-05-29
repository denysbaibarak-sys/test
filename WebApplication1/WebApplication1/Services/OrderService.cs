using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WebApplication1.Models;

public class OrderService
{
    public void AddOrder(Order newOrder)
    {
        if (newOrder == null || newOrder.OrderedItems == null || newOrder.OrderedItems.Count == 0)
            throw new ArgumentException("Замовлення порожнє або недійсне.");

        using (var db = new AppDbContext())
        {
            var targetRestaurant = db.Restaurants
                                     .Include(r => r.Menu)
                                     .FirstOrDefault(r => r.Id == newOrder.RestaurantId);

            if (targetRestaurant == null)
            {
                throw new ArgumentException($"Ресторан з ID {newOrder.RestaurantId} не знайдено.");
            }

            foreach (var orderedItem in newOrder.OrderedItems)
            {
                bool dishExists = targetRestaurant.Menu.Any(m => m.Name == orderedItem.Name);
                if (!dishExists)
                {
                    throw new ArgumentException($"Страви '{orderedItem.Name}' немає в меню ресторану '{targetRestaurant.Name}'.");
                }
            }

            newOrder.RestaurantName = targetRestaurant.Name;

            if (string.IsNullOrEmpty(newOrder.OrderId))
                newOrder.OrderId = "ORD-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            newOrder.TotalPrice = newOrder.OrderedItems.Sum(item => item.Price * item.Quantity);
            newOrder.ItemsSummary = string.Join(", ", newOrder.OrderedItems.Select(i => $"{i.Quantity} x {i.Name}"));
            newOrder.Status = "В обробці";
            newOrder.OrderDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            newOrder.UpdatedAt = DateTime.Now;

            db.Orders.Add(newOrder);
            db.SaveChanges();
        }
    }

    public List<Order> GetAllOrders()
    {
        using (var db = new AppDbContext())
        {
            return db.Orders.Include(o => o.OrderedItems).ToList();
        }
    }

    public List<Order> GetOrdersAfter(DateTime lastUpdate)
    {
        using (var db = new AppDbContext())
        {
            return db.Orders
                     .Include(o => o.OrderedItems)
                     .Where(o => o.UpdatedAt > lastUpdate)
                     .ToList();
        }
    }

    public void TestUpdateOrderStatus(string orderId)
    {
        using (var db = new AppDbContext())
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                order.Status = "Доставлено";
                order.UpdatedAt = DateTime.Now;

                db.SaveChanges();
            }
        }
    }

    public void ClearAllOrders()
    {
        using (var db = new AppDbContext())
        {
            db.Orders.RemoveRange(db.Orders);
            db.SaveChanges();
        }
    }
}