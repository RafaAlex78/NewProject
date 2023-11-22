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
    private bool ShouldCrouch => Input.GetKeyDown(CrouchKey) && !DuringCrouchAnimation && CharacterController.isGrounded;
    float BobSpeed => IsCrouching ? CrouchBobSpeed : IsSprinting ? SprintBobSpeed : WalkBobSpeed;
    float BobAmount => IsCrouching ? CrouchBobAmount : IsSprinting ? SprintBobAmount : WalkBobAmount;


    [Header("Fuctional Options")]
    [SerializeField] private bool CanSprint = true;
    [SerializeField] private bool CanJump = true;
    [SerializeField] private bool CanCrouch = true;
    [SerializeField] private bool CanUseHeadbob = true;

    [Header("Controls")]
    [SerializeField] private KeyCode SprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode JumpKey = KeyCode.Space;
    [SerializeField] private KeyCode CrouchKey = KeyCode.LeftControl;


    [Header("Movement Parameters")]
    [SerializeField] private float WalkSpeed = 3.0f;
    [SerializeField] private float SprintSpeed = 6.0f;
    [SerializeField] private float CrouchSpeed = 1.5f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float LookSpeedX = 2.0f; 
    [SerializeField, Range(1, 10)] private float LookSpeedY = 2.0f; 
    [SerializeField, Range(1, 180)] private float UpperLookLimit = 80.0f; 
    [SerializeField, Range(1, 180)] private float LowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float JumpForce = 8.0f;
    [SerializeField] private float Gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float CrouchHeight = 0.5f;
    [SerializeField] private float StandingHeight = 2.0f;
    [SerializeField] private float TimeToCrouch = 2.0f;
    [SerializeField] private Vector3 CrouchingCenter = new Vector3(0,0.5f,0);
    [SerializeField] private Vector3 StandingCenter = new Vector3(0,0,0);

    [Header("Headbob Parameters")]
    [SerializeField] private float WalkBobSpeed = 14f;
    [SerializeField] private float WalkBobAmount = 0.05f;
    [SerializeField] private float SprintBobSpeed = 18f;
    [SerializeField] private float SprintBobAmount = 0.1f;
    [SerializeField] private float CrouchBobSpeed = 8f;
    [SerializeField] private float CrouchBobAmount = 0.025f;

    private float DefaultYPos = 0;
    private float Timer = 0;


    private bool IsCrouching;
    private bool DuringCrouchAnimation;

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
        DefaultYPos = PlayerCamera.transform.localPosition.y;
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
            if(CanCrouch)
            {
                HandleCrouch();
            }
            if (CanUseHeadbob)
            {
                HandleHeadBob();
            }
            ApplyFinalMovements();
        }
        
    }

    private void HandleMovementInput()
    {
        
        CurrentInput = new Vector2((IsCrouching ? CrouchSpeed : IsSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Vertical"), (IsCrouching ? CrouchSpeed : IsSprinting ? SprintSpeed : WalkSpeed) * Input.GetAxis("Horizontal"));

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
    private void HandleCrouch()
    {
        if(ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private void HandleHeadBob()
    {
        if(!CharacterController.isGrounded)
        {
            return;
        }

        if (Mathf.Abs(MoveDirection.x) > 0.1f || Mathf.Abs(MoveDirection.z) > 0.1f)
        {
            Timer += Time.deltaTime * BobSpeed;
            PlayerCamera.transform.localPosition = new Vector3(PlayerCamera.transform.localPosition.x, DefaultYPos + Mathf.Sin(Timer) * BobAmount, PlayerCamera.transform.localPosition.z);

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

    private IEnumerator CrouchStand()
    {
        if(IsCrouching && Physics.Raycast(PlayerCamera.transform.position, Vector3.up, 1f))
        {
            //yield return new WaitUntil(() => !Physics.Raycast(PlayerCamera.transform.position, Vector3.up, 1f));
            yield break;
        }

        DuringCrouchAnimation = true;
        float timeElapsed = 0;
        float targetHeight = IsCrouching ? StandingHeight : CrouchHeight;
        float currentHeight = CharacterController.height;
        Vector3 targetCenter = IsCrouching ? StandingCenter : CrouchingCenter;
        Vector3 currentCenter = CharacterController.center;

        while(timeElapsed<TimeToCrouch)
        {
            CharacterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/TimeToCrouch);
            CharacterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/TimeToCrouch);
            timeElapsed+= Time.deltaTime;
            yield return null;
        }
        IsCrouching = !IsCrouching;

        CharacterController.height = targetHeight;
        CharacterController.center = targetCenter;


        DuringCrouchAnimation = false;
    }
}
