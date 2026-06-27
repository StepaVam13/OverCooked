using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyRebindManager : MonoBehaviour
{
    [Header("UI Кнопки Player 1")]
    [SerializeField] private Button p1UpBtn;
    [SerializeField] private Button p1DownBtn;
    [SerializeField] private Button p1LeftBtn;
    [SerializeField] private Button p1RightBtn;
    [SerializeField] private Button p1InteractBtn;
    [SerializeField] private Button p1ChopBtn;
    [SerializeField] private Button p1ThrowBtn;

    [Header("UI Кнопки Player 2")]
    [SerializeField] private Button p2UpBtn;
    [SerializeField] private Button p2DownBtn;
    [SerializeField] private Button p2LeftBtn;
    [SerializeField] private Button p2RightBtn;
    [SerializeField] private Button p2InteractBtn;
    [SerializeField] private Button p2ChopBtn;
    [SerializeField] private Button p2ThrowBtn;

    private Button activeRebindButton;
    private string activePrefsKey;
    private bool isRebinding = false;

    private void Start()
    {
        UpdateAllButtonLabels();
    }

    // Метод обновляет текст на всех кнопках настроек согласно сохраненным клавишам
    private void UpdateAllButtonLabels()
    {
        UpdateButtonLabel(p1UpBtn, "P1_Up", KeyCode.W);
        UpdateButtonLabel(p1DownBtn, "P1_Down", KeyCode.S);
        UpdateButtonLabel(p1LeftBtn, "P1_Left", KeyCode.A);
        UpdateButtonLabel(p1RightBtn, "P1_Right", KeyCode.D);
        UpdateButtonLabel(p1InteractBtn, "P1_Interact", KeyCode.E);
        UpdateButtonLabel(p1ChopBtn, "P1_Chop", KeyCode.F);
        UpdateButtonLabel(p1ThrowBtn, "P1_Throw", KeyCode.Space);

        UpdateButtonLabel(p2UpBtn, "P2_Up", KeyCode.UpArrow);
        UpdateButtonLabel(p2DownBtn, "P2_Down", KeyCode.DownArrow);
        UpdateButtonLabel(p2LeftBtn, "P2_Left", KeyCode.LeftArrow);
        UpdateButtonLabel(p2RightBtn, "P2_Right", KeyCode.RightArrow);
        UpdateButtonLabel(p2InteractBtn, "P2_Interact", KeyCode.L);
        UpdateButtonLabel(p2ChopBtn, "P2_Chop", KeyCode.K);
        UpdateButtonLabel(p2ThrowBtn, "P2_Throw", KeyCode.O);
    }

    private void UpdateButtonLabel(Button button, string prefsKey, KeyCode defaultKey)
    {
        if (button == null) return;
        KeyCode savedKey = (KeyCode)PlayerPrefs.GetInt(prefsKey, (int)defaultKey);
        button.GetComponentInChildren<Text>().text = savedKey.ToString();
    }

    // Этот метод мы вызываем при клике по кнопке настройки в инспекторе!
    public void StartRebind(string prefsKey)
    {
        if (isRebinding) return;

        activePrefsKey = prefsKey;

        // Автоматически находим, какую именно кнопку нажал игрок через EventSystem
        GameObject selectedObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (selectedObj != null)
        {
            activeRebindButton = selectedObj.GetComponent<Button>();
            activeRebindButton.GetComponentInChildren<Text>().text = "...";
            isRebinding = true;
            StartCoroutine(WaitForKeyPress());
        }
    }

    private IEnumerator WaitForKeyPress()
    {
        yield return new WaitForSecondsRealtime(0.1f); // Минимальная задержка против случайного клика

        while (isRebinding)
        {
            if (Input.anyKeyDown)
            {
                // Ищем, какую именно кнопку на клавиатуре нажал игрок
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode))
                    {
                        // Запрещаем назначать Escape и Q (так как они зарезервированы под меню/паузу)
                        if (kcode == KeyCode.Escape || kcode == KeyCode.Q) continue;

                        // Сохраняем клавишу в память компьютера
                        PlayerPrefs.SetInt(activePrefsKey, (int)kcode);
                        PlayerPrefs.Save();

                        // Обновляем текст на кнопке
                        activeRebindButton.GetComponentInChildren<Text>().text = kcode.ToString();
                        isRebinding = false;

                        // Говорим игрокам на сцене немедленно обновить свои клавиши!
                        NotifyPlayersToLoadKeybinds();
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    private void NotifyPlayersToLoadKeybinds()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController p in players)
        {
            p.LoadKeybinds();
        }
    }
}