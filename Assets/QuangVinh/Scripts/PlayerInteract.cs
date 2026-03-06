using UnityEngine;

public class PlayerInteract: MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        IInteract interactable = other.GetComponent<IInteract>();
        if (interactable != null)
        {
            interactable.Interact();
        }
    }
}
