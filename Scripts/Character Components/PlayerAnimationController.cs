using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    private Animator animator;
    private Vector2 movement;
    private SectorAim sectorAim;
    // Start is called before the first frame update
    void Start()
    {
        animator=this.GetComponent<Animator>();
        sectorAim=this.GetComponent<SectorAim>();
    }

    // Update is called once per frame
    void Update()
    {
        //set the animation based on the movement of the player

        if(movement.x > 0.5f || movement.x < -0.5f){
            animator.SetBool("Walk Left",movement.x<0.5f);
            animator.SetBool("Walk Right",movement.x>0);
        }
        else
        {
            animator.SetBool("Walk Left",false);
            animator.SetBool("Walk Right",false);
        }
        
        if(movement.y > 0.5f || movement.y < -0.5f){
            animator.SetBool("Walk Up",movement.y>0);
            animator.SetBool("Walk Down",movement.y<0);
        }
        else
        {
            animator.SetBool("Walk Up",false);
            animator.SetBool("Walk Down",false);
        }

        
        //default: set to idle (done in the animation controller)
    }

    // public void swing(){
    //     animator.SetTrigger("Swing");
    // }

    //[SerializeField] Projectile gameProjectile;

    public void PlayCharge()
    {
        Vector3 mid = sectorAim.GetMidvector();
        Vector3 flatMidVector = new Vector3(mid.x, 0, mid.z);
        if(Vector3.Dot(flatMidVector, transform.right) <= -0.1f)            // When the player is roughly on the right side of the court, do the right charge (note that we add a bias towards the left because the right animation looks a bit too extreme for the middle)
            StartRightCharge();
        else
            StartLeftCharge();
    }

    void StartLeftCharge()
    {

        animator.SetTrigger("Charge Left");
    
    }

    void StartRightCharge()
    {
        animator.SetTrigger("Charge Right");
    }

    public void PlayBlock()
    {
        animator.SetTrigger("Block");
    }

    public void PlaySwing()
    {

        animator.SetTrigger("Swing");
    }



    public void UpdateVelocity(Vector2 moveVelocity)
    {
        movement = moveVelocity;
    }
}
