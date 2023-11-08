using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScoredProductions.DTM
{
	public class BlockDestructionHost : MonoBehaviour
	{
		public bool Disintegrate; // If the object breaks as a single whole object or it falls appart as all its individual peices.

		[HideInInspector]
		public BlockDestructionBase[] Parents; // Each block is a parent and has a branch, even with no children, parents with no children are end branches.

		[HideInInspector]
		[SerializeField]
		private string[] ChildrenIds; // Saved IDs and connections.

		public string[] PrintedIds = new string[0]; // To avoid tampering with the original.

		[HideInInspector]
		[SerializeField]
		private bool Valid; // Identifies if the stucture is valid and will run at runtime.

		private bool performedCheck = false; // Value to stop constant full validations.

		private bool[] ConnectedCheck; // Container to check which Parents are still connected.

		public void Start() {
			if (Parents != null) {
				CheckValidity(); // Performs basic check if required.
				ConnectedCheck = new bool[Parents.Length];
				for (int x = 0; x < ConnectedCheck.Length; x++) { // Populate the array as all true.
					ConnectedCheck[x] = true;
				}
			}
		}

		/// <summary>
		/// Function to check the stucture and if it is valid to run.
		/// </summary>
		/// <param name="log"> True: Forces a full check and logs errors to console. </param>
		/// <returns></returns>
		public bool CheckValidity(bool log = false) {
			if (!performedCheck || log) {
				Valid = false;
				ValidateBranchesAndInterfaces();
				if (!InfiniteBranchChecker()) {
					if (log) {
						Debug.LogError("Infinite loop checker failed: Validation failed");
					}
				} else if (Parents == null || Parents.Length == 0) {
					if (log) {
						Debug.Log("Empty structure is invalid by default");
					}
				} else {
					Valid = true;
					Parents.ToList().ForEach(e => {
						if (e.HostContainer != this) {
							if (log) {
								Debug.LogError(e.name + " - Host Container doesnt match this object: Validation failed");
							}
							Valid = false;
						}
					});
				}
				MulitpleParentCheck(log);
				performedCheck = true;
			}
			if (ChildrenIds != null) {
				PrintedIds = new string[ChildrenIds.Length];
				ChildrenIds.CopyTo(PrintedIds, 0);
			}
			return Valid;
		}

		/// <summary>
		/// Check if the branch is just the Parent and no children.
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		private bool IsEndBranch(BlockDestructionBase block) {
			foreach (string id in ChildrenIds) {
				string[] split = id.Split('|');

				if (split[0].Equals(block.GetID())) {
					if (split.Length > 1) {
						return false;
					} else {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Add a block to the Host structure.
		/// </summary>
		/// <param name="parent"></param>
		public void AddParent(BlockDestructionBase parent) {
			performedCheck = Valid = false;
			List<BlockDestructionBase> temp = Parents == null ? new List<BlockDestructionBase>() : Parents.ToList();
			if (!temp.Contains(parent)) {
				temp.Add(parent);
				Parents = temp.ToArray();

				parent.GenerateID();

				List<string> tempChildren = ChildrenIds == null ? new List<string>() : ChildrenIds.ToList();
				if (string.IsNullOrEmpty(tempChildren.Find(e => e.Split('|')[0].Equals(parent.GetID())))) {
					tempChildren.Add(parent.GetID());
					PrintedIds = ChildrenIds = tempChildren.ToArray();
				}

				MulitpleParentCheck(true);
			}
		}

		/// <summary>
		/// Remove a block from the Host structure.
		/// </summary>
		/// <param name="parent"></param>
		public void RemoveParent(BlockDestructionBase parent) {
			performedCheck = Valid = false;
			List<BlockDestructionBase> temp = Parents.ToList();

			temp.ForEach(e => {
				if (e != parent) {
					RemoveChildBranch(e, parent);
				}
			});

			if (temp.Contains(parent)) {
				temp.Remove(parent);
				Parents = temp.ToArray();
			}

			List<string> tempChildren = ChildrenIds.ToList();
			int childPos = tempChildren.FindIndex(e => e.Split('|')[0].Equals(parent.GetID()));
			if (childPos != -1) {
				tempChildren.RemoveAt(childPos);
				PrintedIds = ChildrenIds = tempChildren.ToArray();
			}
		}

		/// <summary>
		/// Adds the child to the parents tree. Adds both to the Host structure. Fails if both are equal or the addition causes an infinite loop.
		/// </summary>
		/// <param name="parent"> Structure to add to. </param>
		/// <param name="child"> Block to add onto the structure. </param>
		public void AddChildBranch(BlockDestructionBase parent, BlockDestructionBase child) {
			performedCheck = Valid = false;
			if (parent != child) {
				AddParent(parent);
				AddParent(child);

				int childPos = ChildrenIds.ToList().FindIndex(e => e.Split('|')[0].Equals(parent.GetID()));

				string original = ChildrenIds[childPos];
				string[] split = original.Split('|');

				bool check = true;
				for (int x = 1; x < split.Length; x++) {
					if (split[x].Equals(child.GetID())) {
						check = false;
						break;
					}
				}
				if (check) {
					ChildrenIds[childPos] = original + "|" + child.GetID();
				}
			} else {
				Debug.LogError("Same object inserted for both Parent and Child, Operation Canceled");
			}
			if (!InfiniteBranchChecker()) {
				Debug.LogWarning("Object inserted would cause an infinite loop, branch reverted.");
				RemoveChildBranch(parent, child);
			}
			ValidateBranchesAndInterfaces();
		}

		/// <summary>
		/// Removes the child from the parents structure.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="child"></param>
		public void RemoveChildBranch(BlockDestructionBase parent, BlockDestructionBase child) {
			performedCheck = Valid = false;
			int childPos = ChildrenIds.ToList().FindIndex(e => e.Split('|')[0].Equals(parent.GetID()));
			if (childPos != -1) {
				string[] split = ChildrenIds[childPos].Split('|');

				string temp = split[0];

				for (int x = 1; x < split.Length; x++) {
					if (!split[x].Equals(child.GetID())) {
						temp = temp + "|" + split[x];
					}
				}
				ChildrenIds[childPos] = temp;
			}
		}

		/// <summary>
		/// Removes non-connecting interfaces and IDs.
		/// </summary>
		public void ValidateBranchesAndInterfaces() {
			if (Parents != null && ChildrenIds != null) {
				List<BlockDestructionBase> tempParentList = Parents.ToList();

				foreach (BlockDestructionBase parentblock in tempParentList) { // Filter out unused Parents.
					bool notfound = true;
					if (parentblock != null) {
						foreach (string branch in ChildrenIds) {
							string[] split = branch.Split('|');

							if (parentblock.GetID().Equals(split[0])) {
								notfound = false;
								break;
							}
						}
					}

					if (notfound) {
						tempParentList.Remove(parentblock);
						Parents = tempParentList.ToArray();
					}
				}

				for (int x = 0; x < ChildrenIds.Length; x++) { // Filter out unused IDs.
					string[] split = ChildrenIds[x].Split('|');

					string temp = split[0];

					bool parentCheck = false;
					for (int y = 0; y < split.Length; y++) {
						if (y == 0) { // Parent Entry
							if (tempParentList.Find(e => e.GetID().Equals(split[0])) == null) {
								List<string> c = ChildrenIds.ToList();
								c.Remove(ChildrenIds[x]);
								ChildrenIds = c.ToArray();
								parentCheck = true;
								break;
							}
						} else { // Child Entries
							if (tempParentList.Find(e => e.GetID().Equals(split[y])) != null) {
								temp = temp + "|" + split[y];
							}
						}
					}

					if (!parentCheck) {
						ChildrenIds[x] = temp;
					}
				}
			}
		}

		/// <summary>
		/// Function to begin the operation to seperate child blocks.
		/// </summary>
		/// <param name="obj"></param>
		public void NotifyHostOfDestruction(BlockDestructionBase obj) {
			if (!Valid || !Parents.Contains(obj)) {
				return;
			}

			if (obj.GetType() == typeof(BlockDestructionChild)) {
				for (int x = 0; x < Parents.Length; x++) {
					if (Parents[x] == obj) {
						ConnectedCheck[x] = false;
					}
				}
			}

			string branch = ChildrenIds.ToList().Find(e => e.Split('|')[0].Equals(obj.GetID()));

			if (!string.IsNullOrEmpty(branch)) {
				GameObject top = obj.RequestObject(false);

				string[] split = branch.Split('|');
				for (int x = 1; x < split.Length; x++) {
					AttachToSeperation(top, split[x]);
				}
			}
		}

		/// <summary>
		/// Function to manage the attachment and block breaks of child blocks.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="child"></param>
		private void AttachToSeperation(GameObject parent, string child) {
			BlockDestructionBase childBlock = Parents.ToList().Find(e => e.GetID().Equals(child));
			for (int x = 0; x < Parents.Length; x++) {
				if (Parents[x] == childBlock) {
					if (!ConnectedCheck[x]) {
						return;
					} else {
						ConnectedCheck[x] = false;
					}
					break;
				}
			}

			string branch = ChildrenIds.ToList().Find(e => e.Split('|')[0].Equals(childBlock.GetID()));

			if (!string.IsNullOrEmpty(branch)) {
				childBlock.NotifyDestruction();

				if (Disintegrate) {
					childBlock.InitiateBreak();
				} else {
					childBlock.InitiateBreak(parent.transform);
				}

				if (!IsEndBranch(childBlock)) {
					string[] split = branch.Split('|');
					for (int x = 1; x < split.Length; x++) {
						AttachToSeperation(childBlock.RequestObject(true), split[x]);
					}
				}
			}
		}

		/// <summary>
		/// Confirms Block is registered in the Host. Checks both Parents container and ChildrensIDs. Cleans containers if fails.
		/// </summary>
		/// <param name="reg"></param>
		/// <returns></returns>
		public bool ConfirmRegistered(BlockDestructionBase reg) {
			if (Parents.Contains(reg) && !string.IsNullOrEmpty(ChildrenIds.ToList().Find(e => e.Split('|')[0].Equals(reg.GetID())))) {
				return true;
			}
			ValidateBranchesAndInterfaces();
			return false;
		}

		/// <summary>
		/// Prints containers into human readable format
		/// </summary>
		/// <param name="PrintToConsole"></param>
		/// <returns></returns>
		public string[] PrintStoreToString(bool PrintToConsole = false) {
			ValidateBranchesAndInterfaces();
			List<BlockDestructionBase> temp = Parents == null ? new List<BlockDestructionBase>() : Parents.ToList();
			string parent = "";
			List<string> children = new List<string>();
			List<string> Output = new List<string>();

			if (ChildrenIds != null) {
				foreach (string branch in ChildrenIds) {
					string[] split = branch.Split('|');
					children.Clear();
					for (int x = 0; x < split.Length; x++) {
						if (x == 0) {
							parent = temp.Find(e => e.GetID().Equals(split[x])).gameObject.name;
						} else {
							children.Add(temp.Find(e => e.GetID().Equals(split[x])).gameObject.name);
						}
					}
					if (!string.IsNullOrEmpty(parent)) {
						if (children.Count > 0) {
							string output = "Block: " + parent + " | Children found: " + children.Count;
							foreach (string child in children) {
								output = output + " / " + child;
							}
							Output.Add(output);
						} else {
							Output.Add("Block: " + parent + " | No Children found");
						}
					}
				}
			}

			if (PrintToConsole) {
				if (Output.Count > 0) {
					Output.ForEach(e => Debug.Log(e));
				} else {
					Debug.Log(gameObject.name + ": No entries found.");
				}
			}

			return Output.ToArray();
		}

		/// <summary>
		/// loops through the store, make sure it doesnt make an infinite loop.
		/// </summary>
		/// <returns></returns>
		public bool InfiniteBranchChecker() {
			if (Parents != null && ChildrenIds != null) {
				foreach (BlockDestructionBase branch in Parents) {
					string children = ChildrenIds.ToList().Find(e => e.Split('|')[0].Equals(branch.GetID()));
					if (!string.IsNullOrEmpty(children)) {
						if (!InfiniteSearch(branch, children)) {
							return false;
						}
					} else {
						Debug.LogWarning("Null branch found, check canceled and attempting to validate, please try again.");
						ValidateBranchesAndInterfaces();
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Processes the branch level of the InfiniteBranchChecker.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="branch"></param>
		/// <returns></returns>
		private bool InfiniteSearch(BlockDestructionBase origin, string branch) {
			List<BlockDestructionBase> tempParentList = Parents.ToList();
			string[] split = branch.Split('|');

			for (int x = 1; x < split.Length; x++) {
				BlockDestructionBase child = tempParentList.Find(e => e.GetID().Equals(split[x]));
				if (child == origin) {
					return false;
				}

				string children = ChildrenIds.ToList().Find(e => e.Split('|')[0].Equals(split[x]));
				if (child != null && !string.IsNullOrEmpty(children)) {
					if (!InfiniteSearch(origin, children)) {
						return false;
					}
				} else {
					Debug.LogWarning("Null branch found, check canceled and attempting to validate, please try again.");
					ValidateBranchesAndInterfaces();
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Warns the user if an object is found under multiple parents, currently does not invalidate.
		/// </summary>
		/// <param name="log"> True: Prints results</param>
		/// <returns></returns>
		public bool MulitpleParentCheck(bool log) {
			bool check = false;
			if (Parents != null && ChildrenIds != null) {
				foreach (BlockDestructionBase block in Parents) {
					int blockcheck = 0;
					foreach (string branch in ChildrenIds) {
						string[] split = branch.Split('|');
						for (int x = 1; x < split.Length; x++) {
							if (block.GetID().Equals(split[x])) {
								blockcheck++;
							}
							if (blockcheck > 1) {
								break;
							}
						}
						if (blockcheck > 1) {
							break;
						}
					}
					if (blockcheck > 1) {
						if (log) {
							Debug.LogWarning(block.gameObject.name + " has been found under multiple parents, this could cause unexpected behaviour during gameplay.");
						}
						check = true;
					}
				}
			}
			return check;
		}

		/// <summary>
		/// Shortcut to attach a loader to this class.
		/// </summary>
		/// <returns></returns>
		public bool AttachLoader() {
			if (GetComponent<ManualHostLoader>() == null) {
				return gameObject.AddComponent<ManualHostLoader>().FindConnection();
			}
			return gameObject.GetComponent<ManualHostLoader>().FindConnection();
		}
	}
}