using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementL : MonoBehaviour
{
    public float speed = 5f;

    private CharacterController controller;

    private Vector3 move;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);
    }
}