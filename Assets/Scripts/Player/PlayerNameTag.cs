using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class PlayerNameTag : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text nameTag;
    [SerializeField] private PhotonView nameView;
    [SerializeField] private bool debugMode;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!debugMode)
        {
            nameTag.text = nameView.Owner.NickName;
        }
        
    }
}
