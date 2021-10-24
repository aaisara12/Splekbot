using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockWallInput : MonoBehaviour, ICharacterInput
{
    public float horizontal {get; private set;}

    public float vertical {get; private set;}

    public bool sprintKeyPressed {get; private set;}

    public bool fireKeyPressed {get; private set;}

    public bool blockKeyPressed {get; private set;}

    public float horizontalMouse {get; private set;}

    public event Action OnJumpKeyPressed;
    public event Action OnFireStart;
    public event Action OnFireEnd;

    BoxCollider wallCollider;
    [SerializeField] Projectile gameProjectile;

    void Awake()
    {
        wallCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        CheckBounds();
    }

    void CheckBounds()
    {
        Collider[] projectiles = Physics.OverlapBox(wallCollider.center, wallCollider.bounds.extents);
        foreach(var thing in projectiles)
        {
            Projectile projectile = thing.GetComponent<Projectile>();
            if(projectile == gameProjectile)
            {
                StartCoroutine(DoBlockShot());
            }
        }
        
    }
    IEnumerator DoBlockShot()
    {
        Debug.Log("WALL BLOCK");
        OnFireStart?.Invoke();
        fireKeyPressed = true;
        yield return new WaitForSeconds(0.1f);
        OnFireEnd?.Invoke();
        fireKeyPressed = false;
    }


}
