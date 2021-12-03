using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twin : MonoBehaviour
{
    public TrashManager trashManager;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        trashManager = GameObject.Find("TrashContainer").GetComponent<TrashManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int chunkIndex = Mathf.FloorToInt(-collision.transform.position.y / trashManager.chunkHeight);
        int objectIndex = 0;
        for (int i = 0; i < trashManager.currentTrash[chunkIndex].Count; i++)
        {
            if (trashManager.currentTrash[chunkIndex][i] == collision.gameObject)
            {
                objectIndex = i;
                break;
            }
        }
        if (!trashManager.loadedTrash[chunkIndex][objectIndex].isDangerous)
        {
            player.collectedPlastic++;
            trashManager.objectsToRemove.Add(new Unity.Mathematics.int2(chunkIndex, objectIndex));
        }
    }
}
