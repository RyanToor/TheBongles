using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowTheRobotUI : MonoBehaviour
{
    private Text metalReadout, pieReadout;
    private LevelManager_Robot levelManager;
    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager_Robot>();
        metalReadout = transform.Find("Metal Panel/Metal").GetComponent<Text>();
        pieReadout = transform.Find("Pie Panel/Pies").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        metalReadout.text = levelManager.metal.ToString();
        pieReadout.text = levelManager.Pies.ToString();
    }
}
