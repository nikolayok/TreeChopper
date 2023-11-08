using UnityEngine;

namespace ScoredProductions.DTM
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(BlockDestructionHost))]
	public class ManualHostLoader : MonoBehaviour
	{
		public BlockDestructionHost Connection;

		public BlockDestructionBase Parent;
		public BlockDestructionBase Child;

		public string[] FoundBranches;

		public BlockDestructionBase[] FoundReferences = new BlockDestructionBase[0];

		/// <summary>
		/// Connects the loader to the Container.
		/// </summary>
		/// <returns></returns>
		public bool FindConnection() {
			if (gameObject.GetComponent<BlockDestructionHost>() != null) {
				Connection = gameObject.GetComponent<BlockDestructionHost>();
				PrintFoundBranches();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Prints the containers array into a readable format.
		/// </summary>
		public void PrintFoundBranches() {
			if (Connection != null) {
				FoundBranches = Connection.PrintStoreToString();
				if (Connection.Parents != null) {
					FoundReferences = new BlockDestructionBase[Connection.Parents.Length];
					Connection.Parents.CopyTo(FoundReferences, 0);
				}
			}
		}

		/// <summary>
		/// Adds the Parent and Child combination to the array.
		/// </summary>
		public void AddToBranch() {
			if (Parent != null && Connection != null) {
				if (Child == null) {
					Connection.AddParent(Parent);
					Parent.HostContainer = Connection;
				} else {
					Connection.AddChildBranch(Parent, Child);
					Parent.HostContainer = Child.HostContainer = Connection;
				}

				PrintFoundBranches();
			}
		}

		/// <summary>
		/// Remove the Parent and Child combination from the array.
		/// </summary>
		public void RemoveFromBranch() {
			if (Parent != null) {
				if (Child == null) {
					Connection.RemoveParent(Parent);
				} else if (Parent == Child) {
					Child = null;
					Connection.RemoveParent(Parent);
				} 
				else {
					Connection.RemoveChildBranch(Parent, Child);
				}
				PrintFoundBranches();
			}
		}

		/// <summary>
		/// Print the Connections store to the Console.
		/// </summary>
		public void PrintFoundToConsole() {
			Connection.PrintStoreToString(true);
		}
	}
}
