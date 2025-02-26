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
    
    public void Movement(InputAction.CallbackContext horizontalValue)
    {
        if (Mathf.Abs(horizontalValue.ReadValue<float>()) <= 0.5 && !IsOnGround()) return;
        
        playerRigidbody.linearVelocityX = horizontalValue.ReadValue<float>() * currentEntityData.speed;
        
    }
    
    private float jumpTime = 0f; // Temps pendant lequel le bouton est appuyé
    private bool isJumping = false; // Indicateur pour savoir si le joueur est en train de sauter
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
            Debug.Log("a");
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
            Debug.Log(shootAngle);
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
