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
        public GameObject modelObject;
        public int price;
        public float speed = 7f;
        public float acceleration = 5f;
        public string boardID;
    }

    [Header("Список гироскутеров")]
    [SerializeField] private List<HoverboardItem> shopItems;
    [SerializeField] private Transform rotationPlatform;

    [Header("UI Элементы сцены")]
    [SerializeField] private Text globalMoneyText;
    [SerializeField] private Text boardNameText;
    [SerializeField] private Text statsText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Text actionButtonText;

    [Header("UI Выбора игрока в магазине")]
    [SerializeField] private Text playerSelectionText; // Текст "НАСТРОЙКА: ИГРОК 1"

    [SerializeField] private float modelYOffset = 2.5f; // Высота гироскутера над подиумом (верните сюда ваши 2.5)

    private int currentIndex = 0;
    private string playerPrefix = "P1_"; // По умолчанию настраиваем Первого игрока

    private void Start()
    {
        currentIndex = 0;
        UpdateShopScene();
    }

    // Метод для переключения магазина между Игроком 1 и Игроком 2
    public void TogglePlayerShop()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        if (playerPrefix == "P1_")
        {
            playerPrefix = "P2_";
            playerSelectionText.text = "НАСТРОЙКА: ИГРОК 2";
        }
        else
        {
            playerPrefix = "P1_";
            playerSelectionText.text = "НАСТРОЙКА: ИГРОК 1";
        }
        UpdateShopScene();
    }

    public void NextItem()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        currentIndex = (currentIndex + 1) % shopItems.Count;
        UpdateShopScene();
    }

    public void PreviousItem()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        currentIndex--;
        if (currentIndex < 0) currentIndex = shopItems.Count - 1;
        UpdateShopScene();
    }

    private void UpdateShopScene()
    {
        if (globalMoneyText == null || rotationPlatform == null || boardNameText == null || statsText == null || actionButton == null || actionButtonText == null || playerSelectionText == null)
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: Одно или несколько UI-полей не привязаны в инспекторе Shop3DManager!");
            return;
        }

        int globalMoney = PlayerPrefs.GetInt("GlobalMoney", 0);
        globalMoneyText.text = $"БАЛАНС: {globalMoney}$";

        rotationPlatform.rotation = Quaternion.identity;

        for (int i = 0; i < shopItems.Count; i++)
        {
            if (shopItems[i].modelObject == null)
            {
                Debug.LogError($"КРИТИЧЕСКАЯ ОШИБКА: У элемента {i} не привязана 3D-модель!");
                return;
            }

            bool isActive = (i == currentIndex);
            shopItems[i].modelObject.SetActive(isActive);

            if (isActive)
            {
                shopItems[i].modelObject.transform.parent = rotationPlatform;
                shopItems[i].modelObject.transform.localPosition = new Vector3(0f, modelYOffset, 0f); // Используем отступ по высоте
            }
        }

        HoverboardItem currentItem = shopItems[currentIndex];
        boardNameText.text = currentItem.boardName;
        statsText.text = $"СКОРОСТЬ: {currentItem.speed}\nРАЗГОН: {currentItem.acceleration}";

        // Проверяем статус гироскутера под конкретного игрока (используем playerPrefix!)
        string equippedBoard = PlayerPrefs.GetString(playerPrefix + "EquippedBoard", "Default");

        if (currentItem.boardID == "Default")
        {
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
            // Проверяем покупку с учетом префикса игрока (у каждого свой кошелек покупок!)
            bool isPurchased = PlayerPrefs.GetInt(playerPrefix + "Bought_" + currentItem.boardID, 0) == 1;

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
                actionButton.interactable = globalMoney >= currentItem.price;
            }
        }
    }

    public void OnActionButtonClick()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        HoverboardItem currentItem = shopItems[currentIndex];
        int globalMoney = PlayerPrefs.GetInt("GlobalMoney", 0);
        string equippedBoard = PlayerPrefs.GetString(playerPrefix + "EquippedBoard", "Default");

        if (currentItem.boardID == "Default")
        {
            PlayerPrefs.SetString(playerPrefix + "EquippedBoard", "Default");
        }
        else
        {
            bool isPurchased = PlayerPrefs.GetInt(playerPrefix + "Bought_" + currentItem.boardID, 0) == 1;

            if (isPurchased)
            {
                PlayerPrefs.SetString(playerPrefix + "EquippedBoard", currentItem.boardID);
            }
            else
            {
                if (globalMoney >= currentItem.price)
                {
                    globalMoney -= currentItem.price;
                    PlayerPrefs.SetInt("GlobalMoney", globalMoney);
                    PlayerPrefs.SetInt(playerPrefix + "Bought_" + currentItem.boardID, 1);
                    PlayerPrefs.SetString(playerPrefix + "EquippedBoard", currentItem.boardID);
                }
            }
        }

        PlayerPrefs.Save();
        UpdateShopScene();
    }

    public void BackToMenu()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClickSound();
        SceneManager.LoadScene(0);
    }
}