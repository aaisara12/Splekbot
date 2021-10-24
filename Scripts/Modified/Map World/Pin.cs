using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class Pin : MonoBehaviour
{
    [Header("Options")] //
    public bool IsAutomatic;
    public bool HideIcon;
    public bool IsLocked;
    public bool IsCompleted;
    public string SceneToLoad;
    public string DialogueToLoad;
    public string ImageToLoad;
    public SpriteRenderer Trail;
    public Sprite IncompletePin;
    public Sprite CompletePin;
    private SpriteRenderer spriteRenderer;

    [Header("Pins")] //
    public Pin UpPin;
    public Pin DownPin;
    public Pin LeftPin;
    public Pin RightPin;

    private Dictionary<Direction, Pin> _pinDirections;



    public int levelNum = 0;
    static PinData[] saveData;
    static bool isSaveInitialized = false;
    static int NUM_PINS = 4;
    public int numWins = 0;
    void Awake()
    {
        // Initialize the pin save data array
        if(!isSaveInitialized)
        {
            saveData = new PinData[4];
            for(int i = 0; i < NUM_PINS; i++)
            {
                saveData[i].isLocked = !(i == 0);
                saveData[i].playedDialogue = false;
            }
            isSaveInitialized = true;
        }

        // Read in data
        PinData data = saveData[levelNum];
        IsLocked = data.isLocked;
        IsCompleted = data.playedDialogue;
        numWins = data.numWins;

        isFinished = data.isFinished;

        saveData[levelNum].sceneRef = this;     // PinData is a struct (so we can't modify data and expect a true change)

    }

    void OnDestroy()
    {
        saveData[levelNum].isLocked = IsLocked;
        saveData[levelNum].playedDialogue = IsCompleted;
        saveData[levelNum].numWins = numWins;
        saveData[levelNum].isFinished = isFinished;
    }

    public static Action<String> OnUnlockedLevel;
    public static bool TryUnlockPin(int pinNum)
    {
        if(pinNum >= saveData.Length) { return false;}
        PinData data = saveData[pinNum];
        if(data.sceneRef != null && data.isLocked)
        {
            data.sceneRef.IsLocked = false;
            //Debug.Log("SUCCESSFUL UNLOCK of PIN " + pinNum);
            OnUnlockedLevel?.Invoke(data.sceneRef.SceneToLoad);
            
            MapManager mm = FindObjectOfType<MapManager>();
            if(mm != null && mm.lastPin.numWins == 0)
                FindObjectOfType<BasicGameManager>().ActivateEndingDialogue();
            mm.lastPin.numWins++;


            return true;
        }
        //Debug.Log("FAILURE TO UNLOCK");
        return false;
    }

    bool isFinished = false;        // I created this new bool to keep track of whether the level has been successfully beaten (IsCompleted now representes whether dialogue has been completed)
    public static void FinishPin(int pinNum)
    {
        PinData data = saveData[pinNum];
        if(data.sceneRef != null)
            data.sceneRef.isFinished = true;
    }
    


    /// <summary>
    /// Use this for initialisation
    /// </summary>
    private void Start()
    {
       
        // Load the directions into a dictionary for easy access
        _pinDirections = new Dictionary<Direction, Pin>
        {
            { Direction.Up, UpPin },
            { Direction.Down, DownPin },
            { Direction.Left, LeftPin },
            { Direction.Right, RightPin }
        };
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite == null)
            spriteRenderer.sprite = IncompletePin;
        if (IsLocked && Trail != null)
        {
            Trail.GetComponent<Renderer>().enabled = false;
            Trail.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isFinished)
            spriteRenderer.sprite = CompletePin;
    }


    /// <summary>
    /// Get the pin in a selected direction
    /// Using a switch statement rather than linq so this can run in the editor
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Pin GetPinInDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return UpPin;
            case Direction.Down:
                return DownPin;
            case Direction.Left:
                return LeftPin;
            case Direction.Right:
                return RightPin;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, null);
        }
    }


    /// <summary>
    /// This gets the first pin thats not the one passed 
    /// </summary>
    /// <param name="pin"></param>
    /// <returns></returns>
    public Pin GetNextPin(Pin pin)
    {
        return _pinDirections.FirstOrDefault(x => x.Value != null && x.Value != pin).Value;
    }

    public void unlockPin() {
        IsLocked = false;
    }


    /// <summary>
    /// Draw lines between connected pins
    /// </summary>
    private void OnDrawGizmos()
    {
        if (UpPin != null) DrawLine(UpPin);
        if (RightPin != null) DrawLine(RightPin);
        if (DownPin != null) DrawLine(DownPin);
        if (LeftPin != null) DrawLine(LeftPin);
    }


    /// <summary>
    /// Draw one pin line
    /// </summary>
    /// <param name="pin"></param>
    protected void DrawLine(Pin pin)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, pin.transform.position);
    }
}




// Used for storing important data about the pin after unloading scene
public struct PinData
{
    public bool isLocked;
    public bool playedDialogue;
    public int numWins;
    public Pin sceneRef;
    public bool isFinished;
}