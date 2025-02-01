using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map Data", menuName = "Create New Menu Data")]
public class MapData : ScriptableObject
{
    [SerializeField] private string mapName;
    [SerializeField] private float player1SpawnX;
    [SerializeField] private float player1SpawnY;
    [SerializeField] private float player2SpawnX;
    [SerializeField] private float player2SpawnY;
    [SerializeField] private float player3SpawnX;
    [SerializeField] private float player3SpawnY;
    [SerializeField] private float player4SpawnX;
    [SerializeField] private float player4SpawnY;
    [SerializeField] private float leftSideEdge;
    [SerializeField] private float rightSideEdge;
    [SerializeField] private List<Vector2> mcguffinSpawnLocations;
    [SerializeField] private bool stageisVertical;

    public string MapName
    {
        get { return mapName; }
    }

    public float Player1SpawnX
    {
        get { return  player1SpawnX; }
    }

    public float Player1SpawnY
    { 
        get { return player1SpawnY; } 
    }

    public float Player2SpawnX
    {
        get { return player2SpawnX; }
    }

    public float Player2SpawnY
    {
        get { return  player2SpawnY; }
    }

    public float Player3SpawnX
    {
        get { return player3SpawnX; }
    }

    public float Player3SpawnY
    {
        get { return player3SpawnY; }
    }

    public float Player4SpawnX
    {
        get { return player4SpawnX; }
    }

    public float Player4SpawnY
    {
        get { return player4SpawnY; }
    }

    public float LeftSideEdge
    {
        get { return leftSideEdge; }
    }

    public float RightSideEdge
    {
        get { return rightSideEdge; }
    }

    public List<Vector2> McguffinSpawnLocations
    {
        get { return mcguffinSpawnLocations; }
    }

    public bool StageisVertical
    {
        get { return stageisVertical; }
    }
}
