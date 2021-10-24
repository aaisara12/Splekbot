using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorAim : MonoBehaviour
{
    //[SerializeField] Transform leftMidpoint;
    //[SerializeField] Transform rightMidpoint;
    [SerializeField] CourtController courtController;
    Vector3 leftMidPoint;
    Vector3 rightMidPoint;

    Vector3 leftBoundVector;    // The direction vector parallel to the ray that defines the leftmost edge of the sector
    Vector3 rightBoundVector;   // The direction vector parallel to the ray that defines the rightmost edge of the sector
    Vector3 midVector;          // The direction vector parallel to the ray that divides the sector in half

    [Range(-100, 100)]
    [SerializeField] float percentFromMid;  // The percent of the way between the mid line of the sector and the right edge

    Vector2 aimDirection;      // The direction the player is aiming

    ICharacterInput input;

    public event System.Action<Vector2> OnChangeDirection;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        input = GetComponent<ICharacterInput>();

        MidPoints midPoints = courtController.GetRelativeMidpoints(transform);
        leftMidPoint = midPoints.leftMidpoint;
        rightMidPoint = midPoints.rightMidpoint;

        //midvector definition shenanigans
        leftBoundVector = GetNormalizedVectorBetween(transform.position, leftMidPoint);
        rightBoundVector = GetNormalizedVectorBetween(transform.position, rightMidPoint);
        midVector = (leftBoundVector + rightBoundVector).normalized;

    }

    // Update is called once per frame
    void Update()
    {
        ModifyPercent(input.horizontalMouse);

        leftBoundVector = GetNormalizedVectorBetween(transform.position, leftMidPoint);
        rightBoundVector = GetNormalizedVectorBetween(transform.position, rightMidPoint);
        midVector = (leftBoundVector + rightBoundVector).normalized;
        
        // GetAngleBetween is used to calculate how many degrees we currently have to work with on either side of the mid vector
        // Note that this angle changes as we move relative to the left and right mid court points.
        // Once we know how many degrees we have to work with, we take a percentage of that angle and set our direction vector to point at that angle relative to the midvector.
        // We do this so that the aim vector will maintain the same percentage of angle away from the mid vector as the player moves.
        aimDirection = GetRotatedVector(new Vector2(midVector.x, midVector.z), GetAngleBetween(midVector, rightBoundVector) * (percentFromMid/100));
        OnChangeDirection?.Invoke(aimDirection);
    }

    void OnDrawGizmos()
    {
        // Draw the boundary of the sector
        Gizmos.color = Color.red;       
        Gizmos.DrawLine(transform.position, leftMidPoint);
        Gizmos.DrawLine(transform.position, rightMidPoint);

        // Draw the midline of the sector
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, midVector * 2);

        // Draw the aim vector
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, new Vector3(aimDirection.x, 0, aimDirection.y));
    }

    //Vector3 GetNormalizedVectorBetween(Transform looker, Transform other) => (other.position - looker.position).normalized;
    Vector3 GetNormalizedVectorBetween(Vector3 lookerPos, Vector3 otherPos) => (otherPos - lookerPos).normalized;

    Vector2 GetRotatedVector(Vector2 startVector, float deg)        // Returns the input vector rotated by degrees
    {
        deg *= Mathf.Deg2Rad;
        Vector2 col1 = new Vector2(Mathf.Cos(deg), Mathf.Sin(deg));
        Vector2 col2 = new Vector2(-Mathf.Sin(deg), Mathf.Cos(deg));

        return (col1 * startVector.x) + (col2 * startVector.y);
    }

    float GetAngleBetween(Vector3 from, Vector3 to)
    {
        return Vector2.SignedAngle(new Vector2(from.x, from.z), new Vector2(to.x, to.z));
    }

    void ModifyPercent(float amount)    // Safely change the aim direction (without going out of bounds)
    {
        percentFromMid += amount;
        if(percentFromMid > 100)
            percentFromMid = 100;
        if(percentFromMid < -100)
            percentFromMid = -100;
    }

    public Vector2 GetAimDirection() => aimDirection;

    public Vector3 GetMidvector()
    {
        return midVector;
    }

}
