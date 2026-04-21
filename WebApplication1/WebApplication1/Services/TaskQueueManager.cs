using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public static class TaskQueueManager
{
    // Потокобезпечна черга повідомлень/завдань (Message Queue)
    private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

    private static bool _isRunning = false;
    private static Logger _logger = new Logger(); // Наш логер

    // Запуск Воркера (Workers - threads or tasks)
    public static void StartWorker()
    {
        if (_isRunning) return;
        _isRunning = true;

        // Створюємо окремий фоновий потік, який буде жити весь час роботи сервера
        Task.Run(() =>
        {
            _logger.Log("System: Worker thread started.");

            while (_isRunning)
            {
                // Перевіряємо, чи є в черзі завдання
                if (_queue.TryDequeue(out Action workItem))
                {
                    try
                    {
                        // Виконуємо завдання (наприклад, збереження замовлення в БД)
                        workItem.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.Log("Worker Error: " + ex.Message);
                    }
                }
                else
                {
                    // Якщо черга порожня, воркер "відпочиває" 100 мілісекунд, щоб не вантажити процесор
                    Thread.Sleep(100);
                }
            }
        });
    }

    // Метод для додавання завдання в чергу
    public static void EnqueueTask(Action workItem)
    {
        _queue.Enqueue(workItem);
    }
}