using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Метод для запуска игры (загружает сцену по индексу 1)
    public void PlayGame()
    {
        // Загружает следующую сцену в списке сборки
        SceneManager.LoadScene(1);
    }

    // Метод для выхода из игры
    public void QuitGame()
    {
        Application.Quit(); // Закрывает запущенное приложение
        Debug.Log("Игра закрыта!"); // Будет писаться в консоль редактора Unity
    }
}