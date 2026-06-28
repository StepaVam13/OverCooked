using UnityEngine;

public class StoveCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform tablePoint;
    [SerializeField] private CookingRecipeSO[] cookingRecipes;

    [Header("Параметры сгорания еды")]
    [SerializeField] private KitchenObjectSO burntFoodSO; // Карточка сгоревшей еды (угли)
    [SerializeField] private float burningTimeMax = 8f;   // Время до сгорания после готовности (секунды)
    [SerializeField] private float shakeTimeBeforeBurnt = 4f; // За сколько секунд до сгорания начнется тряска кастрюли
    [Header("Визуал огня плиты")]
    [SerializeField] private GameObject cookingFlameVisual; // Физический объект пламени под кастрюлей/сковородой



    private KitchenObject currentObjectOnTable;
    private float cookingTimer = 0f;
    private float burningTimer = 0f;
    private bool isCooking = false;
    private bool isCookedAndWaiting = false; // Еда готова, но её не забирают
    private CookingRecipeSO activeRecipe;

    private void Start()
    {
        UpdateFlameVisual();
    }

    private void Update()
    {
        // 1. Процесс варки/жарки сырого продукта
        if (isCooking && currentObjectOnTable != null)
        {
            cookingTimer += Time.deltaTime;
            if (cookingTimer >= activeRecipe.cookingTimeMax)
            {
                // Приготовилось! Заменяем сырое на готовое блюдо
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
        if (isCookedAndWaiting && currentObjectOnTable != null)
        {
            burningTimer += Time.deltaTime;

            // Вычисляем, сколько секунд ОСТАЛОСЬ до сгорания еды
            float timeRemaining = burningTimeMax - burningTimer;

            // Если до сгорания осталось меньше времени, чем указано в shakeTimeBeforeBurnt — трясем!
            if (timeRemaining <= shakeTimeBeforeBurnt)
            {
                float shakeAmount = 0.03f; // Сила тряски

                currentObjectOnTable.transform.position = tablePoint.position + new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    0f,
                    Random.Range(-shakeAmount, shakeAmount)
                );

                if (Mathf.Repeat(burningTimer, 0.5f) < Time.deltaTime)
                {
                    SoundManager.Instance.PlayWarningSound(transform.position);
                }
            }

            // Время вышло — еда превращается в угли
            if (burningTimer >= burningTimeMax)
            {
                Destroy(currentObjectOnTable.gameObject);

                // Создаем угли ровно на плите БЕЗ родителя
                GameObject burntObject = Instantiate(burntFoodSO.prefab, tablePoint.position, Quaternion.identity);
                currentObjectOnTable = burntObject.GetComponent<KitchenObject>();
                currentObjectOnTable.ResetScale();

                // Отключаем коллайдер углям
                Collider col = burntObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                isCookedAndWaiting = false;
                Debug.LogWarning("Еда на плите сгорела и превратилась в угли!");
            }
        }

        UpdateFlameVisual();
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

    private void UpdateFlameVisual()
    {
        if (cookingFlameVisual != null)
        {
            // Огонь горит, только если идет активная готовка (isCooking) 
            // ИЛИ если еда уже сварилась, но еще горячая и ждет на плите (isCookedAndWaiting)
            bool shouldBurn = isCooking || isCookedAndWaiting;
            cookingFlameVisual.SetActive(shouldBurn);
        }
    }
}