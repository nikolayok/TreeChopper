using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace ScoredProductions.DTM
{
	[CustomEditor(typeof(BlockDestructionHost))]
	public class BlockDestructionHostEditor : Editor
	{
		public override void OnInspectorGUI() {

			DrawDefaultInspector();

			BlockDestructionHost myScript = (BlockDestructionHost)target;

			if (!myScript.CheckValidity()) {
				GUILayout.Label("Host currently not valid and wont run at runtime");
			} else {
				GUILayout.Label("Host is valid and will run at runtime");
			}

			if (GUILayout.Button("Validate")) {
				myScript.CheckValidity(true);
			}

			if (GUILayout.Button("Attach Loader")) {
				myScript.AttachLoader();
			}
		}
	}
}

#endif