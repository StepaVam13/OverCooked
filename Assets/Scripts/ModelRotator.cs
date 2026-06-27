using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 250f; // Скорость вращения
    private Vector3 previousMousePosition;

    private void Update()
    {
        // Когда игрок кликает левой кнопкой мыши
        if (Input.GetMouseButtonDown(0))
        {
            previousMousePosition = Input.mousePosition;
        }

        // Когда игрок удерживает левую кнопку мыши и ведет её в сторону
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;

            // Вращаем платформу по оси Y на основе движения мыши по оси X
            transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);

            previousMousePosition = Input.mousePosition;
        }
    }
}