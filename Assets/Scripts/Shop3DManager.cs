using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Shop3DManager : MonoBehaviour
{
    [System.Serializable]
    public class HoverboardItem
    {
        public string boardName;
        public GameObject modelObject; // 3D модель этого гироскутера на этой сцене
        public int price;
        public float speed = 7f;       // Скорость этого гироскутера в игре
        public float acceleration = 5f; // Разгон этого гироскутера в игре
        public string boardID;         // Уникальный ID (например: "Default", "Speedster", "Drifter")
    }

    [Header("Список гироскутеров")]
    [SerializeField] private List<HoverboardItem> shopItems;
    [SerializeField] private Transform rotationPlatform; // Вращающаяся подставка

    [Header("UI Элементы сцены")]
    [SerializeField] private Text globalMoneyText;
    [SerializeField] private Text boardNameText;
    [SerializeField] private Text statsText;
    [SerializeField] private Button actionButton; // Кнопка КУПИТЬ / ВЫБРАТЬ
    [SerializeField] private Text actionButtonText;

    [SerializeField] private float modelYOffset = 0.2f; // Высота гироскутера над подиумом

    private int currentIndex = 0;

    private void Start()
    {
        currentIndex = 0;
        UpdateShopScene();
    }

    // Кнопка стрелочки Вправо
    public void NextItem()
    {
        currentIndex = (currentIndex + 1) % shopItems.Count;
        UpdateShopScene();
    }

    // Кнопка стрелочки Влево
    public void PreviousItem()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = shopItems.Count - 1;
        UpdateShopScene();
    }

    private void UpdateShopScene()
    {
        int globalMoney = PlayerPrefs.GetInt("GlobalMoney", 0);
        globalMoneyText.text = $"БАЛАНС: {globalMoney}$";

        // Сбрасываем вращение платформы при переключении, чтобы новый гироскутер стоял ровно
        rotationPlatform.rotation = Quaternion.identity;

        // Включаем модель только выбранного гироскутера и привязываем его к платформе вращения
        for (int i = 0; i < shopItems.Count; i++)
        {
            bool isActive = (i == currentIndex);
            shopItems[i].modelObject.SetActive(isActive);

            if (isActive)
            {
                shopItems[i].modelObject.transform.parent = rotationPlatform;
                shopItems[i].modelObject.transform.localPosition = new Vector3(0f, modelYOffset, 0f);
                
            }
        }

        HoverboardItem currentItem = shopItems[currentIndex];
        boardNameText.text = currentItem.boardName;
        statsText.text = $"СКОРОСТЬ: {currentItem.speed}\nРАЗГОН: {currentItem.acceleration}";

        // Проверяем статус гироскутера (Куплен / Экипирован / Закрыт)
        string equippedBoard = PlayerPrefs.GetString("EquippedBoard", "Default");

        if (currentItem.boardID == "Default")
        {
            // Стартовый гироскутер всегда куплен
            if (equippedBoard == "Default")
            {
                actionButtonText.text = "ЭКИПИРОВАНО";
                actionButton.interactable = false;
            }
            else
            {
                actionButtonText.text = "ВЫБРАТЬ";
                actionButton.interactable = true;
            }
        }
        else
        {
            // Для покупных проверяем статус владения в памяти
            bool isPurchased = PlayerPrefs.GetInt("Bought_" + currentItem.boardID, 0) == 1;

            if (isPurchased)
            {
                if (equippedBoard == currentItem.boardID)
                {
                    actionButtonText.text = "ЭКИПИРОВАНО";
                    actionButton.interactable = false;
                }
                else
                {
                    actionButtonText.text = "ВЫБРАТЬ";
                    actionButton.interactable = true;
                }
            }
            else
            {
                actionButtonText.text = $"КУПИТЬ ({currentItem.price}$)";
                // Кнопка покупки активна только если хватает денег в кошельке
                actionButton.interactable = globalMoney >= currentItem.price;
            }
        }
    }

    // Событие при нажатии на главную кнопку действия (Купить или Экипировать)
    public void OnActionButtonClick()
    {
        HoverboardItem currentItem = shopItems[currentIndex];
        int globalMoney = PlayerPrefs.GetInt("GlobalMoney", 0);

        if (currentItem.boardID == "Default")
        {
            PlayerPrefs.SetString("EquippedBoard", "Default");
        }
        else
        {
            bool isPurchased = PlayerPrefs.GetInt("Bought_" + currentItem.boardID, 0) == 1;

            if (isPurchased)
            {
                PlayerPrefs.SetString("EquippedBoard", currentItem.boardID);
            }
            else
            {
                if (globalMoney >= currentItem.price)
                {
                    globalMoney -= currentItem.price;
                    PlayerPrefs.SetInt("GlobalMoney", globalMoney);
                    PlayerPrefs.SetInt("Bought_" + currentItem.boardID, 1);
                    PlayerPrefs.SetString("EquippedBoard", currentItem.boardID);
                }
            }
        }

        PlayerPrefs.Save();
        UpdateShopScene();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0); // Возврат в Главное меню
    }
}