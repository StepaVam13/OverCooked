using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    private Vector3 originalScale;

    private void Awake()
    {
        // Запоминаем масштаб, который вы настроили в префабе
        originalScale = transform.localScale;
    }

    // Метод для принудительного возврата родного масштаба
    public void ResetScale()
    {
        transform.localScale = originalScale;
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }
}