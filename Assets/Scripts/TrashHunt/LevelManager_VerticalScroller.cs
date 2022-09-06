using System.Collections;
using UnityEngine;

public class LevelManager_VerticalScroller : LevelManager
{
    public ChunkManager chunkManager;
    public Player player;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void SendSaveData()
    {
        base.SendSaveData();
    }

    protected override IEnumerator CheckLoaded()
    {
        promptManager.Prompt();
        while (chunkManager.chunksLoaded <= chunkManager.chunkBuffer)
        {
            yield return null;
        }
        player.isloaded = true;
        player.twin.GetComponent<Collider2D>().enabled = true;
        chunkManager.InitialiseToppers();
        StartCoroutine(base.CheckLoaded());
    }
}
