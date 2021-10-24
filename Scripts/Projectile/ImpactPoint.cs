using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactPoint
{
    public ImpactPoint(Vector3 i,float t){
        impactTime=t;
        impactLocation=i;
    }
    
    public Vector3 impactLocation;     // Location of projectile landing in world space
    public float impactTime;           // Time at which projectile will land at impactLocation
}
