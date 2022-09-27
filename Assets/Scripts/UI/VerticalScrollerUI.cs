using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollerUI : MinigameUI
{
    public GameObject[] healthObjects;
    public Sprite[] shieldSprites;

    private Player player;

    // Start is called before the first frame update
    protected override void Start()
    {
        starScoreIndex = 0;
        player = GameObject.Find("Player").GetComponent<Player>();
        secondaryCounterText = transform.Find("ReadoutPanel/DepthPanel/Depth").GetComponent<TMPro.TextMeshProUGUI>();
        base.Start();
        for (int i = 0; i < 3; i++)
        {
            healthObjects[3 + i].GetComponent<Image>().sprite = shieldSprites[Mathf.Clamp(GameManager.Instance.upgrades[0][0] - 1, 0, shieldSprites.Length - 1)];
        }
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