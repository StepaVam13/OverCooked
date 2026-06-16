using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    private List<KitchenObjectSO> ingredientsOnPlate = new List<KitchenObjectSO>();
    [SerializeField] private List<KitchenObjectSO> allowedIngredients;

    public bool TryAddIngredient(KitchenObjectSO ingredient)
    {
        if (!allowedIngredients.Contains(ingredient)) return false;
        if (ingredientsOnPlate.Contains(ingredient)) return false;

        ingredientsOnPlate.Add(ingredient);

        // Пытаемся найти визуал по имени
        Transform visual = transform.Find(ingredient.objectName);
        if (visual != null)
        {
            visual.gameObject.SetActive(true);
        }
        else
        {
            // Если объект не найден, выводим подробную подсказку в консоль!
            Debug.LogError($"[ВИЗУАЛ] Тарелка не смогла найти внутри себя дочерний объект с именем '{ingredient.objectName}'! " +
                           $"Проверьте, что внутри префаба тарелки объект супа называется именно '{ingredient.objectName}' (без лишних пробелов).");
        }

        return true;
    }

    public List<KitchenObjectSO> GetIngredients() => ingredientsOnPlate;

    // Метод очищает тарелку от еды, выключая все визуальные модели
    public void ClearPlate()
    {
        ingredientsOnPlate.Clear(); // Очищаем список ингредиентов в коде

        // Пробегаем по списку разрешенных ингредиентов и выключаем их визуал
        foreach (var ingredient in allowedIngredients)
        {
            Transform visual = transform.Find(ingredient.objectName);
            if (visual != null)
            {
                visual.gameObject.SetActive(false);
            }
        }
    }
}