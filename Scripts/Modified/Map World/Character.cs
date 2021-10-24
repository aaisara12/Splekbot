using UnityEngine;

public class Character : MonoBehaviour
{
    public float Speed = 3f;
    public bool IsMoving { get; private set; }


    //blicking text
    public GameObject blickingText;

    public Pin CurrentPin { get; private set; }
    private Pin _targetPin;
    private MapManager _mapManager;

    public SpriteRenderer Mark;



    // AARON
    [SerializeField] Animator animator;

    public void Awake()
    {
        //animator = GetComponent<Animator>();
    }

    public void Initialise(MapManager mapManager, Pin startPin)
    {
        _mapManager = mapManager;
        SetCurrentPin(startPin);
    }


    /// <summary>
    /// This runs once a frame
    /// </summary>
    private void Update()
    {
        if (_targetPin == null) return;

        // Get the characters current position and the targets position
        var currentPosition = transform.position;
        var targetPosition = _targetPin.transform.position;

        // If the character isn't that close to the target move closer
        if (Vector3.Distance(currentPosition, targetPosition) > .02f)
        {
            transform.position = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                Time.deltaTime * Speed
            );
        }
        else
        {
            if (_targetPin.IsAutomatic)
            {

                // Get a direction to keep moving in
            
                var pin = _targetPin.GetNextPin(CurrentPin);
                MoveToPin(pin);
            }
            else
            {
                SetCurrentPin(_targetPin);
            }
        }

        if(Time.time - timeSinceArrive > 3  &&  isBlinkingEnabled)
            blickingText.gameObject.SetActive(true);
    }


    /// <summary>
    /// Check the if the current pin has a reference to another in a direction
    /// If it does the move there
    /// </summary>
    /// <param name="direction"></param>
    public void TrySetDirection(Direction direction)
    {
        // Try get the next pin
        var pin = CurrentPin.GetPinInDirection(direction);

        // If there is a pin then move to it
        if (pin == null || pin.IsLocked) return;

        // add rocket sound here
        if(animator != null)
        {
            if(direction == Direction.Left)
                animator.SetTrigger("Move Left");
            else if (direction == Direction.Right)
                animator.SetTrigger("Move Right");
            else
                animator.SetTrigger("Move Right");
            AudioManager.instance.PlaySound("jetpack");
        }
        
        FindObjectOfType<BasicGameManager>().hasWon = false;

        MoveToPin(pin);
    }


    /// <summary>
    /// Move to a new pin
    /// </summary>
    /// <param name="pin"></param>
    private void MoveToPin(Pin pin)
    {
        Mark.GetComponent<FadeIn>().FadeOutObject();
        Mark.GetComponent<Renderer>().enabled = false;
        _targetPin = pin;
        IsMoving = true;
        //blickingText.gameObject.SetActive(false);
        DisableBlinking();

    }


    /// <summary>
    /// Set the current pin
    /// </summary>
    /// <param name="pin"></param>
    public void SetCurrentPin(Pin pin)
    {
        // stop rocket sound
        if (IsMoving && animator != null)
        {                    // If we were just moving and now have stopped, play the landing animation
            animator.SetTrigger("Land");
            AudioManager.instance.StopSound("jetpack");
        }
        CurrentPin = pin;
        _targetPin = null;
        transform.position = pin.transform.position;
        IsMoving = false;

        // Tell the map manager that
        // the current pin has changed
        _mapManager.UpdateGui();
        if (!pin.IsCompleted)
        {
            Mark.GetComponent<Renderer>().enabled = true;
            Mark.GetComponent<FadeIn>().FadeInObject();
            
            //blickingText.gameObject.SetActive(true);
        }
        else
        {
            Mark.GetComponent<FadeIn>().FadeOutObject();
            Mark.GetComponent<Renderer>().enabled = false;
        }

        MapManager.lastPinName = CurrentPin.name;       // We want the map manager to remember where the player last was
        
        //timeSinceArrive = Time.time;
        EnableBlinking();
    }

    //
    public void UpdateCompletion()
    {
        if (CurrentPin.IsCompleted)
        {
            Mark.GetComponent<FadeIn>().FadeOutObject();
            Mark.GetComponent<Renderer>().enabled = false;
        }

    }

    bool isBlinkingEnabled = false;
    float timeSinceArrive = 0;      // The time since the player landed on this node
    public void DisableBlinking()
    {
        blickingText.gameObject.SetActive(false);
        isBlinkingEnabled = false;
    }

    public void EnableBlinking()
    {
        blickingText.gameObject.SetActive(true);
        isBlinkingEnabled = true;
        timeSinceArrive = Time.time;
    }
}