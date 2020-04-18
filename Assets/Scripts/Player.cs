using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ludum_Dare_46
{
	public class Player : MonoBehaviour
	{
		[Serializable]
		public class ConfigurationData
		{
			public float MoveSpeed; // How fast a player moves through the world
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
			// Reset the movement vector
			MoveVector = Vector2.zero;

			// Add the current axis inputs to the player's movement
			MoveVector.x = Input.GetAxis("Horizontal");
			MoveVector.y = Input.GetAxis("Vertical");

			// Apply force to the player's movement
			transform.position += (Vector3)MoveVector.normalized * Conf.MoveSpeed * Time.deltaTime;

			// Set Animator vars
			Animator.SetFloat("MoveSpeed", MoveVector.magnitude);

			// Check if the player has switched horizontal movement directions
			if (MoveVector.normalized.x * PreviousHorizontal < 0)
				Animator.SetTrigger("FlipTrigger");

			// Set the last known horizontal movement value
			if (MoveVector.x != 0) PreviousHorizontal = MoveVector.normalized.x;
		}
	}
}
