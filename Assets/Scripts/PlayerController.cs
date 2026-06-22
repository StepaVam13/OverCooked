using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки броска продуктов")]
    [SerializeField] private float throwForce = 12f;       // Сила броска вперед
    [SerializeField] private float throwUpwardForce = 4f;  // Сила броска вверх (для навеса)
    [Header("Настройки гироскутера (Инерция)")]
    [SerializeField] private float acceleration = 5f; // Скорость разгона (чем меньше, тем дольше разгоняется)
    [SerializeField] private float deceleration = 3f; // Скорость торможения/инерция (чем меньше, тем дольше скользит)
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform handPoint; // Точка крепления предмета в руках

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 lastMoveDir;
    private IInteractable lastSelectedCounter;
    private IInteractable selectedCounter;
    private KitchenObject holdingObject; // Предмет, который сейчас в руках


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInput();
        HandleInteractions();

        // Если нажат Пробел — бросаем предмет из рук
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowItem();
        }
    }

    private void FixedUpdate()
    {
        // 1. Физика разгона и торможения (инерция)
        Vector3 targetVelocity = moveInput * moveSpeed;

        if (moveInput != Vector3.zero)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        // 2. Плавный поворот в сторону РЕАЛЬНОГО физического движения (вектора скорости)
        Vector3 movementDirection = rb.velocity;
        movementDirection.y = 0f; // Игнорируем высоту, поворачиваем только по горизонтали

        // Если персонаж физически движется (скорость больше минимальной)
        if (movementDirection.magnitude > 0.1f)
        {
            // Вычисляем направление взгляда в сторону физического движения
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection.normalized, Vector3.up);

            // Плавное сглаживание поворота (Slerp)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveInput != Vector3.zero)
        {
            lastMoveDir = moveInput;
        }

        // Кнопка обычного взаимодействия (E) — теперь ОДНА чистая проверка!
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (selectedCounter != null)
            {
                // Если перед глазами есть стол — работаем со столом
                selectedCounter.Interact(this);
            }
            else
            {
                // Если стола перед глазами нет — пробуем подобрать лежащий рядом на полу предмет!
                TryProximityPickup();
            }
        }

        // Кнопка альтернативного взаимодействия (F)
        if (Input.GetKeyDown(KeyCode.F))
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

        // Проверяем наличие стола перед собой с помощью Raycast (используем правильную переменную lastMoveDir)
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

        // --- ЛОГИКА АВТОМАТИЧЕСКОЙ ПОДСВЕТКИ ---
        if (selectedCounter != lastSelectedCounter)
        {
            // 1. Гасим подсветку на старом столе
            if (lastSelectedCounter != null)
            {
                MonoBehaviour oldCounterObj = lastSelectedCounter as MonoBehaviour;
                if (oldCounterObj != null && oldCounterObj.TryGetComponent(out InteractableHighlight oldHighlight))
                {
                    oldHighlight.SetSelected(false);
                }
            }

            // 2. Зажигаем подсветку на новом столе
            if (selectedCounter != null)
            {
                MonoBehaviour newCounterObj = selectedCounter as MonoBehaviour;
                if (newCounterObj != null && newCounterObj.TryGetComponent(out InteractableHighlight newHighlight))
                {
                    newHighlight.SetSelected(true);
                }
            }

            // Запоминаем текущий стол как прошлый
            lastSelectedCounter = selectedCounter;
        }
    }

    // Методы для работы с удерживаемым предметом
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

            // Возвращаем предмет на стандартный слой Default
            newObject.gameObject.layer = LayerMask.NameToLayer("Default");

            // БРОНЕБОЙНОЕ РЕШЕНИЕ: Отключаем коллайдеры у самого объекта И ВСЕХ его дочерних элементов,
            // чтобы они физически не могли заблокировать луч игрока из рук!
            Collider[] colliders = newObject.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // Отключаем физику у всех Rigidbody в объекте и его детях
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

            // --- ФИЗИКА ТРЕНИЯ (чтобы предмет не катился вечно) ---
            // --- ФИЗИКА ТРЕНИЯ (корректируем значения) ---
            itemRb.drag = 0.5f;        // Было 3f. Снижаем до 0.5f, чтобы предмет падал быстро и тяжело!
            itemRb.angularDrag = 10f;   // Оставляем высоким (даже увеличиваем до 10), чтобы он сразу перестал катиться при приземлении // Быстро останавливает вращение (качение сферы)

            Collider itemCollider = item.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
                itemCollider.isTrigger = false;
            }

            // --- ТРЮК СО СЛОЕМ (чтобы пол не мешал подбирать) ---
            // Временно переводим летящий предмет на слой Counters, чтобы луч игрока видел только его
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
                // ПОДБОР: Отключаем физику и коллайдер прямо здесь
                Rigidbody rb = kitchenObject.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                Collider collider = kitchenObject.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;

                // Даем предмет в руки игроку
                SetKitchenObject(kitchenObject);
                Debug.Log($"Подобрали {kitchenObject.GetKitchenObjectSO().objectName} с пола по близости!");
                break;
            }
        }
    }
}