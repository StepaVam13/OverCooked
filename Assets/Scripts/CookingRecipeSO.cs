using UnityEngine;

[CreateAssetMenu(fileName = "NewCookingRecipe", menuName = "Kitchen/Cooking Recipe")]
public class CookingRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;      // Что варим (нарезанный томат)
    public KitchenObjectSO output;     // Что получаем (готовый суп)
    public float cookingTimeMax = 4f;  // Время варки в секундах
}