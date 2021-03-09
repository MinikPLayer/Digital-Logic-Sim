using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ChipLoader {

	public static Dictionary<string, Chip> previouslyLoadedChips;

	public static SavedChip[] GetAllSavedChips(string[] chipPaths)
    {
		SavedChip[] savedChips = new SavedChip[chipPaths.Length];

		// Read saved chips from file
		for (int i = 0; i < chipPaths.Length; i++)
		{
			savedChips[i] = GetSavedChip(chipPaths[i]);
		}
		return savedChips;
	}

	public static SavedChip GetSavedChip(string chipPath)
    {
		using (StreamReader reader = new StreamReader(chipPath))
		{
			string chipSaveString = reader.ReadToEnd();
			return JsonUtility.FromJson<SavedChip>(chipSaveString);
		}
	}

	public static void LoadAllChips (string[] chipPaths, Manager manager) 
	{
		SavedChip[] savedChips = GetAllSavedChips(chipPaths);

		SortChipsByOrderOfCreation (ref savedChips);
		// Maintain dictionary of loaded chips (initially just the built-in chips)
		previouslyLoadedChips = manager.GetBuiltInChips(); // reset
		/*Dictionary<string, Chip> loadedChips = manager.GetBuiltInChips();/*new Dictionary<string, Chip> ();
		for (int i = 0; i < manager.builtinChips.Length; i++) {
			Chip builtinChip = manager.builtinChips[i];
			loadedChips.Add (builtinChip.chipName, builtinChip);
		}*/

		for (int i = 0; i < savedChips.Length; i++) {
			SavedChip chipToTryLoad = savedChips[i];
			ChipSaveData loadedChipData = LoadChip (chipToTryLoad, manager, manager.wirePrefab);
			Chip loadedChip = manager.LoadChip (loadedChipData);
			//previouslyLoadedChips.Add (loadedChip.chipName, loadedChip);
		}
	}

	// Instantiates all components that make up the given clip, and connects them up with wires
	// The components are parented under a single "holder" object, which is returned from the function
	public static ChipSaveData LoadChip (SavedChip chipToLoad, Manager manager, Wire wirePrefab) {
		ChipSaveData loadedChipData = new ChipSaveData ();
		int numComponents = chipToLoad.savedComponentChips.Length;
		loadedChipData.componentChips = new Chip[numComponents];
		loadedChipData.chipName = chipToLoad.name;
		loadedChipData.chipColour = chipToLoad.colour;
		loadedChipData.chipNameColour = chipToLoad.nameColour;
		loadedChipData.creationIndex = chipToLoad.creationIndex;

		// Spawn component chips (the chips used to create this chip)
		// These will have been loaded already, and stored in the previouslyLoadedChips dictionary
		for (int i = 0; i < numComponents; i++) {
			SavedComponentChip componentToLoad = chipToLoad.savedComponentChips[i];
			string componentName = componentToLoad.chipName;
			Vector2 pos = new Vector2 ((float) componentToLoad.posX, (float) componentToLoad.posY);

			if (!previouslyLoadedChips.ContainsKey (componentName)) {
				Debug.LogError ("Cannot find sub component: " + componentName + " While loading " + chipToLoad.name);
				/*if(chipToLoad.name == componentName)
                {
					Debug.LogError("Cannot load objects recursively!");
					continue;
                }
				
				Chip c = manager.LoadChip(LoadChip(GetSavedChip(SaveSystem.GetPathToSaveFile(componentName)), ref previouslyLoadedChips, manager, wirePrefab));
				previouslyLoadedChips.Add(c.chipName, c);*/
				continue;
			}

			Chip loadedComponentChip = GameObject.Instantiate (previouslyLoadedChips[componentName], pos, Quaternion.identity);
			loadedChipData.componentChips[i] = loadedComponentChip;

			// Load input pin names
			for (int inputIndex = 0; inputIndex < componentToLoad.inputPins.Length; inputIndex++) {
				loadedChipData.componentChips[i].inputPins[inputIndex].pinName = componentToLoad.inputPins[inputIndex].name;
			}

			// Load output pin names
			for (int ouputIndex = 0; ouputIndex < componentToLoad.outputPinNames.Length; ouputIndex++) {
				loadedChipData.componentChips[i].outputPins[ouputIndex].pinName = componentToLoad.outputPinNames[ouputIndex];
			}
		}

		List<Wire> wires = new List<Wire>();
		// Connect pins with wires
		for (int chipIndex = 0; chipIndex < chipToLoad.savedComponentChips.Length; chipIndex++) {
			Chip loadedComponentChip = loadedChipData.componentChips[chipIndex];
			for (int inputPinIndex = 0; inputPinIndex < loadedComponentChip.inputPins.Length; inputPinIndex++) {
				SavedInputPin savedPin = chipToLoad.savedComponentChips[chipIndex].inputPins[inputPinIndex];
				Pin pin = loadedComponentChip.inputPins[inputPinIndex];

				// If this pin should receive input from somewhere, then wire it up to that pin
				if (savedPin.parentChipIndex != -1) {
					Pin connectedPin = loadedChipData.componentChips[savedPin.parentChipIndex].outputPins[savedPin.parentChipOutputIndex];
					pin.cyclic = savedPin.isCylic;
					//Pin.TryConnect (connectedPin, pin);
					if (Pin.TryConnect (connectedPin, pin)) {
						Wire loadedWire = GameObject.Instantiate (wirePrefab, loadedComponentChip.gameObject.transform);
						loadedWire.Connect (connectedPin, loadedComponentChip.inputPins[inputPinIndex]);

						wires.Add(loadedWire);
					}
					
				}
			}
		}

		loadedChipData.wires = wires.ToArray();
		return loadedChipData;
	}

	public static SavedWireLayout LoadWiringFile (string path) {
		using (StreamReader reader = new StreamReader (path)) {
			string wiringSaveString = reader.ReadToEnd ();
			return JsonUtility.FromJson<SavedWireLayout> (wiringSaveString);
		}
	}

	static void SortChipsByOrderOfCreation (ref SavedChip[] chips) {
		var sortedChips = new List<SavedChip> (chips);
		sortedChips.Sort ((a, b) => a.creationIndex.CompareTo (b.creationIndex));
		chips = sortedChips.ToArray ();
	}

}