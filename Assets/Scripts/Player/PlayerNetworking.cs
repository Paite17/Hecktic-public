using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerNetworking : MonoBehaviour
{
    // this is for helping to ignore everything going on in the other players movement script for example
    public MonoBehaviour[] scriptsToIgnore;

    private PhotonView photonView;
    [SerializeField] private TextMeshPro playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        scriptsToIgnore = new MonoBehaviour[4];
        photonView = GetComponent<PhotonView>();

        if (!photonView.IsMine)
        {
            // manually add the opposing players scripts to scriptsToIgnore
            scriptsToIgnore[0] = GetComponent<PlayerMovement>();
            scriptsToIgnore[1] = GetComponent<PlayerInput>();
            scriptsToIgnore[2] = this;
            scriptsToIgnore[3] = GetComponent<PlayerStatus>();

            // disables the scripts themselves
            foreach (var scripts in scriptsToIgnore)
            {

                scripts.enabled = false;
            } 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // sets the username label above the player
    // redundant, remove later
    public void SetUserName()
    {
        playerNameText.text = PhotonNetwork.NickName;
    }
}
