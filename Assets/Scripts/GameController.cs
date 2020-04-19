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
			[Range(1, 10)] public float PhoneChargeMaxDrainRateModifier; // The maximum modifier that can be applied to the charge drain rate
			[Range(0, 100)] public float PhoneChargeRate; // How fast the phone charges while connected to an outlet
			public int ScorePerSecond; // How many points the player scores per second they survive
		}
		public ConfigurationData Conf = new ConfigurationData();

		internal float CurrentGateTimer { get; private set; } // The timer for getting to the current gate
		internal float CurrentPhoneCharge { get; private set; } // The current charge on the phone battery

		private bool IsCharging = false; // Whether the player is currently charging their phone via outlet
		public bool GetIsCharging() => IsCharging;
		public void SetIsCharging(bool charging) => IsCharging = charging;

		[Serializable]
		public class ObjectRefs
		{
			public Tutorial Tutorial; // The starting screen that the player sees when starting a new game
			public GateNotification GateNotification; // Notification system for sending gate change messages to the player
			public Notification PhoneNotification; // Notification system for sending texts to the player via phone
		}
		public ObjectRefs Refs = new ObjectRefs();

		internal int FlightNumber { get; private set; } // This is the flight the player is supposed to catch this round!
		internal float Score { get; private set; } // This is the player's current score, to be displayed at the end of the round
		internal float TimeSurvived { get; private set; } // This is how long the player has been playing this round
		private bool Playing = false; // Is the game playing or paused?
		public bool IsPlaying() => Playing;

		// Start is called before the first frame update
		void Start()
		{
			// Set the initial timer values
			ResetGateTimer();
			CurrentPhoneCharge = Conf.InitialPhoneCharge;

			// Set the flight number for this round
			FlightNumber = UnityEngine.Random.Range(1000, 10000);

			// Show the tutorial object
			Refs.Tutorial.SetStartMessage(FlightNumber.ToString());
			Refs.Tutorial.StartTutorial();
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
			if (Playing)
			{
				// Handle phone charge timer
				if (IsCharging)
				{
					// Charge the phone battery if the player is standing next to an outlet
					if (CurrentPhoneCharge < 100) CurrentPhoneCharge += Conf.PhoneChargeRate * Time.deltaTime;
					else CurrentPhoneCharge = 100;
				}
				else
				{
					// The more charge the phone carries, the fast it drains!!!
					float phoneDrainModifier = GetPhoneDrainModifier(CurrentPhoneCharge, Conf.PhoneChargeMaxDrainRateModifier, 50);
					if (CurrentPhoneCharge > 0) CurrentPhoneCharge -= Conf.PhoneChargeDrainRate * phoneDrainModifier * Time.deltaTime;
					else
					{
						CurrentPhoneCharge = 0; // TODO :: This is where the player should lose the game!
					}
				}

				// Update the Score
				Score += Conf.ScorePerSecond * Time.deltaTime;
				TimeSurvived += Time.deltaTime;
			}
		}

		private string FormatTime(float time)
		{
			int minutes = (int) time / 60;
			int seconds = (int) time - 60 * minutes;
			return string.Format("{0:00}:{1:00}", minutes, seconds);
		}

		/**
		 * currentCharge - the current phone charge remaining
		 * w - The maximum drain modifier when the battery is full
		 * normalChargeconstraint - the amount of charge remaining where the modifier gets closer to 1, thus normal drain
		**/
		float GetPhoneDrainModifier(float currentCharge, float maxDrainRate, float normalChargeConstraint)
		{
			// When the current charge is at a certain point, the modifier should be 1 (normal)
			// Otherwise, it should be determined linearly
			switch (currentCharge)
			{
				case var _ when currentCharge <= normalChargeConstraint:
					return 1;
				default:
					return ((maxDrainRate - 1) / normalChargeConstraint) * (currentCharge - normalChargeConstraint) + 1;
			}
		}

		// Start all mechanics and timers
		public void StartGame()
		{
			Playing = true;
			// Start the gate timer
			StartCoroutine(StartGateTimer());
		}
	}
}
