using UnityEngine;

public class ElectricBoxCounter : MonoBehaviour, IInteractable
{
    [Header("Красная лампа аварии")]
    [SerializeField] private GameObject alertLightVisual; // Объект лампочки

    private float blinkTimer = 0f;
    private bool isLightOn = false; // Независимый флаг состояния лампочки

    private void Start()
    {
        if (alertLightVisual != null) alertLightVisual.SetActive(false);
    }

    private void Update()
    {
        if (KitchenEventSystem.Instance == null || alertLightVisual == null) return;

        // Проверяем, темно ли сейчас на кухне
        bool isDark = KitchenEventSystem.Instance.IsDark();

        if (isDark)
        {
            // Мигаем красной лампочкой в темноте
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= 0.4f) // Мигаем чуть быстрее (каждые 0.4 секунды)
            {
                blinkTimer = 0f;
                isLightOn = !isLightOn; // Переключаем флаг
                alertLightVisual.SetActive(isLightOn); // Включаем/выключаем лампочку
            }
        }
        else
        {
            // Если свет горит — лампа гарантированно выключена
            alertLightVisual.SetActive(false);
            isLightOn = false;
        }
    }

    public void Interact(PlayerController player)
    {
        if (KitchenEventSystem.Instance != null && KitchenEventSystem.Instance.IsDark())
        {
            KitchenEventSystem.Instance.RestorePower();
        }
    }

    public void InteractAlternate(PlayerController player) { }
}