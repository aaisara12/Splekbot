using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;
public class matchController : MonoBehaviour
{


    [Header ("Points")]
    public int playTo;
    public int playerPoints;
    public int AIPoints;

    [Header ("Starting Locations")]
    public Transform playerStart;
    public Transform AIStart;

    //Manage The Court Position
    private CourtController court;

    [Header ("Serving")]
    public float serveSpeed = 5;
    public float serveHeight = 5;


    [Header ("Prefabs")]
    public Projectile globalProjectile;


    //Player and AI References
    [Header ("Refs")]
    public GameObject ai;
    public GameObject player;
    private string playerObjectName;
    private string aiObjectName;

    //Animation stuff
    private bool gameActive=false;
    private Animator animator;


    [Header ("UI - Intro Animation")]
    public TextMeshProUGUI enemyScoreLabel;
    public Image enemyPortrait;
    public TextMeshProUGUI playerScoreLabel;
    [Header ("UI - Victory Screen")]
    public TextMeshProUGUI endScreenLabel;



    //tutorial mode
    private bool tutorialModeOn=false;
    //pause
    private bool isPaused=false;

    void Awake()
    {
        playerObjectName = player.gameObject.name;
        aiObjectName = ai.gameObject.name;


        // I added this here so that the match controller can get the correct match type when it's ready (bc the game manager will be ready first)
        if(BasicGameManager.instance != null)
        {
            BasicGameManager.MatchType matchType = BasicGameManager.instance.GetMatchType();
            if(matchType == BasicGameManager.MatchType.regular)
                setTutorialModeOn(false);
            else
                setTutorialModeOn(true);
        }

        
        // AARON (this is for tutorial)
        hasBallDespawned = false;
    }



    // NOTE:  I moved the 
    void Start(){

        // NOTE:  Moved object name assignments to Awake() so that other Start() calls have guaranteed access
        
        Cursor.lockState=CursorLockMode.Locked;

        animator=this.GetComponent<Animator>();


        //if we aren't in the tutorial, play match starch
        if(!tutorialModeOn){
            Time.timeScale=0f;
            animator.SetTrigger("Match Start");
        }

        resetScore();  
        court = GetComponent<CourtController>();
 
        enemyPortrait.sprite = ai.GetComponent<SpriteRenderer>().sprite;

        //connect event
        globalProjectile.OnProjectileScored+=pointScored;



        //if we are in tutorial mode
        if(tutorialModeOn){
            //set the AI off
            ai.gameObject.SetActive(false);
            resetPositions();
            //spawnBall();
        }


    }


    public void startMatch(){
        Time.timeScale=1f;
        spawnBall();
        gameActive=true;
    }

    void Update(){
 
        if(Input.GetKeyDown(KeyCode.Escape)){
            pause();
        }

        // Instant win button for testing dialogue quickly
        if(Input.GetKeyDown(KeyCode.Alpha0))
            Victory();
    }

    // NOTE: Using TutorialMode() instead
    public void setTutorialModeOn(bool t){
        tutorialModeOn=t;
        ai.gameObject.SetActive(!t);
    }

    private void resetScore(){
        playerPoints=0;
        AIPoints=0;
        updateUI();
    }
    private void updateUI(){
        enemyScoreLabel.text=AIPoints+"";
        playerScoreLabel.text=playerPoints+"";
    }


    // AARON (for tutorial mode to know state of ball)
    public bool hasBallDespawned {get; private set;}    // Assumptions:  MatchController knows when projectile is in play or not
    public event System.Action OnTutorialGoalScored;

    public void pointScored(string name){
        //if the tutorial mode is on, just give us a new point
        if(tutorialModeOn){
            globalProjectile.resetBounceCount();
            hasBallDespawned = true;
            if(name == playerName())
                OnTutorialGoalScored?.Invoke();
            newPoint();
            return;
        }

        bool lastHitByPlayer = name==playerObjectName;
        if(lastHitByPlayer)
            playerPoints++;
        else
            AIPoints++;
            gameActive=false;
        globalProjectile.resetBounceCount();
        updateUI();
        animator.SetTrigger("Point");
        Time.timeScale=0.25f;

    }

    public string playerName(){

        return playerObjectName;
    }
    public string AIName(){
        return aiObjectName;
    }

    public void newPoint(){

        if(tutorialModeOn){
            //allow someone else to spawn in the ball, but do reset positions
            //resetPositions();  // We don't want the player to keep jumping back to original location (might be disorienting)
            return;
        }

        //see if the game is over
        if(AIPoints>=playTo){
            Time.timeScale=0f;
            Defeat();
            return;
        }
        if(playerPoints>=playTo){
            Time.timeScale=0f;
            Victory();
            return;
        }


        gameActive=true;
        Time.timeScale=1f;
        resetPositions();
        spawnBall();

    }


    public void resetPositions(){

        player.GetComponent<CharacterController>().enabled=false;
        ai.GetComponent<CharacterController>().enabled=false;

        Vector3 playerSpawnPoint = court.AISideMidpoint();
        playerSpawnPoint = court.PlayerSideMidpoint();
        playerSpawnPoint.y += player.GetComponent<SpriteRenderer>().bounds.extents.y;


        Vector3 aiSpawnPoint = court.AISideMidpoint();
        aiSpawnPoint.y += ai.GetComponent<SpriteRenderer>().bounds.extents.y;
        ai.transform.position = aiSpawnPoint;


        player.GetComponent<CharacterController>().enabled=true;
        ai.GetComponent<CharacterController>().enabled=true;

        player.GetComponent<StaminaStat>().ResetStamina();
        ai.GetComponent<StaminaStat>().ResetStamina();
    }

    public void spawnBall(){    
        globalProjectile.hideProjectile();
        // globalProjectile.resetBounceCount();


        float serveDirection = Mathf.RoundToInt(Random.value*2)-1;

        Vector3 spawnLocation = Vector3.zero;
        //if we are in the tutorial, always serve to the player
        if(tutorialModeOn)
            serveDirection=1;
        if(serveDirection>0){
            spawnLocation=court.PlayerSideMidpoint();
        }
        else{
            spawnLocation=court.AISideMidpoint();
            if(ai.GetComponent<AIInput>() != null)      // Added this check to make sure we don't get null refs in tutorial mode
                ai.GetComponent<AIInput>().setImpactPoint(new ImpactPoint(spawnLocation,Time.time+5f));
        }
        globalProjectile.lastHitter=null;
        globalProjectile.transform.position = spawnLocation+new Vector3(0,5,0);
        
        globalProjectile.setVelocity(new Vector3(0,0,0));
        globalProjectile.showProjectile();



        // AARON (for tutorial)
        if(tutorialModeOn)
            hasBallDespawned = false;
    }


    // TUTORIAL STUFF  \/\/\/\/  (IState script isn't a monobehaviour and can't do coroutines itself)
    public void SpawnBallDelayed(float delay)
    {
        if(!isWaitingForSpawn)
            StartCoroutine(Delay(delay));
    }

    bool isWaitingForSpawn = false;
    IEnumerator Delay(float delay)
    {
        isWaitingForSpawn = true;
        yield return new WaitForSeconds(delay);
        spawnBall();
        isWaitingForSpawn = false;
    }

    // TUTORIAL STUFF  /\/\/\





    public event System.Action<string> OnMatchEnd;
    public static event System.Action OnUserWin;       // AARON: Added this so wouldn't have to do checking myself

    // Keep this public so that the tutorial system can call this
    public void Victory(){
        Cursor.lockState=CursorLockMode.Confined;
        Cursor.visible=true;
        animator.SetTrigger("EndMatchScreen");
        endScreenLabel.text="Victory!";
        OnMatchEnd?.Invoke("Player");
        OnUserWin?.Invoke();
        //unlock the next world
    }
    public void Defeat(){
        Cursor.lockState=CursorLockMode.Confined;
        Cursor.visible=true;
        animator.SetTrigger("EndMatchScreen");
        endScreenLabel.text="Defeat";
        OnMatchEnd?.Invoke("AI");
    }

    public void replay(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void quitToWorldMap(){

        // Suggested edit (Aaron)
        BasicGameManager gm = FindObjectOfType<BasicGameManager>();
        if(gm != null)
        {
            //Debug.Log("CALLED GM");
            Time.timeScale = 1;
            gm.ReturnToLevelSelect();
            return;
        }        
    }


    public void pause(){
        if(isPaused){
            animator.SetTrigger("PauseLeave");
            BasicGameManager.instance.UnfreezeInputs();
            player.GetComponent<UserInput>().EnableInputIn(0.3f);
            return;
        }
        if(Time.timeScale!=1)
            return;
        isPaused=true;
        animator.SetTrigger("PauseEnter");
        Cursor.lockState=CursorLockMode.Confined;
        Cursor.visible=true;
        Time.timeScale=0;
        BasicGameManager.instance.FreezeInputs();
        player.GetComponent<UserInput>().DisableInput();
    }
    public void resume(){
        isPaused=false;
        Cursor.lockState=CursorLockMode.Locked;
        Cursor.visible=false;
        Time.timeScale=1f;
        BasicGameManager.instance.UnfreezeInputs();
        player.GetComponent<UserInput>().EnableInputIn(0.3f);
    }


    // Suggestion: 
    //  The match shouldn't start immediately upon entering the scene.
    //  Give the game manager the ability to specifiy what type of match
    //  to start by implementing the two functions below.

    // public void TutorialMode()
    // {
    //     // Begin "tutorial mode", which includes:
    //     //      1. No intro animation
    //     //      2. Enemy player is disabled
    //     //      3. Points are not incremented 
    //     //      4. Projectile is served to user ONLY

    //     ai.gameObject.SetActive(false);
    //     resetPositions();
    //     spawnBall();
    // }

    // public void RegularMode()
    // {
    //     // Begin "regular mode", which is how the match is typically played (what we have right now)
    //     Time.timeScale=0f;
    //     animator.SetTrigger("Match Start");
    // }



    // Suggestion:
    //  Provide a function that will allow the game manager to stop the current point,
    //  and pause the match.
    //  Provide a function that will allow the game manager to resume the match.

    public void StopMatch()
    {
        // Reset the projectile and don't serve it anymore
    }

    public void ResumeMatch()
    {
        // Respawn the projectile and serve it from the side of whoever was supposed
        // to serve next (before the match was paused)
    }



}
