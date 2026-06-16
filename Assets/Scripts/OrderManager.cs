using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [SerializeField] private List<RecipeSO> availableRecipes; // Список всех возможных рецептов
    [SerializeField] private Text orderTextUI;               // Текстовое поле UI для заказов (правый верхний угол)

    private List<RecipeSO> activeOrders = new List<RecipeSO>();
    private float orderSpawnTimer = 0f;
    [Header("Настройки заказов")]
    [SerializeField] private float orderSpawnTimerMax = 12f; // Время между заказами в секундах
    [SerializeField] private int maxOrdersCount = 3;          // Максимальное число заказов на экране

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
        orderSpawnTimer += Time.deltaTime;
        if (orderSpawnTimer >= orderSpawnTimerMax)
        {
            orderSpawnTimer = 0f;
            if (activeOrders.Count < maxOrdersCount)
            {
                SpawnNewOrder();
            }
        }
    }

    private void SpawnNewOrder()
    {
        RecipeSO randomRecipe = availableRecipes[Random.Range(0, availableRecipes.Count)];
        activeOrders.Add(randomRecipe);
        UpdateOrderUI();
    }

    private void UpdateOrderUI()
    {
        orderTextUI.text = "АКТИВНЫЕ ЗАКАЗЫ:\n";
        foreach (var recipe in activeOrders)
        {
            // Выводим только красивое русское название рецепта
            orderTextUI.text += $"- {recipe.recipeName}\n";
        }
    }

    public bool DeliverDish(List<KitchenObjectSO> plateIngredients)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            RecipeSO recipe = activeOrders[i];

            // Проверяем, совпадает ли состав тарелки с рецептом заказа
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
                    // Заказ успешно выполнен!
                    activeOrders.RemoveAt(i);
                    UpdateOrderUI();
                    Debug.Log("Блюдо успешно доставлено!");
                    return true;
                }
            }
        }
        Debug.Log("Такого блюда нет в заказах!");
        return false;
    }
}