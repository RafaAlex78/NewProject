using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private bool CanMove;
    //Verify if we can sprint
    private bool IsSprinting => CanSprint && Input.GetKey(SprintKey);
    private bool ShouldJump => Input.GetKeyDown(JumpKey) && CharacterController.isGrounded;


    [Header("Fuctional Options")]
    [SerializeField] private bool CanSprint = true;
    [SerializeField] private bool CanJump = true;

    [Header("Controls")]
    [SerializeField] private KeyCode SprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode JumpKey = KeyCode.Space;


    [Header("Movement Parameters")]
    [SerializeField] private float WalkSpeed = 3.0f;
    [SerializeField] private float SprintSpeed = 6.0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float LookSpeedX = 2.0f; 
    [SerializeField, Range(1, 10)] private float LookSpeedY = 2.0f; 
    [SerializeField, Range(1, 180)] private float UpperLookLimit = 80.0f; 
    [SerializeField, Range(1, 180)] private float LowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float JumpForce = 8.0f;
    [SerializeField] private float Gravity = 30.0f;



    private Camera PlayerCamera;
    private CharacterController CharacterController;

    private Vector3 MoveDirection;
    private Vector2 CurrentInput;

    private float RotationX = 0.0f;
    
    // Start is called before the first frame update
    void Awake()
    {
        PlayerCamera = GetComponentInChildren<Camera>();
        CharacterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    {
        if(CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            if(CanJump)
            {
                HandleJump();
            }
            ApplyFinalMovements();
        }
        
    }

    private void HandleMovementInput()
    {
        
        CurrentInput = new Vector2((IsSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Vertical"), (IsSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = MoveDirection.y;
        MoveDirection = (transform.TransformDirection(Vector3.forward) * CurrentInput.x) + (transform.TransformDirection(Vector3.right) * CurrentInput.y);
        MoveDirection.y = moveDirectionY;

    }

    private void HandleMouseLook()
    {
        RotationX -= Input.GetAxis("Mouse Y") * LookSpeedY;
        RotationX = Mathf.Clamp(RotationX, -UpperLookLimit, LowerLookLimit);

        PlayerCamera.transform.localRotation = Quaternion.Euler(RotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * LookSpeedX, 0);
    }

    private void HandleJump()
    {
        if(ShouldJump)
        {
            MoveDirection.y = JumpForce;
        }
    }
    private void ApplyFinalMovements()
    {
        if(!CharacterController.isGrounded)
        {
            MoveDirection.y -= Gravity * Time.deltaTime;
        }
        CharacterController.Move(MoveDirection * Time.deltaTime);

    }
}
