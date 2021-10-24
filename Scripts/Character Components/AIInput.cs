using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIInput : MonoBehaviour, ICharacterInput
{
    public float vertical {get; private set;}
    public float horizontal {get; private set;}
    public bool sprintKeyPressed {get; private set;}
    public bool fireKeyPressed {get; private set;}
    public bool blockKeyPressed {get; private set;}

    public float horizontalMouse {get; private set;}


    //Set Where the AI Should Be Walking To
    private Vector3 targetPosition;

    
    // for when we implement perlin noise
    //public float decimalPercision;

    private SectorAim sectorAim;

    //debugging 
    public GameObject positionMarker;


    public event Action OnJumpKeyPressed;
    public event Action OnFireStart;
    public event Action OnFireEnd;



    private ImpactPoint impactPoint;




    //to figure out sprinting
    private CharacterMovement characterMovement;


    //states
    bool STATE_WAITING_FOR_HIT =false;
    bool STATE_WALK_TOWARDS_IMPACT_LOCATION=false;
    bool STATE_IDLE = false;


    private float aiMidPointDepth;
    [Header ("\"AI Skill\"")]
    [SerializeField] GameObject Opponent;
    [SerializeField] float locationAccuracy;
    [Range(0, 1)]
    [SerializeField] float aimSpeedPercent = 0.5f;
    [Range(0, 3)]
    [SerializeField] float maxDistanceAimFromPlayer = 1;


    private CourtController courtController;

    // AARON
    bool hasSwung = false;  // Has the player made their attempt at a swing yet?


    public void Start(){
        courtController = FindObjectOfType<CourtController>();
        aiMidPointDepth = courtController.AISideMidpoint().z;
        sectorAim = FindObjectOfType<UserInput>().GetComponent<SectorAim>();
        STATE_WALK_TOWARDS_IMPACT_LOCATION=true;



        impactPoint = new ImpactPoint(new Vector3(0,0,0),Time.time-1f);
        targetPosition=new Vector3(0,0,0);

        //get the character movement reference
        characterMovement = this.GetComponent<CharacterMovement>();

    }

    void Awake()
    {
        GetComponent<SphereCollider>().radius = GetComponent<CharacterSwing>().GetSwingRange() * 0.75f;
    }


    
    Vector3 targetAimPosition = Vector3.zero;
    bool hasChosenAimPosition = false;
    void Update()
    {
        behave();
        //set the position of our debug markerS
        positionMarker.transform.position=targetPosition;
        findMidpointDestination();

        if(STATE_WAITING_FOR_HIT && Opponent != null)
            horizontalMouse = MechanicsHelper.CalculateMouseMovement(GetComponent<SectorAim>(), targetAimPosition, aimSpeedPercent);
        else
            horizontalMouse = 0;

    }


    void walkTowardsPosition(){
        
        Vector3 direction = targetPosition-transform.position;
        //accuracy, we don't need to get to the EXACT position, so just round a bit
        direction  = new Vector3(
            Mathf.Round(direction.x),
            0,
            Mathf.Round(direction.z));
        direction.Normalize();

        // Aaron: Added these two lines to be compatible with PlayerMovement (instead of AIMovement)
        vertical = direction.z;
        horizontal = direction.x;
    }






    void behave(){

        //if the impact time is in the future, we are going there
        if(impactPoint.impactTime > Time.time){
            STATE_WALK_TOWARDS_IMPACT_LOCATION=true;
            STATE_WAITING_FOR_HIT=false;
            STATE_IDLE=false;
            targetPosition=impactPoint.impactLocation;

        }
        //if the impact time is in the past, that means we need to go the midpoint
        else{

            targetPosition=findMidpointDestination();
            hasChosenAimPosition = false;
            fireKeyPressed = false;
        }


        if(atTargetLocation()){
            if(STATE_WAITING_FOR_HIT == false && !hasChosenAimPosition)      // When we get to a charging position, choose a new location to aim at
            {
                targetAimPosition = Opponent.transform.position + Vector3.right*UnityEngine.Random.Range(-maxDistanceAimFromPlayer, maxDistanceAimFromPlayer);
                hasChosenAimPosition = true;
            }
            vertical=0;
            horizontal=0;
            STATE_WAITING_FOR_HIT=true;
            if(!hasSwung)
            {
                fireKeyPressed = true;
                hasSwung = true;
            }
            

        }
        else{
            //walk towards our destination
            walkTowardsPosition();


            //figure out whether to sprint
            Vector2 characterSpeed = characterMovement.getCurrentVelocity();
            Vector2 distanceToTravel = new Vector2(this.transform.position.x-targetPosition.x,this.transform.position.z-targetPosition.z);
            float timeToDest = distanceToTravel.magnitude/characterSpeed.magnitude;



            //Decide whether we need to sprint
            if(timeToDest > impactPoint.impactTime-Time.time && impactPoint.impactTime>=Time.time){
                sprintKeyPressed=true;
            }
            else{
                sprintKeyPressed=false;
            }
        }

        

    }

    

    Vector3 findMidpointDestination(){
        Vector3 midVector = sectorAim.GetMidvector();

        Vector3 playerLocation = sectorAim.transform.position;

        // Debug.Log("MidVector:"+midVector);
        float timeToIntersect = Mathf.Abs(aiMidPointDepth-playerLocation.z / midVector.z);
        // Debug.Log("timeToIntersect:"+timeToIntersect);
        float xIntersection = timeToIntersect*midVector.x;
        // Debug.Log("xIntersection:"+xIntersection);
        Vector3 intersection = new Vector3(xIntersection,playerLocation.y,aiMidPointDepth);
        return intersection;
    }


    bool atTargetLocation(){

        if(Mathf.Abs(transform.position.x-targetPosition.x)>locationAccuracy ||
        Mathf.Abs(transform.position.z-targetPosition.z)>locationAccuracy)
            return false;
        return true;
    }


    public void setImpactPoint(ImpactPoint dummyImpactPoint){
        //see if the location is something we actually want to go to
        if(courtController.pointInsidePlayerSide(dummyImpactPoint.impactLocation))
            return;

            
        impactPoint = dummyImpactPoint;
        hasSwung = false;
    }


    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Projectile")){

            //if he was walking, and the ball enters he range, just start charging
            if(STATE_WALK_TOWARDS_IMPACT_LOCATION){
                fireKeyPressed=true;
            }
            //otherwise, if we wasn't walking, that means he was at his destination, and he should swing
            else{
                fireKeyPressed = false;
                hasChosenAimPosition = false;
            }




        }
    }

    //when the projectile leaves, let go of the fire key (this would techinically happen twice for normal hits, but "letting go" twice doesnt do anything)
    //but in the normal response case, it allows the ai to swing  
    public void OnTriggerExit(Collider other){
        if(other.CompareTag("Projectile")){
            fireKeyPressed=false;
        }
    }

}
