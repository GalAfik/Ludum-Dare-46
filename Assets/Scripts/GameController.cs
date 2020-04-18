using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ludum_Dare_46
{
	public class GameController : MonoBehaviour
	{
		[Serializable]
		public class ConfigurationData
		{
			public int MinGateTimer; // The minimum amount of time given to the player to get to the next gate
			public int MaxGateTimer; // The maximum amount of time the player has to get to the next gate
			[Range(0, 100)] public float InitialPhoneCharge; // The charge on the phone at the start of a game
			[Range(0, 5)] public float PhoneChargeDrainRate; // How fast the phone will drain
		}
		public ConfigurationData Conf = new ConfigurationData();

		internal float CurrentGateTimer { get; private set; } // The timer for getting to the current gate
		internal float CurrentPhoneCharge { get; private set; } // The current charge on the phone battery

		// Start is called before the first frame update
		void Start()
		{
			// Set the initial timer values
			ResetGateTimer();
			CurrentPhoneCharge = Conf.InitialPhoneCharge;

			// TODO :: This will eventually be moved to the StartGame method
			StartCoroutine(StartGateTimer());
		}

		// Resets the gate timer to a new amount of time
		// This should be called alongside StartGateTimer() to start the timer after being set
		void ResetGateTimer()
		{
			// Sets the gate timer to a new random integer between the min and max gate timers
			CurrentGateTimer = UnityEngine.Random.Range(Conf.MinGateTimer, Conf.MaxGateTimer + 1);
		}

		// This starts the current gate timer
		// This should be called after resetting the timer with ResetGateTimer
		IEnumerator StartGateTimer()
		{
			while (CurrentGateTimer > 0)
			{
				// Wait for one second
				yield return new WaitForSeconds(1);
				// Decrement the gate timer
				CurrentGateTimer--;
			}
		}

		// Update is called once per frame
		void Update()
		{
			// Handle phone charge timer
			if (CurrentPhoneCharge > 0) CurrentPhoneCharge -= Conf.PhoneChargeDrainRate * Time.deltaTime;
			else CurrentPhoneCharge = 0; // TODO :: This is where the player should lose the game!

			//print("GameController::CurrentGateTimer = " + CurrentGateTimer);
			//print("GameController::CurrentPhoneCharge = " + CurrentPhoneCharge);
		}
	}
}
