using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollerUI : MinigameUI
{
    public GameObject[] healthObjects;

    private Player player;

    // Start is called before the first frame update
    protected override void Start()
    {
        starScoreIndex = 0;
        player = GameObject.Find("Player").GetComponent<Player>();
        secondaryCounterText = transform.Find("ReadoutPanel/DepthPanel/Depth").GetComponent<Text>();
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        secondaryCount = Mathf.Abs(Mathf.Clamp(Mathf.Ceil(player.transform.position.y), -Mathf.Infinity, 0));
        for (int i = 0; i < healthObjects.Length; i++)
        {
            healthObjects[i].SetActive(i < player.health);
        }
        base.Update();
    }
}