using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ludum_Dare_46
{
	#pragma warning disable CS0618 // Type or member is obsolete
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

		public ParticleSystem TrailParticleSystem; // The Particle system that generates a trail when the player moves over a certain speed

		private Vector2 MoveVectorModifier;

		private void Awake()
		{
			// Get the Animator object
			Animator = GetComponent<Animator>();

			// Set the initial speed modifier
			MoveVectorModifier = Vector2.zero;

			// Disable the trail ps
			TrailParticleSystem.enableEmission = false;
		}

		// Update is called once per frame
		void Update()
		{
			// Make sure the player stands still while not moving
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;

			if (GameController.IsPlaying())
			{
				// Reset the movement vector
				MoveVector = Vector2.zero;

				// Add the current axis inputs to the player's movement
				MoveVector.x = Input.GetAxis("Horizontal");
				MoveVector.y = Input.GetAxis("Vertical");

				// Apply a modifier to the player's speed when the gate timer is below a certain point
				float moveSpeedModifier = GetMoveSpeedModifier(GameController.CurrentGateTimer, Conf.MaxSpeedModifier, 10);

				MoveVector = MoveVector.normalized * Conf.MoveSpeed * moveSpeedModifier + MoveVectorModifier;
				// Apply force to the player's movement
				transform.position += (Vector3) MoveVector * Time.deltaTime;
			}
			// Set Animator vars
			Animator.SetFloat("MoveSpeed", MoveVector.magnitude);

			// Check if the player has switched horizontal movement directions
			if (MoveVector.normalized.x * PreviousHorizontal < 0)
				Animator.SetTrigger("FlipTrigger");

			// Set the last known horizontal movement value
			if (MoveVector.x != 0) PreviousHorizontal = MoveVector.normalized.x;

			// If the player is moving faster than their normal speed, turn on the trail effect and change the soundtrack pitch
			if (MoveVector.magnitude > Conf.MoveSpeed + .1f)
			{
				// Turn on the trail particle effect
				TrailParticleSystem.enableEmission = true;
				// Change the soundtrack pitch
				float pitchModifier = ((MoveVector.magnitude / Conf.MoveSpeed) - 1) / 2;
				GameController?.Refs.AudioController?.SetSoundtrackPitch(1 + pitchModifier);
			}
			else
			{
				TrailParticleSystem.enableEmission = false;
				GameController?.Refs.AudioController?.SetSoundtrackPitch(1f);
			}
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			// If the player enters an outlet's zone, their phone will start charging
			if (collision.gameObject.CompareTag("Outlet"))
			{
				GameController.SetIsCharging(true);
				// Play a phone charging sound
				GameController?.Refs.AudioController?.Play(Sounds.PHONE_CHARGING);
			}

			// If the player enters a gatezone that matches the target gate in GameController, the gate should be reset
			if (collision.gameObject.CompareTag("Gate"))
			{
				// Check if this gate matches the 
				if (GameController.TargetGateReached == false && collision.gameObject.GetComponent<Gate>().TargetGate == GameController.TargetGate)
				{
					// Let the game controller know that the current gate has been reached
					GameController.TargetGateReached = true;
					// Play a success sound
					GameController?.Refs.AudioController?.Play(Sounds.GATE_REACHED);
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

			// When the player is no longer touching a PeopleMover, their speed modifiers are reset
			if (collision.gameObject.CompareTag("PeopleMover"))
			{
				MoveVectorModifier = Vector2.zero;
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

		private void OnTriggerStay2D(Collider2D collision)
		{
			// When the player is touching a people mover object, they should move faster
			if (collision.gameObject.CompareTag("PeopleMover"))
			{
				// Get the modifier
				float modifier = collision.gameObject.GetComponent<PeopleMover>().MoveSpeedModifier;

				// Apply the people mover's up vector to the player's move vector
				MoveVectorModifier = (Vector2) collision.gameObject.transform.up * modifier;
			}
		}
	}
}
