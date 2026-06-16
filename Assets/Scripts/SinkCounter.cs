using UnityEngine;

public class SinkCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform tablePoint;
    [SerializeField] private KitchenObjectSO cleanPlateSO; // Чистая тарелка
    [SerializeField] private KitchenObjectSO dirtyPlateSO; // Грязная тарелка
    [SerializeField] private int washStepsMax = 5;         // Сколько раз нажать F, чтобы помыть

    private KitchenObject currentObjectOnTable;
    private int washProgress = 0;

    public void Interact(PlayerController player)
    {
        if (currentObjectOnTable == null)
        {
            // Если в руках у игрока грязная тарелка, кладем её в раковину
            // Если в руках у игрока грязная тарелка, кладем её в раковину
            if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == dirtyPlateSO)
            {
                currentObjectOnTable = player.GetKitchenObject();
                player.ClearKitchenObject();

                currentObjectOnTable.transform.parent = null; // Убираем родителя!
                currentObjectOnTable.transform.position = tablePoint.position;
                currentObjectOnTable.transform.localRotation = Quaternion.identity;
                currentObjectOnTable.ResetScale();
                washProgress = 0;
            }
        }
        else
        {
            // Если тарелка уже помыта (стала чистой), забираем её
            if (!player.HasKitchenObject() && currentObjectOnTable.GetKitchenObjectSO() == cleanPlateSO)
            {
                player.SetKitchenObject(currentObjectOnTable);
                currentObjectOnTable = null;
            }
        }
    }

    public void InteractAlternate(PlayerController player)
    {
        // Процесс мытья (нажатие F)
        if (currentObjectOnTable != null && currentObjectOnTable.GetKitchenObjectSO() == dirtyPlateSO)
        {
            washProgress++;

            // Визуально можно покачивать тарелку при мытье
            currentObjectOnTable.transform.localRotation = Quaternion.Euler(0, washProgress * 20f, 0);
            if (washProgress >= washStepsMax)
            {
                Destroy(currentObjectOnTable.gameObject);

                // Спавним чистую тарелку БЕЗ родителя (убираем tablePoint из конца)
                GameObject cleanPlate = Instantiate(cleanPlateSO.prefab, tablePoint.position, Quaternion.identity);
                currentObjectOnTable = cleanPlate.GetComponent<KitchenObject>();
                currentObjectOnTable.ResetScale();
                Debug.Log("Тарелка успешно отмыта!");
            }
        }
    }
}