using UnityEngine;

public class StoveCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform tablePoint;
    [SerializeField] private CookingRecipeSO[] cookingRecipes;

    [Header("Параметры сгорания еды")]
    [SerializeField] private KitchenObjectSO burntFoodSO; // Карточка сгоревшей еды (угли)
    [SerializeField] private float burningTimeMax = 8f;   // Время до сгорания после готовности (секунды)

    private KitchenObject currentObjectOnTable;
    private float cookingTimer = 0f;
    private float burningTimer = 0f;
    private bool isCooking = false;
    private bool isCookedAndWaiting = false; // Еда готова, но её не забирают
    private CookingRecipeSO activeRecipe;

    private void Update()
    {
        // 1. Процесс варки/жарки сырого продукта
        if (isCooking && currentObjectOnTable != null)
        {
            cookingTimer += Time.deltaTime;
            if (cookingTimer >= activeRecipe.cookingTimeMax)
            {
                // Приготовилось! Заменяем сырое на готовое блюдо (суп или картофель фри)
                Destroy(currentObjectOnTable.gameObject);

                GameObject cookedObject = Instantiate(activeRecipe.output.prefab, tablePoint.position, Quaternion.identity);
                currentObjectOnTable = cookedObject.GetComponent<KitchenObject>();
                currentObjectOnTable.ResetScale();

                Collider col = cookedObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                isCooking = false;

                // Запускаем режим ожидания подгорания
                isCookedAndWaiting = true;
                burningTimer = 0f;
            }
        }

        // 2. Процесс сгорания готового блюда, если его долго не забирают
        // 2. Процесс сгорания готового блюда, если его долго не забирают
        if (isCookedAndWaiting && currentObjectOnTable != null)
        {
            burningTimer += Time.deltaTime;

            // Визуальный эффект: трясем кастрюлю строго НАД плитой (tablePoint.position)
            if (burningTimer >= burningTimeMax * 0.6f)
            {
                float shakeAmount = 0.03f; // Сила тряски

                // Трясем еду относительно мировых координат плиты, а не центра карты!
                currentObjectOnTable.transform.position = tablePoint.position + new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    0f,
                    Random.Range(-shakeAmount, shakeAmount)
                );
            }

            // Время вышло — еда превращается в угли
            if (burningTimer >= burningTimeMax)
            {
                Destroy(currentObjectOnTable.gameObject);

                // Создаем угли ровно на плите БЕЗ родителя
                GameObject burntObject = Instantiate(burntFoodSO.prefab, tablePoint.position, Quaternion.identity);
                currentObjectOnTable = burntObject.GetComponent<KitchenObject>();
                currentObjectOnTable.ResetScale();

                // Отключаем коллайдер углям, чтобы они не мешали лучу игрока
                Collider col = burntObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                isCookedAndWaiting = false;
                Debug.LogWarning("Еда на плите сгорела и превратилась в угли!");
            }
        }
    }

    public void Interact(PlayerController player)
    {
        if (currentObjectOnTable == null)
        {
            // Кладем сырой продукт на плиту
            if (player.HasKitchenObject() && HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
            {
                currentObjectOnTable = player.GetKitchenObject();
                player.ClearKitchenObject();
                currentObjectOnTable.transform.parent = tablePoint;
                currentObjectOnTable.transform.localPosition = Vector3.zero;

                activeRecipe = GetRecipeWithInput(currentObjectOnTable.GetKitchenObjectSO());
                cookingTimer = 0f;
                isCooking = true;
                isCookedAndWaiting = false;
            }
        }
        else
        {
            // Игрок подошел с тарелкой
            if (player.HasKitchenObject() && player.GetKitchenObject().CompareTag("Plate"))
            {
                PlateKitchenObject plate = player.GetKitchenObject().GetComponent<PlateKitchenObject>();

                // Накладываем только готовый суп/фри (пока они не сгорели!)
                if (plate != null && isCookedAndWaiting)
                {
                    if (plate.TryAddIngredient(currentObjectOnTable.GetKitchenObjectSO()))
                    {
                        Destroy(currentObjectOnTable.gameObject);
                        currentObjectOnTable = null;
                        isCookedAndWaiting = false;
                    }
                }
            }
            else if (!player.HasKitchenObject())
            {
                // Игрок с пустыми руками может забрать:
                // 1. Сырой продукт, который еще не успел свариться
                // 2. Сгоревшую еду (угли), чтобы выбросить их в мусорку
                if (isCooking || currentObjectOnTable.GetKitchenObjectSO() == burntFoodSO)
                {
                    player.SetKitchenObject(currentObjectOnTable);
                    currentObjectOnTable = null;
                    isCooking = false;
                    isCookedAndWaiting = false;
                }
            }
        }
    }

    public void InteractAlternate(PlayerController player) { }

    public KitchenObject GetCurrentObjectOnTable() => currentObjectOnTable;

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