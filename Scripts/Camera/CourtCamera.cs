using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtCamera : MonoBehaviour
{
    [SerializeField] Transform player = null;

    [Range(0, 1)]
    [SerializeField] float parallaxRatio = 0.1f;

    [Range(0, 20)]
    [SerializeField] float damp = 10;

    Vector3 playerCenter = Vector3.zero;
    Vector3 cameraCenter = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        playerCenter = player.position;
        cameraCenter = transform.position;
    }


    void LateUpdate()
    {
        float playerHorizontalChange = Vector3.Dot((player.position - playerCenter), player.right);      // How far right the player has deviated from his original position

        float playerForwardChange = Vector3.Dot((player.position - playerCenter), player.forward);


        Vector3 targetCameraPosition = cameraCenter + (transform.right * playerHorizontalChange * parallaxRatio) + transform.forward * playerForwardChange;
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, map(damp));
    }

    float map(float damp) => 0.1f - ((0.09f)*(damp/20));        // Attempts to map damp value from range (0:20) -- which is standard -- to (0.1:0.01) -- which most closely mirrors damp effect of cinemachine dolly cameras

}
