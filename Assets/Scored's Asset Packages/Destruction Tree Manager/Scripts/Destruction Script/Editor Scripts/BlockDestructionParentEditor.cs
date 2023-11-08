using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace ScoredProductions.DTM
{
	[CustomEditor(typeof(BlockDestructionParent))]
	public class BlockDestructionParentEditor : Editor
	{
		public override void OnInspectorGUI() {

			DrawDefaultInspector();

			BlockDestructionParent myScript = (BlockDestructionParent)target;

			GUILayout.Label("Serialized data will be loaded on Start.");

			if (myScript.ChildBlocks == null) {
				if (GUILayout.Button("Load Serialized data and Print")) {
					myScript.RestoreArray();
					myScript.PrintArrayToConsole();
				}
			} else {
				if (GUILayout.Button("Print Array To Console")) {
					myScript.PrintArrayToConsole();
				}
			}
			if (GUILayout.Button("Attach Loader")) {
				myScript.AttachLoader();
			}
		}
	}
}

#endif