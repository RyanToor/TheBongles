using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float floatSpeed, boostForce;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += floatSpeed * Time.deltaTime * Vector3.up;
        if (transform.position.y > 0 - GetComponent<SpriteRenderer>().bounds.extents.y)
        {
            Pop();
        }
    }

    private void Pop()
    {
        Destroy();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce((collision.gameObject.transform.position - transform.position) * boostForce, ForceMode2D.Impulse);
            Pop();
        }
    }
}
