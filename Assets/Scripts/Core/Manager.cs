using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

	public event System.Action<Chip> customChipCreated;

	public ChipEditor chipEditorPrefab;
	public ChipPackage chipPackagePrefab;
	public Wire wirePrefab;
	public Chip[] builtinChips;

	public ChipEditor activeChipEditor;
	int currentChipCreationIndex;
	static Manager instance;

	public Dictionary<string, Chip> GetBuiltInChips()
    {
		Dictionary<string, Chip> dic = new Dictionary<string, Chip>();
		for(int i = 0;i<builtinChips.Length;i++)
        {
			dic.Add(builtinChips[i].chipName, builtinChips[i]);
        }

		return dic;
    }

	void Awake () {
		instance = this;
		activeChipEditor = FindObjectOfType<ChipEditor> ();
		FindObjectOfType<CreateMenu> ().onChipCreatePressed += SaveAndPackageChip;
	}

	void Start () {
		SaveSystem.Init ();
		SaveSystem.LoadAll (this);
	}

	public static ChipEditor ActiveChipEditor {
		get {
			return instance.activeChipEditor;
		}
	}

	public Chip LoadChip (ChipSaveData loadedChipData) {
		activeChipEditor.LoadFromSaveData (loadedChipData);
		currentChipCreationIndex = activeChipEditor.creationIndex;

		Chip loadedChip = PackageChip ();
		LoadNewEditor ();
		return loadedChip;
	}

	void SaveAndPackageChip () {

		ChipSaver.Save (activeChipEditor);
		if (activeChipEditor.chipEditMode)
			DeletePackedChip();

		PackageChip ();
		LoadNewEditor ();
	}
	
	void DeletePackedChip()
    {
		ChipPackage[] packages = transform.GetComponentsInChildren<ChipPackage>();
		for (int i = 0; i < packages.Length; i++)
		{
			Chip chip = packages[i].GetComponent<Chip>();
			if (chip.chipName == activeChipEditor.chipName)
			{
				Destroy(packages[i].gameObject);
				return;
			}
		}
    }

	Chip PackageChip () {
		ChipPackage package = Instantiate (chipPackagePrefab, parent : transform);
		package.PackageCustomChip (activeChipEditor);
		package.gameObject.SetActive (false);

		Chip customChip = package.GetComponent<Chip> ();
		if(!activeChipEditor.chipEditMode)
			customChipCreated?.Invoke (customChip);
		currentChipCreationIndex++;

		if(!ChipLoader.previouslyLoadedChips.ContainsKey(customChip.chipName))
			ChipLoader.previouslyLoadedChips.Add(customChip.chipName, customChip);
		return customChip;
	}

	public void LoadNewEditor () {
		if (activeChipEditor) {
			Destroy (activeChipEditor.gameObject);
		}
		activeChipEditor = Instantiate (chipEditorPrefab, Vector3.zero, Quaternion.identity);
		activeChipEditor.creationIndex = currentChipCreationIndex;
	}

	public void SpawnChip (Chip chip) {
		activeChipEditor.chipInteraction.SpawnChip (chip);
	}

	public void LoadMainMenu () {
		UnityEngine.SceneManagement.SceneManager.LoadScene (0);
	}

}