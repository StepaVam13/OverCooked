using UnityEngine;

public class KitchenEventSystem : MonoBehaviour
{
    public static KitchenEventSystem Instance { get; private set; }

    [Header("Настройки освещения")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light player1Spotlight;  // Фара Первого игрока
    [SerializeField] private Light player2Spotlight;  // Фара Второго игрока
    [SerializeField] private float normalIntensity = 1f;
    [SerializeField] private float darkIntensity = 0.05f;

    [Header("Тайминг ивента")]
    [SerializeField] private float eventInterval = 45f;

    private float timer = 0f;
    private bool isDark = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (directionalLight != null) directionalLight.intensity = normalIntensity;
        if (player1Spotlight != null) player1Spotlight.gameObject.SetActive(false);
        if (player2Spotlight != null) player2Spotlight.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (!isDark)
        {
            timer += Time.deltaTime;
            if (timer >= eventInterval)
            {
                StartDarkEvent();
            }
        }
    }

    private void StartDarkEvent()
    {
        isDark = true;
        timer = 0f;

        if (directionalLight != null) directionalLight.intensity = darkIntensity;

        // Включаем фару Первого игрока
        if (player1Spotlight != null) player1Spotlight.gameObject.SetActive(true);

        // Включаем фару Второго игрока, только если он сам сейчас активен на сцене (режим кооператива)
        if (player2Spotlight != null && player2Spotlight.transform.root.gameObject.activeInHierarchy)
        {
            player2Spotlight.gameObject.SetActive(true);
        }

        Debug.Log("ВНИМАНИЕ: На кухне выбило пробки! Доедьте до электрощитка на стене и включите автомат на [E]!");
    }

    public void RestorePower()
    {
        if (!isDark) return;

        isDark = false;
        timer = 0f;

        if (directionalLight != null) directionalLight.intensity = normalIntensity;

        // Выключаем фары у обоих игроков
        if (player1Spotlight != null) player1Spotlight.gameObject.SetActive(false);
        if (player2Spotlight != null) player2Spotlight.gameObject.SetActive(false);

        Debug.Log("Электричество успешно восстановлено!");
    }

    public bool IsDark() => isDark;
}