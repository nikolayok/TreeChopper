using UnityEngine;

namespace ScoredProductions.Global
{
	[RequireComponent(typeof(CharacterController))]
	public class MyBasicFPC : MonoBehaviour
	{
		public float XSpeed = 0.02f;
		public float ZSpeed = 0.02f;

		CharacterController controller;

		Vector3 MoveVector = new Vector3(0, 0, 0);

		// Use this for initialization
		void Start() {
			controller = GetComponent<CharacterController>();
			Cursor.lockState = CursorLockMode.Locked;
		}

		// Update is called once per frame
		void Update() {
			if (Input.GetKey(KeyCode.A)) {
				MoveVector -= this.transform.right * ZSpeed;
			}

			if (Input.GetKey(KeyCode.D)) {
				MoveVector += this.transform.right * ZSpeed;
			}

			if (Input.GetKey(KeyCode.W)) {
				MoveVector += this.transform.forward * XSpeed;
			}

			if (Input.GetKey(KeyCode.S)) {
				MoveVector -= this.transform.forward * XSpeed;
			}

			controller.Move(MoveVector);
			MoveVector = Vector3.zero;
		}
	}
}