using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    // Внутренний класс для отслеживания времени каждого конкретного заказа
    [System.Serializable]
    public class ActiveOrder
    {
        public RecipeSO recipe;
        public float timer; // Таймер заказа (от 0 до 120 секунд)
    }

    [Header("Настройки заказов")]
    [SerializeField] private List<RecipeSO> availableRecipes;
    [SerializeField] private Text orderTextUI;
    [SerializeField] private float orderSpawnTimerMax = 12f;
    [SerializeField] private int maxOrdersCount = 3;

    [Header("Настройки времени и штрафов")]
    [SerializeField] private float orderTimeoutMax = 120f; // 120 секунд (2 минуты) на заказ
    [SerializeField] private int timeoutPenalty = 50;      // Штраф ($) за просроченный заказ

    [Header("Настройки заказов (Инерция)")]
    [SerializeField] private float orderSpawnTimer = 0f;

    private List<ActiveOrder> activeOrders = new List<ActiveOrder>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnNewOrder();
    }

    private void Update()
    {
        // 1. Спавн новых заказов по таймеру
        orderSpawnTimer += Time.deltaTime;
        if (orderSpawnTimer >= orderSpawnTimerMax)
        {
            orderSpawnTimer = 0f;
            if (activeOrders.Count < maxOrdersCount)
            {
                SpawnNewOrder();
            }
        }

        // 2. Обновление таймеров текущих заказов и проверка на просрочку (2 минуты)
        for (int i = 0; i < activeOrders.Count; i++)
        {
            activeOrders[i].timer += Time.deltaTime;

            if (activeOrders[i].timer >= orderTimeoutMax)
            {
                // Заказ просрочен!
                Debug.Log($"Заказ {activeOrders[i].recipe.recipeName} просрочен!");

                SoundManager.Instance.PlayFailSound();

                // Вычитаем штраф через LevelManager
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.DeductMoney(timeoutPenalty);
                }

                activeOrders.RemoveAt(i);
                i--; // Сдвигаем индекс назад, так как элемент удален
            }
        }

        // ДОБАВЬТЕ ЭТУ СТРОКУ СЮДА:
        // Теперь интерфейс перерисовывается каждый кадр, и таймеры блюд будут плавно идти в реальном времени!
        UpdateOrderUI();
    }

    private void SpawnNewOrder()
    {
        if (availableRecipes.Count == 0) return;

        RecipeSO randomRecipe = availableRecipes[Random.Range(0, availableRecipes.Count)];

        ActiveOrder newOrder = new ActiveOrder();
        newOrder.recipe = randomRecipe;
        newOrder.timer = 0f;

        activeOrders.Add(newOrder);
        UpdateOrderUI();
    }

    private void UpdateOrderUI()
    {
        orderTextUI.text = "АКТИВНЫЕ ЗАКАЗЫ:\n";
        foreach (var order in activeOrders)
        {
            // Показываем игроку, сколько секунд осталось на выполнение этого заказа
            int remainingTime = Mathf.CeilToInt(orderTimeoutMax - order.timer);
            orderTextUI.text += $"- {order.recipe.recipeName} ({remainingTime}с)\n";
        }
    }

    public bool DeliverDish(List<KitchenObjectSO> plateIngredients)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            RecipeSO recipe = activeOrders[i].recipe;

            if (recipe.requiredIngredients.Count == plateIngredients.Count)
            {
                bool matches = true;
                foreach (var reqIng in recipe.requiredIngredients)
                {
                    if (!plateIngredients.Contains(reqIng))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    // Рассчитываем награду в зависимости от скорости сдачи
                    float timeSpent = activeOrders[i].timer;
                    int reward = CalculateMoneyReward(timeSpent);

                    // Начисляем деньги в LevelManager
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.AddMoney(reward);
                    }

                    activeOrders.RemoveAt(i);
                    UpdateOrderUI();
                    return true;
                }
            }
        }
        Debug.Log("Такого блюда нет в заказах!");
        return false;
    }

    // Метод динамического расчета денег (Чем быстрее — тем больше!)
    private int CalculateMoneyReward(float timeSpent)
    {
        if (timeSpent <= 40f)
        {
            // Сдал быстрее чем за 40 секунд — Супер-награда с бонусом!
            return 150;
        }
        else if (timeSpent <= 85f)
        {
            // Обычное среднее время (от 40 до 85 секунд)
            return 100;
        }
        else
        {
            // Долго возился (от 85 до 120 секунд) — Минимальная награда
            return 50;
        }
    }
}