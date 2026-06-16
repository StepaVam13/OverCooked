using UnityEngine;

[CreateAssetMenu(fileName = "NewKitchenObject", menuName = "Kitchen/Kitchen Object")]
public class KitchenObjectSO : ScriptableObject
{
    public string objectName;      // Имя предмета (например, Томат)
    public GameObject prefab;      // 3D-модель предмета
    public Sprite icon;            // Иконка для UI
}