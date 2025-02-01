using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    public int ID;
    public string charName;
    public GameObject charObject;  // this is exclusively used on the char select screen to display the correct model


    // returns the name of a character relative to their ID to be used in GameManager specifically for spawning the right player prefab
    public static string GetCharacterName(int charID)
    {
        string charName = string.Empty;
        switch (charID)
        {
            case 1:
                charName = "Lew";
                break;
            case 2:
                charName = "Bryngles";
                break;
            case 3:
                charName = "Chum";
                break;
            case 4:
                charName = "King Chod";
                break;
            default:
                charName = "Lew";
                break;
        }

        return charName;
    }

    // like the one above but for the charstate instead
    public static PlayableCharacterState GetCharacterState(int charID)
    {
        PlayableCharacterState thisChar = PlayableCharacterState.LEW;
        switch (charID)
        {
            case 1:
                thisChar = PlayableCharacterState.LEW;
                break;
            case 2:
                thisChar = PlayableCharacterState.BRYNGLES;
                break;
            case 3:
                thisChar = PlayableCharacterState.CHUM;
                break;
            case 4:
                thisChar = PlayableCharacterState.KING_CHOD;
                break;
        }

        return thisChar;
    }
}
