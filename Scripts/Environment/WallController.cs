using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    enum Location
    {
        Left,
        Right
    }
    CourtController court;
    [SerializeField] Location wallPosition;
    [SerializeField] float moveRange = 0;
    [SerializeField] float period = 5;

    Vector3 initialPos;
    bool hasInitialPos = false; // Used for Gizmos logic
    void Awake()
    {
        court = FindObjectOfType<CourtController>();
        switch(wallPosition)
        {
            case Location.Right:
                transform.position = new Vector3(court.getRightCourtX(), transform.position.y, transform.position.z);
                break;
            case Location.Left:
                transform.position = new Vector3(court.getLeftCourtX(), transform.position.y, transform.position.z);
                break;
        }
        initialPos = transform.position;
        hasInitialPos = true;
    }

    // Update is called once per frame
    void Update()
    {
        float delta_z = moveRange * Mathf.Sin(Time.time * (2*Mathf.PI)/period);
        transform.position = initialPos + transform.forward * delta_z;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine((hasInitialPos? initialPos:transform.position) - transform.forward * moveRange, (hasInitialPos? initialPos:transform.position) + transform.forward * moveRange);
    }
}
