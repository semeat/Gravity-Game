using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
	public float moveSpeed = 10f;
	public Vector3 moveDirection;
	public float turnSmoothTime = 0.95f;
	public float dashSpeed = 8f;
	private float nextDashTime = 0;
	public float dashCooldown = 2f;
	bool isDashing = false;
	public bool isAttacking = false;
	public bool canMove = true;
	public int maxHealthPlayer = 100;
	public int currentHealthPlayer;
	public float attackRange = 2f;
	private Rigidbody rb;	
	private Transform playerModel;
	public FixedJoystick joystick;
	public Animator anim;
	private TrailRenderer trailRenderer;
	public HealthBarPlayer healthBarPlayer;
	public GameHandler gameHandler;
    public Transform attackPoint;
	public Collider swordCollider;
	public TrackEnemies trackEnemies;
	public string currentState;

	void Start()
	{
		playerModel = transform.GetChild(0).transform;
		rb = GetComponent<Rigidbody>();
		//anim = gameObject.transform.GetChild(0).GetComponent<Animator>();
		trailRenderer = GetComponent<TrailRenderer>();
		currentHealthPlayer = maxHealthPlayer;
		healthBarPlayer.SetMaxHealthPlayer(maxHealthPlayer);
		Time.timeScale = 1f;
		Vector3 randomSpot = UnityEngine.Random.onUnitSphere * 22;
		transform.position = randomSpot;
		Debug.Log(randomSpot);
		swordCollider.enabled = false;
	}

	void Update()
	{	
		moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
		if (moveDirection != Vector3.zero && isAttacking == false /*&& anim.GetCurrentAnimatorStateInfo(0).IsTag("SwordAttack") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f*/)
		{
			canMove = true;
			RotateForward();
			ChangeAnimationState("Running");
		}
		else if(isAttacking == false)
		{
			ChangeAnimationState("Idle");
		}

		if (Input.GetMouseButtonDown(0))
		{
			if(!isAttacking)
            {
				if (trackEnemies.enemyContact == true)
				{
					MoveTowardsTarget(trackEnemies.closestEnemy);
				}
				
				StartCoroutine(PlayRandomAttack());
			}
		}

		if (Time.time > nextDashTime)
        {
			if (Input.GetKeyDown(KeyCode.E))
			{
				nextDashTime = Time.time + dashCooldown;
				isDashing = true;
			} else
            {
				StartCoroutine(StopDashing());
            }
		}

		if(currentHealthPlayer <= 0)
        {
			gameHandler.GameOver();
        }
	}

    void RotateForward()
    {
		Vector3 dir = moveDirection;
		// calculate angle and rotation
		float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
		Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.up); //sets rotation to target angle degrees around y axis
		playerModel.localRotation = targetRotation;
	}

    void FixedUpdate()
	{
		if(canMove == true)
        {
			rb.MovePosition(rb.position + transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
		}
		if(isDashing)
        {
			commitDash();
        }
	}

	void commitDash()
    {
		rb.AddForce(playerModel.forward * dashSpeed, ForceMode.Impulse);
		trailRenderer.emitting = true;
		isDashing = false;
		Debug.Log("Dashing");
	}

	IEnumerator StopDashing()
    {
		yield return new WaitForSeconds(dashCooldown);
		trailRenderer.emitting = false;
		isDashing = false;
    }

	public IEnumerator PlayRandomAttack()
	{
		swordCollider.enabled = true;
		canMove = false;
		rb.velocity = Vector3.zero;
		isAttacking = true;
		float randomAttack = UnityEngine.Random.Range(0, 4);
		if (randomAttack == 0)
		{
			ChangeAnimationState("SwordAttackHorizontal");
		}
		else if (randomAttack == 1)
		{
			ChangeAnimationState("SwordAttackDown");
		}
		else if (randomAttack == 2)
		{
			ChangeAnimationState("SwordAttackBackhand");
		}
		else if (randomAttack == 3)
		{
			ChangeAnimationState("SwordAttack360");
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
		Debug.Log("Attack done");
		swordCollider.enabled = false;
		isAttacking = false;
	}

	public void TakeDamagePlayer(int damage)
    {
		currentHealthPlayer -= damage;
		healthBarPlayer.SetHealthPlayer(currentHealthPlayer);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void MoveTowardsTarget(Transform target)
    {
		//float angle = Vector3.Angle(transform.GetChild(0).position, target.GetChild(0).position);
		//Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
		//playerModel.localRotation = targetRotation;
		Vector3 direction = target.position - playerModel.position;
		Quaternion rotation = Quaternion.LookRotation(direction, transform.TransformDirection(Vector3.up));
		playerModel.rotation = rotation;
        Debug.Log(playerModel.rotation);
    }

	void ChangeAnimationState(string newState)
    {
		if (currentState == newState) return;

		anim.Play(newState);
		currentState = newState;
		Debug.Log(currentState);
    }
}

