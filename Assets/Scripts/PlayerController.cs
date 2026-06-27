using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки управления (Клавиши)")]
    [SerializeField] private KeyCode upKey = KeyCode.W;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Обычное действие
    [SerializeField] private KeyCode chopKey = KeyCode.F;     // Нарезка
    [SerializeField] private KeyCode throwKey = KeyCode.Space; // Бросок

    [Header("Настройки броска продуктов")]
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float throwUpwardForce = 4f;

    [Header("Настройки гироскутера (Инерция)")]
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform handPoint;

    [HideInInspector] public bool isMovementFrozen = false;

    [Header("Настройки 3D магазина")]
    [SerializeField] private Transform gyroScooterContainer; // Объект "GyroScooter", внутри которого лежат 3D модели разных гироскутеров

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 lastMoveDir;
    private IInteractable selectedCounter;
    private IInteractable lastSelectedCounter;
    private KitchenObject holdingObject;

    [SerializeField] private bool isPlayer2 = false; // Поставьте галочку только у Второго Игрока!

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        LoadKeybinds();
        LoadUpgrades(); // <--- ДОБАВЬТЕ ЭТУ СТРОКУ!

        if (isPlayer2)
        {
            bool isCoopMode = PlayerPrefs.GetInt("IsCoop", 0) == 1;
            if (!isCoopMode)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (isMovementFrozen) return;

        HandleInput();
        HandleInteractions();

        // Кнопка броска продуктов
        if (Input.GetKeyDown(throwKey))
        {
            ThrowItem();
        }
    }

    private void FixedUpdate()
    {
        if (isMovementFrozen)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        Vector3 targetVelocity = moveInput * moveSpeed;

        if (moveInput != Vector3.zero)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        Vector3 movementDirection = rb.velocity;
        movementDirection.y = 0f;

        if (movementDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleInput()
    {
        // Считываем движение по настроенным в инспекторе кнопкам
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(upKey)) moveZ = 1f;
        if (Input.GetKey(downKey)) moveZ = -1f;
        if (Input.GetKey(leftKey)) moveX = -1f;
        if (Input.GetKey(rightKey)) moveX = 1f;

        moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveInput != Vector3.zero)
        {
            lastMoveDir = moveInput;
        }

        // Кнопка обычного взаимодействия (Брать / Класть)
        if (Input.GetKeyDown(interactKey))
        {
            if (selectedCounter != null)
            {
                selectedCounter.Interact(this);
            }
            else
            {
                TryProximityPickup();
            }
        }

        // Кнопка альтернативного взаимодействия (Нарезка / Готовка)
        if (Input.GetKeyDown(chopKey))
        {
            if (selectedCounter != null)
            {
                selectedCounter.InteractAlternate(this);
            }
        }
    }

    private void HandleInteractions()
    {
        float interactDistance = 1.5f;

        if (Physics.Raycast(transform.position, lastMoveDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out IInteractable interactable))
            {
                selectedCounter = interactable;
            }
            else
            {
                selectedCounter = null;
            }
        }
        else
        {
            selectedCounter = null;
        }

        if (selectedCounter != lastSelectedCounter)
        {
            if (lastSelectedCounter != null)
            {
                MonoBehaviour oldCounterObj = lastSelectedCounter as MonoBehaviour;
                if (oldCounterObj != null && oldCounterObj.TryGetComponent(out InteractableHighlight oldHighlight))
                {
                    oldHighlight.SetSelected(false);
                }
            }

            if (selectedCounter != null)
            {
                MonoBehaviour newCounterObj = selectedCounter as MonoBehaviour;
                if (newCounterObj != null && newCounterObj.TryGetComponent(out InteractableHighlight newHighlight))
                {
                    newHighlight.SetSelected(true);
                }
            }

            lastSelectedCounter = selectedCounter;
        }
    }

    public bool HasKitchenObject() => holdingObject != null;

    public KitchenObject GetKitchenObject() => holdingObject;

    public void SetKitchenObject(KitchenObject newObject)
    {
        holdingObject = newObject;
        if (newObject != null)
        {
            newObject.transform.SetParent(handPoint, false);
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;
            newObject.ResetScale();

            newObject.gameObject.layer = LayerMask.NameToLayer("Default");

            Collider[] colliders = newObject.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            Rigidbody[] rbs = newObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.isKinematic = true;
            }
        }
    }

    public void ClearKitchenObject()
    {
        holdingObject = null;
    }

    private void ThrowItem()
    {
        if (HasKitchenObject())
        {
            KitchenObject item = GetKitchenObject();

            if (item.CompareTag("Plate")) return;

            item.transform.parent = null;
            ClearKitchenObject();

            Rigidbody itemRb = item.GetComponent<Rigidbody>();
            if (itemRb == null)
            {
                itemRb = item.gameObject.AddComponent<Rigidbody>();
            }
            itemRb.isKinematic = false;

            itemRb.drag = 0.5f;
            itemRb.angularDrag = 10f;

            Collider itemCollider = item.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
                itemCollider.isTrigger = false;
            }

            item.gameObject.layer = LayerMask.NameToLayer("Counters");

            Vector3 forceDirection = transform.forward * throwForce + Vector3.up * throwUpwardForce;
            itemRb.AddForce(forceDirection, ForceMode.Impulse);
        }
    }

    private void TryProximityPickup()
    {
        if (HasKitchenObject()) return;

        float pickupRadius = 1.2f;
        Vector3 pickupCenter = transform.position + transform.forward * 0.4f;

        Collider[] colliders = Physics.OverlapSphere(pickupCenter, pickupRadius);
        foreach (Collider col in colliders)
        {
            KitchenObject kitchenObject = col.GetComponent<KitchenObject>();
            if (kitchenObject != null)
            {
                Rigidbody rb = kitchenObject.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                Collider collider = kitchenObject.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;

                SetKitchenObject(kitchenObject);
                Debug.Log($"Подобрали {kitchenObject.GetKitchenObjectSO().objectName} с пола по близости!");
                break;
            }
        }
    }

    public void LoadKeybinds()
    {
        // Определяем префикс настроек в зависимости от имени объекта игрока на сцене ("Player1" или "Player2")
        string prefix = gameObject.name == "Player1" ? "P1_" : "P2_";

        // Считываем клавиши из памяти. Если они еще не были переназначены — используем дефолтные кнопки из инспектора
        upKey = (KeyCode)PlayerPrefs.GetInt(prefix + "Up", (int)upKey);
        downKey = (KeyCode)PlayerPrefs.GetInt(prefix + "Down", (int)downKey);
        leftKey = (KeyCode)PlayerPrefs.GetInt(prefix + "Left", (int)leftKey);
        rightKey = (KeyCode)PlayerPrefs.GetInt(prefix + "Right", (int)rightKey);
        interactKey = (KeyCode)PlayerPrefs.GetInt(prefix + "Interact", (int)interactKey);
        chopKey = (KeyCode)PlayerPrefs.GetInt(prefix + "Chop", (int)chopKey);
        throwKey = (KeyCode)PlayerPrefs.GetInt(prefix + "Throw", (int)throwKey);
    }

    public void LoadUpgrades()
    {
        // Считываем, какой гироскутер сейчас экипирован в магазине
        string equippedBoard = PlayerPrefs.GetString("EquippedBoard", "Default");

        // 1. АКТИВИРУЕМ ТОЛЬКО КУПЛЕННУЮ МОДЕЛЬ под ногами у игрока!
        if (gyroScooterContainer != null)
        {
            foreach (Transform child in gyroScooterContainer)
            {
                // Если имя 3D-модели совпадает с ID экипированного гироскутера — включаем её, остальные гасим
                child.gameObject.SetActive(child.name == equippedBoard);
            }
        }

        // 2. УСТАНАВЛИВАЕМ ХАРАКТЕРИСТИКИ ЭТОГО ГИРОСКУТЕРА
        if (equippedBoard == "Default")
        {
            moveSpeed = 7f;
            acceleration = 5f;
        }
        else if (equippedBoard == "Speedster")
        {
            moveSpeed = 10f; // Быстрый!
            acceleration = 6f;
        }
        else if (equippedBoard == "Drifter")
        {
            moveSpeed = 8.5f;
            acceleration = 9f; // С заносом и невероятно резвым стартом!
        }
    }
}