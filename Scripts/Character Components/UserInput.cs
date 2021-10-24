using System;
using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour, ICharacterInput
{
    public float vertical {get; private set;}
    public float horizontal {get; private set;}
    public bool sprintKeyPressed {get; private set;}
    public bool fireKeyPressed {get; private set;}
    public bool blockKeyPressed {get; private set;}

    public float horizontalMouse {get; private set;}

    public event Action OnJumpKeyPressed;
    public event Action OnFireStart;
    public event Action OnFireEnd;

    private AudioManager audioManager;

    float timeSinceDown = 0;
    bool hasReleased = true;

    void Update()
    {
        if(Time.time - timeSinceDown > 3 && !hasReleased || Time.timeScale == 0)
        {
            audioManager.StopSound("charge");
            //Debug.Log("MORE THAN 3 SEC");
            hasReleased = true;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            OnFireEnd?.Invoke();
            audioManager.StopSound("charge");
            hasReleased = true;
        }
        if(isDisabled) {return;}

        audioManager = AudioManager.instance;
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        horizontalMouse = Input.GetAxis("Mouse X");

        // We are no longer executing the swing action from an event broadcast from this specific script -- instead, we let the state machine handle it
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(OnJumpKeyPressed != null)
                OnJumpKeyPressed();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnFireStart?.Invoke();
            audioManager.PlaySound("charge");
            timeSinceDown = Time.time;
            hasReleased = false;
        }
        

        sprintKeyPressed = Input.GetKey(KeyCode.LeftShift);
        fireKeyPressed = Input.GetKey(KeyCode.Mouse0);
        blockKeyPressed = Input.GetKey(KeyCode.Mouse1);

    }

    bool isDisabled = false;
    public void DisableInput()
    {
        isDisabled = true;
        vertical = 0;
        horizontal = 0;
        sprintKeyPressed = false;
        fireKeyPressed = false;
        blockKeyPressed = false;
        horizontalMouse = 0;

    }

    public void EnableInputIn(float waitTime)
    {
        if(!isWaiting)
            StartCoroutine(DelayReturnControl(waitTime));
    }

    bool isWaiting;
    IEnumerator DelayReturnControl(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isDisabled = false;
        isWaiting = false;
    }


}
