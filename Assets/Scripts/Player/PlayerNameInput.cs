using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerNameInput : MonoBehaviour
{
    // storing to playerpref
    const string PLAYER_NAME_PREFKEY = "PlayerName";

    // Start is called before the first frame update
    void Start()
    {
        string defaultName = string.Empty;
        TMP_InputField inputField = GetComponent<TMP_InputField>();

        if (inputField != null)
        {
            if (PlayerPrefs.HasKey(PLAYER_NAME_PREFKEY))
            {
                defaultName = PlayerPrefs.GetString(PLAYER_NAME_PREFKEY);
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    // sets the name of the player and lets it get saved in playerprefs
    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player name is null or empty!");
            return;
        }

        PhotonNetwork.NickName = value;
        PlayerPrefs.SetString(PLAYER_NAME_PREFKEY, value);
    }
}
