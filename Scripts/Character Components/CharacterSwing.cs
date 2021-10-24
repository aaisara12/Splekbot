using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Attached to a player object which has an empty child object (hitbox) attached to it 
public class CharacterSwing : MonoBehaviour
{
    [Range(0.1f, 5)]
    [SerializeField] float swingStrength;
    [SerializeField] Transform swingPoint;
    [SerializeField] LayerMask projLayers;
    [SerializeField] float swingRange = 0.5f;


    [SerializeField] Projectile gameProjectile; // The one and only projectile for this game
    public event System.Action OnFinishSwing;   // Invoked when the swing has been completed


    Vector3 swingVector = Vector2.zero;  // The direction and magnitude of the swing
    
    PlayerAnimationController animationController;
    float nextValidTime = 0f;

    private AudioManager audioManager;

    private ICharacterInput playerInput;

    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager Found!!!!");
        }
    }

    void Awake()
    {
        swingRange = swingRange+gameProjectile.GetComponent<SphereCollider>().radius;
        animationController = GetComponent<PlayerAnimationController>();
        playerInput = GetComponent<ICharacterInput>();

        if(animationController == null)
        {
            Debug.LogWarning(gameObject.name + " is missing an animation controller component.");
            return;
        }
        if(playerInput == null)
        {
            Debug.LogError(gameObject.name + " is missing a component that implements ICharacterInput.");
            return;
        }
            
    }


    public void Swing()
    {
        animationController.PlaySwing();        // This needs to be adjusted based on which charge animation was played

        if(Mathf.Abs(Vector3.Distance(gameProjectile.transform.position, swingPoint.position)) <= swingRange && gameProjectile.lastHitter != this)
        {
            gameProjectile.Hit(swingVector, this);

            // if(OnFinishSwing != null)
            //     OnFinishSwing();

            // camera shake
            if (swingVector.magnitude > 3)
            {
                GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse(Camera.main.transform.forward);
                // audioManager.PlaySound("explosion");
            }
        }

        OnFinishSwing?.Invoke();

    }

    public void Block()
    {
        // TODO: Give the block its own hitbox (probably should be more generous than the swing hitbox)
        animationController.PlayBlock();
        if(Vector3.Distance(gameProjectile.transform.position, swingPoint.position) < swingRange && gameProjectile.lastHitter != this)
        {
            gameProjectile.Block(this);
        }
    }

    
    // Visualize hitbox
    void OnDrawGizmosSelected() 
    {
        if (swingPoint == null) { return; }

        Gizmos.DrawWireSphere(swingPoint.position, swingRange);
    }


    // Attempt to load the game projectile with the specified launch direction
    public void SetSwingVector(float forwards, float lateral)       // The vector is broken up into descriptive parameters to make it less confusing how to format the input vector
    {
        swingVector = new Vector3(lateral, 0, forwards);
    }

    public float GetSwingRange()
    {
        return swingRange;
    }

    
}