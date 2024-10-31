using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float speed = 5.0f;
    public float mouseSensitivity = 2.0f;
    private float verticalLookRotation = 0f;

    void Update()
    {
        // Xử lý xoay camera
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        Camera.main.transform.localEulerAngles = Vector3.right * verticalLookRotation;

        // Xử lý di chuyển
        float moveForwardBackward = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float moveLeftRight = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        Vector3 move = transform.right * moveLeftRight + transform.forward * moveForwardBackward;
        transform.Translate(move, Space.World);
    }
}
