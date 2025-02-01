using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    [SerializeField] private Transform pipeDestination;
    [SerializeField] private float pipeDownwardsOffset;   // the value the player lerps to on the y axis when they use the pipe
    [SerializeField] private float pipeUpwardsOffset;   // the same but upwards
    private float animDuration = 1f;
    

    public void GoDownPipe(GameObject thisPlr)
    {
        StartCoroutine(PipeSequence(thisPlr));
    }

    private IEnumerator PipeSequence(GameObject thisPlr)
    {
        FindObjectOfType<AudioManager>().Play("Pipe");
        thisPlr.GetComponent<BoxCollider2D>().enabled = false;
        thisPlr.GetComponent<Rigidbody2D>().gravityScale = 0f;
        thisPlr.GetComponent<PlayerMovement>().CanMove = false;

        float timeElapsed = 0;

        //thisPlr.transform.position = new Vector3(thisPlr.transform.position.x, Mathf.Lerp(thisPlr.transform.position.y, pipeDownwardsOffset, animDuration), thisPlr.transform.position.z);
        while (timeElapsed < animDuration)
        {
            thisPlr.transform.position = new Vector3(thisPlr.transform.position.x, Mathf.Lerp(thisPlr.transform.position.y, pipeDownwardsOffset, timeElapsed / animDuration), thisPlr.transform.position.z);
            timeElapsed += Time.deltaTime;

            yield return null;
        } 

        thisPlr.transform.position = new Vector3(thisPlr.transform.position.x, pipeDownwardsOffset, thisPlr.transform.position.z);

        yield return new WaitForSeconds(0.5f);
        thisPlr.transform.position = new Vector3(pipeDestination.position.x, thisPlr.transform.position.y, thisPlr.transform.position.z);
        FindObjectOfType<AudioManager>().Play("Pipe");
        timeElapsed = 0;
        //thisPlr.transform.position = new Vector3(thisPlr.transform.position.x, Mathf.Lerp(thisPlr.transform.position.y, pipeUpwardsOffset, animDuration), thisPlr.transform.position.z);
        while (timeElapsed < animDuration)
        {
            thisPlr.transform.position = new Vector3(thisPlr.transform.position.x, Mathf.Lerp(thisPlr.transform.position.y, pipeUpwardsOffset, timeElapsed / animDuration), thisPlr.transform.position.z);
            timeElapsed += Time.deltaTime;

            yield return null;
        } 

        //yield return new WaitForSeconds(0.2f);


        thisPlr.GetComponent<BoxCollider2D>().enabled = true;
        thisPlr.GetComponent<Rigidbody2D>().gravityScale = 2.3f;
        thisPlr.GetComponent<PlayerMovement>().CanMove = true;
    }
}
