using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollerUI : MonoBehaviour
{
    private Player player;
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        text = transform.Find("ReadoutPanel/Plastic").GetComponent<Text>();
        print(text.text);
    }

    // Update is called once per frame
    void Update()
    {
        text.text = player.collectedPlastic.ToString();
    }
}
