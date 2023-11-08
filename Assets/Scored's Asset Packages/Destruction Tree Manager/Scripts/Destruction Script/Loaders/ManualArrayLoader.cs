using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScoredProductions.DTM
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(BlockDestructionParent))]
	public class ManualArrayLoader : MonoBehaviour
	{
		public BlockDestructionParent Connection;

		public BlockDestructionChild[,,] tempContainer = new BlockDestructionChild[0, 0, 0];

		public BlockDestructionChild ToConnect;

		// Need tag to say use real position instead of logical (0 => first)

		public int PositionX;

		public int PositionY;

		public int PositionZ;

		public string[] NotesList;

		public BlockDestructionChild[] ReferenceList = new BlockDestructionChild[0];

		/// <summary>
		/// Connects to the container and loads a copy of its array.
		/// </summary>
		/// <returns></returns>
		public bool FindConnection() {
			if (gameObject.GetComponent<BlockDestructionParent>() != null) {
				Connection = gameObject.GetComponent<BlockDestructionParent>();

				Connection.RestoreArray();

				if (Connection.ChildBlocks == null) {
					tempContainer = Connection.ChildBlocks = new BlockDestructionChild[0, 0, 0];
				} else {
					tempContainer = new BlockDestructionChild[Connection.ChildBlocks.GetLength(0), Connection.ChildBlocks.GetLength(1), Connection.ChildBlocks.GetLength(2)];

					for (int x = 0; x < Connection.ChildBlocks.GetLength(0); x++) {
						for (int y = 0; y < Connection.ChildBlocks.GetLength(1); y++) {
							for (int z = 0; z < Connection.ChildBlocks.GetLength(2); z++) {
								tempContainer[x, y, z] = Connection.ChildBlocks[x, y, z];
							}
						}
					}
				}

				PrintArrayToInspector();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Empties and resets the array.
		/// </summary>
		public void ClearContainer() {
			tempContainer = new BlockDestructionChild[0, 0, 0];
			PositionX = PositionY = PositionZ = 0;
			PrintArrayToInspector();
		}

		/// <summary>
		/// Save the created array into the connected container.
		/// </summary>
		public void SaveContainer() {
			if (CheckArrayValid()) {
				if (tempContainer != null) {
					Connection.ChildBlocks = new BlockDestructionChild[tempContainer.GetLength(0), tempContainer.GetLength(1), tempContainer.GetLength(2)];
					for (int x = 0; x < tempContainer.GetLength(0); x++) {
						for (int y = 0; y < tempContainer.GetLength(1); y++) {
							for (int z = 0; z < tempContainer.GetLength(2); z++) {
								Connection.ChildBlocks[x, y, z] = tempContainer[x, y, z];
							}
						}
					}
				}
				Connection.FlattenArray();
				Debug.Log("Array Valid, Save Successful");
			} else {
				Debug.LogError("Array Invalid: All Entries much be connected to eachother by at least one direction.");
			}
			PrintArrayToInspector();
		}

		/// <summary>
		/// Prints the array into the Inspector in a readable format.
		/// </summary>
		public void PrintArrayToInspector() {
			List<string> entries = new List<string>();
			if (tempContainer != null) {
				List<BlockDestructionChild> store = tempContainer.Cast<BlockDestructionChild>().ToList();
				for (int x = 0; x < tempContainer.GetLength(0); x++) {
					for (int y = 0; y < tempContainer.GetLength(1); y++) {
						for (int z = 0; z < tempContainer.GetLength(2); z++) {
							string tag = "Pos: X: " + x.ToString() + " Y: " + y.ToString() + " Z: " + z.ToString() + " >> ";
							if (tempContainer[x, y, z] == null) {
								entries.Add(tag + "*NULL*");
							} else {
								if (store.FindAll(e => e == tempContainer[x, y, z]).Count > 1) {
									tag += "{Large} ";
								}
								entries.Add(tag + tempContainer[x, y, z].gameObject.name);
							}
						}
					}
				}
				ReferenceList = tempContainer.Cast<BlockDestructionChild>().ToArray();
			}
			NotesList = entries.ToArray();
		}

		/// <summary>
		/// Adds the entry to the array.
		/// </summary>
		public void AddToArray() {
			if (PositionX > -1 && PositionY > -1 && PositionZ > -1) {
				if (!CheckIfNeighbouringBlock(PositionX, PositionY, PositionZ)) {
					RemoveDuplicates();
				}

				int xLimit = PositionX + 1 > tempContainer.GetLength(0) ? PositionX + 1 : tempContainer.GetLength(0);
				int yLimit = PositionY + 1 > tempContainer.GetLength(1) ? PositionY + 1 : tempContainer.GetLength(1);
				int zLimit = PositionZ + 1 > tempContainer.GetLength(2) ? PositionZ + 1 : tempContainer.GetLength(2);

				BlockDestructionChild[,,] temp = new BlockDestructionChild[xLimit, yLimit, zLimit];

				for (int x = 0; x < tempContainer.GetLength(0); x++) {
					for (int y = 0; y < tempContainer.GetLength(1); y++) {
						for (int z = 0; z < tempContainer.GetLength(2); z++) {
							temp[x, y, z] = tempContainer[x, y, z];
						}
					}
				}

				temp[PositionX, PositionY, PositionZ] = ToConnect;

				tempContainer = temp;

				PrintArrayToInspector();
			} else {
				Debug.LogError("Position values corresponds to Array positions, as such they cant be negative.");
			}
		}

		/// <summary>
		/// Remove an entry from a specified position.
		/// </summary>
		public void RemoveFromArray() {
			if (PositionX > -1 && PositionY > -1 && PositionZ > -1) {
				if (PositionX < tempContainer.GetLength(0) && PositionY < tempContainer.GetLength(1) && PositionZ < tempContainer.GetLength(2)) {
					if (tempContainer[PositionX, PositionY, PositionZ] == ToConnect) {
						tempContainer[PositionX, PositionY, PositionZ] = null;
					} else {
						Debug.LogWarning(ToConnect.gameObject.name + ": Was not found at this position.");
					}

					PrintArrayToInspector();
				} else {
					Debug.LogError("Position values fall outside the Array.");
				}
			} else {
				Debug.LogError("Position values corresponds to Array positions, as such they cant be negative.");
			}
		}

		/// <summary>
		/// Check if the new position is neighbouring any current positions.
		/// </summary>
		/// <param name="posx"></param>
		/// <param name="posy"></param>
		/// <param name="posz"></param>
		/// <returns></returns>
		private bool CheckIfNeighbouringBlock(int posx, int posy, int posz) {
			List<Vector3Int> foundPos = new List<Vector3Int>();
			for (int x = 0; x < tempContainer.GetLength(0); x++) {
				for (int y = 0; y < tempContainer.GetLength(1); y++) {
					for (int z = 0; z < tempContainer.GetLength(2); z++) {
						if (tempContainer[x, y, z] == ToConnect) {
							foundPos.Add(new Vector3Int(x, y, z));
						}
					}
				}
			}
			foreach (Vector3Int e in foundPos) {
				if (Math.Abs(e.x - posx) == 1 || Math.Abs(e.y - posy) == 1 || Math.Abs(e.z - posz) == 1) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Remove entry from the array.
		/// </summary>
		private void RemoveDuplicates() {
			for (int x = 0; x < tempContainer.GetLength(0); x++) {
				for (int y = 0; y < tempContainer.GetLength(1); y++) {
					for (int z = 0; z < tempContainer.GetLength(2); z++) {
						if (tempContainer[x, y, z] == ToConnect) {
							tempContainer[x, y, z] = null;
						}
					}
				}
			}
		}

		/// <summary>
		/// Print the notes into the Unity Console
		/// </summary>
		public void PrintListToConsole() {
			foreach (string log in NotesList) {
				Debug.Log(log);
			}
		}

		/// <summary>
		/// Check to make sure the array being saved is valid.
		/// </summary>
		/// <returns></returns>
		private bool CheckArrayValid() {
			ArrayShrinker();
			PrintArrayToInspector();

			int xMax = tempContainer.GetLength(0);
			int yMax = tempContainer.GetLength(1);
			int zMax = tempContainer.GetLength(2);

			int ValidValue = 0;
			int CheckValue = 0;

			bool[,,] checks = new bool[xMax, yMax, zMax];
			bool _break = false;
			for (int x = 0; x < xMax; x++) {
				for (int y = 0; y < yMax; y++) {
					for (int z = 0; z < zMax; z++) {
						if (tempContainer[x, y, z] != null) {
							_break = true;
							Search(ref checks, x, y, z);
						}
						if (_break) {
							break;
						}
					}
					if (_break) {
						break;
					}
				}
				if (_break) {
					break;
				}
			}

			for (int x = 0; x < xMax; x++) {
				for (int y = 0; y < yMax; y++) {
					for (int z = 0; z < zMax; z++) {
						if (tempContainer[x, y, z] != null) {
							ValidValue++;
						}
						if (checks[x, y, z]) {
							CheckValue++;
						}
					}
				}
			}

			if (CheckValue == ValidValue) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Search to fill the checklist of connected blocks in the container.
		/// </summary>
		/// <param name="checklist"> Checklist of the same size as the comparing container. </param>
		/// <param name="xpos"> Start X position </param>
		/// <param name="ypos"> Start Y position </param>
		/// <param name="zpos"> Start Z position </param>
		/// <returns></returns>
		private void Search(ref bool[,,] checklist, int xpos, int ypos, int zpos) {
			if (tempContainer[xpos, ypos, zpos] != null) {
				checklist[xpos, ypos, zpos] = true;

				if (xpos + 1 < checklist.GetLength(0) && !checklist[xpos + 1, ypos, zpos]) {
					Search(ref checklist, xpos + 1, ypos, zpos);
				}
				if (xpos - 1 >= 0 && !checklist[xpos - 1, ypos, zpos]) {
					Search(ref checklist, xpos - 1, ypos, zpos);
				}
				if (ypos + 1 < checklist.GetLength(1) && !checklist[xpos, ypos + 1, zpos]) {
					Search(ref checklist, xpos, ypos + 1, zpos);
				}
				if (ypos - 1 >= 0 && !checklist[xpos, ypos - 1, zpos]) {
					Search(ref checklist, xpos, ypos - 1, zpos);
				}
				if (zpos + 1 < checklist.GetLength(2) && !checklist[xpos, ypos, zpos + 1]) {
					Search(ref checklist, xpos, ypos, zpos + 1);
				}
				if (zpos - 1 >= 0 && !checklist[xpos, ypos, zpos - 1]) {
					Search(ref checklist, xpos, ypos, zpos - 1);
				}
			}
		}

		/// <summary>
		/// Search for the max/min of the array and shrink the array to the found boundries.
		/// </summary>
		private void ArrayShrinker() {
			int xlow = 0;
			int ylow = 0;
			int zlow = 0;
			int xhigh = tempContainer.GetLength(0) - 1;
			int yhigh = tempContainer.GetLength(1) - 1;
			int zhigh = tempContainer.GetLength(2) - 1;

			// Lowerbound x
			for (int x = 0; x <= xhigh; x++) {
				bool pass = false;
				for (int y = ylow; y <= yhigh; y++) {
					for (int z = zlow; z <= zhigh; z++) {
						if (tempContainer[x, y, z] != null) {
							y = yhigh;
							z = zhigh;
							pass = true;
						}
					}
				}

				if (pass) {
					xlow = x;
					break;
				}
			}

			// Lowerbound y
			for (int y = 0; y <= yhigh; y++) {
				bool pass = false;
				for (int x = xlow; x <= xhigh; x++) {
					for (int z = zlow; z <= zhigh; z++) {
						if (tempContainer[x, y, z] != null) {
							x = xhigh;
							z = zhigh;
							pass = true;
						}
					}
				}

				if (pass) {
					ylow = y;
					break;
				}
			}

			// Lowerbound z
			for (int z = 0; z <= zhigh; z++) {
				bool pass = false;
				for (int x = xlow; x <= xhigh; x++) {
					for (int y = ylow; y <= yhigh; y++) {
						if (tempContainer[x, y, z] != null) {
							x = xhigh;
							y = yhigh;
							pass = true;
						}
					}
				}

				if (pass) {
					zlow = z;
					break;
				}
			}

			// Upperbound x
			for (int x = xhigh; x >= xlow; x--) {
				bool pass = false;
				for (int y = yhigh; y >= ylow; y--) {
					for (int z = zhigh; z >= zlow; z--) {
						if (tempContainer[x, y, z] != null) {
							y = 0;
							z = 0;
							pass = true;
						}
					}
				}

				if (pass) {
					xhigh = x;
					break;
				}
			}

			// Upperbound y
			for (int y = yhigh; y >= ylow; y--) {
				bool pass = false;
				for (int x = xhigh; x >= xlow; x--) {
					for (int z = zhigh; z >= zlow; z--) {
						if (tempContainer[x, y, z] != null) {
							x = 0;
							z = 0;
							pass = true;
						}
					}
				}

				if (pass) {
					yhigh = y;
					break;
				}
			}

			// Upperbound z
			for (int z = zhigh; z >= zlow; z--) {
				bool pass = false;
				for (int x = xhigh; x >= xlow; x--) {
					for (int y = yhigh; y >= ylow; y--) {
						if (tempContainer[x, y, z] != null) {
							x = 0;
							y = 0;
							pass = true;
						}
					}
				}

				if (pass) {
					zhigh = z;
					break;
				}
			}

			// Transfer to new container
			if (xlow != 0
				|| ylow != 0
				|| zlow != 0
				|| xhigh != tempContainer.GetLength(0) - 1
				|| yhigh != tempContainer.GetLength(1) - 1
				|| zhigh != tempContainer.GetLength(2) - 1) {

				if (xhigh > 0 && yhigh > 0 && zhigh > 0) {
					BlockDestructionChild[,,] newContainer = new BlockDestructionChild[xhigh - xlow + 1, yhigh - ylow + 1, zhigh - zlow + 1];

					for (int x = xlow; x <= xhigh; x++) {
						for (int y = ylow; y <= yhigh; y++) {
							for (int z = zlow; z <= zhigh; z++) {
								newContainer[x - xlow, y - ylow, z - zlow] = tempContainer[x, y, z];
							}
						}
					}

					tempContainer = newContainer;
				}
			}
		}
	}
}
