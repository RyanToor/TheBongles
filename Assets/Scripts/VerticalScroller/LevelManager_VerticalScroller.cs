using System.Collections;
using System.Collections.Generic;
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
        StartCoroutine(CheckLoaded());
        base.Start();
    }

    protected override void SendSaveData()
    {
        base.SendSaveData();
    }

    private IEnumerator CheckLoaded()
    {
        while (chunkManager.chunksLoaded <= chunkManager.chunkBuffer)
        {
            yield return null;
        }
        Destroy(GameObject.Find("LoadingCanvas(Clone)"));
        player.isloaded = true;
        player.twin.GetComponent<Collider2D>().enabled = true;

        GameObject.Find("SoundManager").GetComponent<AudioManager>().PlayMusic("Trash Hunt");

        chunkManager.InitialiseToppers();
    }
}
