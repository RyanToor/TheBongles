using UnityEngine;

public class Twin : MonoBehaviour
{
    public TrashManager trashManager;
    public Player player;

    private AudioManager audioManager;
    private Animator animator;
    private LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        trashManager = GameObject.Find("TrashContainer").GetComponent<TrashManager>();
        animator = GetComponent<Animator>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
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
            levelManager.plastic++;
            trashManager.objectsToRemove.Add(new Unity.Mathematics.int2(chunkIndex, objectIndex));
            AudioSource grabSound = audioManager.PlaySFXAtLocation("Plastic", collision.transform.position, 1000);
            grabSound.minDistance = 5;
            animator.SetTrigger("Grab");
            GameManager.Instance.SpawnCollectionIndicator(collision.transform.position, levelManager.collectionIndicatorColor);
        }
    }
}