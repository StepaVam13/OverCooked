using System.Collections.Generic;
using UnityEngine;

public class PlateStackCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private KitchenObjectSO cleanPlateSO; // Ссылка на карточку чистой тарелки
    [SerializeField] private List<GameObject> plateVisuals; // Список 3D-моделей тарелок в стопке (снизу вверх)

    private int platesCount;
    private int maxPlatesCount;

    private void Start()
    {
        maxPlatesCount = plateVisuals.Count;
        platesCount = maxPlatesCount; // Изначально стопка полная
        UpdateVisuals();
    }

    public void Interact(PlayerController player)
    {
        if (!player.HasKitchenObject())
        {
            // У ИГРОКА ПУСТЫЕ РУКИ -> Пытаемся взять тарелку
            if (platesCount > 0)
            {
                platesCount--;
                UpdateVisuals();

                // Спавним тарелку в руки игроку БЕЗ родителя (чтобы масштаб не ломался)
                GameObject spawnedPlate = Instantiate(cleanPlateSO.prefab, player.transform.position, Quaternion.identity);
                KitchenObject kitchenObject = spawnedPlate.GetComponent<KitchenObject>();
                player.SetKitchenObject(kitchenObject);
            }
        }
        else
        {
            // У ИГРОКА ЧТО-ТО В РУКАХ -> Проверяем, чистая ли это тарелка
            if (player.GetKitchenObject().GetKitchenObjectSO() == cleanPlateSO)
            {
                // Проверяем, есть ли место в стопке
                if (platesCount < maxPlatesCount)
                {
                    platesCount++;
                    UpdateVisuals();

                    // Уничтожаем тарелку в руках у игрока
                    Destroy(player.GetKitchenObject().gameObject);
                    player.ClearKitchenObject();
                }
            }
        }
    }

    public void InteractAlternate(PlayerController player) { }

    // Метод включает только те тарелки, индекс которых меньше текущего количества
    private void UpdateVisuals()
    {
        for (int i = 0; i < plateVisuals.Count; i++)
        {
            plateVisuals[i].SetActive(i < platesCount);
        }
    }
}