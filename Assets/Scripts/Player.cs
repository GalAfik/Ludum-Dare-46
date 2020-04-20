using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ludum_Dare_46
{
	public class Player : MonoBehaviour
	{
		public GameController GameController; // The game controller object

		[Serializable]
		public class ConfigurationData
		{
			public float MoveSpeed; // How fast a player moves through the world
			[Range(1.0f, 2.0f)] public float MaxSpeedModifier = 1.2f; // This modifier is applied to the move speed when the gate timer gets close to 0
		}
		public ConfigurationData Conf = new ConfigurationData();
		private Vector2 MoveVector; // The current movement vector of the player
		private float PreviousHorizontal = -1; // The last known horizontal movement of the player

		private Animator Animator; // Animation Controller

		private void Awake()
		{
			// Get the Animator object
			Animator = GetComponent<Animator>();
		}

		// Update is called once per frame
		void Update()
		{
			if (GameController.IsPlaying())
			{
				// Reset the movement vector
				MoveVector = Vector2.zero;

				// Add the current axis inputs to the player's movement
				MoveVector.x = Input.GetAxis("Horizontal");
				MoveVector.y = Input.GetAxis("Vertical");

				// Apply a modifier to the player's speed when the gate timer is below a certain point
				float moveSpeedModifier = GetMoveSpeedModifier(GameController.CurrentGateTimer, Conf.MaxSpeedModifier, 10);

				// Apply force to the player's movement
				transform.position += (Vector3) MoveVector.normalized * Conf.MoveSpeed * moveSpeedModifier * Time.deltaTime;

				// Set Animator vars
				Animator.SetFloat("MoveSpeed", MoveVector.magnitude);

				// Check if the player has switched horizontal movement directions
				if (MoveVector.normalized.x * PreviousHorizontal < 0)
					Animator.SetTrigger("FlipTrigger");

				// Set the last known horizontal movement value
				if (MoveVector.x != 0) PreviousHorizontal = MoveVector.normalized.x;
			}
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			// If the player enters an outlet's zone, their phone will start charging
			if (collision.gameObject.CompareTag("Outlet"))
			{
				GameController.SetIsCharging(true);
			}

			// If the player enters a gatezone that matches the target gate in GameController, the gate should be reset
			if (collision.gameObject.CompareTag("Gate"))
			{
				// Check if this gate matches the 
				if (GameController.TargetGateReached == false && collision.gameObject.GetComponent<Gate>().TargetGate == GameController.TargetGate)
				{
					// Let the game controller know that the current gate has been reached
					GameController.TargetGateReached = true;
					// Set a new target gate
					StartCoroutine(GameController.ResetTargetGate());
				}
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			// If the player exits a charging area, their phone stops charging and starts draining again
			if (collision.gameObject.CompareTag("Outlet"))
			{
				GameController.SetIsCharging(false);
			}
		}

		float GetMoveSpeedModifier(float currentGateTimer, float maxSpeedModifier, float normalSpeedConstraint)
		{
			// When the current gate time is at a certain point, the modifier should be 1 (normal)
			// Otherwise, it should be determined linearly
			switch (currentGateTimer)
			{
				case var _ when currentGateTimer > normalSpeedConstraint:
					return 1;
				default:
					return (-(maxSpeedModifier - 1) / normalSpeedConstraint) * (currentGateTimer - normalSpeedConstraint) + 1;
			}
		}
	}
}
