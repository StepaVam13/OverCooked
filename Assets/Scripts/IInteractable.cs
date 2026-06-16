public interface IInteractable
{
    // Обычное действие (взять/положить предмет)
    void Interact(PlayerController player);

    // Альтернативное действие (нарезка, готовка)
    void InteractAlternate(PlayerController player);
}