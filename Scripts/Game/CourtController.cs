using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtController : MonoBehaviour
{

    [Header ("Court Location")]
    public bool showCourt=true;
    public Transform midPoint;
    public float width;
    public float depth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(showCourt)
            debugDrawCourt();   
    }


    public float getTopCourtZ(){
        return midPoint.position.z+depth/2;
    }
    
    public float getBotCourtZ(){
        return midPoint.position.z-depth/2;
    }
    public float getLeftCourtX(){
        return midPoint.position.x-width/2;
    }    
    public float getRightCourtX(){
        return midPoint.position.x+width/2;
    }

    public Vector3 getTopLeftCorner(){
        return new Vector3(midPoint.position.x+width/2,midPoint.position.y,midPoint.position.z+depth/2);
    }

    public Vector3 getBotLeftCorner(){
        return new Vector3(midPoint.position.x+width/2,midPoint.position.y,midPoint.position.z-depth/2);
    }

    public Vector3 getTopRightCorner(){
        return new Vector3(midPoint.position.x-width/2,midPoint.position.y,midPoint.position.z+depth/2);
    }

    public Vector3 getBotRightCorner(){
        return new Vector3(midPoint.position.x-width/2,midPoint.position.y,midPoint.position.z-depth/2);
    }
    public Vector3 getLeftMidpoint(){
        return new Vector3(midPoint.position.x-width/2,midPoint.position.y,midPoint.position.z);
    }
    public Vector3 getRightMidpoint(){
        return new Vector3(midPoint.position.x+width/2,midPoint.position.y,midPoint.position.z);
    }


    private void debugDrawCourt(){
        Vector3 topLeftCorner = getTopLeftCorner();
        Vector3 botLeftCorner = getBotLeftCorner();
        Vector3 topRightCorner = getTopRightCorner();
        Vector3 botRightCorner = getBotRightCorner();

        //draw a box
        Debug.DrawLine(topLeftCorner,botLeftCorner,Color.green,Time.deltaTime);
        Debug.DrawLine(topLeftCorner,topRightCorner,Color.green,Time.deltaTime);
        Debug.DrawLine(botRightCorner,topRightCorner,Color.green,Time.deltaTime);
        Debug.DrawLine(botRightCorner,botLeftCorner,Color.green,Time.deltaTime);

        //midline
        
        Debug.DrawLine(getLeftMidpoint(),getRightMidpoint(),Color.magenta,Time.deltaTime);
    }


    public Vector3 getMidpoint(){
        return midPoint.transform.position;
    }


    public Vector3 AISideMidpoint(){
        Vector3 mp = new Vector3(midPoint.position.x,midPoint.position.y,midPoint.position.z+depth/4);
        return mp;
    }

    public Vector3 PlayerSideMidpoint(){
        Vector3 mp = new Vector3(midPoint.position.x,midPoint.position.y,midPoint.position.z-depth/4);
        return mp;
    }

    public bool positionInsideCourt(Vector3 position,float leeway){
        if(Mathf.Abs(position.x) > midPoint.position.x+width/2+leeway)
            return false;
        if(Mathf.Abs(position.z) > midPoint.position.z+depth/2+leeway)
            return false;
        return true;
    }

    public bool positionInsideSidelines(Vector3 position, float leeway)
    {
        if(Mathf.Abs(position.x-midPoint.position.x) < width/2+leeway)
            return true;
        return false; 
    }


    public MidPoints GetRelativeMidpoints(Transform subjectTransform)
    {
        Vector3 forwardDirection = subjectTransform.forward;    // This is the subject's forward vector

        MidPoints mpts = new MidPoints();
        //Player facing AI
        if(forwardDirection.z>0){
            mpts.leftMidpoint = getLeftMidpoint();
            mpts.rightMidpoint = getRightMidpoint();
        }
        //AI Facing Player
        else{
            mpts.leftMidpoint = getRightMidpoint();
            mpts.rightMidpoint = getLeftMidpoint(); 
        }

        return mpts;
    }


    public bool pointInsidePlayerSide(Vector3 point){
        return point.z <= midPoint.position.z && point.x >= midPoint.position.z-depth/2;
    }


    // public bool pointOnPlayerSide(Vector3 point){
    //     return point.z <= midPoint.position.z;
    // }
    // public bool pointOnAISide(Vector3 point){
    //     return point.z <= midPoint.position.z;
    // }

    public int getSide(Vector3 point){
        if(point.z >= midPoint.position.z+depth/2)
            return 1;
        else
            return -1;
    }

}


public struct MidPoints
{
    public Vector3 leftMidpoint;
    public Vector3 rightMidpoint;
}