using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep
    {
        GrabTomato,
        GoToCuttingBoard,
        ChopTomato,
        TakeChoppedTomato,
        GoToStove,
        WaitForCook,
        GrabPotato,
        SlicePotato,
        FryFries,
        GrabDirtyPlate,
        WashPlate,
        PlateFood,
        Deliver,
        Complete
    }

    [Header("Игрок")]
    [SerializeField] private PlayerController player;

    [Header("Столы (Подсветка)")]
    [SerializeField] private InteractableHighlight tomatoContainer;
    [SerializeField] private InteractableHighlight potatoContainer;
    [SerializeField] private InteractableHighlight cuttingBoard;
    [SerializeField] private InteractableHighlight stove;
    [SerializeField] private InteractableHighlight pan;
    [SerializeField] private InteractableHighlight dirtyPlatesStack;
    [SerializeField] private InteractableHighlight sink;
    [SerializeField] private InteractableHighlight deliveryTable;

    [Header("UI Обучения")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Text actionPromptText;

    [Header("Настройки ингредиентов")]
    [SerializeField] private KitchenObjectSO tomatoRawSO;
    [SerializeField] private KitchenObjectSO tomatoChoppedSO;
    [SerializeField] private KitchenObjectSO tomatoSoupSO;
    [SerializeField] private KitchenObjectSO potatoRawSO;
    [SerializeField] private KitchenObjectSO potatoChoppedSO;
    [SerializeField] private KitchenObjectSO friesSO;
    [SerializeField] private KitchenObjectSO cleanPlateSO;
    [SerializeField] private KitchenObjectSO dirtyPlateSO;

    private TutorialStep currentStep;
    private bool isWaitingForContinue = false;

    private void Start()
    {
        Time.timeScale = 1f;
        SetStep(TutorialStep.GrabTomato);
    }

    private void Update()
    {
        // Изменили считывание клавиши со Space на Q!
        if (isWaitingForContinue)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (currentStep == TutorialStep.Complete)
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(0); // Выход в меню
                }
                else
                {
                    StartActionPhase();
                }
            }
            return;
        }

        CheckTutorialProgress();
    }

    private void SetStep(TutorialStep step)
    {
        currentStep = step;
        isWaitingForContinue = true;
        player.isMovementFrozen = true;
        tutorialPanel.SetActive(true);
        actionPromptText.text = "Нажми Q, чтобы начать выполнять"; // <--- Текст обновлен под Q

        // Гасим все подсветки
        tomatoContainer.SetSelected(false);
        if (potatoContainer != null) potatoContainer.SetSelected(false);
        cuttingBoard.SetSelected(false);
        stove.SetSelected(false);
        if (pan != null) pan.SetSelected(false);
        if (dirtyPlatesStack != null) dirtyPlatesStack.SetSelected(false);
        if (sink != null) sink.SetSelected(false);
        deliveryTable.SetSelected(false);

        switch (step)
        {
            case TutorialStep.GrabTomato:
                tutorialText.text = "Добро пожаловать на кухню!\n\nПодойди к ЯЩИКУ С ПОМИДОРАМИ (подсвечен желтым) и нажми E, чтобы взять помидор.";
                tomatoContainer.SetSelected(true);
                break;

            case TutorialStep.GoToCuttingBoard:
                tutorialText.text = "Отлично, помидор в руках!\n\nТеперь отнеси его на РАЗДЕЛOЧНУЮ ДОСКУ и нажми E, чтобы положить.";
                cuttingBoard.SetSelected(true);
                break;

            case TutorialStep.ChopTomato:
                tutorialText.text = "Помидор на доске.\n\nНажимай клавишу F несколько раз, чтобы нарезать его.";
                cuttingBoard.SetSelected(true);
                break;

            case TutorialStep.TakeChoppedTomato:
                tutorialText.text = "Помидор нарезан!\n\nНажми E, чтобы забрать нарезанный помидор в руки.";
                cuttingBoard.SetSelected(true);
                break;

            case TutorialStep.GoToStove:
                tutorialText.text = "Теперь несем нарезку вариться.\n\nПодойди к ПЛИТЕ С КАСТРЮЛЕЙ и нажми E, чтобы положить помидор.";
                stove.SetSelected(true);
                break;

            case TutorialStep.WaitForCook:
                tutorialText.text = "Суп варится автоматически.\n\nПока он готовится, давай займемся картофелем фри!";
                stove.SetSelected(true);
                break;

            case TutorialStep.GrabPotato:
                tutorialText.text = "Подойди к КОРОБКЕ С КАРТОФЕЛЕМ и нажми E, чтобы взять одну картофелину.";
                if (potatoContainer != null) potatoContainer.SetSelected(true);
                break;

            case TutorialStep.SlicePotato:
                tutorialText.text = "Картошку тоже нужно нарезать.\n\nОтнеси её на РАЗДЕЛOЧНУЮ ДОСКУ, нажми E (положить) и нарежь на F.";
                cuttingBoard.SetSelected(true);
                break;

            case TutorialStep.FryFries:
                tutorialText.text = "Теперь пожарим картофель фри.\n\nЗабери нарезанную картошку с доски, отнеси к СКОВОРОДКЕ и нажми E для жарки. Подожди, пока она пожарится.";
                if (pan != null) pan.SetSelected(true);
                break;

            case TutorialStep.GrabDirtyPlate:
                tutorialText.text = "Картофель фри и суп готовы! Но у нас закончились чистые тарелки.\n\nПодойди к СТОЛУ С ГРЯЗНОЙ ПОСУДОЙ и нажми E, чтобы взять одну грязную тарелку.";
                if (dirtyPlatesStack != null) dirtyPlatesStack.SetSelected(true);
                break;

            case TutorialStep.WashPlate:
                tutorialText.text = "Тарелка грязная, в неё нельзя класть еду.\n\nОтнеси её в РАКОВИНУ, нажми E (положить) и зажимай/нажимай F, чтобы отмыть её.";
                if (sink != null) sink.SetSelected(true);
                break;

            case TutorialStep.PlateFood:
                tutorialText.text = "Отлично, теперь у тебя в руках чистая тарелка!\n\nПодойди к плите с готовым супом (или к сковородке) и нажми E, чтобы положить еду в тарелку.";
                stove.SetSelected(true);
                break;

            case TutorialStep.Deliver:
                tutorialText.text = "Блюдо в тарелке!\n\nОтнеси готовое блюдо на СТОЛ ВЫДАЧИ ЗАКАЗОВ и нажми E.";
                deliveryTable.SetSelected(true);
                break;
        }
    }

    private void StartActionPhase()
    {
        isWaitingForContinue = false;
        player.isMovementFrozen = false;
        tutorialPanel.SetActive(false);
    }

    private void CheckTutorialProgress()
    {
        switch (currentStep)
        {
            case TutorialStep.GrabTomato:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == tomatoRawSO)
                {
                    SetStep(TutorialStep.GoToCuttingBoard);
                }
                break;

            case TutorialStep.GoToCuttingBoard:
                if (!player.HasKitchenObject() && FindItemOnCounter(cuttingBoard) == tomatoRawSO)
                {
                    SetStep(TutorialStep.ChopTomato);
                }
                break;

            case TutorialStep.ChopTomato:
                if (FindItemOnCounter(cuttingBoard) == tomatoChoppedSO)
                {
                    SetStep(TutorialStep.TakeChoppedTomato);
                }
                break;

            case TutorialStep.TakeChoppedTomato:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == tomatoChoppedSO)
                {
                    SetStep(TutorialStep.GoToStove);
                }
                break;

            case TutorialStep.GoToStove:
                if (!player.HasKitchenObject() && FindItemOnCounter(stove) == tomatoChoppedSO)
                {
                    SetStep(TutorialStep.WaitForCook);
                }
                break;

            case TutorialStep.WaitForCook:
                if (FindItemOnCounter(stove) == tomatoSoupSO)
                {
                    SetStep(TutorialStep.GrabPotato);
                }
                break;

            case TutorialStep.GrabPotato:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == potatoRawSO)
                {
                    SetStep(TutorialStep.SlicePotato);
                }
                break;

            case TutorialStep.SlicePotato:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == potatoChoppedSO)
                {
                    SetStep(TutorialStep.FryFries);
                }
                break;

            case TutorialStep.FryFries:
                if (FindItemOnCounter(pan) == friesSO)
                {
                    SetStep(TutorialStep.GrabDirtyPlate);
                }
                break;

            case TutorialStep.GrabDirtyPlate:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == dirtyPlateSO)
                {
                    SetStep(TutorialStep.WashPlate);
                }
                break;

            case TutorialStep.WashPlate:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == cleanPlateSO)
                {
                    SetStep(TutorialStep.PlateFood);
                }
                break;

            case TutorialStep.PlateFood:
                if (player.HasKitchenObject() && player.GetKitchenObject() is PlateKitchenObject)
                {
                    PlateKitchenObject plate = player.GetKitchenObject() as PlateKitchenObject;
                    if (plate.GetIngredients().Contains(tomatoSoupSO) || plate.GetIngredients().Contains(friesSO))
                    {
                        SetStep(TutorialStep.Deliver);
                    }
                }
                break;

            case TutorialStep.Deliver:
                if (!player.HasKitchenObject())
                {
                    CompleteTutorial();
                }
                break;
        }
    }

    private KitchenObjectSO FindItemOnCounter(InteractableHighlight counterHighlight)
    {
        if (counterHighlight == null) return null;

        if (counterHighlight.TryGetComponent(out ClearCounter clearCounter))
        {
            KitchenObject obj = clearCounter.GetCurrentObjectOnTable();
            if (obj != null) return obj.GetKitchenObjectSO();
        }
        else if (counterHighlight.TryGetComponent(out CuttingCounter cuttingCounter))
        {
            KitchenObject obj = cuttingCounter.GetCurrentObjectOnTable();
            if (obj != null) return obj.GetKitchenObjectSO();
        }
        else if (counterHighlight.TryGetComponent(out StoveCounter stoveCounter))
        {
            KitchenObject obj = stoveCounter.GetCurrentObjectOnTable();
            if (obj != null) return obj.GetKitchenObjectSO();
        }
        return null;
    }

    private void CompleteTutorial()
    {
        currentStep = TutorialStep.Complete;
        isWaitingForContinue = true;
        player.isMovementFrozen = true;
        tutorialPanel.SetActive(true);
        tutorialText.text = "ПОЗДРАВЛЯЕМ!\n\nТы успешно завершил обучение. Ты умеешь готовить суп, жарить картофель фри и мыть грязную посуду в раковине!\n\nНажми Q для возврата в главное меню."; // <--- Текст обновлен под Q
        actionPromptText.text = "Нажми Q для выхода"; // <--- Текст обновлен под Q
    }
}