using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowScript : MonoBehaviour
{


    private SpriteRenderer shadowSprite;
    private GameObject shadowGameObject;
    private float spriteHeight;

    private SpriteRenderer parentSprite;
    // Start is called before the first frame update
    void Start()
    {

        //create a new shadow game object
        shadowGameObject = new GameObject(this.name+" shadow");
        shadowSprite = shadowGameObject.AddComponent<SpriteRenderer>();

        //make sure its a child of our current object
        shadowGameObject.transform.parent=this.gameObject.transform;

        //check if this object has a sprite renderer
        parentSprite = this.GetComponent<SpriteRenderer>();
        if(!parentSprite)
        {
            throw new System.Exception("Shadow Script: Parent object does not have sprite to shadow");
        }

        //copy all the specifics of the sprite
        shadowSprite.sprite = parentSprite.sprite;
        shadowSprite.color= new Color(0, 0, 0, 0.5f); //Color.black;
        //save the height of the sprite so its possible to calculate the correct height from the ground
        spriteHeight=shadowSprite.bounds.extents.y;


    }

    // Update is called once per frame
    void Update()
    {
        shadowSprite.sprite = parentSprite.sprite;
        castShadow();
    }

    void castShadow(){
        //find the ground 

        RaycastHit raycastHit;


        if (Physics.Raycast(this.transform.position, (Vector3.down),out raycastHit, Mathf.Infinity,LayerMask.GetMask("Ground"))){

            //define where the shadow should be
            shadowGameObject.transform.position=raycastHit.point;
            shadowGameObject.transform.position = new Vector3(this.transform.position.x,raycastHit.point.y+0.1f,this.transform.position.z);
            shadowGameObject.transform.rotation = Quaternion.Euler(-90,0,0);
            
            //scale it based off of distance from the ground (default is 1)
            float ratio =  1f;


            
            //if we are not exactly at the ground (which would be scale 1), determine the scale based on how high we are
            if(Mathf.Abs(raycastHit.point.y - (this.transform.position.y+spriteHeight))!=0){
                ratio = this.transform.localScale.x / Mathf.Abs(raycastHit.point.y - (this.transform.position.y+spriteHeight)) ;
            }

            //actually set the scale
            shadowGameObject.transform.localScale = new Vector3(1,1,1)*ratio;
        }

    }
}
