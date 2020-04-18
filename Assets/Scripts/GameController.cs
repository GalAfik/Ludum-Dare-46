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
			public float MaxGateTimer; // The timer for getting to the current gate
			public float InitialPhoneCharge; // The charge on the phone at the start of a game
			[Range(0, 5)] public float PhoneChargeDrainRate; // How fast the phone will drain
		}
		public ConfigurationData Conf = new ConfigurationData();

		private float CurrentGateTimer; // The timer for getting to the current gate
		private float CurrentPhoneCharge; // The current charge on the phone battery

		// Start is called before the first frame update
		void Start()
		{
			// Set the initial timer values
			CurrentGateTimer = Conf.MaxGateTimer;
			CurrentPhoneCharge = Conf.InitialPhoneCharge;

			StartCoroutine(ResetGateTimer());
		}

		IEnumerator ResetGateTimer()
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

			print("GameController::CurrentGateTimer = " + CurrentGateTimer);
			print("GameController::CurrentPhoneCharge = " + CurrentPhoneCharge);
		}
	}
}
