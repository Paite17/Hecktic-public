using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Collision handling for breakable bricks
public class BreakableBrick : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.tag == "Player" && collision.gameObject.GetComponent<PlayerMovement>().powerupState == CharPowerupState.GIANT)
        {
            Vector2 velocityOfPlayer = collision.gameObject.GetComponent<Rigidbody2D>().velocity;
            GetComponent<Collider2D>().enabled = false;
            gameObject.SetActive(false);
            FindObjectOfType<AudioManager>().Play("BrickBreak");

            // fuckin hell
            if (collision.gameObject.GetComponent<PlayerMovement>().IsFacingRight)
            {
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(6f, collision.gameObject.GetComponent<Rigidbody2D>().velocity.y);
            }
            else
            {
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-6f, collision.gameObject.GetComponent<Rigidbody2D>().velocity.y);
            }
        }

        if (collision.gameObject.tag == "Rock")
        {
            gameObject.SetActive(false);
            FindObjectOfType<AudioManager>().Play("BrickBreak");
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // detect if the top of the player's collider is hitting the block
            if (contact.normal.y > 0.9f && collision.gameObject.tag == "Player")
            {
                // TODO: the particle + SFX

                FindObjectOfType<AudioManager>().Play("BrickBreak");
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "BellyFlop":
                FindObjectOfType<AudioManager>().Play("BrickBreak");
                gameObject.SetActive(false);     
                break;
        }
    }
}
