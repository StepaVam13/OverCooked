using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep
    {
        GrabDirtyPlate1,         // 1. Берем первую грязную тарелку
        PutInSink1,              // 2. Кладем в раковину
        WashPlate1,              // 3. Моем её
        TakeCleanPlate1,         // 4. Забираем чистую тарелку в руки
        PlacePlate1,             // 5. Кладем чистую тарелку на стол ожидания
        GrabTomato,              // 6. Берем помидор
        GoToCuttingBoard,        // 7. Относим помидор на доску 
        ChopTomato,              // 8. Нарезаем помидор на доске
        TakeChoppedTomato,       // 9. Забираем нарезанный помидор
        GoToStove,               // 10. Кладем вариться суп
        WaitForCook,             // 11. Ждем готовности супа
        TakePlateForSoup,        // 12. Забираем тарелку со стола ожидания
        PlateSoup,               // 13. Наливаем суп в тарелку (спасаем от сгорания)
        DeliverSoup,             // 14. Относим суп на выдачу

        PowerOutage,             // 15. НОВЫЙ ШАГ: Выключение света и починка щитка!

        GrabDirtyPlate2,         // 16. Берем вторую грязную тарелку
        PutInSink2,              // 17. Кладем в раковину
        WashPlate2,              // 18. Моем её
        TakeCleanPlate2,         // 19. Забираем чистую в руки
        PlacePlate2,             // 20. Кладем чистую тарелку на стол ожидания
        GrabPotato,              // 21. Берем картошку
        ChopPotato,              // 22. Нарезаем картошку на доске
        TakeChoppedPotato,       // 23. Забираем нарезанную картошку
        GoToPan,                 // 24. Ставим жариться на сковороду
        WaitForFries,            // 25. Ждем готовности картофеля фри
        TakePlateForFries,       // 26. Забираем чистую тарелку со стола ожидания
        PlateFries,              // 27. Накладываем картофель фри в тарелку
        DeliverFries,            // 28. Относим картошку на выдачу
        Complete
    }

    [Header("Игрок")]
    [SerializeField] private PlayerController player;

    [Header("Столы на сцене")]
    [SerializeField] private PlateStackCounter dirtyPlateTable; // Стопка грязных тарелок
    [SerializeField] private SinkCounter sink;              // Раковина
    [SerializeField] private ClearCounter helperTable;      // Стол ожидания чистой тарелки
    [SerializeField] private ContainerCounter tomatoContainer; // Коробка помидоров
    [SerializeField] private ContainerCounter potatoContainer; // Коробка картофеля
    [SerializeField] private CuttingCounter cuttingBoard;   // Разделочный стол
    [SerializeField] private StoveCounter stove;            // Плита для супа
    [SerializeField] private StoveCounter pan;              // Плита/сковорода для картофеля фри
    [SerializeField] private DeliveryCounter deliveryTable;  // Сдача
    [SerializeField] private ElectricBoxCounter electricBox; // Электрощиток на стене (НОВОЕ!)

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
        SetStep(TutorialStep.GrabDirtyPlate1); // Начинаем с посуды!
    }

    private void Update()
    {
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
        actionPromptText.text = "Нажми Q, чтобы начать выполнять";

        // Гасим все подсветки
        if (dirtyPlateTable != null) dirtyPlateTable.GetComponent<InteractableHighlight>().SetSelected(false);
        if (sink != null) sink.GetComponent<InteractableHighlight>().SetSelected(false);
        if (helperTable != null) helperTable.GetComponent<InteractableHighlight>().SetSelected(false);
        if (tomatoContainer != null) tomatoContainer.GetComponent<InteractableHighlight>().SetSelected(false);
        if (potatoContainer != null) potatoContainer.GetComponent<InteractableHighlight>().SetSelected(false);
        if (cuttingBoard != null) cuttingBoard.GetComponent<InteractableHighlight>().SetSelected(false);
        if (stove != null) stove.GetComponent<InteractableHighlight>().SetSelected(false);
        if (pan != null) pan.GetComponent<InteractableHighlight>().SetSelected(false);
        if (deliveryTable != null) deliveryTable.GetComponent<InteractableHighlight>().SetSelected(false);
        if (electricBox != null) electricBox.GetComponent<InteractableHighlight>().SetSelected(false);

        switch (step)
        {
            // --- ЭТАП СУПА ---
            case TutorialStep.GrabDirtyPlate1:
                tutorialText.text = "Добро пожаловать на кухню!\n\nПервое правило шефа: готовь посуду ДО того, как еда сварится, иначе всё сгорит!\n\nПодойди к столу грязной посуды и возьми ПЕРВУЮ ГРЯЗНУЮ ТАРЕЛКУ на [E].";
                if (dirtyPlateTable != null) dirtyPlateTable.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.PutInSink1:
                tutorialText.text = "Отнеси её в РАКОВИНУ и положи внутрь на кнопку [E].";
                if (sink != null) sink.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.WashPlate1:
                tutorialText.text = "Помой тарелку! Нажимай клавишу [F] несколько раз, пока она не отмоется.";
                if (sink != null) sink.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.TakeCleanPlate1:
                tutorialText.text = "Отлично! Забери чистую тарелку из раковины на кнопку [E].";
                if (sink != null) sink.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.PlacePlate1:
                tutorialText.text = "Тарелка готова!\n\nПоложи чистую тарелку на свободный СТОЛ ОЖИДАНИЯ на кнопку [E], чтобы освободить руки.";
                if (helperTable != null) helperTable.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.GrabTomato:
                tutorialText.text = "Посуда готова. Теперь займемся супом.\n\nПодойди к ЯЩИКУ С ПОМИДОРАМИ и возьми один на кнопку [E].";
                if (tomatoContainer != null) tomatoContainer.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.GoToCuttingBoard:
                tutorialText.text = "Отнеси сырой помидор к РАЗДЕЛOЧНОМУ СТОЛУ и нажми [E].";
                if (cuttingBoard != null) cuttingBoard.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.ChopTomato:
                tutorialText.text = "Нажимай клавишу [F] несколько раз, чтобы нарезать помидор.";
                if (cuttingBoard != null) cuttingBoard.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.TakeChoppedTomato:
                tutorialText.text = "Забери нарезку в руки на кнопку [E].";
                if (cuttingBoard != null) cuttingBoard.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.GoToStove:
                tutorialText.text = "Положи нарезанный помидор на ПЛИТУ С КАСТРЮЛЕЙ вариться на кнопку [E].";
                if (stove != null) stove.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.WaitForCook:
                tutorialText.text = "Суп варится автоматически.\n\nВнимание: когда он сварится, у тебя будет всего 20 секунд до того, как он СГОРИТ! Ждем готовности...";
                if (stove != null) stove.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.TakePlateForSoup:
                tutorialText.text = "СУП ГОТОВ! Быстро!\n\nЗабери чистую тарелку со стола ожидания в руки на кнопку [E].";
                if (helperTable != null) helperTable.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.PlateSoup:
                tutorialText.text = "А теперь быстро подойди к плите и нажми [E], чтобы налить суп в тарелку, пока он не сгорел!";
                if (stove != null) stove.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.DeliverSoup:
                tutorialText.text = "Успех, суп спасен!\n\nОтнеси готовую порцию супа на СТОЛ ВЫДАЧИ и сдай заказ на кнопку [E].";
                if (deliveryTable != null) deliveryTable.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            // --- НОВЫЙ ИВЕНТ ЭЛЕКТРИЧЕСТВА ---
            case TutorialStep.PowerOutage:
                tutorialText.text = "О НЕТ! На кухне выбило пробки и полностью погас свет!\n\nБыстро доедь на гироскутере к ЭЛЕКТРОЩИТКУ на стене (подсвечен моргающей красной лампой) и нажми [E], чтобы поднять рубильник.";
                if (electricBox != null) electricBox.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            // --- ЭТАП КАРТОШКИ ---
            case TutorialStep.GrabDirtyPlate2:
                tutorialText.text = "Свет снова горит, ура! Теперь приготовим картофель фри.\n\nПоскольку чистых тарелок снова нет, подойди к столу грязной посуды и возьми ВТОРУЮ ГРЯЗНУЮ ТАРЕЛКУ на [E].";
                if (dirtyPlateTable != null) dirtyPlateTable.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.PutInSink2:
                tutorialText.text = "Снова отнеси её в РАКОВИНУ и положи внутрь на кнопку [E].";
                if (sink != null) sink.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.WashPlate2:
                tutorialText.text = "Помой тарелку! Нажимай клавишу [F] несколько раз, пока она не отмоется.";
                if (sink != null) sink.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.TakeCleanPlate2:
                tutorialText.text = "Отлично! Забери чистую тарелку из раковины на кнопку [E].";
                if (sink != null) sink.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.PlacePlate2:
                tutorialText.text = "Положи чистую тарелку на свободный СТОЛ ОЖИДАНИЯ на кнопку [E], чтобы освободить руки.";
                if (helperTable != null) helperTable.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.GrabPotato:
                tutorialText.text = "Посуда готова. Теперь займемся картошкой фри!\n\nПодойди к КОРОВКЕ С КАРТОФЕЛЕМ и возьми одну на [E].";
                if (potatoContainer != null) potatoContainer.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.ChopPotato:
                tutorialText.text = "Отнеси сырую картошку на РАЗДЕЛOЧНУЮ ДОСКУ, нажми [E] и нарежь её на [F].";
                if (cuttingBoard != null) cuttingBoard.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.TakeChoppedPotato:
                tutorialText.text = "Забери нарезанную картошку в руки на кнопку [E].";
                if (cuttingBoard != null) cuttingBoard.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.GoToPan:
                tutorialText.text = "Положи нарезанный картофель к СКОЛОВОРОДКЕ вариться на кнопку [E].";
                if (pan != null) pan.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.WaitForFries:
                tutorialText.text = "Картофель фри жарится автоматически.\n\nВнимание: у тебя снова будет всего 20 секунд до того, как картошка СГОРИТ на сковороде! Ждем готовности...";
                if (pan != null) pan.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.TakePlateForFries:
                tutorialText.text = "КАРТОШКА ПОЖАРИЛАСЬ! Быстро!\n\nЗабери чистую тарелку со стола ожидания в руки на кнопку [E].";
                if (helperTable != null) helperTable.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.PlateFries:
                tutorialText.text = "А теперь быстро подойди к сковороде и нажми [E], чтобы положить картофель фри в тарелку, пока он не сгорел!";
                if (pan != null) pan.GetComponent<InteractableHighlight>().SetSelected(true);
                break;

            case TutorialStep.DeliverFries:
                tutorialText.text = "Успех, картошка спасена!\n\nОтнеси тарелку на СТОЛ ВЫДАЧИ и сдай её на кнопку [E].";
                if (deliveryTable != null) deliveryTable.GetComponent<InteractableHighlight>().SetSelected(true);
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
            // --- ЭТАП СУПА ---
            case TutorialStep.GrabDirtyPlate1:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == dirtyPlateSO)
                {
                    SetStep(TutorialStep.PutInSink1);
                }
                break;

            case TutorialStep.PutInSink1:
                if (!player.HasKitchenObject() && FindItemOnCounter(sink.GetComponent<InteractableHighlight>()) == dirtyPlateSO)
                {
                    SetStep(TutorialStep.WashPlate1);
                }
                break;

            case TutorialStep.WashPlate1:
                if (FindItemOnCounter(sink.GetComponent<InteractableHighlight>()) == cleanPlateSO)
                {
                    SetStep(TutorialStep.TakeCleanPlate1);
                }
                break;

            case TutorialStep.TakeCleanPlate1:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == cleanPlateSO)
                {
                    SetStep(TutorialStep.PlacePlate1);
                }
                break;

            case TutorialStep.PlacePlate1:
                if (!player.HasKitchenObject() && FindItemOnCounter(helperTable.GetComponent<InteractableHighlight>()) == cleanPlateSO)
                {
                    SetStep(TutorialStep.GrabTomato);
                }
                break;

            case TutorialStep.GrabTomato:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == tomatoRawSO)
                {
                    SetStep(TutorialStep.GoToCuttingBoard);
                }
                break;

            case TutorialStep.GoToCuttingBoard:
                if (!player.HasKitchenObject() && FindItemOnCounter(cuttingBoard.GetComponent<InteractableHighlight>()) == tomatoRawSO)
                {
                    SetStep(TutorialStep.ChopTomato);
                }
                break;

            case TutorialStep.ChopTomato:
                if (FindItemOnCounter(cuttingBoard.GetComponent<InteractableHighlight>()) == tomatoChoppedSO)
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
                if (!player.HasKitchenObject() && FindItemOnCounter(stove.GetComponent<InteractableHighlight>()) == tomatoChoppedSO)
                {
                    SetStep(TutorialStep.WaitForCook);
                }
                break;

            case TutorialStep.WaitForCook:
                if (FindItemOnCounter(stove.GetComponent<InteractableHighlight>()) == tomatoSoupSO)
                {
                    SetStep(TutorialStep.TakePlateForSoup);
                }
                break;

            case TutorialStep.TakePlateForSoup:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == cleanPlateSO)
                {
                    SetStep(TutorialStep.PlateSoup);
                }
                break;

            case TutorialStep.PlateSoup:
                if (player.HasKitchenObject() && player.GetKitchenObject() is PlateKitchenObject)
                {
                    PlateKitchenObject plate = player.GetKitchenObject() as PlateKitchenObject;
                    if (plate.GetIngredients().Contains(tomatoSoupSO))
                    {
                        SetStep(TutorialStep.DeliverSoup);
                    }
                }
                break;

            case TutorialStep.DeliverSoup:
                if (!player.HasKitchenObject())
                {
                    // ВЫРУБАЕМ СВЕТ ПРИНУДИТЕЛЬНО ровно в секунду сдачи супа!
                    if (KitchenEventSystem.Instance != null)
                    {
                        KitchenEventSystem.Instance.StartDarkEvent();
                    }

                    SetStep(TutorialStep.PowerOutage); // Переходим к новому шагу с починкой света
                }
                break;

            // --- ШАГ ПОЧИНКИ ЭЛЕКТРИЧЕСТВА ---
            case TutorialStep.PowerOutage:
                // Ждем, пока игрок доедет до щитка и включит рубильник (IsDark снова станет false)
                if (KitchenEventSystem.Instance != null && !KitchenEventSystem.Instance.IsDark())
                {
                    SetStep(TutorialStep.GrabDirtyPlate2); // После починки света переходим к картошке!
                }
                break;

            // --- ЭТАП КАРТОШКИ ---
            case TutorialStep.GrabDirtyPlate2:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == dirtyPlateSO)
                {
                    SetStep(TutorialStep.PutInSink2);
                }
                break;

            case TutorialStep.PutInSink2:
                if (!player.HasKitchenObject() && FindItemOnCounter(sink.GetComponent<InteractableHighlight>()) == dirtyPlateSO)
                {
                    SetStep(TutorialStep.WashPlate2);
                }
                break;

            case TutorialStep.WashPlate2:
                if (FindItemOnCounter(sink.GetComponent<InteractableHighlight>()) == cleanPlateSO)
                {
                    SetStep(TutorialStep.TakeCleanPlate2);
                }
                break;

            case TutorialStep.TakeCleanPlate2:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == cleanPlateSO)
                {
                    SetStep(TutorialStep.PlacePlate2);
                }
                break;

            case TutorialStep.PlacePlate2:
                if (!player.HasKitchenObject() && FindItemOnCounter(helperTable.GetComponent<InteractableHighlight>()) == cleanPlateSO)
                {
                    SetStep(TutorialStep.GrabPotato);
                }
                break;

            case TutorialStep.GrabPotato:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == potatoRawSO)
                {
                    SetStep(TutorialStep.ChopPotato);
                }
                break;

            case TutorialStep.ChopPotato:
                if (FindItemOnCounter(cuttingBoard.GetComponent<InteractableHighlight>()) == potatoChoppedSO)
                {
                    SetStep(TutorialStep.TakeChoppedPotato);
                }
                break;

            case TutorialStep.TakeChoppedPotato:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == potatoChoppedSO)
                {
                    SetStep(TutorialStep.GoToPan);
                }
                break;

            case TutorialStep.GoToPan:
                if (!player.HasKitchenObject() && FindItemOnCounter(pan.GetComponent<InteractableHighlight>()) == potatoChoppedSO)
                {
                    SetStep(TutorialStep.WaitForFries);
                }
                break;

            case TutorialStep.WaitForFries:
                if (FindItemOnCounter(pan.GetComponent<InteractableHighlight>()) == friesSO)
                {
                    SetStep(TutorialStep.TakePlateForFries);
                }
                break;

            case TutorialStep.TakePlateForFries:
                if (player.HasKitchenObject() && player.GetKitchenObject().GetKitchenObjectSO() == cleanPlateSO)
                {
                    SetStep(TutorialStep.PlateFries);
                }
                break;

            case TutorialStep.PlateFries:
                if (player.HasKitchenObject() && player.GetKitchenObject() is PlateKitchenObject)
                {
                    PlateKitchenObject plate = player.GetKitchenObject() as PlateKitchenObject;
                    if (plate.GetIngredients().Contains(friesSO))
                    {
                        SetStep(TutorialStep.DeliverFries);
                    }
                }
                break;

            case TutorialStep.DeliverFries:
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
        else if (counterHighlight.TryGetComponent(out SinkCounter sinkCounter))
        {
            KitchenObject obj = sinkCounter.GetCurrentObjectOnTable();
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
        tutorialText.text = "ПОЗДРАВЛЯЕМ!\n\nТы успешно завершил обучение. Ты освоил все тонкости работы на кухне: от мытья грязной посуды до починки пробок на щитке, приготовления и сдачи супа и картофеля фри!\n\nНажми Q для возврата в главное меню.";
        actionPromptText.text = "Нажми Q для выхода";
    }
}