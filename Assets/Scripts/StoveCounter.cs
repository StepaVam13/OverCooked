using UnityEngine;

public class StoveCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform tablePoint;
    [SerializeField] private CookingRecipeSO[] cookingRecipes;

    private KitchenObject currentObjectOnTable;
    private float cookingTimer = 0f;
    private bool isCooking = false;
    private CookingRecipeSO activeRecipe;

    private void Update()
    {
        if (isCooking && currentObjectOnTable != null)
        {
            cookingTimer += Time.deltaTime;
            if (cookingTimer >= activeRecipe.cookingTimeMax)
            {
                // Приготовилось! Заменяем на суп
                // Приготовилось! Заменяем на суп
                Destroy(currentObjectOnTable.gameObject);

                // Спавним объект сразу в позиции tablePoint и делаем tablePoint его родителем
                GameObject cookedObject = Instantiate(activeRecipe.output.prefab, tablePoint.position, Quaternion.identity, tablePoint);
                currentObjectOnTable = cookedObject.GetComponent<KitchenObject>();
                currentObjectOnTable.transform.localPosition = Vector3.zero;
                currentObjectOnTable.transform.localRotation = Quaternion.identity;

                isCooking = false;
            }
        }
    }

    public void Interact(PlayerController player)
    {
        if (currentObjectOnTable == null)
        {
            // НА ПЛИТЕ ПУСТО
            if (player.HasKitchenObject() && HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
            {
                currentObjectOnTable = player.GetKitchenObject();
                player.ClearKitchenObject();
                currentObjectOnTable.transform.parent = tablePoint;
                currentObjectOnTable.transform.localPosition = Vector3.zero;

                activeRecipe = GetRecipeWithInput(currentObjectOnTable.GetKitchenObjectSO());
                cookingTimer = 0f;
                isCooking = true;
            }
        }
        else
        {
            // НА ПЛИТЕ ЧТО-ТО ЛЕЖИТ
            if (player.HasKitchenObject() && player.GetKitchenObject().CompareTag("Plate"))
            {
                // Игрок подошел с тарелкой. Пробуем переложить еду в тарелку.
                PlateKitchenObject plate = player.GetKitchenObject().GetComponent<PlateKitchenObject>();

                // Накладываем только если суп УЖЕ СВАРИЛСЯ (isCooking == false)
                if (plate != null && !isCooking)
                {
                    if (plate.TryAddIngredient(currentObjectOnTable.GetKitchenObjectSO()))
                    {
                        Destroy(currentObjectOnTable.gameObject);
                        currentObjectOnTable = null;
                    }
                }
            }
            else if (!player.HasKitchenObject())
            {
                // У игрока пустые руки.
                // Разрешаем забрать предмет только если он ЕЩЕ В ПРОЦЕССЕ варки (например, положили по ошибке)
                if (isCooking)
                {
                    player.SetKitchenObject(currentObjectOnTable);
                    currentObjectOnTable = null;
                    isCooking = false;
                }
                // Если суп уже готов (isCooking == false), забрать его голыми руками НЕЛЬЗЯ!
            }
        }
    }

    public void InteractAlternate(PlayerController player) { }

    private bool HasRecipeWithInput(KitchenObjectSO inputSO) => GetRecipeWithInput(inputSO) != null;
    private CookingRecipeSO GetRecipeWithInput(KitchenObjectSO inputSO)
    {
        foreach (var recipe in cookingRecipes)
        {
            if (recipe.input == inputSO) return recipe;
        }
        return null;
    }
}