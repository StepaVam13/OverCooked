using UnityEngine;

public class DeliveryCounter : MonoBehaviour, IInteractable
{
    // Теперь ссылаемся на специальный скрипт стопки грязной посуды
    [SerializeField] private DirtyPlateStackCounter dirtyPlateReturnCounter;

    public void Interact(PlayerController player)
    {
        if (player.HasKitchenObject())
        {
            PlateKitchenObject plate = player.GetKitchenObject().GetComponent<PlateKitchenObject>();
            if (plate != null)
            {
                bool isSuccess = OrderManager.Instance.DeliverDish(plate.GetIngredients());
                if (isSuccess)
                {
                    Destroy(plate.gameObject);
                    player.ClearKitchenObject();

                    // Даем сигнал стопке создать новую грязную тарелку наверх
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