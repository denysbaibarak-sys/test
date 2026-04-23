using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using ClientAppe.Models;
using ClientAppe.Services;

namespace ClientAppe.ViewModels
{
    public class OrdersViewModel : ViewModelBase
    {
        private readonly ApiService _apiService = new ApiService();

        // Зберігаємо всі замовлення, які прийшли з сервера
        private List<OrderModel> _allOrders = new List<OrderModel>();

        // Ті, що зараз відображаються на екрані
        private ObservableCollection<OrderModel> _orders;
        public ObservableCollection<OrderModel> Orders
        {
            get => _orders;
            set { _orders = value; OnPropertyChanged(); }
        }

        // Прапорець, щоб знати, яка вкладка активна
        private bool _isActiveTab = true;
        public bool IsActiveTab
        {
            get => _isActiveTab;
            set { _isActiveTab = value; OnPropertyChanged(); }
        }

        private bool _isPolling = true;

        public string ActiveOrdersText => $"Мої замовлення ({_allOrders.Count(o => o.Status != "Доставлено")})";
        public string HistoryOrdersText => $"Історія ({_allOrders.Count(o => o.Status == "Доставлено")})";

        public ICommand SwitchTabCommand { get; }

        public OrdersViewModel()
        {
            SwitchTabCommand = new RelayCommand(tab =>
            {
                if (tab is string tabName)
                {
                    IsActiveTab = tabName == "Active";
                    FilterOrders();
                }
            });

            LoadOrders();
            StartPollingAsync();
        }

        private async void LoadOrders()
        {
            _allOrders = await _apiService.GetOrdersAsync();
            FilterOrders(); // Відразу фільтруємо при завантаженні
        }

        private async void StartPollingAsync()
        {
            while (_isPolling)
            {
                // Реалізація Short Polling: затримка 5 секунд між запитами
                await Task.Delay(5000);

                try
                {
                    var newOrders = await _apiService.GetOrdersAsync();
                    
                    // Перевіряємо, чи є зміни (проста перевірка за кількістю або останнім статусом)
                    // Для спрощення просто оновлюємо список
                    _allOrders = newOrders;
                    FilterOrders();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Polling error: " + ex.Message);
                }
            }
        }

        public void StopPolling()
        {
            _isPolling = false;
        }

        private void FilterOrders()
        {
            if (IsActiveTab)
            {
                var active = _allOrders.Where(o => o.Status != "Доставлено").ToList();
                Orders = new ObservableCollection<OrderModel>(active);
            }
            else
            {
                var history = _allOrders.Where(o => o.Status == "Доставлено").ToList();
                Orders = new ObservableCollection<OrderModel>(history);
            }

            // Кажемо UI, що цифри на кнопках могли змінитися
            OnPropertyChanged(nameof(ActiveOrdersText));
            OnPropertyChanged(nameof(HistoryOrdersText));
        }
    }
}