using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Kitchen/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;                          // Название блюда (например, Томатный суп)
    public List<KitchenObjectSO> requiredIngredients; // Список ингредиентов, которые должны быть на тарелке
}