using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrashRequests : MonoBehaviour
{
    public float singleFuelReward, doubleFuelReward;
    public int singleGlassBonus, doubleGlassBonus;
    public GameObject trashPrefab;
    public Image[] requestSprites;
    public NameSprite[] requestSpriteArray;

    [Header("Vibration Data")]
    [SerializeField] float fulfilledIntensity;
    [SerializeField] float fulfilledDuration;
    [SerializeField] AnimationCurve fulfilled1, fulfilled2;

    private LevelManager_ScrapGrabber levelManager;
    private string[] currentRequests;
    private bool[] locked;

    void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager_ScrapGrabber>();
        currentRequests = new string[2]
        {
            requestSpriteArray[Random.Range(0, requestSpriteArray.Length)].name,
            requestSpriteArray[Random.Range(0, requestSpriteArray.Length)].name
        };
        locked = new bool[2] { false, false };
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < requestSpriteArray.Length; j++)
            {
                if (requestSpriteArray[j].name == currentRequests[i])
                {
                    requestSprites[i].sprite = requestSpriteArray[j].sprite;
                    break;
                }
            }
        }
    }

    public void CheckCatch(List<string> caughtNames)
    {
        if (!locked[0])
        {
            int requestsFulfilled = 0;
            foreach (string caughtName in caughtNames)
            {
                if (currentRequests[0] == caughtName)
                {
                    requestsFulfilled = 1;
                    caughtNames.Remove(caughtName);
                    GetComponent<Animator>().SetTrigger("Switch");
                    levelManager.brainy.SetTrigger("Celebrate");
                    break;
                }
            }
            if (requestsFulfilled == 1)
            {
                foreach (string caughtName in caughtNames)
                {
                    if (currentRequests[1] == caughtName)
                    {
                        requestsFulfilled = 2;
                        break;
                    }
                }
                switch (requestsFulfilled)
                {
                    case 1:
                        levelManager.glass += singleGlassBonus;
                        GameManager.Instance.SpawnCollectionIndicator(GameObject.FindGameObjectWithTag("Player").transform.position, levelManager.LightsOn? levelManager.collectionIndicatorColor : levelManager.darkCollectionIndicatorColour, "+ " + singleGlassBonus);
                        levelManager.remainingTime = Mathf.Clamp(levelManager.remainingTime + singleFuelReward, 0, levelManager.maxTime);
                        locked[0] = true;
                        InputManager.Instance.Vibrate(fulfilledIntensity, fulfilledDuration, fulfilled1);
                        break;
                    case 2:
                        levelManager.glass += doubleGlassBonus;
                        GameManager.Instance.SpawnCollectionIndicator(GameObject.FindGameObjectWithTag("Player").transform.position, levelManager.LightsOn ? levelManager.collectionIndicatorColor : levelManager.darkCollectionIndicatorColour, "+ " + doubleGlassBonus);
                        levelManager.remainingTime = Mathf.Clamp(levelManager.remainingTime + doubleFuelReward, 0, levelManager.maxTime);
                        locked[0] = true;
                        locked[1] = true;
                        InputManager.Instance.Vibrate(fulfilledIntensity, fulfilledDuration, fulfilled2);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void SwitchRequest()
    {
        requestSprites[0].sprite = requestSprites[1].sprite;
        currentRequests[0] = currentRequests[1];
        currentRequests[1] = requestSpriteArray[Random.Range(0, requestSpriteArray.Length)].name;
        for (int i = 0; i < requestSpriteArray.Length; i++)
        {
            if (requestSpriteArray[i].name == currentRequests[1])
            {
                requestSprites[1].sprite = requestSpriteArray[i].sprite;
                break;
            }
        }
        if (locked[1] == false)
        {
            locked[0] = false;
        }
        else
        {
            locked[1] = false;
            GetComponent<Animator>().SetTrigger("Switch");
        }
    }
}
