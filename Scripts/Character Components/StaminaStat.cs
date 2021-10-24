using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaStat : MonoBehaviour
{
    public event System.Action<float> OnStaminaChanged;     // Event that is broadcast when the stamina value changes
    public event System.Action OnExhausted;
    public event System.Action OnRecovered;
    [SerializeField] bool isExhausted = false;

    [SerializeField] float maxStamina = 100;
    [SerializeField] float stamina = 100;
    [SerializeField] float regenRate = 5;       // Stamina units per second regen
    [SerializeField] float regenCooldown = 2;
    float lastLossTime = 0;                  // The time at which the last stamina loss took place

    // Start is called before the first frame update
    void Start()
    {
        if(OnStaminaChanged != null)        // This is here to initialize the UI bar 
            OnStaminaChanged(stamina);
    }

    void Update()
    {

        if(Time.time - lastLossTime >= regenCooldown && stamina < maxStamina)
        {
            stamina += regenRate * Time.deltaTime;
            if(stamina >= maxStamina)
            {
                stamina = maxStamina;
                isExhausted = false;
                OnRecovered?.Invoke();
            }
            if(OnStaminaChanged != null)
                OnStaminaChanged(stamina);
        }
    }

    public bool LoseStamina(float amount)      // Returns whether the character successfully exhausted stamina (if the given amount of stamina to remove was available in the stamina pool)
    {
        if(amount <= 0 || isExhausted)     // If invalid stamina amount or player is exhausted, then don't do anything and signal an error
            return false;
        
        if(amount > stamina)
        {
            amount = stamina;       // Ensures you will lose the last bit of stamina
            isExhausted = true;
            OnExhausted?.Invoke();
        }

        stamina -= amount;
        lastLossTime = Time.time;
        
        if(OnStaminaChanged != null)
            OnStaminaChanged(stamina);

        return true;
    }

    public float GetMaxStamina() => maxStamina;

    public void ResetStamina(){
        stamina=maxStamina;
        isExhausted = false;
        OnStaminaChanged?.Invoke(stamina);
        OnRecovered?.Invoke();
    }
}
