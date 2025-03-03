using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
	private NewControls playerControls;
	
	[SerializeField] private Animator playerAnimator;
	
	public string[] allAnimations = new[] { "isRunning", "isJumping", "isFalling", "isWalking", "isSliding" };
	
	
	
	public Transform pivot;
	public bool flipAimArm;
	public float pivotCorrection;
	
	public Transform projectileStartPoint;
	public GameObject cacPoint;
	public GameObject dashPoint;

	public GameObject armOriginal;
	public GameObject armAim;

	private bool HasCurrentlyHealthbonus;

	private float currentCACTime= 0;
	private float currentRecoverTime = 0;
	private float currentRecoverReset = 0;
	
	
	[SerializeField] private GameObject projectilePrefab;
	
	private float currentCoolDown = 0;
	enum PlayerState
	{
		Idle,
		Walk,
		Run,
		Jump,
		Fall,
		Slide,
		DamageUp,
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
			case PlayerState.Slide :
				playerAnimator.SetBool("isSliding", true);
				ResetAllAnimatorExept("isSliding");
				break;
			case PlayerState.DamageUp :
				playerAnimator.SetBool("isDamageUp", true);
				ResetAllAnimatorExept("isDamageUp");
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

	public PlayerData Data;

	#region COMPONENTS
    public Rigidbody2D RB { get; private set; }
	
	
	#endregion

	#region STATE PARAMETERS

	public bool isFacingRight { get; private set; }
	public bool isJumping { get; private set; }
	public bool isWallJumping { get; private set; }
	public bool isDashing { get; private set; }
	public bool isSliding { get; private set; }

	
	public float lastOnGroundTime { get; private set; }
	public float lastOnWallTime { get; private set; }
	public float lastOnWallRightTime { get; private set; }
	public float lastOnWallLeftTime { get; private set; }


	private bool isJumpCut;
	private bool isJumpFalling;

	
	private float wallJumpStartTime;
	private int lastWallJumpDir;

	
	private int dashesLeft;
	private bool dashRefilling;
	private Vector2 lastDashDir;
	private bool isDashAttacking;

	private float targetSpeed;
	
	#endregion

	#region INPUT PARAMETERS
	private Vector2 moveInput;

	public float lastPressedJumpTime { get; private set; }
	public float lastPressedDashTime { get; private set; }
	#endregion

	#region CHECK PARAMETERS
	[Header("Checks")] 
	[SerializeField] private Transform groundCheckPoint;
	[SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform frontWallCheckPoint;
	[SerializeField] private Transform backWallCheckPoint;
	[SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask enemyLayer;
	#endregion

    private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
		playerControls = new NewControls();
	}

	private void OnEnable()
	{
		playerControls.Enable();
	}

	private void Start()
	{
		SetGravityScale(Data.gravityScale);
		isFacingRight = true;
		HasCurrentlyHealthbonus = Data.hasHealthBonus;
		RemovePlayerControl(false);
		cacPoint.SetActive(false);
		dashPoint.SetActive(false);
	}

	private void Update()
	{
        #region TIMERS
        lastOnGroundTime -= Time.deltaTime;
		lastOnWallTime -= Time.deltaTime;
		lastOnWallRightTime -= Time.deltaTime;
		lastOnWallLeftTime -= Time.deltaTime;

		lastPressedJumpTime -= Time.deltaTime;
		lastPressedDashTime -= Time.deltaTime;
		#endregion

		#region INPUT HANDLER

		moveInput.x = playerControls.CharacterControll.Move.ReadValue<Vector2>().x;
		moveInput.y = playerControls.CharacterControll.Move.ReadValue<Vector2>().y;

		
		
		if (moveInput.x != 0)
			CheckDirectionToFace(moveInput.x > 0);

		if (Mathf.Abs(moveInput.x) == 0 && (RB.linearVelocity.y == 0))
		{
			playerAnimator.SetBool("isWalking", false);
			playerAnimator.SetBool("isRunning", false);
			playerAnimator.SetBool("isJumping", false);
		}
		else if (Mathf.Abs(moveInput.x) < 0.5 && Mathf.Abs(moveInput.x) != 0)
		{
			if (!isDashing)
			{
				playerAnimator.SetBool("isWalking", true);
				playerAnimator.SetBool("isRunning", false);
			}
			
			
		}
		else if (Mathf.Abs(moveInput.x) <= 1 && Mathf.Abs(moveInput.x) != 0)
		{
			if (!isDashing)
			{
				playerAnimator.SetBool("isRunning", true);
				playerAnimator.SetBool("isWalking", false);
			}
			
		}

		if ((RB.linearVelocity.y == 0))
		{
			playerAnimator.SetBool("isJumping", false);
			playerAnimator.SetBool("isFalling", false);
		}
		else if ((RB.linearVelocity.y > 0))
		{
			if (currentRecoverTime <= 0)
			{
				playerAnimator.SetBool("isJumping", true);
				playerAnimator.SetBool("isFalling", false);
			}
			else
			{
				playerAnimator.SetBool("isDamageUp", true);
				playerAnimator.SetBool("isFalling", false);
			}
			
		}
		else if ((RB.linearVelocity.y < 0))
		{
			playerAnimator.SetBool("isJumping", false);
			playerAnimator.SetBool("isDamageUp", false);
            
			if (isSliding)
			{
				playerAnimator.SetBool("isFalling", false);
			}
			else
			{
				if (!isDashing)
				{
					playerAnimator.SetBool("isFalling", true);
				}
				
			}
			
		}

		if (isDashing)
		{
			playerAnimator.SetBool("isDashing", true);
			dashPoint.SetActive(true);
		}
		else
		{
			playerAnimator.SetBool("isDashing", false);
			dashPoint.SetActive(false);
		}

		/*if (Mathf.Abs(RB.linearVelocity.x) < 0)
		{
			playerAnimator.SetBool("isDamageUp", false);
		}*/
		
		if (currentRecoverReset > 0)
		{
			RemovePlayerControl(true);
			currentRecoverReset -= Time.deltaTime;
		}
		else
		{
			RemovePlayerControl(false);
			playerAnimator.SetBool("isDamageUp", false);
		}
		
		
		
		if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J) || playerControls.CharacterControll.Jump.WasPressedThisFrame())
        {
			OnJumpInput();
        }

		if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J) || playerControls.CharacterControll.Jump.WasReleasedThisFrame())
		{
			OnJumpUpInput();
		}

		if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
		{
			OnDashInput();
		}
		#endregion

		targetSpeed = moveInput.x * Data.runMaxSpeed;
		
		#region COLLISION CHECKS
		if (!isDashing && !isJumping)
		{

			if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer))
			{
				if(lastOnGroundTime < -0.1f)
				{
					
				}

				lastOnGroundTime = Data.coyoteTime;
            }


			if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)
			     || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) &&
			         !isFacingRight)) && !isWallJumping)
			{
				
				lastOnWallRightTime = Data.coyoteTime;
			}



			if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)
			     || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) &&
			         isFacingRight)) && !isWallJumping)
			{
				
				lastOnWallLeftTime = Data.coyoteTime;
			}
				

			
			lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
			
			
			
			
			if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, enemyLayer))
			{
				Damage(Vector2.up, "isDamageUp");
			}
			
			if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, enemyLayer) && isFacingRight)
			     || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, enemyLayer) &&
			         !isFacingRight)) && !isWallJumping)
			{
				Damage(Vector2.left, "isDamageUp");
			}

			if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, enemyLayer) && !isFacingRight)
			     || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, enemyLayer) &&
			         isFacingRight)) && !isWallJumping)
			{
				Damage(Vector2.right, "isDamageUp");
			}
		}
		#endregion

		#region JUMP CHECKS
		if (isJumping && RB.linearVelocity.y < 0)
		{
			isJumping = false;

			isJumpFalling = true;
			
		}

		if (RB.linearVelocity.y > 0)
		{
			
			
			
		}
		else if (RB.linearVelocity.y < 0)
		{
			
		}

		if (isWallJumping && Time.time - wallJumpStartTime > Data.wallJumpTime)
		{
			isWallJumping = false;
		}

		if (lastOnGroundTime > 0 && !isJumping && !isWallJumping)
        {
			isJumpCut = false;

			isJumpFalling = false;
		}

		if (!isDashing)
		{
	
			if (CanJump() && lastPressedJumpTime > 0)
			{
				isJumping = true;
				isWallJumping = false;
				isJumpCut = false;
				isJumpFalling = false;
				
				Jump();

				
			}
		
			else if (CanWallJump() && lastPressedJumpTime > 0)
			{
				isWallJumping = true;
				isJumping = false;
				isJumpCut = false;
				isJumpFalling = false;

				wallJumpStartTime = Time.time;
				lastWallJumpDir = (lastOnWallRightTime > 0) ? -1 : 1;
		
				WallJump(lastWallJumpDir);
			}
		}
		#endregion

		#region DASH CHECKS
		if (CanDash() && lastPressedDashTime > 0)
		{
			
			Sleep(Data.dashSleepTime); 

		
			if (moveInput != Vector2.zero)
				lastDashDir = moveInput;
			else
				lastDashDir = isFacingRight ? Vector2.right : Vector2.left;



			isDashing = true;
			isJumping = false;
			isWallJumping = false;
			isJumpCut = false;

			StartCoroutine(nameof(StartDash), lastDashDir);
		}
		#endregion

		#region SLIDE CHECKS

		if (CanSlide() && ((lastOnWallLeftTime > 0 && moveInput.x < 0) || (lastOnWallRightTime > 0 && moveInput.x > 0)))
		{
			isSliding = true;
			playerAnimator.SetBool("isSliding", true);
			
		}

		else
		{
			isSliding = false;
			playerAnimator.SetBool("isSliding", false);
		}
			
		#endregion

		#region GRAVITY
		if (!isDashAttacking)
		{
		
			if (isSliding)
			{
				SetGravityScale(0);
			}
			else if (RB.linearVelocity.y < 0 && moveInput.y < 0)
			{
	
				SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
	
				RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFastFallSpeed));
			}
			else if (isJumpCut)
			{
	
				SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
				RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
			}
			else if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
			{
				SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
			}
			else if (RB.linearVelocity.y < 0)
			{
				
				SetGravityScale(Data.gravityScale * Data.fallGravityMult);
				
				RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
			}
			else
			{
				
				SetGravityScale(Data.gravityScale);
			}
		}
		else
		{
			SetGravityScale(0);
		}
		#endregion
    }

	
    private void FixedUpdate()
	{
		Aim();
		
		if (currentRecoverTime > 0)
		{
			currentRecoverTime -= Time.deltaTime;
		}

		if (currentCACTime > 0)
		{
			currentCACTime -= Time.deltaTime;
		}
		else
		{
			playerAnimator.SetBool("isCAC", false);
		}
		
		if (!isDashing)
		{
			if (isWallJumping)
				Run(Data.wallJumpRunLerp);
			else
				Run(1);
		}
		else if (isDashAttacking)
		{
			Run(Data.dashEndRunLerp);
		}
		
		if (isSliding)
			Slide();
    }

    #region INPUT CALLBACKS
    public void OnJumpInput()
	{
		lastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut())
			isJumpCut = true;
	}

	public void OnDashInput()
	{
		lastPressedDashTime = Data.dashInputBufferTime;
	}
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

	private void Sleep(float duration)
    {
		StartCoroutine(nameof(PerformSleep), duration);
    }

	private IEnumerator PerformSleep(float duration)
    {
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(duration);
		Time.timeScale = 1;
	}
    #endregion
    
    
    #region RUN METHODS
    private void Run(float lerpAmount)
	{
		
		targetSpeed = Mathf.Lerp(RB.linearVelocity.x, targetSpeed, lerpAmount);
		
		
		
		
		#region Calculate AccelRate
		float accelRate;

		if (lastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
		#endregion

		#region Add Bonus Jump Apex Acceleration
		if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum

		if(Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && lastOnGroundTime < 0)
		{

			accelRate = 0; 
		}
		#endregion

		float speedDif = targetSpeed - RB.linearVelocity.x;


		float movement = speedDif * accelRate;
		
		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
		
	}

	private void Turn()
	{
		if (!isDashing)
		{
			Vector3 scale = transform.localScale; 
			scale.x *= -1;
			transform.localScale = scale;

			isFacingRight = !isFacingRight;
		}
		
	}
    #endregion

    #region JUMP METHODS
    private void Jump()
	{
		lastPressedJumpTime = 0;
		lastOnGroundTime = 0;

		#region Perform Jump
		float force = Data.jumpForce;
		if (RB.linearVelocity.y < 0)
			force -= RB.linearVelocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion
	}

	private void WallJump(int dir)
	{
		lastPressedJumpTime = 0;
		lastOnGroundTime = 0;
		lastOnWallRightTime = 0;
		lastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; 

		if (Mathf.Sign(RB.linearVelocity.x) != Mathf.Sign(force.x))
			force.x -= RB.linearVelocity.x;

		if (RB.linearVelocity.y < 0) 
			force.y -= RB.linearVelocity.y;
		
		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	private void Aim()
	{
		float angle = Mathf.Atan2(playerControls.CharacterControll.Aim.ReadValue<Vector2>().y, playerControls.CharacterControll.Aim.ReadValue<Vector2>().x) * Mathf.Rad2Deg;

		
		if (!isFacingRight)
		{
			if (flipAimArm)
			{
				angle += 180f;
			}
			projectileStartPoint.transform.rotation = Quaternion.Euler(projectileStartPoint.transform.rotation.x,projectileStartPoint.transform.rotation.y,projectileStartPoint.transform.rotation.z+180f);
		}
		
		pivot.rotation = Quaternion.Euler(0f, 0f, angle + pivotCorrection);

		if (playerControls.CharacterControll.Aim.ReadValue<Vector2>().normalized == Vector2.zero)
		{
			armOriginal.SetActive(true);
			armAim.SetActive(false);
			CAC();
		}
		else
		{
			
			if (playerControls.CharacterControll.Shoot.ReadValue<float>() > 0)
			{
				if (currentCoolDown > 0)
				{
					currentCoolDown -= Time.deltaTime;
				}
				else
				{
					if (!isFacingRight)
					{
						
						if (flipAimArm)
						{
							angle -= 180f;
						}
						Rigidbody2D projectileRb = Instantiate(projectilePrefab, projectileStartPoint.transform.position, Quaternion.Euler(0, 0, angle)).GetComponent<Rigidbody2D>();
						projectileRb.linearVelocity = playerControls.CharacterControll.Aim.ReadValue<Vector2>().normalized * Data.projectileSpeed;
					}
					else
					{
						Rigidbody2D projectileRb = Instantiate(projectilePrefab, projectileStartPoint.transform.position, Quaternion.Euler(0, 0, angle)).GetComponent<Rigidbody2D>();
						projectileRb.linearVelocity = playerControls.CharacterControll.Aim.ReadValue<Vector2>().normalized * Data.projectileSpeed;
					}
					
					
					currentCoolDown = Data.cooldownProjectile;
					
				}
			}
			else
			{
				currentCoolDown = 0;
			}

			if (currentCoolDown > 0)
			{
				armOriginal.SetActive(true);
				armAim.SetActive(false);
			}
			else
			{
				armOriginal.SetActive(false);
				armAim.SetActive(true);
			}
		}
	}

	private void CAC()
	{
		if (playerControls.CharacterControll.Shoot.ReadValue<float>() > 0)
		{
			if (Mathf.Abs(moveInput.x) == 0)
			{
				if (currentCACTime <= 0)
				{
					currentCACTime = Data.cooldownCAC;
					playerAnimator.SetBool("isCAC", true);
				}
				else
				{
					
				}
			}
			else
			{
				OnDashInput();
			}
			
		}
		
		
	}
	private void Damage(Vector2 direction, string state)
	{
		float rand = UnityEngine.Random.Range(-0.7f, 0.7f);
		float force = Data.jumpForce;
		currentRecoverReset = 0.5f;
		
		if (RB.linearVelocity.y < 0)
			force -= RB.linearVelocity.y;


		if (direction == Vector2.up)
		{
			direction = new Vector2(rand,1);
		}
		else if (direction == Vector2.right)
		{
			direction = new Vector2(1,Mathf.Abs(rand)+0.2f);
		}
		else if (direction == Vector2.left)
		{
			direction = new Vector2(-1,Mathf.Abs(rand)+0.2f);
		}
		

		RB.AddForce(direction * (force), ForceMode2D.Impulse);
		

		if (currentRecoverTime > 0)
		{
			playerAnimator.SetBool(state, true);
		}
		else if (HasCurrentlyHealthbonus)
		{
			HasCurrentlyHealthbonus = false;
			currentRecoverTime = Data.recoverTime;
			playerAnimator.SetBool(state, true);
		}
		else
		{
			currentRecoverTime = Data.recoverTime;
			playerAnimator.SetBool(state, true);
			Death();
		}
	}

	
	private void Death()
	{
		
	}

	private void RemovePlayerControl(bool value)
	{
		if (value == true)
		{
			playerControls.Disable();
		}
		else
		{
			playerControls.Enable();
		}
		
	}
	
	#region DASH METHODS
	
	private IEnumerator StartDash(Vector2 dir)
	{
		

		lastOnGroundTime = 0;
		lastPressedDashTime = 0;

		float startTime = Time.time;

		dashesLeft--;
		isDashAttacking = true;

		SetGravityScale(0);

		
		while (Time.time - startTime <= Data.dashAttackTime)
		{
			RB.linearVelocity = dir.normalized * Data.dashSpeed;

			yield return null;
		}

		startTime = Time.time;

		isDashAttacking = false;

		SetGravityScale(Data.gravityScale);
		RB.linearVelocity = Data.dashEndSpeed * dir.normalized;

		while (Time.time - startTime <= Data.dashEndTime)
		{
			yield return null;
		}

	
		isDashing = false;
	}
	
	private IEnumerator RefillDash(int amount)
	{
		dashRefilling = true;
		yield return new WaitForSeconds(Data.dashRefillTime);
		dashRefilling = false;
		dashesLeft = Mathf.Min(Data.dashAmount, dashesLeft + 1);
	}
	#endregion

	#region OTHER MOVEMENT METHODS
	private void Slide()
	{

		if(RB.linearVelocity.y > 0)
		{
		    RB.AddForce(-RB.linearVelocity.y * Vector2.up,ForceMode2D.Impulse);
		}
		
		float speedDif = Data.slideSpeed - RB.linearVelocity.y;	
		float movement = speedDif * Data.slideAccel;
		
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		RB.AddForce(movement * Vector2.up);
	}
    #endregion


    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != isFacingRight)
			Turn();
	}

    private bool CanJump()
    {
		return lastOnGroundTime > 0 && !isJumping;
    }

	private bool CanWallJump()
    {
		return lastPressedJumpTime > 0 && lastOnWallTime > 0 && lastOnGroundTime <= 0 && (!isWallJumping ||
			 (lastOnWallRightTime > 0 && lastWallJumpDir == 1) || (lastOnWallLeftTime > 0 && lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
    {
		return isJumping && RB.linearVelocity.y > 0;
    }

	private bool CanWallJumpCut()
	{
		return isWallJumping && RB.linearVelocity.y > 0;
	}

	private bool CanDash()
	{
		if (!isDashing && dashesLeft < Data.dashAmount && lastOnGroundTime > 0 && !dashRefilling)
		{
			StartCoroutine(nameof(RefillDash), 1);
		}

		return dashesLeft > 0;
	}

	public bool CanSlide()
    {
		if (lastOnWallTime > 0 && !isJumping && !isWallJumping && !isDashing && lastOnGroundTime <= 0)
			return true;
		else
			return false;
	}
    #endregion


    
}

