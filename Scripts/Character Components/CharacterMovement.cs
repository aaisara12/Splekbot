using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Standard player movement with smoothing, no sprint function 

public class CharacterMovement : MonoBehaviour 
{
    private CharacterController controller;
    private ICharacterInput playerInput;
    private PlayerAnimationController animationController;
    private StaminaStat staminaStat;

    [SerializeField] float runSpeed = 10f;
    float originalRunSpeed;         // A save of the speed with which the character started
    [SerializeField] float smoothTime = 1f;
    [SerializeField] private float sprintMultiplier = 1.5f;     // How much to multiply top speed by when sprinting
    [SerializeField] private float sprintCost = 20;             // How much stamina sprinting depletes every second
    [SerializeField] private float exhaustionMultiplier = 0.4f;
    
    float xVelocity = 0f;
    float zVelocity = 0f; 

    float newxVelocity = 0f;
    float newzVelocity = 0f;

    bool hasBeenInitialized = false;
    bool isSprintDisabled = false;      // Has the sprint ability been disabled? 

    bool isExhausted = false;



    public bool isSprinting { get; private set;}        // Used for sprint objective


    void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<ICharacterInput>();
        staminaStat = GetComponent<StaminaStat>();

        if(controller == null)
            Debug.LogWarning(gameObject.name + " is missing a Character Controller component.");
        if(playerInput == null)
            Debug.LogError(gameObject.name + " is missing a component that implements ICharacterInput.");
        if(staminaStat == null)
            Debug.LogWarning(gameObject.name + " is missing a StaminaStat component.");
        else
        {
            staminaStat.OnExhausted += HandleExhausted;
            staminaStat.OnRecovered += HandleRecovered;
        }

        


        originalRunSpeed = runSpeed;



        hasBeenInitialized = true;      // This value is used to prevent member function from trying to execute fully before the data members have been initialized
    }

    

    void Update() 
    {
        if(controller == null || playerInput == null) {return;}

        Vector3 rawInputVelocity = new Vector3(playerInput.horizontal, 0, playerInput.vertical);

        float multiplier = 1f;      // How much to multiply speed by based on whether sprinting

        isSprinting = false;
        if(!isSprintDisabled)       
        {
            if( staminaStat != null && 
                playerInput.sprintKeyPressed && 
                rawInputVelocity.magnitude > 0 && 
                staminaStat.LoseStamina(sprintCost * Time.deltaTime))
                {
                    multiplier = sprintMultiplier;
                    isSprinting = true;
                }      

                
        }
        


        Vector3 targetVelocity = rawInputVelocity.normalized * runSpeed * multiplier * (isExhausted? exhaustionMultiplier : 1);    // Compressed the normalization code below - Aaron

        //Ramps to a velocity depending on keyboard input and runSpeed 
        newxVelocity = Mathf.SmoothDamp(newxVelocity, targetVelocity.x, ref xVelocity, smoothTime);
        newzVelocity = Mathf.SmoothDamp(newzVelocity, targetVelocity.z, ref zVelocity, smoothTime);  
        Vector3 move = new Vector3(newxVelocity, 0, newzVelocity); 
        




        //animation stuff
        animationController.UpdateVelocity(new Vector2(newxVelocity,newzVelocity));

        if(controller.enabled == true)
            controller.Move(move * Time.deltaTime);
    }

    public void ResetBaseSpeed()
    {
        if(!hasBeenInitialized) { return; }

        runSpeed = originalRunSpeed;
    }

    public void MultiplyBaseSpeed(float factor)
    {
        if(factor >= 0 && hasBeenInitialized)
            runSpeed = originalRunSpeed*factor;
    }

    public void DisableSprint(bool shouldDisable)   // This feels like anti-best practices to allow any script to simply disable the player's ability to sprint (so much room for bugs)
    {
        isSprintDisabled = shouldDisable;
    }


    //Get the current velocity
    public Vector2 getCurrentVelocity(){
        return new Vector2(newxVelocity,newzVelocity);
    }



    void HandleExhausted()
    {
        isExhausted = true;
        GetComponent<SpriteRenderer>().color = Color.red;
        isSprintDisabled = true;
    }

    void HandleRecovered()
    {
        isExhausted = false;
        isSprintDisabled = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}