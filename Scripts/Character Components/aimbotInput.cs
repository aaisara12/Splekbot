using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aimbotInput : MonoBehaviour, ICharacterInput
{
    public float horizontal => 0;

    public float vertical => 0;

    public bool sprintKeyPressed => false;

    public bool fireKeyPressed {get; private set;}

    public bool blockKeyPressed => false;

    public float horizontalMouse {get; private set;}

    public event Action OnJumpKeyPressed;
    public event Action OnFireStart;
    public event Action OnFireEnd;

    [SerializeField] float chargeTime = 2f;
    [SerializeField] float speedPercent = 0.5f;
    [SerializeField] Transform opponent;

    public void Start()
    {
        horizontalMouse = 0;
        StartCoroutine(RepeatSwing());
    }

    public void Update()
    {
        if(Input.GetKey(KeyCode.Space))
            horizontalMouse = MechanicsHelper.CalculateMouseMovement(GetComponent<SectorAim>(), opponent.position, speedPercent);
        else
            horizontalMouse = 0;
    }

    IEnumerator RepeatSwing()
    {
        while(true)
        {
            fireKeyPressed = true;
            yield return new WaitForSeconds(chargeTime);
            fireKeyPressed = false;
            yield return new WaitForSeconds(0.5f);
        }
    }


}
