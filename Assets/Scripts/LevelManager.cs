using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Параметры уровня")]
    [SerializeField] private float levelTimeMax = 180f; // Время уровня в секундах (3 минуты)
    [SerializeField] private int targetMoney = 500;     // Сумма для победы ($)

    [Header("UI элементы")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text moneyText;
    [SerializeField] private GameObject endLevelUI;    // Панель конца игры (Win/Lose)
    [SerializeField] private Text endLevelResultText;  // Текст Победа/Поражение

    private float currentTimer;
    private int currentMoney = 0;
    private bool isLevelOver = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentTimer = levelTimeMax;
        UpdateUI();
        endLevelUI.SetActive(false); // Прячем экран конца игры на старте
    }

    private void Update()
    {
        if (isLevelOver) return;

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
        {
            currentTimer = 0f;
            EndLevel();
        }

        UpdateUI();
    }

    // Начисление денег (с бонусом за скорость)
    public void AddMoney(int amount)
    {
        if (isLevelOver) return;
        currentMoney += amount;
        UpdateUI();
    }

    // Списание денег (штраф за просроченный заказ)
    public void DeductMoney(int amount)
    {
        if (isLevelOver) return;
        currentMoney -= amount;
        if (currentMoney < 0) currentMoney = 0; // Баланс не может уйти в минус
        UpdateUI();
    }

    private void UpdateUI()
    {
        timerText.text = $"ВРЕМЯ: {Mathf.CeilToInt(currentTimer)}с";
        moneyText.text = $"БАЛАНС: {currentMoney} / {targetMoney}$";
    }

    // Логика завершения уровня
    private void EndLevel()
    {
        isLevelOver = true;
        Time.timeScale = 0f; // Замораживаем игру
        endLevelUI.SetActive(true);

        if (currentMoney >= targetMoney)
        {
            // Считаем сдачу (всё, что заработано сверх цели)
            int excessMoney = currentMoney - targetMoney;

            // Сохраняем эти деньги в глобальный кошелек на компьютере
            int globalMoney = PlayerPrefs.GetInt("GlobalMoney", 0);
            globalMoney += excessMoney;
            PlayerPrefs.SetInt("GlobalMoney", globalMoney);
            PlayerPrefs.Save();

            endLevelResultText.text = $"ПОБЕДА!\nВы набрали: {currentMoney}$\nВ глобальный кошелек добавлено: +{excessMoney}$!";
            endLevelResultText.color = Color.green;
        }
        else
        {
            endLevelResultText.text = $"ПОРАЖЕНИЕ!\nВы набрали только: {currentMoney}$";
            endLevelResultText.color = Color.red;
        }
    }

    // Методы для кнопок на экране конца игры
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Переход в Главное меню
    }
}