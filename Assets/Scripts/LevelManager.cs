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
    [SerializeField] private GameObject nextLevelButton; // Ссылка на кнопку "Следующий уровень" на панели

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

        // Останавливаем фоновую музыку
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMusic();
        }

        if (currentMoney >= targetMoney)
        {
            // --- ПОБЕДА ---
            int excessMoney = currentMoney - targetMoney;

            int globalMoney = PlayerPrefs.GetInt("GlobalMoney", 0);
            globalMoney += excessMoney;
            PlayerPrefs.SetInt("GlobalMoney", globalMoney);

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextLevelIndex = currentSceneIndex + 1;

            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (nextLevelIndex > unlockedLevel)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextLevelIndex);
            }

            PlayerPrefs.Save();

            endLevelResultText.text = $"ПОБЕДА!\nВы набрали: {currentMoney}$\nВ кошелек добавлено: +{excessMoney}$!";
            endLevelResultText.color = Color.green;

            // ВКЛЮЧАЕМ кнопку "Следующий уровень" только при победе!
            if (nextLevelButton != null) nextLevelButton.SetActive(true);
        }
        else
        {
            // --- ПОРАЖЕНИЕ ---
            endLevelResultText.text = $"ПОРАЖЕНИЕ!\nВы набрали только: {currentMoney}$";
            endLevelResultText.color = Color.red;

            // ВЫКЛЮЧАЕМ кнопку "Следующий уровень" при поражении!
            if (nextLevelButton != null) nextLevelButton.SetActive(false);
        }
    }

    // Кнопка: Следующий уровень (на экране победы)
    public void RestartLevel()
    {
        // ДОБАВЬТЕ ЭТУ СТРОКУ:
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        // ДОБАВЬТЕ ЭТУ СТРОКУ:
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void LoadNextLevel()
    {
        // ДОБАВЬТЕ ЭТУ СТРОКУ:
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();

        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex <= 5)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}