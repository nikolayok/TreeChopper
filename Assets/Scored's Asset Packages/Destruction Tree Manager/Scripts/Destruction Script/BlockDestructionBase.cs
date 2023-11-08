using System;
using System.Collections;
using UnityEngine;

namespace ScoredProductions.DTM
{
	[ExecuteInEditMode]
	public abstract class BlockDestructionBase : MonoBehaviour
	{
		public bool Destructible = true; // if it will fall apart when damaged.

		[NonSerialized]
		public bool Destroyed = false; // If the objects death has been triggered. // RETHINK NAME AND PURPOSE.

		[Range(0, 99999)]
		public float DestroyTime = 30; // How much time passes before the object despawns.

		public bool DestroyOnSleep; // If the object is destroyed on sleep.

		public BlockDestructionHost HostContainer;

		protected Rigidbody rigidbodyRef;

		/// <summary>
		/// Assigned ID, stored to distinguesh between other interfaces.
		/// </summary>
		[HideInInspector] [SerializeField]
		private string ID;

		public string GetID() { 
			return ID; 
		}

		public string GenerateID() { // ID can only be edited with this function.
			return ID = Guid.NewGuid().ToString();
		}

		public abstract void Start();

		public abstract void Update();

		/// <summary>
		/// Request the game object to send to the manager to detatch. 
		/// </summary>
		/// <param name="all"> Requests seperated blocks in Parent class, no effect on Child class. </param>
		/// <returns></returns>
		public abstract GameObject RequestObject(bool all);

		/// <summary>
		/// Used to confirm that the Block is connected to the Host Container if attached to one.
		/// </summary>
		/// <returns></returns>
		public bool ConfirmRegisteredWithHost() {
			if (HostContainer != null) {
				if (HostContainer.ConfirmRegistered(this)) {
					return true;
				}
				Debug.LogWarning(gameObject.name + " / Failed to validate with Host block.");
			}
			return false;
		}

		/// <summary>
		/// Function to initiate the break in the inherited classes, used by Host to trigger breaks.
		/// </summary>
		public abstract void InitiateBreak(Transform parent = null);

		/// <summary>
		/// Function to notify Host Containers that a break has occured.
		/// </summary>
		public abstract void NotifyDestruction();

		/// <summary>
		/// Destroy after time passed specified in DestroyTime.
		/// </summary>
		protected void DestroyOnDelay() {
			Destroy(gameObject, DestroyTime);
		}

		/// <summary>
		/// Destroy when the rigid body identifys the object as asleep. If no Rigidbody the it will immediatly destroy.
		/// </summary>
		/// <returns></returns>
		protected IEnumerator WaitUntillSleep() {
			if (rigidbodyRef != null) {
				yield return new WaitUntil(rigidbodyRef.IsSleeping);
			}
			Destroy(gameObject);
		}
	}
}