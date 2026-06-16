using UnityEngine;

[CreateAssetMenu(fileName = "NewCuttingRecipe", menuName = "Kitchen/Cutting Recipe")]
public class CuttingRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;      // Что режем (например, сырой томат)
    public KitchenObjectSO output;     // Что получаем (нарезанный томат)
    public int maxProgressSteps = 5;   // Сколько раз нужно нажать "F", чтобы нарезать
}