using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{
    [SerializeField] private Rigidbody2D playerRigidbody;

    [SerializeField] private Animator playerAnimator;

    public string[] allAnimations = new[] { "isRunning", "isJumping", "isFalling", "isWalking" };

    enum PlayerState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall
    }

    private PlayerState currentState = PlayerState.Idle;

    private void ChangeState(PlayerState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case PlayerState.Idle : 
                ResetAllAnimatorExept("");
                break;
            case PlayerState.Walk :
                playerAnimator.SetBool("isWalking", true);
                ResetAllAnimatorExept("isWalking");
                break;
            case PlayerState.Run :
                playerAnimator.SetBool("isRunning", true);
                ResetAllAnimatorExept("isRunning");
                break;
            case PlayerState.Jump :
                playerAnimator.SetBool("isJumping", true);
                ResetAllAnimatorExept("isJumping");
                break;
            case PlayerState.Fall :
                playerAnimator.SetBool("isFalling", true);
                ResetAllAnimatorExept("isFalling");
                break;
        }
    }

    private void ResetAllAnimatorExept(string animationName)
    {
        foreach (var animName in allAnimations)
        {
            if (animName == animationName) continue;
            playerAnimator.SetBool(animName, false);
        }
    }
    
    public void Movement(InputAction.CallbackContext horizontalValue)
    {
        if (Mathf.Abs(horizontalValue.ReadValue<float>()) <= 0.5 && !IsOnGround()) return;
        
        playerRigidbody.linearVelocityX = horizontalValue.ReadValue<float>() * currentEntityData.speed;

        if (currentState == PlayerState.Fall || currentState == PlayerState.Jump) return;
        if (Mathf.Abs(horizontalValue.ReadValue<float>()) == 0)
        {
            ChangeState(PlayerState.Idle);
        }
        else if (Mathf.Abs(horizontalValue.ReadValue<float>()) <= 0.6)
        {
            ChangeState(PlayerState.Walk);
        }
        else
        {
            ChangeState(PlayerState.Run);
        }
        
        
    }
    
    private float jumpTime = 0f;
    private bool isJumping = false;
    private const float MAX_JUMP_TIME = 0.15f;
    
    private Vector2 currentJumpForce;

    private float currentVelocity;

    private void FixedUpdate()
    {
        if (isJumping && jumpTime < MAX_JUMP_TIME)
        {
            jumpTime += Time.deltaTime;
            
            currentJumpForce = Vector2.Lerp(currentJumpForce, Vector2.zero, Time.deltaTime);
            playerRigidbody.AddForce(currentJumpForce);
        }

        if (playerRigidbody.linearVelocityY < 0)
        {
            ChangeState(PlayerState.Fall);
        }

        if (currentState == PlayerState.Fall)
        {
            if (IsOnGround())
            {
                ChangeState(PlayerState.Idle);
            }
        }
        
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (IsOnGround())
            {
                isJumping = true;
                jumpTime = 0;
                currentJumpForce = new Vector2(0, currentEntityData.jumpPower);
                ChangeState(PlayerState.Jump);
            }
        }

        if (context.canceled)
        {
            isJumping = false;
        }
    }

    private bool IsOnGround()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        playerRigidbody.GetContacts(contacts);
        return contacts.Any(contact => contact.normal.y >= 0.9f);
    }

    [SerializeField] private GameObject projectilePrefab;

    private Vector2 shootAngle;
    
    public void Shoot(InputAction.CallbackContext input)
    {
        
        if (shootAngle != Vector2.zero)
        {
            Rigidbody2D projectileRb = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, Mathf.Atan2(shootAngle.y, shootAngle.x) * Mathf.Rad2Deg)).GetComponent<Rigidbody2D>();
            projectileRb.linearVelocity = shootAngle * currentEntityData.projectileSpeed;
        }
    }

    public void GetShootAngle(InputAction.CallbackContext directionVector)
    {
        
        shootAngle = directionVector.ReadValue<Vector2>().normalized;
    }

    protected override void Die()
    {
        
    }
}
