using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform handPoint; // Точка крепления предмета в руках

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 lastMoveDir;
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
    }

    private void FixedUpdate()
    {
        // Перемещение физического тела игрока
        rb.velocity = moveInput * moveSpeed;

        // Плавный поворот в сторону движения
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
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

        // Кнопка обычного взаимодействия (E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (selectedCounter != null)
            {
                selectedCounter.Interact(this);
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

        // Проверяем наличие стола перед собой с помощью Raycast
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
    }

    // Методы для работы с удерживаемым предметом
    public bool HasKitchenObject() => holdingObject != null;

    public KitchenObject GetKitchenObject() => holdingObject;

    public void SetKitchenObject(KitchenObject newObject)
    {
        holdingObject = newObject;
        if (newObject != null)
        {
            newObject.transform.parent = handPoint;
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;

            // Принудительно возвращаем тарелке её родной размер
            newObject.ResetScale();
        }
    }

    public void ClearKitchenObject()
    {
        holdingObject = null;
    }
}