using System;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    #region Paras
    public float moveSpeed;
    public float runSpeed;
    public float rotationFactor;

    private Camera cam;
    private Vector2 currentMovementInput;
    private Vector3 dir;
    private bool isMovePressed;
    private bool isRunPressed;

    private Vector3 positionToLookAt;
    private Quaternion currentRotation;
    private Quaternion targetRotation;

    private Animator anim;
    private int isWalkingHash;
    private int isRunningHash;
    private bool isWalking;
    private bool isRunning;

    private CharacterController characterController;

    [HideInInspector] public bool canMove = true;
    #endregion

    #region Sys funcs
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        cam = Camera.main;

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    private void Update()
    {
        if (!canMove) return;
        GetMovementInput();
        HandleMovement();
        HandleAnimation();
    }
    #endregion

    #region Movement and Run
    private void GetMovementInput()
    {
        currentMovementInput = new Vector2((int)Input.GetAxisRaw("Horizontal"),(int)Input.GetAxisRaw("Vertical"));
        isMovePressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        isRunPressed = Input.GetKey(KeyCode.LeftShift);
    }
    private void HandleMovement()
    {
        if (isMovePressed)
        {
            dir = currentMovementInput.x * cam.transform.right.normalized + currentMovementInput.y * cam.transform.forward.normalized;
            dir.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationFactor * Time.deltaTime);
            if (isRunPressed) characterController.Move(dir * runSpeed * Time.deltaTime);
            else characterController.Move(dir * moveSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Animation
    private void HandleAnimation()
    {
        isWalking = anim.GetBool(isWalkingHash);
        isRunning = anim.GetBool(isRunningHash);

        if(isMovePressed && !isWalking && !isRunning)
        {
            anim.SetBool(isWalkingHash,true);
        }
        else if(!isMovePressed && isWalking)
        {
            anim.SetBool(isWalkingHash,false);
        }

        if(isMovePressed && isRunPressed && !isRunning)
        {
            anim.SetBool(isRunningHash,true);
        }
        else if((!isMovePressed || !isRunPressed) && isRunning)
        {
            anim.SetBool(isRunningHash,false);
        }
    }
    #endregion
}
