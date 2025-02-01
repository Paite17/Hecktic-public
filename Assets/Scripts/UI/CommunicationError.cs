using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class CommunicationError : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<AudioManager>().Play("ConnectionLost");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene(1);
        }
    }
}
