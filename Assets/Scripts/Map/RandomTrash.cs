using UnityEngine;

public class RandomTrash : MonoBehaviour
{
    public Sprite sprite;
    public string trashType;
    public FloatingObjects floatingObjectsScript;

    // Start is called before the first frame update
    void Start()
    {
        transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = sprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            if (!collision.CompareTag("Region") || collision.CompareTag("Region") && !collision.gameObject.GetComponent<Region>().isUnlocked)
            {
                GameObject.Find("Map").GetComponent<Map>().SpawnRandomTrash();
                floatingObjectsScript.objectsToRemove.Add(gameObject);
            }
        }
    }
}