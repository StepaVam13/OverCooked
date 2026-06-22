using UnityEngine;

public class KitchenEventSystem : MonoBehaviour
{
    [Header("Настройки освещения")]
    [SerializeField] private Light directionalLight; // Главный свет сцены (солнце)
    [SerializeField] private Light playerSpotlight;   // Налобный фонарик игрока
    [SerializeField] private float normalIntensity = 1f; // Обычная яркость сцены
    [SerializeField] private float darkIntensity = 0.05f; // Яркость при отключении света

    [Header("Тайминги ивента")]
    [SerializeField] private float eventInterval = 40f; // Как часто выключается свет (секунды)
    [SerializeField] private float eventDuration = 10f; // На сколько секунд выключается свет

    private float timer = 0f;
    private bool isDark = false;

    private void Start()
    {
        // На старте гарантируем, что свет горит, а фонарик выключен
        if (directionalLight != null) directionalLight.intensity = normalIntensity;
        if (playerSpotlight != null) playerSpotlight.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Не запускаем ивент паузы, если игра на паузе
        if (Time.timeScale == 0f) return;

        timer += Time.deltaTime;

        if (!isDark)
        {
            // Ждем начала отключения света
            if (timer >= eventInterval)
            {
                StartDarkEvent();
            }
        }
        else
        {
            // Ждем, пока свет снова дадут
            if (timer >= eventDuration)
            {
                StopDarkEvent();
            }
        }
    }

    // Выключаем свет
    private void StartDarkEvent()
    {
        isDark = true;
        timer = 0f;

        if (directionalLight != null) directionalLight.intensity = darkIntensity;
        if (playerSpotlight != null) playerSpotlight.gameObject.SetActive(true);

        Debug.Log("ВНИМАНИЕ: На кухне выбило пробки! Работаем при фонариках!");
    }

    // Включаем свет обратно
    private void StopDarkEvent()
    {
        isDark = false;
        timer = 0f;

        if (directionalLight != null) directionalLight.intensity = normalIntensity;
        if (playerSpotlight != null) playerSpotlight.gameObject.SetActive(false);

        Debug.Log("Электричество восстановлено!");
    }
}