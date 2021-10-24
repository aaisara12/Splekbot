using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

///////////////////////////
/*
    WHAT THIS CLASS IS INTENDED TO DO:
    1. Tell dialogue system when to play which dialogue
    2. Transition game from map world to level world
*/
///////////////////////////


// TODO!!:   Remember to reset hasWon to false when you go to START SCREEN!!

public class BasicGameManager : MonoBehaviour
{
    DialogueTrigger dialogueEngine;
    public bool isReadingDialogue = false;     // Is the user in the middle of reading dialogue?  This should be used by the state machine to determine whether the scene is safe to be changed
    levelInfo currentLevelInfo;         // Data package containing info about the current level (if in the map scene, then it's the level that should be loaded next)
    [SerializeField] string levelSelectSceneName;


    Pin currentPin;     // Remember which pin the user was at when they last left


    public static BasicGameManager instance 
    { 
        get { return _instance;}
    }

    private static BasicGameManager _instance;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);


        // Singleton initialization
        if(_instance != null)
            Destroy(this.gameObject);
        _instance = this;           


        dialogueEngine = GetComponent<DialogueTrigger>();

        if(dialogueEngine == null)
            Debug.LogError("The game manager could not find a DialogueTrigger component on this gameobject");
        
        if(levelSelectSceneName == "")
            Debug.LogWarning("No scene name has been specified as the level select scene!");


        // KEONA CODE (scene transitions and escape panel)
        if(transitionAnimator == null)
            Debug.LogError("The game manager is missing a transition animator component!");

        if(worldEscapePanel == null)
        {
            Debug.LogError("An escape panel reference is missing!");
            return;
        }
 
        worldEscapePanel.SetActive(false);
        inputsFrozen = false;

        matchController.OnUserWin += HandleVictory; // Doesn't unsubscribe anywhere yet
    }
    
    public void ActivateLevelNode(levelInfo info)    // This will be called by Charles's world map scripts when he presses ENTER on a level node
    {
        currentLevelInfo = info;
        
        dialogueEngine.retrieveDialogueText(info.dialoguePrefix);        // Load the current dialogue into dialogue engine

        StartUpDialogue("first_situation");

    }

    // TODO: This should only be called if the current pin's win count is > 0 (we can get a reference to this pin with currentPin (and checking its win count)
    public void ActivateEndingDialogue()
    {
        if(FindObjectOfType<MapManager>().lastPin.levelNum == 0)
            FindObjectOfType<DialogueTrigger>().retrieveDialogueText("Tutorial");

        //Debug.Log("Activating Second Situation");
        StartUpDialogue("second_situation");
    }


    public void StartLevel()    // This should be called by a button press
    {
        hasWon = false;     // We must reset this value when we leave map select
        StartCoroutine(LoadSceneWithTransition("Level_" + currentLevelInfo.levelName));
    }

    void StartUpDialogue(string situation)  // Brings up the dialogue window with the appropriate text for the situation
    {
        SoftFreezeInputs();
        //Debug.Log("Starting up dialogue");
        dialogueEngine.loadDialogue(situation);        // Load first part of dialogue

        isReadingDialogue = true;

        dialogueEngine.onDialogueFinished += HandleFinishDialogue;     // OnFinishDialogue is not yet implemented
    }

    // Called after scene transitions are over
    void HandleEnterLevel()
    {
        worldEscapePanel.SetActive(false);          // Make sure all panels are closed before heading into a new scene
        if(SceneManager.GetActiveScene().name.EndsWith("Tutorial"))
            FindObjectOfType<TutorialStateMachine>().StartTutorial();
    }



    public enum MatchType
    {
        regular,
        tutorial,
        boss
    }

    public MatchType GetMatchType()
    {
        // This may need to be modified to work with boss level
        return SceneManager.GetActiveScene().name.EndsWith("Level_Tutorial")? MatchType.tutorial : MatchType.regular;
    }


    public bool hasWon = false;
    void HandleVictory()
    {
        // TODO: If win count is == 0, then play ending dialogue
        // TODO: increment win count for the current pin

        hasWon = true;
    }

    public bool GetVictoryStatus()
    {
        return hasWon;
    }


    bool shouldDisplayLevelPageAfterDialogue = false;   // Very messy way of letting me trigger the dialogue panel after finishing dialogue 
    void HandleFinishDialogue()
    {
        isReadingDialogue = false;  

        UnfreezeInputs();
        //Debug.Log("GM Detected end of dialogue");

        ToggleUI ui_manager = FindObjectOfType<ToggleUI>();
        if(ui_manager != null && !FindObjectOfType<BasicGameManager>().hasWon)  // Super jank
            ui_manager.EnableLevelPanel();

        dialogueEngine.onDialogueFinished -= HandleFinishDialogue;
    }  


    public void Quit(){
        Application.Quit(0);
    }

    public void ReturnToLevelSelect()
    {
        TutorialStateMachine ts = FindObjectOfType<TutorialStateMachine>();
        if(ts != null && ts.isInTutorial == true)
            ts.EndTutorial();
    

        StartCoroutine(LoadSceneWithTransition(levelSelectSceneName));
        tutorialDialogueNum = 0;        // Resets the dialogue number count for tutorial level (really the tutorial module should be handling this)
        UnfreezeInputs();
    }




    public void GoToIntro()
    {
        StartCoroutine(LoadSceneWithTransition("Intro"));
    }
    void PlayIntro()
    {
        dialogueEngine.retrieveDialogueText("tutorial1");
        StartUpDialogue("intro");
        dialogueEngine.onDialogueFinished += HandleFinishIntro;
    }
    void HandleFinishIntro()
    {
        dialogueEngine.onDialogueFinished -= HandleFinishIntro;
        ReturnToLevelSelect();
    }


    // The GM basically serves as memory for the map manager... this feels wrong 
    // public Pin GetCurrentPin() => currentPin;
    // public void SetCurrentPin(Pin pin)
    // {
    //     currentPin = pin;
    // }



    //////////////////////////////
    // LEVEL LOADING (KEONA)
    //////////////////////////////

    [Header("Scene Transition")]
    [SerializeField] Animator transitionAnimator;
    [SerializeField] float sceneTransitionTime = 1f;
    [Header("Escape Panel")]
    [SerializeField] GameObject worldEscapePanel;
    int levelIndex = 0;


    IEnumerator LoadSceneWithTransition(string sceneName)
    {
        //Debug.Log("Attempting to load scene " + sceneName + " with transition.");
        //Play animation
        transitionAnimator.SetTrigger("Start");

        //wait
        yield return new WaitForSeconds(sceneTransitionTime);

        transitionAnimator.ResetTrigger("Start");
    
        //Load scene
        SceneManager.LoadScene(sceneName);      // This really should be asynchronous, but I'm too lazy to make an event handler

        UnfreezeInputs();

        transitionAnimator.SetTrigger("Back");

        yield return new WaitForSeconds(sceneTransitionTime);

        transitionAnimator.ResetTrigger("Back");

        if(sceneName.StartsWith("Level_"))
            HandleEnterLevel();
        
        if(sceneName.StartsWith("Intro"))
            PlayIntro();
        
    }


    /////////////////////////////////
    // Input
    ////////////////////////////////

    public bool inputsFrozen {get; private set;}
    void Update()
    {
        if(Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Mouse0) && SceneManager.GetActiveScene().name.EndsWith("Start"))
            GoToIntro();
        // KEONA CODE (escape panel)
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            string current = SceneManager.GetActiveScene().name;

            if(current == levelSelectSceneName)
            {
                if(worldEscapePanel.gameObject.activeSelf)
                    UnfreezeInputs();
                else
                    FreezeInputs();
                
                worldEscapePanel.gameObject.SetActive(!worldEscapePanel.gameObject.activeSelf);
            }
            
        }

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(isReadingDialogue)
                dialogueEngine.continueDialogue();
        }

        if(inputsFrozen) {return;}

        if(Input.GetKeyDown(KeyCode.Return))
        {
            
            TryOpenLevel();
            
        }
 

        
            

        

        // if(Input.GetKeyDown(KeyCode.T))
        //     GetComponent<TutorialStateMachine>().StartTutorial();
        // if(Input.GetKeyDown(KeyCode.F))
        //     GetComponent<TutorialStateMachine>().EndTutorial();

    }

    public void FreezeInputs()
    {
        Time.timeScale = 0;
        inputsFrozen = true;
        //Debug.Log("Freezing inputs");
    }

    public void SoftFreezeInputs()
    {
        inputsFrozen = true;
    }
    public void UnfreezeInputs()
    {
        Time.timeScale = 1;
        if(!isWaitingUnfreeze)
            StartCoroutine(DelayUnfreeze());    // This is still susceptible to people spamming the escape menu and making it unfreeze while still in menu
    }
    bool isWaitingUnfreeze = false;
    IEnumerator DelayUnfreeze()
    {
        isWaitingUnfreeze = true;
        yield return new WaitForSeconds(0.2f);
        inputsFrozen = false;
        isWaitingUnfreeze = false;
    }


    void TryOpenLevel()
    {
        ToggleUI ui_manager = FindObjectOfType<ToggleUI>();

        if(ui_manager == null)
        {
            //Debug.LogError("ToggleUI reference not found!");
            return;
        }

        Pin currentPin = null;

        if(ui_manager.TryGetCurrentPin(ref currentPin))
        {
            //Debug.Log("Detected enter");
            levelInfo info = new levelInfo();
            info.levelName = currentPin.SceneToLoad;
            info.dialoguePrefix = currentPin.DialogueToLoad;

            currentLevelInfo = info;
            
            Character c = FindObjectOfType<Character>();
            if(c != null)
                c.DisableBlinking();

                
            //Debug.Log("Is current pin still null? " + (currentPin == null));
            if(!currentPin.IsCompleted)
            {
                // TODO: Make a levelInfo object to pass into activateLevelNode
                // TODO: Remember to call ToggleUI's EnableLevelCanvas method when the dialogue finishes
                
                                
                ActivateLevelNode(info);    // This is called to start the dialogue
                currentPin.IsCompleted = true;
                FindObjectOfType<Character>().UpdateCompletion();
            }
            else
                ui_manager.EnableLevelPanel();      // In the case that we've already seen the dialogue, enable the panel
        }
    }   




    int tutorialDialogueNum = 0;
    public void PlayNextTutorialDialogue()
    {
        dialogueEngine.retrieveDialogueText("Tutorial_" + tutorialDialogueNum);
        StartUpDialogue("first_situation");
        tutorialDialogueNum++;
        
    }

    [SerializeField] GameObject CreditsPanel; //credits panel button toggle

    //open panel function
    public void openCredits()
    {
        if (CreditsPanel != null)
        {
            bool isActive = CreditsPanel.activeSelf;

            CreditsPanel.SetActive(!isActive); //if active set unactive, if unactive set active
        }
    }

}

[System.Serializable]
public struct levelInfo
{
    public string levelName;
    public string dialoguePrefix;
}










