***************************************************
* The Destruction Tree Manager					  *
* By Duncan 'ScoredOne' Mellor                    *
* @ScoredOne / ScoredOne1994@gmail.com            *
***************************************************

-- Glossery --

1. Introduction
2. Example Prefabs
3. Scripts
4. Loaders
5. Saving Prefabs

-- Introduction --

	Hello and thank you for downloading the Destruction Tree Manager, this readme is the guide on how to use it and also everything included in this package.
	This package is for the script to quickly and easily link objects together to allow for connected destruction of objects.

-- Example Prefabs --

	A scene named 'Basic Example' is included with all example prefabs included in the scene ready to test and play with to learn how the code works.

	 - BodyTemplate; A humanoid character structure built to accomidate Unity animations.
	 The builder for these characters here : http://u3d.as/16ZX
	 I currently dont have anything to auto gen the destruction code into these structures as of yet, it was made manually.

	Test Structures:
	- Basic tree structure; A basic Host structure built from the default cylinder, demonstrates a basic strucutre with branches.

	- Large Block Test 1; A basic Parent structure, built 6 x 2 x 2 and also demonstrates Large objects in the structure.

	- Large Block Test 2; A smaller Parent structure example, built 2 x 3 x 1 and demonstrates Large objects with some null spaces.

	- Test Bridge; A more complicated structure showing how all the features of these scripts together and how they interact with eachother.

	- Test Structure; A basic Host structure built to test connecting the scripts into the Host structure and how they interact.

	- Test Structure 2; A basic Host structure built to test 2 branches both connecting to the same nodes.

-- Scripts --

	4 scripts are for use inside the example scene, these are placed in the Other folder.

	There are 4 main scripts, once set up of prefabs is complete only these scripts are needed and all others can be removed, these are:
	- BlockDestructionBase
	- BlockDestructionHost
	- BlockDestructionChild
	- BlockDestructionParent

	For structure;
	BlockDestructionBase is the abstract class for BlockDestructionChild and BlockDestructionParent. BlockDestructionHost stands on its own.
	BlockDestructionHost manages trees comprised of classes using BlockDestructionBase. Custom classes using BlockDestructionBase with work within BlockDestructionHost.
	BlockDestructionParent manages a 3D array comprised of BlockDestructionChild classes.

	How they work;
	BlockDestructionHost:
	The host works by creating strings which act as branches possessing the IDs created in BlockDestructionBase.
	The structure of the strings is that the first ID is the parent of branch, additional entries seperated by the character '|' are the children of the branch.
	When a BlockDestructionBase notifies that is has been destroyed. The class finds the branch of which the object notifying is the parent of and proceeds to 
	process the destruction inside all children of the branch and subsequent branches of the children.
	The host stores references all objects in the trees, allowing for fast execution.
	The host possesses some protections and warnings such as an infinite loop identifyer and multiple parent detection.
	For the host to run it will make sure it is valid at game start, if it is invalid it will not run.

	BlockDestructionParent:
	The parent works by managing a 3D array of BlockDestructionChild classes which it saves inside a serializable array.
	It initiates a break when it finds all of the entries in any layer of the first dimension (x) e.g a break in all entries of [3, y, z] would trigger a break including all 
	entries of x > 3.
	Also although it doesnt trigger a break, it will seperate groupes of entries that dont connect to the earliest connected entries.
	It does this by searching neighbouring entries (via +/-1 x, y, z).

-- Loaders --

	BlockDestructionHost and BlockDestructionParent require loaders to insert usable data to operate, once data is inserted the loaders can be removed.
	Loaders are able display the information inside the classes in a readable format with links and references.

	BlockDestructionHost:
	Fields:
	- Connection; Script this loader is attached to.
	- Parent; Block you wish to add to the tree / make parent of child.
	- Child; Block you wish to add to a parent Block.
	- Found Branches; Printed structure of the Host listed in readable string format.
	- Found References; Links to the GameObjects used in the structure.

	Buttons:
	- Connect; Finds the first Host Object within this GameObjects and loads it into Connection. (Hidden if populated / Hides all other buttons if not populated)
	- Add*; Adds the Parent Object to the tree / Adds the Parent and Child to the tree and assignes the Child to the Parents Branch. (Hidden when Parent isnt populated)
	- Remove*; Removes the Parent and its branch from the tree / Removes the Child from the Parents Branch. (Hidden when Parent isnt populated)
	- Refresh References; Reloads the Found Branches and Found References Arrays.
	- Print to Console; Prints Found Branches into the Unity Console.
	* Duel functionality, does an action with only the Parent populated, performs another action with Parent and Child populated.

	Notes:
	This loader is directly connected to the Host containers array and as such will be saved when Unity serializes the data.
	If you wish to make test adjustments without losing the original. Save the object as a prefab.

	BlockDestructionParent:
	Fields:
	- Connection; Script this loader is attached to.
	- To Connect; Child Block to be added into the loader.
	- Position X; X co-ordinate of where the Child Block is to be placed.
	- Position Y; Y co-ordinate of where the Child Block is to be placed.
	- Position Z; Z co-ordinate of where the Child Block is to be placed.
	- Notes List; Printed structure of the Loader array in readable string format.
	- Reference List; Links to the GameObjects used in the structure.

	Buttons:
	- Connect; Finds the first Parent Object within this GameObjects and loads it into Connection. (Hidden if populated / Hides all other buttons if not populated)
	- Add/Replace*; Adds the Child (To Connect) to the array in the position entered in Position X, Position Y and Position Z. (Hidden when To Connect is not populated)
	- Remove; Removes the Child (To Connect) from the position identifed in the array in the position entered in Position X, Position Y and Position Z. (Hidden when To Connect is not populated)
	- Refresh Notes; Reloads the Notes List and Reference List.
	- Print Log to Console; Prints Notes List into the Unity Console.
	- Save and Apply; Replaces the data in the Parent with the data inside the Loader.
	- Reset; Clears the loaders data to be empty.
	* This structure supports "Large Objects". If you add the same object to an adjacent position it will enter the object into that position without removing the original.
	  Objects with multiple positions will be labled "{Large}". If you attempt to add a large object to a NON-ADJACENT position, ALL previous entries will be removed, please
	  excess caution when making large entries.

	Notes:
	This loader is NOT directly connected to the Parent container. For changes to be set they must both be set by pressing save and then saved when Unity serializes data.
	As such like the Host data, although you can make adjustments prior to saving the data unlike the Host loader, save the object as a prefab to prevent losing data.
	
	If after saving the structure appeares to have changes (more specificaly shrank), when saving the array it implements a function to shrink the array container to its
	smallest bounds, the structure is preserved and removable null entries are taken out.
	e.g Y = 0, Z = 0.
	X = 0 => null;						X = 0 => object 1;
	X = 1 => object 1;					X = 1 => null;
	X = 2 => null;		Turns into:		X = 2 => object 2;
	X = 3 => object 2;		
	X = 4 => null;			
	(This example would not be a valid structure as the 2 objects are not connected, simply demonstrates the sinking functionality)

-- Saving Prefabs --

	Once the data has been saved into the classes containers, the object can be saved as a prefab by simply dragging the object into the inspector.
	Do NOT edit the data when it has been stored as a prefab as it will use the original serialized prefab data at runtime and overwrite it.
	If you wish to edit a prefabs data, add it to the scene and break the prefab. This will allow you edit the data and re-save it.
