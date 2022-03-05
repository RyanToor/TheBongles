using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float mineRadius;
    public void MineHit()
    {
        GetComponent<Animator>().SetTrigger("Hit");
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, mineRadius);
        foreach (Collider2D collider in nearbyObjects)
        {
            if (collider.CompareTag("RandomTrash"))
            {
                Destroy(collider.gameObject);
            }
        }
    }
}
