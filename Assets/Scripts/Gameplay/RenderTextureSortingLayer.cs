using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    // purpose of this is to just change the sorting layer of the render texture object so that the parallax background doesn't get in the way of it
public class RenderTextureSortingLayer : MonoBehaviour
{
    [SerializeField] private int sortingLayer;
    private Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.sortingOrder = sortingLayer;
    }
}
