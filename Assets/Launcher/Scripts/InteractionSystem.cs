using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public float distance = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(Vector3.one * 0.5f);

            if (Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
}
