using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class Debug3DAnimationTest : MonoBehaviour
{
    [SerializeField] private Animator modelAnim;
    private bool move;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed");
            move = true;
            modelAnim.SetBool("Move", move);
        }

    }
}
