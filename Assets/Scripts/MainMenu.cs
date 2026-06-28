using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню для переключения")]
    [SerializeField] private GameObject mainPanel;      // Кнопки Играть/Настройки/Выход
    [SerializeField] private GameObject settingsPanel;  // Панель настроек кнопок
    [SerializeField] private GameObject gameModePanel;  // Панель выбора режима (Соло/Кооп)
    [Header("Отображение баланса")]
    [SerializeField] private Text globalMoneyText; // Текст баланса на главном экране

    private void Start()
    {
        Time.timeScale = 1f;
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);

        // ОБНОВЛЯЕМ БАЛАНС ПРИ СТАРТЕ МЕНЮ
        UpdateGlobalMoneyText();
    }

    // Открыть выбор режима (Один / Вдвоем)
    public void OpenGameModeSelection()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(true);
    }

    // Вернуться из выбора режима к главным кнопкам
    public void CloseGameModeSelection()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (gameModePanel != null) gameModePanel.SetActive(false);
    }

    // Кнопка: Одиночная игра
    public void PlaySolo()
    {
        PlayerPrefs.SetInt("IsCoop", 0); // 0 = Соло-режим
        PlayerPrefs.Save();
        SceneManager.LoadScene(1); // Запуск первого уровня
    }

    // Кнопка: Кооператив вдвоем
    public void PlayCoop()
    {
        PlayerPrefs.SetInt("IsCoop", 1); // 1 = Кооп-режим
        PlayerPrefs.Save();
        SceneManager.LoadScene(1); // Запуск первого уровня
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Игра закрыта!");
    }

    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // Метод для открытия 3D-магазина
    public void OpenShopScene()
    {
        Time.timeScale = 1f; // На всякий случай сбрасываем паузу

        // Загружаем сцену магазина по её точному названию в Unity
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