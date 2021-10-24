using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MapManager : MonoBehaviour
{
	public Canvas CanvasObject;
	public Character Character;
	public Pin StartPin;
	public TextMeshProUGUI SelectedLevelText;
	public Image SelectedLevelImage;


	public static string lastPinName;	// Very messy way of remembering which pin we were last on (it's possible there are other objects with the same name)
										// A better way would be to have an array of pin references with their index corresponding to which level they represented
										// And then we could simply have the map manager remember the last level number we were on and retrieve the corresponding pin
										// from the array. 
	
	public Pin lastPin;

	ToggleUI toggle;

	void Awake()
	{
		if(!string.IsNullOrEmpty(lastPinName))
		{
			lastPin = GameObject.Find(lastPinName).GetComponent<Pin>();		// Once again, super messy way of finding the right pin, but we don't have a lot of time
		}

		StartPin = lastPin == null? StartPin : lastPin;			// Aaron: We want the map manager to remember where it left off

		toggle = FindObjectOfType<ToggleUI>();
	}

	
	/// <summary>
	/// Use this for initialization
	/// </summary>
	private void Start()
	{
		
		// This needs to be in start because Pin.TryUnlockPin relies on the references of the pins being updated (which happens in Awake())
		// Try to unlock the next pin if the player won
		if(lastPin != null && BasicGameManager.instance.GetVictoryStatus())
		{
			Pin.TryUnlockPin(lastPin.levelNum + 1);
			Pin.FinishPin(lastPin.levelNum);
		}
			


		
		Character.Initialise(this, StartPin);
		//SelectedLevelImage = this.GetComponent<Image>();
	}


	/// <summary>
	/// This runs once a frame
	/// </summary>
	private void Update()
	{
		// Only check input when character is stopped

		if (Character.IsMoving) return;
		
		// First thing to do is try get the player input
		CheckForInput();
	}

	
	/// <summary>
	/// Check if the player has pressed a button
	/// </summary>
	private void CheckForInput()
	{
		// Debug.Log("Checking for inputs");
		if(toggle.isPanelEnabled || BasicGameManager.instance.isReadingDialogue) {return;}    // Indirectly tie inputs to character to the game manager
		// else
		// 	Debug.Log("Character input enabled"); 
		
		if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
		{
			Character.TrySetDirection(Direction.Up);
		}
		else if(Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
		{
			Character.TrySetDirection(Direction.Down);
		}
		else if(Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
		{
			Character.TrySetDirection(Direction.Left);
		}
		else if(Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
		{
			Character.TrySetDirection(Direction.Right);
		}
	}

	
	/// <summary>
	/// Update the GUI text
	/// </summary>
	public void UpdateGui()
	{
		SelectedLevelText.text = string.Format(Character.CurrentPin.SceneToLoad);
		if (Resources.Load<Sprite>("Images/" + Character.CurrentPin.ImageToLoad) != null)
			SelectedLevelImage.sprite = Resources.Load<Sprite>("Images/" + Character.CurrentPin.ImageToLoad);
		else
			Debug.Log("Could not find "+ Character.CurrentPin.ImageToLoad);
	}
}
