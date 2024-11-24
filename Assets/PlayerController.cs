using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using Unity.VisualScripting;

public class PlayerControler : NetworkBehaviour
{
    [Header("base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0f;
    [HideInInspector]
    public bool canMove = true;
    [SerializeField]
    private float cameraYOffset = 0.5f;
    private float cameraZOffset = 0.2f;
    private Camera PlayerCamera;
    [Header("Animator setup")]
    public Animator anim;
    [SerializeField] private int PlayerSelfLayer = 6;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            PlayerCamera = Camera.main;
            //vi tri cam
            PlayerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z + cameraZOffset);
            PlayerCamera.transform.SetParent(transform);

            if (TryGetComponent(out PlayerWeapon playerWeapone))
                playerWeapone.InitializeWeapons(PlayerCamera.transform);
            gameObject.layer = PlayerSelfLayer;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = PlayerSelfLayer;
            }    
        }
        else
        {
            GetComponent<PlayerControler>().enabled = false;
        }
        AddPlayer();
    }

    [ServerRpc(RequireOwnership = false)]
    void AddPlayer()
    {
        FindAnyObjectByType<InGameManager>().UpdatePlayers();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }


    void Update()
    {
        bool isRunning = false;

        isRunning = Input.GetKey(KeyCode.LeftShift);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        Vector3 localVelocity = transform.InverseTransformDirection(characterController.velocity);
        anim.SetFloat("VelocityX", localVelocity.x);
        anim.SetFloat("VelocityZ", localVelocity.z);

        if (canMove && PlayerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            PlayerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}