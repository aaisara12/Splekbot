using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MechanicsHelper
{
    ///<summary>
    /// Returns a normalized direction vector from myPos pointing towards opponentPos
    ///</summary>
    public static Vector2 GetAimDirection(Vector3 myPos, Vector3 opponentPos)
    {
        Vector2 myPos_2D = new Vector2(myPos.x, myPos.z);
        Vector2 theirPos_2D = new Vector2(opponentPos.x, opponentPos.z);

        return (theirPos_2D - myPos_2D).normalized;
    }


    ///<summary>
    /// Returns a horizontal mouse movement delta that will move <paramref name="mySectorAim"/>'s aim direction so that it points more closely at <paramref name="aimAtPosition"/> (magnitude scaled with <paramref name="aimSpeedFraction"/>)
    ///</summary>

    ///<param name="aimSpeedFraction">Speed at which aim should be changed (from 0 to 1)</param>
    ///<param name="aimAtPosition">The desired position to point at in world space</param>
    ///<param name="mySectorAim">The SectorAim component attached to the aimer</param>
    public static float CalculateMouseMovement(SectorAim mySectorAim, Vector3 aimAtPosition, float aimSpeedFraction)
    {
        if(aimSpeedFraction > 1)
            aimSpeedFraction = 1;
        if(aimSpeedFraction < 0)
            aimSpeedFraction = 0;

        Vector2 currentAim = mySectorAim.GetAimDirection();
        Vector2 targetAim = GetAimDirection(mySectorAim.transform.position, aimAtPosition);  // We assume that the swinger's position is the same as the SectorAim object's
        return Vector2.SignedAngle(currentAim, targetAim) * (aimSpeedFraction * 0.2f);      // Note that 0.2 is just a constant that makes the speed ranges feel just about right
    }
}
