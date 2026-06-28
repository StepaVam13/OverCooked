using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню для переключения")]
    [SerializeField] private GameObject mainPanel;        // Кнопки Играть/Настройки/Выход
    [SerializeField] private GameObject settingsPanel;    // Панель настроек кнопок
    [SerializeField] private GameObject levelSelectPanel; // Панель выбора уровней (Новая!)
    [SerializeField] private GameObject gameModePanel;    // Панель выбора режима (Соло/Кооп)

    [Header("Отображение баланса")]
    [SerializeField] private Text globalMoneyText;

    [Header("Кнопки выбора уровней (для блокировки)")]
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;
    [SerializeField] private Button level4Button;
    [SerializeField] private Button level5Button;

    private int selectedLevelSceneIndex = 1; // Запоминаем выбранный уровень для запуска

    private void Start()
    {
        Time.timeScale = 1f;
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);

        UpdateGlobalMoneyText();
    }

    // Кнопка: ИГРАТЬ (теперь открывает панель выбора уровней)
    public void OpenLevelSelection()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound(); // <--- ДОБАВЬТЕ СЮДА
        if (mainPanel != null) mainPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);

        // Проверяем прогресс игрока и блокируем закрытые уровни
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1); // По умолчанию открыт только 1 уровень

        if (level2Button != null) level2Button.interactable = unlockedLevel >= 2;
        if (level3Button != null) level3Button.interactable = unlockedLevel >= 3;
        if (level4Button != null) level4Button.interactable = unlockedLevel >= 4;
        if (level5Button != null) level5Button.interactable = unlockedLevel >= 5;
    }

    public void CloseLevelSelection()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound(); // <--- ДОБАВЬТЕ СЮДА
        if (mainPanel != null) mainPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    // Метод: Игрок выбрал уровень (вызывается кнопками уровней)
    public void SelectLevel(int sceneIndex)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound(); // <--- ДОБАВЬТЕ СЮДА
        selectedLevelSceneIndex = sceneIndex; // Запоминаем сцену
        levelSelectPanel.SetActive(false);
        gameModePanel.SetActive(true); // Открываем выбор режима для этого уровня!
    }

    public void CloseGameModeSelection()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        gameModePanel.SetActive(false);
        levelSelectPanel.SetActive(true); // Возвращаемся к выбору уровней
    }

    // Запуск Обучения (запускается по имени сцены напрямую)
    public void PlayTutorial()
    {
        
        SceneManager.LoadScene("TutorialScene");
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
    }

    public void PlaySolo()
    {
        PlayerPrefs.SetInt("IsCoop", 0); // Соло
        PlayerPrefs.Save();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        SceneManager.LoadScene(selectedLevelSceneIndex); // Запуск выбранного уровня
    }

    public void PlayCoop()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        PlayerPrefs.SetInt("IsCoop", 1); // Кооп
        PlayerPrefs.Save();
        SceneManager.LoadScene(selectedLevelSceneIndex); // Запуск выбранного уровня
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Игра закрыта!");
    }

    public void OpenSettings()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound(); // <--- ДОБАВЬТЕ СЮДА
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenShopScene()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound(); // <--- ДОБАВЬТЕ СЮДА
        SceneManager.LoadScene("Shop3DScene");
    }

    public void UpdateGlobalMoneyText()
    {
        if (globalMoneyText != null)
        {
            int globalMoney = PlayerPrefs.GetInt("GlobalMoney", 0);
            globalMoneyText.text = $"БАЛАНС: {globalMoney}$";
        }
    }
}