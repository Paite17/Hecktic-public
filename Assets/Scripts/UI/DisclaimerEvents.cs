using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisclaimerEvents : MonoBehaviour
{
    [SerializeField] private GameObject logo;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DisclaimerSequence());
    }

    private IEnumerator DisclaimerSequence()
    {
        yield return new WaitForSeconds(0.5f);
        logo.SetActive(true);
        FindObjectOfType<AudioManager>().Play("CoinCollect");
        yield return new WaitForSeconds(2.5f);

        SceneManager.LoadScene("MainMenu");

    }
}
