using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// THE TARGET WILL NEED TO BE SET PROPERLY FOR WHICHEVER PLAYER YOU DECIDE TO BE
public class CameraFollow : MonoBehaviour
{
    // player target for camera
    public Transform target;
    public Vector3 offset;

    public bool dontFollow;
    public bool allowYAxis;

    public bool hasConnected;

    public float yAxisMinimum; // the lowest the camera can go on the y axis (make it 4)

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        UpdateLocation();
        /*
        if (hasConnected)
        {
            if (!dontFollow)
            {
                Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y, offset.z);

                if (desiredPosition.y < yAxisMinimum)
                {
                    desiredPosition.y = yAxisMinimum;
                }

                transform.position = desiredPosition;
                
            }

            // TODO: have the camera offset lerp off after moving a bit so that the player can see further, then have it centre back when the player stops moving
        }
        */

    }

    public void UpdateLocation()
    {
        if (hasConnected)
        {
            if (!dontFollow)
            {
                if (target != null)
                {
                    Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y, offset.z);

                    if (desiredPosition.y < yAxisMinimum)
                    {
                        desiredPosition.y = yAxisMinimum;
                    }

                    transform.position = desiredPosition;
                }
                

            }

            // TODO: have the camera offset lerp off after moving a bit so that the player can see further, then have it centre back when the player stops moving
        }
    }

    // when a transition occurs we need to correct the Y axis value
    public void CameraCorrection()
    {
        transform.position = target.position + offset;
    }


    // change to a new target gameObject
    public void ChangeTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
