using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public event System.Action OnReachedEndOfLevel;

	public float moveSpeed = 7f;
	public float smoothMoveTime = .1f;
	public float turnSpeed = 8f;

	float angle;
	float smoothInputMagnitude;
	float smoothMoveVelocity;

	Vector3 velocity;

	private Rigidbody rb;

	bool disabled;

	private void Start()
	{
		
		rb = this.GetComponent<Rigidbody>();
		Gaurd.OnGuardHasSpottedPlayer += Disabled;
	}
	void Update()
	{
		Vector3 inputDirection = Vector3.zero;
		if(!disabled)
		{
			inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
		}
		
		float inputMagnitude = inputDirection.magnitude;
		smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

		float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
		angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

		velocity = transform.forward * moveSpeed * smoothInputMagnitude;

	}

	void Disabled()
	{
		disabled = true;
	}
	private void FixedUpdate()
	{
		if(rb!=null)
		{
			rb.MoveRotation(Quaternion.Euler(Vector3.up * angle));
			rb.MovePosition(rb.position + velocity * Time.deltaTime);
		}
	}

	private void OnDestroy()
	{
		Gaurd.OnGuardHasSpottedPlayer -= Disabled;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag =="Finish")
		{
			Disabled();
			if(OnReachedEndOfLevel !=null)
			{
				OnReachedEndOfLevel();
			}
		}
	}
}
