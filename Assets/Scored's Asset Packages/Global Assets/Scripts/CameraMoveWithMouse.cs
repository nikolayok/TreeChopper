using UnityEngine;

namespace ScoredProductions.Global
{
	[RequireComponent(typeof(Camera))]
	public class CameraMoveWithMouse : MonoBehaviour
	{
		public float XSensitivity = 20.0f;
		public float YSensitivity = 20.0f;

		// Update is called once per frame
		void Update() {
			//Camera thisCamera = this.GetComponent<Camera> ();
			transform.Rotate(XSensitivity * -Input.GetAxis("Mouse Y"), YSensitivity * Input.GetAxis("Mouse X"), 0);
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);
		}
	}
}