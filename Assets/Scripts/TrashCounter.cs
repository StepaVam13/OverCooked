using UnityEngine;

public class TrashCounter : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject heldObject = player.GetKitchenObject();

            // Проверяем, держит ли игрок тарелку
            PlateKitchenObject plate = heldObject.GetComponent<PlateKitchenObject>();

            if (plate != null)
            {
                // Если в руках тарелка — очищаем её содержимое, но саму тарелку оставляем в руках
                plate.ClearPlate();
                Debug.Log("Тарелка очищена в мусорке!");
            }
            else
            {
                // Если в руках любой другой предмет (картошка, помидор, угли) — уничтожаем его полностью
                Destroy(heldObject.gameObject);
                player.ClearKitchenObject();
                Debug.Log("Предмет выброшен в мусорку!");
            }
        }
    }

    public void InteractAlternate(PlayerController player) { }
}