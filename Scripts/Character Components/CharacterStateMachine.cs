using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This is THE state machine script for ALL Splek players (user and enemies)

public class CharacterStateMachine : MonoBehaviour
{
    [Header("Transition Data")]
    [SerializeField] float swingCooldown = 0.5f;
    [Header("Charge State Data")]
    [SerializeField] float chargeRate = 1;
    [SerializeField] ParticleSystem chargeParticles;


    StateMachine machine;



    void Awake()
    {
        machine = new StateMachine();

        var input = GetComponent<ICharacterInput>();
        var movement = GetComponent<CharacterMovement>();
        var swing = GetComponent<CharacterSwing>();
        var animController = GetComponent<PlayerAnimationController>();

        var freeState = new FreeState(movement);
        var chargeState = new ChargeState(movement, swing, chargeRate, chargeParticles, animController);
        var swingState = new SwingState(movement, swing);
        var blockState = new BlockState(movement, swing);

        Func<bool> HasLetGoOfCharge() => () => !input.fireKeyPressed;
        Func<bool> HasSwingFinished() => () => true;    // NEEDS IMPLEMENTATION (get from animator controller?)


        Func<bool> CanStartSwing() => () => (Time.time >= nextValidSwingTime) && input.fireKeyPressed;


        machine.AddTransition(freeState, chargeState, CanStartSwing());
        machine.AddTransition(chargeState, swingState, HasLetGoOfCharge());
        machine.AddTransition(swingState, freeState, HasSwingFinished());


        machine.SetState(freeState);



        // Set up charge state event listeners
        chargeState.OnStart += RelayOnStartCharge;
        chargeState.OnEnd += RelayOnEndCharge;
        chargeState.OnNewPercent += RelayOnNewPercentCharge;

        // Set up swing state even listeners
        swingState.OnEnd += RegisterFinishSwing;
    }

    void Update() => machine.Tick();


    float nextValidSwingTime = 0;
    void RegisterFinishSwing()
    {
        nextValidSwingTime = Time.time + swingCooldown;
    }



    // CHARGE STATE EVENTS
    public event System.Action OnStartCharge;
    public event System.Action OnEndCharge;
    public event System.Action<float> OnNewPercentCharge;

    void RelayOnStartCharge() => OnStartCharge?.Invoke();
    void RelayOnEndCharge() => OnEndCharge?.Invoke();
    void RelayOnNewPercentCharge(float newPercent) => OnNewPercentCharge?.Invoke(newPercent);


}
