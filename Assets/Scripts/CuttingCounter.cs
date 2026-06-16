using UnityEngine;

public class CuttingCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform tablePoint;
    [SerializeField] private CuttingRecipeSO[] cuttingRecipes; // Список всех рецептов нарезки

    private KitchenObject currentObjectOnTable;
    private int currentProgress = 0;

    public void Interact(PlayerController player)
    {
        if (currentObjectOnTable == null)
        {
            // СТОЛ ПУСТ
            if (player.HasKitchenObject() && HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
            {
                // Кладем предмет из рук на доску для нарезки
                currentObjectOnTable = player.GetKitchenObject();
                player.ClearKitchenObject();
                currentObjectOnTable.transform.parent = tablePoint;
                currentObjectOnTable.transform.localPosition = Vector3.zero;
                currentProgress = 0;
            }
        }
        else
        {
            // НА РАЗДЕЛОЧНОМ СТОЛЕ ЛЕЖИТ ПРЕДМЕТ
            if (!player.HasKitchenObject())
            {
                // Руки пусты. Забираем предмет со стола в руки.
                player.SetKitchenObject(currentObjectOnTable);
                currentObjectOnTable = null;
            }
            else
            {
                // В руках у игрока ТАРЕЛКА, а на столе лежит ЕДА (например, нарезка)
                PlateKitchenObject plateInHand = player.GetKitchenObject().GetComponent<PlateKitchenObject>();
                if (plateInHand != null)
                {
                    // Пытаемся забрать еду с доски прямо в тарелку в руках
                    if (plateInHand.TryAddIngredient(currentObjectOnTable.GetKitchenObjectSO()))
                    {
                        // Если успешно добавили, уничтожаем продукт на столе
                        Destroy(currentObjectOnTable.gameObject);
                        currentObjectOnTable = null;
                    }
                }
            }
        }
    }

    public void InteractAlternate(PlayerController player)
    {
        // Если на доске лежит предмет и его можно нарезать
        if (currentObjectOnTable != null && HasRecipeWithInput(currentObjectOnTable.GetKitchenObjectSO()))
        {
            currentProgress++;
            CuttingRecipeSO recipe = GetRecipeWithInput(currentObjectOnTable.GetKitchenObjectSO());

            // Если нарезали до конца
            if (currentProgress >= recipe.maxProgressSteps)
            {
                // Уничтожаем сырой предмет
                Destroy(currentObjectOnTable.gameObject);

                // Спавним нарезанный предмет
                GameObject spawnedOutput = Instantiate(recipe.output.prefab);
                currentObjectOnTable = spawnedOutput.GetComponent<KitchenObject>();
                currentObjectOnTable.transform.parent = tablePoint;
                currentObjectOnTable.transform.localPosition = Vector3.zero;
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputSO) => GetRecipeWithInput(inputSO) != null;

    private CuttingRecipeSO GetRecipeWithInput(KitchenObjectSO inputSO)
    {
        foreach (var recipe in cuttingRecipes)
        {
            if (recipe.input == inputSO) return recipe;
        }
        return null;
    }
}