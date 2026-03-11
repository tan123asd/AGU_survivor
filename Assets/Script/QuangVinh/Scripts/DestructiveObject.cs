using UnityEngine;

public class DestructiveObject : MonoBehaviour, IInteract
{
    public bool NotDestruc { get; private set; }
    public string ObjID { get; private set; }

    void Start()
    {
        ObjID ??= GlobalHelper.GentateUniqueID(gameObject);
    }

    public bool CanInteract()
    {
        return !NotDestruc;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        Destroy(gameObject);
    }

}
