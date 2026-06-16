using System.Collections.Generic;
using UnityEngine;

public class DirtyPlateStackCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform tablePoint;
    [SerializeField] private KitchenObjectSO dirtyPlateSO;
    [SerializeField] private float plateHeightSpacing = 0.05f; // Высота шага между тарелками

    // Список физических грязных тарелок, лежащих в стопке
    private List<KitchenObject> dirtyPlatesStack = new List<KitchenObject>();

    public void Interact(PlayerController player)
    {
        if (!player.HasKitchenObject())
        {
            // У игрока пустые руки -> забираем САМУЮ ВЕРХНЮЮ тарелку из стопки
            if (dirtyPlatesStack.Count > 0)
            {
                KitchenObject topPlate = dirtyPlatesStack[dirtyPlatesStack.Count - 1];
                dirtyPlatesStack.RemoveAt(dirtyPlatesStack.Count - 1);

                player.SetKitchenObject(topPlate);
            }
        }
        else
        {
            // Если игрок хочет положить грязную тарелку обратно на этот стол
            if (player.GetKitchenObject().GetKitchenObjectSO() == dirtyPlateSO)
            {
                KitchenObject dirtyPlate = player.GetKitchenObject();
                player.ClearKitchenObject();

                AddPlateToStack(dirtyPlate);
            }
        }
    }

    public void InteractAlternate(PlayerController player) { }

    // Этот метод вызывает стол раздачи при успешной сдаче блюда
    public void SpawnDirtyPlate()
    {
        GameObject spawnedPlateObj = Instantiate(dirtyPlateSO.prefab);
        KitchenObject dirtyPlate = spawnedPlateObj.GetComponent<KitchenObject>();

        AddPlateToStack(dirtyPlate);
    }

    // Внутренний метод: красиво укладывает тарелку наверх стопки
    private void AddPlateToStack(KitchenObject dirtyPlate)
    {
        dirtyPlate.transform.parent = null; // убираем родителя (защита от масштаба)

        // Вычисляем позицию по высоте: базовая высота tablePoint + смещение на количество тарелок
        Vector3 spawnPosition = tablePoint.position + Vector3.up * (dirtyPlatesStack.Count * plateHeightSpacing);
        dirtyPlate.transform.position = spawnPosition;
        dirtyPlate.transform.rotation = Quaternion.identity;
        dirtyPlate.ResetScale();

        // Добавляем в список
        dirtyPlatesStack.Add(dirtyPlate);
    }
}