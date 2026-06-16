using UnityEngine;

public class ContainerCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public void Interact(PlayerController player)
    {
        if (!player.HasKitchenObject())
        {
            // Спавним объект из префаба
            GameObject spawnedObject = Instantiate(kitchenObjectSO.prefab);
            KitchenObject kitchenObject = spawnedObject.GetComponent<KitchenObject>();

            // Даем его в руки игроку
            player.SetKitchenObject(kitchenObject);
        }
    }

    public void InteractAlternate(PlayerController player) { }
}