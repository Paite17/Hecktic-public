using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySpawnPoint : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private GameObject enemyToSpawn;

    public bool hasSpawned;

    public GameObject EnemyToSpawn
    {
        get { return enemyToSpawn; }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(hasSpawned);
        }
        else
        {
            hasSpawned = (bool)stream.ReceiveNext();
        }
    }
}
