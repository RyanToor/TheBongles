using UnityEngine;

public class Twin : MonoBehaviour
{
    public TrashManager trashManager;
    public Player player;

    private AudioManager audioManager;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        trashManager = GameObject.Find("TrashContainer").GetComponent<TrashManager>();
        animator = GetComponent<Animator>();
        GetComponent<Collider2D>().enabled = false;
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
            audioManager.PlaySFXAtLocation("Crinkle", collision.transform.position);
            animator.SetTrigger("Grab");
        }
    }
}
