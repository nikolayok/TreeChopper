using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace ScoredProductions.DTM
{
	[CustomEditor(typeof(ManualHostLoader))]
	public class ManualHostLoaderEditor : Editor
	{
		public string CurrentMessage;

		public override void OnInspectorGUI() {

			DrawDefaultInspector();

			GUILayout.Label(CurrentMessage);

			ManualHostLoader myScript = (ManualHostLoader)target;

			if (myScript.Connection != null) {
				if (myScript.Parent != null) {
					if (GUILayout.Button("Add")) {
						myScript.AddToBranch();
					}
					if (GUILayout.Button("Remove")) {
						myScript.RemoveFromBranch();
					}
				}
				if (GUILayout.Button("Refresh References")) {
					myScript.PrintFoundBranches();
				}
				if (GUILayout.Button("Print to Console")) {
					myScript.PrintFoundToConsole();
				}
			} else {
				myScript.FoundBranches = null;
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