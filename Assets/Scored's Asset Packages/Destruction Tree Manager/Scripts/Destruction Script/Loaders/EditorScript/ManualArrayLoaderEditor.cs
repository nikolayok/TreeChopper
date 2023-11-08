using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace ScoredProductions.DTM
{
	[CustomEditor(typeof(ManualArrayLoader))]
	public class ManualArrayLoaderEditor : Editor
	{
		public string CurrentMessage;

		public override void OnInspectorGUI() {

			DrawDefaultInspector();

			if (!string.IsNullOrEmpty(CurrentMessage)) {
				GUILayout.Label(CurrentMessage);
			}

			ManualArrayLoader myScript = (ManualArrayLoader)target;

			if (myScript.Connection != null) {
				if (myScript.ToConnect != null) {
					if (GUILayout.Button("Add/Replace")) {
						myScript.AddToArray();
					}
					if (GUILayout.Button("Remove")) {
						myScript.RemoveFromArray();
					}
				}
				if (GUILayout.Button("Refresh Notes")) {
					myScript.PrintArrayToInspector();
				}
				if (GUILayout.Button("Print Log to Console")) {
					myScript.PrintListToConsole();
				}
				GUILayout.Label("Save the new Array into the Parents Array");
				if (GUILayout.Button("Save and Apply")) {
					myScript.SaveContainer();
				}
				GUILayout.Label("Resets this array to be empty");
				if (GUILayout.Button("Reset")) {
					myScript.ClearContainer();
				}
			} else {
				if (GUILayout.Button("Connect")) {
					if (myScript.FindConnection()) {
						CurrentMessage = "Connection Successful";
					} else {
						CurrentMessage = "Connection Failed";
					}
				}
			}
		}
	}
}

#endif