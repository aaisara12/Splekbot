using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToggleUI : MonoBehaviour
{
    private Canvas CanvasObject;
    public Character Character;
    public GameObject Image;

    //public BasicGameManager GameManager;

    UnityEngine.UI.Button startButton;      // Aaron: This will be a reference to the big red start button
    UnityAction clickCallback;

    void Awake()
    {
        clickCallback += GM_StartLevel;
        startButton = GetComponentInChildren<UnityEngine.UI.Button>();
        startButton.onClick.AddListener(clickCallback);
    }
    void GM_StartLevel()
    {
        if(BasicGameManager.instance != null)
            BasicGameManager.instance.StartLevel(); // For some reason, Unity isn't happy with directly subscribing the StartLevel() method to the callback action
        else
            Debug.LogWarning("No game manager instance detected.");
    }
    

    // Start is called before the first frame update
    void Start()
    {
        CanvasObject = GetComponent<Canvas>();
        //CanvasObject.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // if (Character.CurrentPin.SceneToLoad != "" && Input.GetKeyUp(KeyCode.Return)  && !Character.IsMoving)
        // {
        //     CanvasObject.enabled = !CanvasObject.enabled;
        //     levelInfo info = new levelInfo();
        //     info.levelName = string.Format(Character.CurrentPin.SceneToLoad);
        //     info.dialoguePrefix = Character.CurrentPin.DialogueToLoad;
        //     if(CanvasObject.enabled == true)
        //     {
        //         if(BasicGameManager.instance != null)
        //             BasicGameManager.instance.ActivateLevelNode(info);
        //         else
        //             Debug.LogWarning("No game manager instance detected.");
        //     }
                
        // }
    }

    // The game manager should use this check the information of the current pin
    public bool TryGetCurrentPin(ref Pin currentPin)        // Really, this should be a function of Character, but I got lazy
    {
        if(!Character.IsMoving)
        {
            currentPin = Character.CurrentPin;
            //Debug.Log("Is current pin null? " + (currentPin == null));
            return true;
        }
        return false;

    }

    float timeSinceEnable = 0;  // Keeps track of when the panel was enabled so you can't immediately close it
    // The game manager should call this as soon as it's done reading the dialogue

    public bool isPanelEnabled = false;
    public void EnableLevelPanel()
    {
        
        //CanvasObject.enabled = true;
        Animator animator = GetComponentInChildren<Animator>();

        if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Case")) {return;}

        isPanelEnabled = true;

        if(animator != null)
        {
            animator.SetTrigger("Level Enter");
            timeSinceEnable = Time.time;
        }

        Character c = FindObjectOfType<Character>();
        if(c != null)
            c.DisableBlinking();

        BasicGameManager.instance.SoftFreezeInputs();
            
    }

    public void DisableLevelPanel()
    {
        

        Animator animator = GetComponentInChildren<Animator>();

        if(!animator.GetCurrentAnimatorStateInfo(0).IsName("LevelEnterAnimation") || (Time.time - timeSinceEnable < 1f)) {return;}

        isPanelEnabled = false;

        if(animator != null)
            animator.SetTrigger("Level Leave");

        Character c = FindObjectOfType<Character>();
        if(c != null)
            c.EnableBlinking();
        
        BasicGameManager.instance.UnfreezeInputs();
    }

}
