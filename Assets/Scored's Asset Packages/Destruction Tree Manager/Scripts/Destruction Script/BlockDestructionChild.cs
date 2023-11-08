using System;
using ScoredProductions.Global;
using UnityEngine;

namespace ScoredProductions.DTM
{
	[RequireComponent(typeof(Collider))]
	public class BlockDestructionChild : BlockDestructionBase, DamageInterface
	{
		[Range(0, Mathf.Infinity)]
		public float Health = 1; // Health of the object

		private float DamageReceived = 0; // Current damage to be processed 
		public void ReceiveDamage(float damage) { DamageReceived += damage; }

		public Action<BlockDestructionChild> BreakTriggered;

		public override void Start() {
			ConfirmRegisteredWithHost(); // Although currently does nothing, the returning bool can be used in the future
		}

		public override void Update() {
			ProcessDamage();
		}

		public override GameObject RequestObject(bool all) {
			return gameObject; // As its a single object just return self.
		}

		/// <summary>
		/// Process any damage.
		/// </summary>
		public void ProcessDamage() {
			if (Destructible) {
				//if (Input.GetKey(KeyCode.F)) { // To test the destruction
				//	DamageRecived = Health + 1;
				//}

				if (DamageReceived > 0) {
					Health -= DamageReceived;
					DamageReceived = 0;
				}

				if (Health <= 0 && !Destroyed) {
					NotifyDestruction();
					InitiateBreak();
				}
			}
		}

		public override void InitiateBreak(Transform parent = null) {
			Destroyed = true; // Only need to be called once

			if (parent == null) {
				transform.parent = null;

				rigidbodyRef = transform.GetComponent<Rigidbody>();

				if (rigidbodyRef == null) { // Impliment physics if not already applyed
					rigidbodyRef = gameObject.AddComponent<Rigidbody>();
				}

				rigidbodyRef.useGravity = true;
				rigidbodyRef.isKinematic = false;
				rigidbodyRef.sleepThreshold = 0.001f; // How much movement is the minimum before it identifys as being asleep (needs more testing)

				if (DestroyOnSleep) { // whether destruction uses sleep or delay
					StartCoroutine(WaitUntillSleep());
				} else {
					DestroyOnDelay();
				}
			} else {
				transform.parent = parent;
			}
		}

		public override void NotifyDestruction() {
			if (BreakTriggered != null) {
				BreakTriggered.Invoke(this);
			}
			if (HostContainer != null) {
				HostContainer.NotifyHostOfDestruction(this);
			}
		}
	}
}