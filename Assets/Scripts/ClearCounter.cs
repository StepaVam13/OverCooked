using UnityEngine;

public class ClearCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform tablePoint; // Точка для предмета на столе
    private KitchenObject currentObjectOnTable;     // Предмет, который сейчас на столе

    public void Interact(PlayerController player)
    {
        if (currentObjectOnTable == null)
        {
            if (player.HasKitchenObject())
            {
                currentObjectOnTable = player.GetKitchenObject();
                player.ClearKitchenObject();

                // Вместо parent = tablePoint пишем:
                currentObjectOnTable.transform.parent = null; // Убираем родителя!
                currentObjectOnTable.transform.position = tablePoint.position;
                currentObjectOnTable.transform.rotation = Quaternion.identity;
                currentObjectOnTable.ResetScale();
            }
        }
        else
        {
            // НА СТОЛЕ ЧТО-ТО ЛЕЖИТ
            if (!player.HasKitchenObject())
            {
                // Руки игрока пусты. Забираем предмет со стола.
                player.SetKitchenObject(currentObjectOnTable);
                currentObjectOnTable = null;
            }
            else
            {
                // И в руках у игрока есть предмет, и на столе лежит предмет.
                // Пытаемся объединить их (еду и тарелку).

                // Сценарий 1: На столе лежит ТАРЕЛКА, а в руках у игрока ЕДА
                PlateKitchenObject plateOnTable = currentObjectOnTable.GetComponent<PlateKitchenObject>();
                if (plateOnTable != null)
                {
                    // Пробуем добавить еду из рук в тарелку на столе
                    if (plateOnTable.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                    {
                        // Если успешно добавили, уничтожаем физический объект еды в руках игрока
                        Destroy(player.GetKitchenObject().gameObject);
                        player.ClearKitchenObject();
                    }
                }
                else
                {
                    // Сценарий 2: В руках у игрока ТАРЕЛКА, а на столе лежит ЕДА
                    PlateKitchenObject plateInHand = player.GetKitchenObject().GetComponent<PlateKitchenObject>();
                    if (plateInHand != null)
                    {
                        // Пробуем забрать еду со стола в тарелку в руках
                        if (plateInHand.TryAddIngredient(currentObjectOnTable.GetKitchenObjectSO()))
                        {
                            // Если успешно добавили, уничтожаем физический объект еды на столе
                            Destroy(currentObjectOnTable.gameObject);
                            currentObjectOnTable = null;
                        }
                    }
                }
            }
        }
    }

    public void InteractAlternate(PlayerController player) { }

    // Метод, который позволяет другим скриптам спавнить предметы на этот стол
    public bool TrySpawnObject(KitchenObjectSO kitchenObjectSO)
    {
        if (currentObjectOnTable != null) return false;

        GameObject spawnedObject = Instantiate(kitchenObjectSO.prefab, tablePoint.position, Quaternion.identity);
        currentObjectOnTable = spawnedObject.GetComponent<KitchenObject>();
        currentObjectOnTable.ResetScale();

        // ОТКЛЮЧАЕМ КОЛЛАЙДЕР, чтобы предмет на столе не мешал лучу игрока
        Collider col = spawnedObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        return true;
    }
}

