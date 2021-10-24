using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    // NOTE: This script must be a component on the game object that contains the SLIDER component responsible for changing the size of the resource bar 
    //       that is also a descendant of the character that owns the stamina stat.

    StaminaStat stamina = null;
    Slider staminaSlider = null;
    [SerializeField] float timeBeforeFade = 0;      // How long should the stamina bar wait at full capacity before fading away visually?
    [SerializeField] Image staminaBarObject = null;
    
    //bool isExhausted = false;
    bool isBlinking = false;
    Color originalColor;

    float timeOfLastChange;


    void Awake()
    {
        stamina = GetComponentInParent<StaminaStat>();
        staminaSlider = GetComponent<Slider>();

        originalColor = staminaBarObject.color;

        if(stamina == null)
        {
            Debug.LogError("Could not find stamina stat reference in parents.");
            return;
        }
        if(staminaSlider == null)
        {
            Debug.LogError("Could not find a slider on this game object.");
            return;
        }

        stamina.OnStaminaChanged += HandleNewStamina;           // Set up the stamina bar to react to changes in the stamina stat of the parent object (ex. main player)
        stamina.OnExhausted += HandleExhausted;
        stamina.OnRecovered += HandleRecover;

        staminaSlider.minValue = 0;
        staminaSlider.maxValue = stamina.GetMaxStamina();

        if(staminaBarObject == null)
        {
            Debug.LogError("Stamina bar UI is missing reference to stamina bar image.");
            return;
        }



            
    }

    void Update()
    {
        
        if(canFade && staminaSlider.value == staminaSlider.maxValue && Time.time - timeOfLastChange > timeBeforeFade)   
        {
            canFade = false;
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
            
    }
    
    void HandleNewStamina(float newStamina)
    {
        // TODO: Implement fatigue color (when try to use stamina, but stamina stat empty -- not when invalid stamina amount given)
        // TODO: Make stamina bar fade away once at max stamina
        if(staminaSlider.value != newStamina)
        {
            timeOfLastChange = Time.time;
            if(isFading)
            {
                //Debug.Log("STOPPING COROUTINE");
                StopAllCoroutines();
                isFading = false;
                
            }
            if(!canFade)
            {
                //Debug.Log("Restoring color");
                Color c = staminaBarObject.color;
                staminaBarObject.color = new Color(c.r, c.g, c.b, 1);
            }
            canFade = true;
            
        }
            

        staminaSlider.value = newStamina;

    }



    void HandleExhausted()
    {
        StartCoroutine(BlinkRed());
    }

    void HandleRecover()
    {
        StopAllCoroutines();        // This is really just meant to target the BlinkRed() coroutine, but for some reason StopCoroutine(BlinkRed()) doesn't work
        staminaBarObject.color = originalColor;
    }





    bool isFading = false;  // is the fading coroutine currently active?
    bool canFade = false;   // is the stamina bar in a state where it is able to begin fading?


    // COPIED FROM AimArrow.cs -- Surely there's a better way to do this (somehow if we could get some kind of GUI helper function that can simply be called)

    IEnumerator FadeOut()
    {
        //Debug.Log("Fade Out coroutine starting");
        isFading = true;
        float currentAlpha = 1f;
        while(staminaBarObject.color.a > 0.01)
        {
            Color c = staminaBarObject.color;
            currentAlpha = Mathf.Lerp(currentAlpha, 0, 0.2f);
            staminaBarObject.color = new Color(c.r, c.g, c.b, currentAlpha);
            yield return new WaitForSeconds(0.01f);
        }

        isFading = false;

        // Is there a more concise way of only changing specific values of the color struct?
        staminaBarObject.color = new Color(originalColor.r, originalColor.g, originalColor.b, staminaBarObject.color.a); // This is to make sure the bar resets when it goes invisible

        //Debug.Log("Fade Out coroutine ending");
    }

    IEnumerator BlinkRed()
    {
        float period = 0.75f;   // The period of the red shift in and out
        float redShift;       // The percent of red shift we currently have for our UI


        while(true)
        {
            redShift = (0.5f * Mathf.Sin( ((2*Mathf.PI)/period) * Time.time ))  + 0.5f; // we don't want the redshift value ever going negative

            staminaBarObject.color = Color.Lerp(originalColor, Color.red, redShift);

            yield return new WaitForSeconds(0.05f);
        }

        
        
    }
}
