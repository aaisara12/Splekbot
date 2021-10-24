using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform follow = null;
    private float maintainDist = 0;                 // The distance away from the follow target to maintain
    private Vector3 targetPosition = Vector3.zero;  // The position we want the camera to eventually get to

    [Range(0, 20)]
    [SerializeField] private float forwardDamp = 0.2f;  // A measure from 0 to 1 of how much to dampen forwards/backwards camera movement


    void Start()
    {
        maintainDist = transform.position.z - follow.position.z;    // Initialize the distance from the follow target as the starting distance
    }


    void LateUpdate()
    {
        // TODO: Maintain a constant forward distance from 
        targetPosition = new Vector3(transform.position.x, transform.position.y, follow.position.z + maintainDist);

        transform.position = Vector3.Lerp(transform.position, targetPosition, map(forwardDamp));
    }

    float map(float damp) => 0.1f - ((0.09f)*(damp/20));        // Attempts to map damp value from range (0:20) -- which is standard -- to (0.1:0.01) -- which most closely mirrors damp effect of cinemachine dolly cameras

}
