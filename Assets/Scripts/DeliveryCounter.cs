using UnityEngine;

public class DeliveryCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private DirtyPlateStackCounter dirtyPlateReturnCounter;
    [SerializeField] private KitchenObjectSO dirtyPlateSO;

    public void Interact(PlayerController player)
    {
        if (player.HasKitchenObject())
        {
            PlateKitchenObject plate = player.GetKitchenObject().GetComponent<PlateKitchenObject>();
            if (plate != null)
            {
                // ОСОБЫЙ ОБХОД ДЛЯ ОБУЧЕНИЯ:
                // Если на сцене нет менеджера заказов — просто принимаем любую тарелку, в которой есть еда
                if (OrderManager.Instance == null)
                {
                    if (plate.GetIngredients().Count > 0)
                    {
                        Destroy(plate.gameObject);
                        player.ClearKitchenObject();
                        Debug.Log("Тарелка успешно сдана в режиме обучения!");
                    }
                    return;
                }

                // Стандартный режим игры:
                bool isSuccess = OrderManager.Instance.DeliverDish(plate.GetIngredients());
                if (isSuccess)
                {
                    Destroy(plate.gameObject);
                    player.ClearKitchenObject();

                    SoundManager.Instance.PlaySuccessSound();

                    if (dirtyPlateReturnCounter != null)
                    {
                        dirtyPlateReturnCounter.SpawnDirtyPlate();
                    }
                }
            }
        }
    }

    public void InteractAlternate(PlayerController player) { }
}