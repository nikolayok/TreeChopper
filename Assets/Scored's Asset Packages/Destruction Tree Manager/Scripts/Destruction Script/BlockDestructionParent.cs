using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScoredProductions.DTM
{
	public class BlockDestructionParent : BlockDestructionBase
	{
		public bool Disintergrate; // If the object breaks as a single whole object or it falls appart as all its individual peices.

		public BlockDestructionChild[,,] ChildBlocks;
		#region ChildBlocks future structure
		//	public BlockDestructionChild[][][] ChildBlocks; // Alot faster but requires everything to be restructured.
		//	int[][][] jagged3d = new int[][][] { // example
		//		new int[][] { new int[] { 111, 112 }, new int[] { 121, 122, 123 } },
		//		new int[][] { new int[] { 211 } }
		//	}
		// .Count [x].Count [x][y].Count
		// string[] serializedLengths; // 4|3/5|2/3|2/3|1/1|... (first value x would always be the same, y and z would change for the array size)
		#endregion

		[HideInInspector]
		public BlockDestructionChild[] SerializedArray; // Flattened array serialized by Unity.

		[HideInInspector]
		public Vector3Int ArraySizes; // serialized array sizes.

		private GameObject SeperatedBlockContainer; // Largest and latest seperated object.

		[NonSerialized]
		public bool HasFullSeperation; // If object has a full seperation.

		public override void Start() {
			RestoreArray(); // Restore array from serialized container.
			ConfirmRegisteredWithHost(); // Although currently does nothing, the returning bool can be used in the future
			LoadActionintoChildren();
		}

		public override void Update() {

		}

		public override GameObject RequestObject(bool all) {
			if (SeperatedBlockContainer == null || all) {
				return gameObject;
			}
			return SeperatedBlockContainer;
		}

		public override void InitiateBreak(Transform parent = null) {
			Destroyed = true; // Only need to be called once

			if (Disintergrate && ChildBlocks != null) {
				for (int x = 0; x < ChildBlocks.GetLength(0); x++) {
					for (int y = 0; y < ChildBlocks.GetLength(1); y++) {
						for (int z = 0; z < ChildBlocks.GetLength(2); z++) {
							if (ChildBlocks[x, y, z] != null) {
								ChildBlocks[x, y, z].InitiateBreak();
							}
						}
					}
				}
			}
			
			if (parent == null) {
				transform.parent = null;

				rigidbodyRef = transform.GetComponent<Rigidbody>();

				if (rigidbodyRef == null) { // Impliment physics if not already applyed
					rigidbodyRef = gameObject.AddComponent<Rigidbody>();
				}

				rigidbodyRef.useGravity = true;
				rigidbodyRef.isKinematic = false;
				rigidbodyRef.sleepThreshold = 0.001f; // How much movement is the minimum before it identifys as being asleep (needs more testing)

				if (DestroyOnSleep) { // whether destruction uses sleep or delay
					StartCoroutine(WaitUntillSleep());
				} else {
					DestroyOnDelay();
				}
			} else {
				transform.parent = parent;
			}
		}

		public override void NotifyDestruction() {
			if (HostContainer != null) {
				HostContainer.NotifyHostOfDestruction(this);
			}
		}

		/// <summary>
		/// Load array into serialized Array.
		/// </summary>
		public void FlattenArray() {
			if (ChildBlocks != null) {
				ArraySizes.x = ChildBlocks.GetLength(0);
				ArraySizes.y = ChildBlocks.GetLength(1);
				ArraySizes.z = ChildBlocks.GetLength(2);

				SerializedArray = ChildBlocks.Cast<BlockDestructionChild>().ToArray();
			}
		}

		/// <summary>
		/// Restore array from serialized Array.
		/// </summary>
		public void RestoreArray() {
			ChildBlocks = new BlockDestructionChild[ArraySizes.x, ArraySizes.y, ArraySizes.z];

			int v = 0;

			for (int x = 0; x < ArraySizes.x; x++) {
				for (int y = 0; y < ArraySizes.y; y++) {
					for (int z = 0; z < ArraySizes.z; z++) {
						ChildBlocks[x, y, z] = SerializedArray[v++];
					}
				}
			}
		}

		/// <summary>
		/// Print array to console for debugging.
		/// </summary>
		public void PrintArrayToConsole() {
			if (ChildBlocks != null) {
				List<BlockDestructionChild> store = ChildBlocks.Cast<BlockDestructionChild>().ToList();
				if (store.Count > 0) {
					for (int x = 0; x < ChildBlocks.GetLength(0); x++) {
						for (int y = 0; y < ChildBlocks.GetLength(1); y++) {
							for (int z = 0; z < ChildBlocks.GetLength(2); z++) {
								string tag = "Pos: X: " + x.ToString() + " Y: " + y.ToString() + " Z: " + z.ToString() + " >> ";
								if (ChildBlocks[x, y, z] == null) {
									Debug.Log(tag + "*NULL*");
								} else {
									if (store.FindAll(e => e == ChildBlocks[x, y, z]).Count > 1) {
										tag += "{Large} ";
									}
									Debug.Log(tag + ChildBlocks[x, y, z].gameObject.name);
								}
							}
						}
					}
				} else {
					Debug.Log(gameObject.name + ": No entries found in the array.");
				}
			}
		}

		/// <summary>
		/// Function loaded into children to initiate the break procedure
		/// </summary>
		/// <param name="child"></param>
		private void CheckForBreak(BlockDestructionChild child) {
			if (!ChildBlocks.Cast<BlockDestructionChild>().ToList().Contains(child)) {
				return;
			}

			int xMax = ChildBlocks.GetLength(0);
			int yMax = ChildBlocks.GetLength(1);
			int zMax = ChildBlocks.GetLength(2);

			for (int x = 0; x < xMax; x++) {
				for (int y = 0; y < yMax; y++) {
					for (int z = 0; z < zMax; z++) {
						if (ChildBlocks[x, y, z] == child) {
							ChildBlocks[x, y, z] = null; // pre nullify the entry so to not be included in finding sperations
						}
					}
				}
			}

			if (ChildBlocks != null && Destructible) {
				bool[,,] checks = new bool[xMax, yMax, zMax];
				for (int x = 0; x < xMax; x++) {
					for (int y = 0; y < yMax; y++) {
						for (int z = 0; z < zMax; z++) {
							if (ChildBlocks[x, y, z] != null && !ChildBlocks[x, y, z].Destroyed) { // find the first valid search point
								ConnectedSearch(ref checks, x, y, z); // populate the checklist
								x = xMax; // end the loops
								y = yMax;
								z = zMax;
							}
						}
					}
				}
				FinishedSearch(checks);
			}
		}

		/// <summary>
		/// Find all blocks connected to the nearest point.
		/// </summary>
		/// <param name="checklist"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		private void ConnectedSearch(ref bool[,,] checklist, int x, int y, int z) {
			if (ChildBlocks[x, y, z] != null && !ChildBlocks[x, y, z].Destroyed) { // Check neighbours for unsearched and valid blocks
				checklist[x, y, z] = true;

				if (x + 1 < ChildBlocks.GetLength(0) && !checklist[x + 1, y, z]) {
					ConnectedSearch(ref checklist, x + 1, y, z);
				}
				if (x - 1 >= 0 && !checklist[x - 1, y, z]) {
					ConnectedSearch(ref checklist, x - 1, y, z);
				}
				if (y + 1 < ChildBlocks.GetLength(1) && !checklist[x, y + 1, z]) {
					ConnectedSearch(ref checklist, x, y + 1, z);
				}
				if (y - 1 >= 0 && !checklist[x, y - 1, z]) {
					ConnectedSearch(ref checklist, x, y - 1, z);
				}
				if (z + 1 < ChildBlocks.GetLength(2) && !checklist[x, y, z + 1]) {
					ConnectedSearch(ref checklist, x, y, z + 1);
				}
				if (z - 1 >= 0 && !checklist[x, y, z - 1]) {
					ConnectedSearch(ref checklist, x, y, z - 1);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="checklist"></param>
		private void FinishedSearch(bool[,,] checklist) {
			int xMax = checklist.GetLength(0);
			int yMax = checklist.GetLength(1);
			int zMax = checklist.GetLength(2);

			List<BlockDestructionChild> Connected = new List<BlockDestructionChild>();
			List<BlockDestructionChild> Seperated = new List<BlockDestructionChild>();
			int EarliestSeperatedSegment = -1;

			for (int x = 0; x < xMax; x++) { // Identify if a layer has serperated
				bool seperated = true;
				for (int y = 0; y < yMax; y++) {
					for (int z = 0; z < zMax; z++) {
						if (checklist[x, y, z]) { // if all x values dont trigger this if statement the area is seperated
							seperated = false;
							z = zMax;
							y = yMax;
						}
					}
				}
				if (seperated) {
					EarliestSeperatedSegment = x;
					HasFullSeperation = true;
					break;
				}
			}

			for (int x = 0; x < xMax; x++) { // Identify which blocks are still connected and need seperating
				for (int y = 0; y < yMax; y++) {
					for (int z = 0; z < zMax; z++) {
						if ((EarliestSeperatedSegment == -1 || x < EarliestSeperatedSegment) && checklist[x, y, z]) {
							Connected.Add(ChildBlocks[x, y, z]);
						} else {
							if (ChildBlocks[x, y, z] != null) {
								Seperated.Add(ChildBlocks[x, y, z]);
							}
						}
					}
				}
			}

			if (Seperated.Count > 0) {
				List<List<BlockDestructionChild>> ToSeperate = RetrieveSeperatedAreas(Seperated);
				int largestValue = ToSeperate.Max(e => e.Count);
				List<BlockDestructionChild> largest = ToSeperate.Find(e => e.Count == largestValue);
				ToSeperate.ForEach(e => ProcessChildBreak(e, e == largest));

				for (int x = 0; x < ChildBlocks.GetLength(0); x++) {
					for (int y = 0; y < ChildBlocks.GetLength(1); y++) {
						for (int z = 0; z < ChildBlocks.GetLength(2); z++) {
							if (Seperated.Contains(ChildBlocks[x, y, z])) {
								ChildBlocks[x, y, z] = null; // Nullify entries to prevent false seperation
							}
						}
					}
				}
			} else if (EarliestSeperatedSegment > -1) {
				ProcessChildBreak(new List<BlockDestructionChild>(), true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SeperatedPositions"></param>
		/// <returns></returns>
		private List<List<BlockDestructionChild>> RetrieveSeperatedAreas(List<BlockDestructionChild> SeperatedPositions) {
			List<List<BlockDestructionChild>> containers = new List<List<BlockDestructionChild>>();
			List<BlockDestructionChild> searchedPositions = new List<BlockDestructionChild>();

			foreach (BlockDestructionChild child in SeperatedPositions) {
				if (!searchedPositions.Contains(child)) {
					List<BlockDestructionChild> seperatedBlocks = new List<BlockDestructionChild>();

					SeperatedSearch(ref seperatedBlocks, FindChildPosition(child), SeperatedPositions);
					containers.Add(seperatedBlocks);

					searchedPositions.AddRange(seperatedBlocks);
				}
			}

			return containers;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newBlock"></param>
		/// <param name="positions"></param>
		/// <param name="seperatedRefs"></param>
		private void SeperatedSearch(ref List<BlockDestructionChild> newBlock, Vector3Int positions, List<BlockDestructionChild> seperatedRefs) {
			if (positions.x == -1 || positions.y == -1 || positions.z == -1) {
				return;
			}
			SeperatedSearch(ref newBlock, positions.x, positions.y, positions.z, seperatedRefs);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newBlock"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="seperatedRefs"></param>
		private void SeperatedSearch(ref List<BlockDestructionChild> newBlock, int x, int y, int z, List<BlockDestructionChild> seperatedRefs) {
			if (!newBlock.Contains(ChildBlocks[x, y, z])) {
				newBlock.Add(ChildBlocks[x, y, z]);

				if (x + 1 < ChildBlocks.GetLength(0) && seperatedRefs.Find(e => e == ChildBlocks[x + 1, y, z]) != null) {
					SeperatedSearch(ref newBlock, x + 1, y, z, seperatedRefs);
				}
				if (x - 1 >= 0 && seperatedRefs.Find(e => e == ChildBlocks[x - 1, y, z]) != null) {
					SeperatedSearch(ref newBlock, x - 1, y, z, seperatedRefs);
				}
				if (y + 1 < ChildBlocks.GetLength(1) && seperatedRefs.Find(e => e == ChildBlocks[x, y + 1, z]) != null) {
					SeperatedSearch(ref newBlock, x, y + 1, z, seperatedRefs);
				}
				if (y - 1 >= 0 && seperatedRefs.Find(e => e == ChildBlocks[x, y - 1, z]) != null) {
					SeperatedSearch(ref newBlock, x, y - 1, z, seperatedRefs);
				}
				if (z + 1 < ChildBlocks.GetLength(2) && seperatedRefs.Find(e => e == ChildBlocks[x, y, z + 1]) != null) {
					SeperatedSearch(ref newBlock, x, y, z + 1, seperatedRefs);
				}
				if (z - 1 >= 0 && seperatedRefs.Find(e => e == ChildBlocks[x, y, z - 1]) != null) {
					SeperatedSearch(ref newBlock, x, y, z - 1, seperatedRefs);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ToBeSeperated"></param>
		/// <param name="mainBreak"></param>
		private void ProcessChildBreak(List<BlockDestructionChild> ToBeSeperated, bool mainBreak) {
			GameObject seperateBlockContainer = new GameObject("Limb Breakaway"); // Take the blocks, create a new object with these blocks in
			seperateBlockContainer.transform.position = transform.position;
			seperateBlockContainer.AddComponent<Rigidbody>();

			ToBeSeperated.ForEach(e => e.transform.parent = seperateBlockContainer.transform);

			if (HostContainer != null && mainBreak) {
				SeperatedBlockContainer = seperateBlockContainer;
				HostContainer.NotifyHostOfDestruction(this);
			}
			Destroy(seperateBlockContainer, DestroyTime);
		}

		/// <summary>
		/// 
		/// </summary>
		private void LoadActionintoChildren() {
			for (int x = 0; x < ChildBlocks.GetLength(0); x++) {
				for (int y = 0; y < ChildBlocks.GetLength(1); y++) {
					for (int z = 0; z < ChildBlocks.GetLength(2); z++) {
						if (ChildBlocks[x, y, z] != null) {
							ChildBlocks[x, y, z].BreakTriggered = CheckForBreak;
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool AttachLoader() {
			if (GetComponent<ManualArrayLoader>() == null) {
				return gameObject.AddComponent<ManualArrayLoader>().FindConnection();
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		private Vector3Int FindChildPosition(BlockDestructionChild child) {
			for (int x = 0; x < ChildBlocks.GetLength(0); x++) {
				for (int y = 0; y < ChildBlocks.GetLength(1); y++) {
					for (int z = 0; z < ChildBlocks.GetLength(2); z++) {
						if (ChildBlocks[x, y, z] == child) {
							return new Vector3Int(x, y, z);
						}
					}
				}
			}
			return new Vector3Int(-1, -1, -1);
		}
	}
}