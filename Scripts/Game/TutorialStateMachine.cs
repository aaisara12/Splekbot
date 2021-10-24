using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TutorialStateMachine : MonoBehaviour
{

    StateMachine machine;

    [SerializeField] bool isInitialized = false;
    [SerializeField] bool last_obj_finished = false;

    [SerializeField] float finishObjectiveDelay = 0.5f;
    
    
    bool isDialogueFinished = false;
    bool[] hasMoved = {false, false, false, false};
    bool hasMadeSwing = false;
    bool hasMadeGoal = false;

    private AudioManager audioManager;

    [SerializeField] UnityEngine.UI.Text objectiveText;     // Maybe this should have been implemented with events? idk

    DialogueTrigger dialogueEngine;


    // PLAYER REFERENCES
    GameObject Player;
    

    [SerializeField] int objectivesCompleted = -1;   // We initialize it like this to account for the extra objective increment due to first dialogue finish


    // TODO:
    // Create event listeners for each of the objectives the player needs to complete
    // Have each event listener raise the corresponding objective flag
    // For some objectives, maybe we can just constantly check things (such as having the player move from point A to point B and saving the initial location)
    // Make sure to write a new dialogue_Tutorial file for each new objective
    // IMPORTANT:  Make sure to have the game manager call StartTutorial() when the user fades into the tutorial scene (in HandleEnterLevel())
    // IMPORTANT:  Have a check to see if all objectives have been completed, and if so call BasicGameManager.instance.ReturnToLevelSelect() to automatically close the level

    // REMEMBER:  Call MatchController.Victory() when the tutorial has finished


    void Awake()
    {
        machine = new StateMachine();


        var inactiveState = new InactiveState();
        var DialogueState = new DialogueState();
        var objTransitionState = new ObjectiveTransition();

        var move_obj = new Move_Obj("Move FORWARDS, BACKWARDS, LEFT, and RIGHT", objectiveText);
        var sprint_obj = new Sprint_Obj("Activate sprint", objectiveText);
        var swing_obj = new Swing_Obj("Charge and swing your racket", objectiveText);
        var hit_obj = new Hit_Obj("Hit the projectile into the goal", objectiveText);




        Func<bool> HasStarted() => () => isInitialized;


        Func<bool> DialogueFinished() => () => isDialogueFinished;

        Func<bool> ObjectiveWaitFinished() => () => ((Time.time - lastObjectiveFinishedTime) > finishObjectiveDelay);



        // Objectives
        Func<bool> move_obj_done() => () => hasMoved[0] && hasMoved[1] && hasMoved[2] && hasMoved[3];
        Func<bool> sprint_obj_done() => () => cm.isSprinting;
        Func<bool> swing_obj_done() => () => hasMadeSwing;
        Func<bool> hit_obj_done() => () => hasMadeGoal;





        Func<bool> HasReset() => () => !isInitialized; // This will keep being updated with what the final objective is


        // TODO: Change all objective transitions to transition to ObjectiveCompleted state
        // TODO: Make transition from ObjectiveCompleted to Dialogue state

        // Transitions

        
        machine.AddTransition(inactiveState, DialogueState, HasStarted());
        machine.AddTransition(objTransitionState, DialogueState, ObjectiveWaitFinished());


        // Objective 1: Movement
        machine.AddTransition(DialogueState, move_obj, () => (DialogueFinished().Invoke() && objectivesCompleted == 0));
        machine.AddTransition(move_obj, objTransitionState, move_obj_done());

        // Objective 2: Sprinting
        machine.AddTransition(DialogueState, sprint_obj, () => (DialogueFinished().Invoke() && objectivesCompleted == 1));
        machine.AddTransition(sprint_obj, objTransitionState, sprint_obj_done());

        // Objective 3:  Swinging
        machine.AddTransition(DialogueState, swing_obj, () => (DialogueFinished().Invoke() && objectivesCompleted == 2));
        machine.AddTransition(swing_obj, objTransitionState, swing_obj_done());

        // Objective 4:  Hitting
        machine.AddTransition(DialogueState, hit_obj, () => (DialogueFinished().Invoke() && objectivesCompleted == 3));
        machine.AddTransition(hit_obj, objTransitionState, hit_obj_done());

        machine.AddTransition(DialogueState, inactiveState, () => objectivesCompleted == 4);



        machine.AddAnyTransition(inactiveState, HasReset());        // The tutorial should be able to end at any step of the way

        machine.SetState(inactiveState);




        dialogueEngine = FindObjectOfType<DialogueTrigger>();
        if(dialogueEngine != null)
            dialogueEngine.onDialogueFinished += HandleFinishDialogue;
        
        

    }

    CharacterController cc;
    CharacterMovement cm;
    CharacterSwing cs;
    matchController mc;

    bool hasEnded = false;
    void Update()
    {
        machine.Tick();

        if(isInitialized && cc != null)      // Very messy way of checking whether the player has moved
        {
            if(cc.velocity.x > 0.5f)
                hasMoved[0] = true;
            if(cc.velocity.x < -0.5f)
                hasMoved[1] = true;
            if(cc.velocity.z > 0.5f)
                hasMoved[2] = true;
            if(cc.velocity.z < -0.5f)
                hasMoved[3] = true;
        }

        if(objectivesCompleted == 4 && !hasEnded)
        {
            hasEnded = true;
            EndTutorial();
            matchController mc = FindObjectOfType<matchController>();
            if(mc != null)
                mc.Victory();
        }
            
        
        
    }




    public void StartTutorial()
    {
        isInTutorial = true;
        //EndTutorial();      // Call the reset in case it wasn't called before

        objectivesCompleted = -1;
        Player = FindObjectOfType<UserInput>().gameObject;
        cc = Player.GetComponent<CharacterController>();
        cm = Player.GetComponent<CharacterMovement>();
        cs = Player.GetComponent<CharacterSwing>();
        mc = FindObjectOfType<matchController>();

        cs.OnFinishSwing += HandleSwingFinish;

        isInitialized = true;

        Objective.OnFinishObjective += HandleObjectiveFinished;
        mc.OnTutorialGoalScored += HandleGoalScored;
    }

    public bool isInTutorial = false;
    // Clean up any tutorial junk
    public void EndTutorial()
    {
        isInTutorial = false;
        isInitialized = false;
        last_obj_finished = false;
        isDialogueFinished = false;
        hasMadeSwing = false;
        hasMadeGoal = false;

        objectiveText.text = "";


        for(int i = 0; i < hasMoved.Length; i++)
            hasMoved[i] = false;
        if(cs != null)
            cs.OnFinishSwing -= HandleSwingFinish;
        
        Objective.OnFinishObjective -= HandleObjectiveFinished;
        if(mc != null)
            mc.OnTutorialGoalScored -= HandleGoalScored;
        
    }




    // Gameplay event handlers
    void HandleSwingFinish()
    {
        if(objectivesCompleted == 2)
            hasMadeSwing = true;
    }

    void HandleFinishDialogue()     // This is invoked when the dialogue finishes
    {
        if(!isDialogueFinished)
            StartCoroutine(FlickDialogueFinished());

        objectivesCompleted++;  // Very messy/lazy way of incrementing number of objectives completed (the player should finish dialogue after every objective)
    }
    void HandleGoalScored()
    {
        hasMadeGoal = true;
    }

    float lastObjectiveFinishedTime = 0;
    void HandleObjectiveFinished()
    {
        lastObjectiveFinishedTime = Time.time;
        //audioManager.PlaySound("win");
        AudioManager.instance.PlaySoundAtLocation("win", transform.position);
    }





    IEnumerator FlickDialogueFinished()
    {
        isDialogueFinished = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        isDialogueFinished = false;
    }

}