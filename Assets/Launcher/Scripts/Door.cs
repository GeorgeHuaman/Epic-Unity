using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    private bool open;

    public void Interact()
    {
        open = !open;

        transform.rotation = Quaternion.Euler(0, open ? 90 : 0, 0);
    }
}