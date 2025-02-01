using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float effectMultiplier;
    public bool dontBeFunny;
    private Transform camPos;
    public Vector3 lastCamPos;

    public float offsetX = 0;

    private PhotonView plrView;


    // Start is called before the first frame update
    void Start()
    {

        camPos = Camera.main.transform;
        lastCamPos = camPos.position;
        //plrView = camPos.GetComponent<CameraFollow>().target.GetComponent<PhotonView>();

    }

    private void LateUpdate()
    {
        if (camPos != null)
        {
            //float usedEffectMultiplier = effectMultiplier;
            //Vector3 deltaMovemnt = camPos.position - lastCamPos;

            //transform.position += deltaMovemnt * effectMultiplier;

            
            if (!dontBeFunny)
            {
                transform.position = new Vector3(offsetX + (camPos.position.x - offsetX) * effectMultiplier, transform.position.y, transform.position.z);
            }
            else
            {
                Vector3 deltaMovemnt = camPos.position - lastCamPos;
                //transform.position += deltaMovemnt * effectMultiplier;

                transform.position = new Vector3(transform.position.x + deltaMovemnt.x * effectMultiplier, transform.position.y, transform.position.z);
                //transform.position = new Vector3(deltaMovemnt.x * effectMultiplier.x, deltaMovemnt.y * effectMultiplier.y, transform.position.z);
                lastCamPos = camPos.position;

            }
            
            //transform.position = new Vector3(deltaMovemnt.x * effectMultiplier.x, deltaMovemnt.y * effectMultiplier.y, transform.position.z);
            //lastCamPos = camPos.position;
            
        }       

        
    }


}
