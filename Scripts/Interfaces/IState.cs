using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnExit();
    void Tick();
}

public class FreeState : IState
{
    CharacterMovement movement;
    public event System.Action OnStart;
    public FreeState(CharacterMovement playerMovement)
    {
        movement = playerMovement;
    }
    public void OnEnter()
    {
        //Debug.Log("Entering FREE state");
        // TODO: Tell animator controller to switch to normal walk mode
        movement.ResetBaseSpeed();
        
        OnStart?.Invoke();
        
    }

    public void OnExit()
    {}

    public void Tick()
    {}
}

public class ChargeState : IState
{
    float chargeAmount;
    float chargeRate;
    float maxCharge;
    Vector2 aimVector;
    CharacterMovement movement;
    CharacterSwing swing;
    ParticleSystem chargeParticles;
    PlayerAnimationController animationController;

    public event System.Action OnStart;         // Invoked when entering the state
    public event System.Action OnEnd;           // Invoked when exiting the state
    public event System.Action<float> OnNewPercent; // Invoked when the chargePercentage has been changed

    private AudioManager audioManager;

    public ChargeState(CharacterMovement playerMovement, CharacterSwing playerSwing, float rate, ParticleSystem particles, PlayerAnimationController playerAnimationController)
    {
        movement = playerMovement;
        swing = playerSwing;
        animationController = playerAnimationController;
        chargeRate = rate;
        chargeParticles = particles;
        maxCharge = 10;                  // HARDCODED -- this should probably be passed in

    }
    public void OnEnter()
    {
        //Debug.Log("Entering CHARGE state");

        chargeAmount = 1;
        aimVector = Vector2.zero;

        movement.MultiplyBaseSpeed(0.1f);
        movement.DisableSprint(true);

        // TODO: Tell animator controller to switch to charge walk mode

        if(chargeParticles != null)
            chargeParticles.Play();

        animationController.PlayCharge();

        audioManager = AudioManager.instance;
        // audioManager.PlaySound("charge");
        OnStart?.Invoke();

    }

    public void OnExit()
    {
        swing.SetSwingVector(aimVector.y, aimVector.x);
        movement.DisableSprint(false);
        // audioManager.StopSound("charge");
        // TODO: Tell animator controller to switch off charge walk mode

        if (chargeParticles != null)
            chargeParticles.Stop();

        OnEnd?.Invoke();
    }

    public void Tick()
    {
        chargeAmount += chargeRate * Time.deltaTime;
        if(chargeAmount > maxCharge)
            chargeAmount = maxCharge;           // Cap charge at max charge
        OnNewPercent?.Invoke(chargeAmount);

        Vector2 mousePos = Input.mousePosition;
        Vector2 aimDirection = (mousePos - Vector2.right * Screen.width/2).normalized;
        SectorAim sectorAim = swing.GetComponent<SectorAim>();           //TODO: This should be changed so that SectorAim component is passed in (not searched for here)
        if(sectorAim != null)
            aimDirection = sectorAim.GetAimDirection();
        aimVector = aimDirection * chargeAmount;

        OnNewPercent?.Invoke((chargeAmount/maxCharge) * 100);

        // TODO: Somehow broadcast event for UI to know where to point arrow (maybe have state machine pass in itself to constructor)
    }
}

public class SwingState : IState
{
    CharacterMovement movement;
    CharacterSwing swing;

    public event System.Action OnEnd;

    public SwingState(CharacterMovement playerMovement, CharacterSwing playerSwing)
    {
        movement = playerMovement;
        swing = playerSwing;

    }
    public void OnEnter()
    {
        //Debug.Log("Entering SWING state");
        movement.MultiplyBaseSpeed(0);
        swing.Swing();      

        // TODO: Tell animator controller to transition to do the swing animation
    }

    public void OnExit()
    {
        movement.ResetBaseSpeed();
        OnEnd?.Invoke();
    }

    public void Tick()
    {}
}

public class BlockState : IState
{
    CharacterMovement movement;
    CharacterSwing swing;
    public BlockState(CharacterMovement playerMovement, CharacterSwing playerSwing)
    {
        movement = playerMovement;
        swing = playerSwing;
    }
    public void OnEnter()
    {
        //Debug.Log("Entering BLOCK state");

        swing.Block();      // This should be replaced later (same as Swing())

        // TODO: Tell animator controller to do block animation
    }

    public void OnExit()
    {
        movement.ResetBaseSpeed();
    }

    public void Tick()
    {
    }
}







///////////////////
// TUTORIAL STATES
///////////////////


public class InactiveState : IState
{
    public void OnEnter()
    {
        //Debug.Log("Entering Inactive State");
    }

    public void OnExit()
    {
    }

    public void Tick()
    {
    }
}

public class DialogueState : IState
{

    public void OnEnter()
    {
        //Debug.Log("Entering Dialogue State");
        // TODO: Call special GM function to continue to next tutorial dialogue using GameObject.FindObjectOfType
        if(BasicGameManager.instance != null)
            BasicGameManager.instance.PlayNextTutorialDialogue();
        
        GameObject.FindObjectOfType<UserInput>().DisableInput();
    }

    public void OnExit()
    {
        GameObject.FindObjectOfType<UserInput>().EnableInputIn(0.5f);
    }

    public void Tick()
    {
    }
}

// This state simply serves as a controlled transition between objective and dialogue states
// With this statemachine class there is no built-in way to describe the length of time of a transition
public class ObjectiveTransition : IState
{
    public void OnEnter()
    {
        //Debug.Log("Entering Objective Transition state");
    }

    public void OnExit()
    {}

    public void Tick()
    {}
}


public class Objective : IState
{
    public static event System.Action OnFinishObjective;        // This should be invoked whenever ANY objective is completed


    string message = "Complete the objective";
    UnityEngine.UI.Text messageBox;
    public Objective(string message, UnityEngine.UI.Text messageBox)
    {
        this.message = "OBJECTIVE: " + message;
        this.messageBox = messageBox;
    }
    public void OnEnter()
    {
        //Debug.Log("Entering objective state for Objective: " + this.message);
        // Display objective message

        if(messageBox != null)
        {
            messageBox.text = message;
            messageBox.color = new Color(200/255f, 90/255f, 90/255f);
        }
            
        Initialize();
    }

    public void OnExit()
    {
        messageBox.color = Color.green;
        OnFinishObjective?.Invoke();
    }

    public virtual void Tick()
    {}
    protected virtual void Initialize()    // Do any initialization
    {}


}


// We need individual types so that the state machine recognizes the different states (it stores states based on type, so we can't reuse the same type)
public class Move_Obj : Objective
{
    public Move_Obj(string message, UnityEngine.UI.Text messageBox) : base(message, messageBox)
    {}
}

public class Sprint_Obj : Objective
{
    public Sprint_Obj(string message, UnityEngine.UI.Text messageBox) : base(message, messageBox)
    {}
}

public class Swing_Obj : Objective
{
    public Swing_Obj(string message, UnityEngine.UI.Text messageBox) : base(message, messageBox)
    {}
}







public class ProjObjective : Objective
{
    matchController mc;     // Because this object will be constructed before the actual match is created, we cannot initialize it

    public ProjObjective(string message, UnityEngine.UI.Text messageBox) : base(message, messageBox)
    {}

    protected override void Initialize()
    {
        // TODO: Spawn the projectile
        mc = GameObject.FindObjectOfType<matchController>();

        if(mc != null)
            mc.spawnBall();

            
    }

    public override void Tick()
    {
        // TODO: If projectile has been despawned, spawn the projectile
        if(mc.hasBallDespawned)
            mc.SpawnBallDelayed(0.5f);
           
    }
}

public class Hit_Obj : ProjObjective
{
    public Hit_Obj(string message, UnityEngine.UI.Text messageBox) : base(message, messageBox)
    {}
}









