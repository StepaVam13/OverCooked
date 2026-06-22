using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
    [SerializeField] private GameObject highlightVisual; // Ссылка на рамку или объект свечения стола

    private void Start()
    {
        // Изначально выключаем подсветку стола
        if (highlightVisual != null)
        {
            highlightVisual.SetActive(false);
        }
    }

    // Метод включает или выключает подсветку
    public void SetSelected(bool isSelected)
    {
        if (highlightVisual != null)
        {
            highlightVisual.SetActive(isSelected);
        }
    }
}