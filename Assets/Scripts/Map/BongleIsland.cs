using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BongleIsland : MonoBehaviour
{
    public float acceleration, pathSeparation, pathYOffset, animationSpeedDivisor;
    public int pathLength;
    public GameObject pathObject, pathContainer, popupPrefab, upgradeMenu, loadScreen;
    public AudioClip musicOriginal;
    public VideoManager videoManager;
    public Animator sailAnimator, islandAnimator, waveAnimator;
    public GameObject[] flipObjects;
    public UpgradeSprites[] upgradeSprites;

    [HideInInspector]
    public bool isInputEnabled = false, isDrawLineCheat = false;
    [HideInInspector]
    public Dictionary<GameObject, GameObject> activePopups = new Dictionary<GameObject, GameObject>();
    [HideInInspector]
    public List<GameObject> pathObjects = new List<GameObject>();

    private Rigidbody2D rb2D;
    private Vector3 lastPathPos, pathOffset;
    private FloatingObjects floatingObjectsScript;
    private Transform popupsContainer, upgradesContainer;
    private AudioManager audioManager;
    private bool prevFlipX;
    private float sailZ;
    private GameObject[][] upgradeSpriteObjects;

    // Start is called before the first frame update

    void Start()
    {
        sailZ = sailAnimator.gameObject.transform.position.z;
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        AudioManager.Instance.PlayAudioAtObject("Wind", gameObject, 20, true);
        AudioManager.Instance.PlayAudioAtObject("AmbientWind", gameObject, 20, true);
        popupsContainer = GameObject.Find("UI/PopupsContainer").transform;
        rb2D = GetComponent<Rigidbody2D>();
        pathOffset = new Vector3(0, pathYOffset, 0);
        floatingObjectsScript = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<FloatingObjects>();
        lastPathPos = transform.position;
        upgradeSpriteObjects = new GameObject[3][];
        upgradesContainer = transform.Find("Upgrades");
        for (int i = 1; i < 4; i++)
        {
            upgradeSpriteObjects[i - 1] = new GameObject[3];
            for (int j = 1; j < 4; j++)
            {
                if (upgradesContainer.Find("Upgrade" + i + "-" + j) != null)
                {
                    upgradeSpriteObjects[i - 1][j - 1] = upgradesContainer.Find("Upgrade" + i + "-" + j).gameObject;
                }
            }
        }
        RefreshUpgrades();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
        Vector2 moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (isInputEnabled)
        {
            rb2D.AddForce(100 * acceleration * Time.deltaTime * moveDir.normalized);
        }

        if (Mathf.Abs(moveDir.x) > 0)
        {
            sailAnimator.gameObject.GetComponent<SpriteRenderer>().flipX = (moveDir.x > 0);
            if (sailAnimator.gameObject.GetComponent<SpriteRenderer>().flipX != prevFlipX && sailAnimator.GetBool("MoveVertical") == false)
            {
                sailAnimator.SetBool("Flip", true);
            }
        }
        prevFlipX = sailAnimator.gameObject.GetComponent<SpriteRenderer>().flipX;
        if (moveDir.magnitude <= 1 && moveDir.magnitude > 0)
        {
            if (Mathf.Abs(moveDir.y) > 0 && Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x))
            {
                sailAnimator.SetBool("MoveVertical", true);
                if (moveDir.y != 0)
                {
                    sailAnimator.SetBool("MoveNorth", moveDir.y > 0);
                }
            }
            else
            {
                sailAnimator.SetBool("MoveVertical", false);
            }
        }
        if (Mathf.Abs(rb2D.velocity.x) > 0)
        {
            waveAnimator.gameObject.GetComponent<SpriteRenderer>().flipX = (rb2D.velocity.x > 0);
        }
        if (Mathf.Abs(rb2D.velocity.y) > 0 && Mathf.Abs(rb2D.velocity.y) > Mathf.Abs(rb2D.velocity.x))
        {
            waveAnimator.SetBool("MoveVertical", true);
            if (rb2D.velocity.y != 0)
            {
                waveAnimator.SetBool("MoveNorth", rb2D.velocity.y > 0);
            }
        }
        else
        {
            waveAnimator.SetBool("MoveVertical", false);
        }
        Vector3 tempPos = sailAnimator.gameObject.transform.localPosition;
        if (sailAnimator.GetBool("MoveVertical") == true && sailAnimator.GetBool("MoveNorth") == false)
        {
            sailAnimator.gameObject.transform.localPosition = new Vector3(tempPos.x, tempPos.y, -sailZ);
        }
        else
        {
            sailAnimator.gameObject.transform.localPosition = new Vector3(tempPos.x, tempPos.y, sailZ);
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            GetComponent<InputField>().Select();
            GetComponent<InputField>().ActivateInputField();
        }
    }

    private void LateUpdate()
    {
        islandAnimator.speed = rb2D.velocity.magnitude / animationSpeedDivisor + 1;
        waveAnimator.speed = rb2D.velocity.magnitude / animationSpeedDivisor + 1;
        sailAnimator.SetFloat("Speed", rb2D.velocity.magnitude);
        waveAnimator.SetFloat("Speed", rb2D.velocity.magnitude);
        Vector3 latestVector = transform.position + pathOffset - lastPathPos;
        if (latestVector.magnitude > pathSeparation)
        {
            Vector3 newLineLocation = lastPathPos + (latestVector / 2);
            lastPathPos = transform.position + pathOffset;
            GameObject newPathObject = Instantiate(pathObject, newLineLocation, Quaternion.identity, pathContainer.transform);
            newPathObject.GetComponent<SpriteRenderer>().color = isDrawLineCheat ? Color.black : Color.white;
            float pathAngle = Vector3.Angle(new Vector3(1, 0, 0), latestVector.normalized);
            if (latestVector.y < 0)
            {
                pathAngle *= -1;
            }
            newPathObject.transform.Rotate(0.0f, 0.0f, pathAngle);

            pathObjects.Add(newPathObject);
            if (pathObjects.Count > pathLength)
            {
                Destroy(pathObjects[0]);
                pathObjects.RemoveAt(0);
            }
        }
        if (Input.GetAxis("Jump") > 0 && isInputEnabled)
        {
            float minDist = 100f;
            GameObject currentMinPopup = null;
            foreach (KeyValuePair<GameObject, GameObject> popupEntry in activePopups)
            {
                float popupEntryDist = (popupEntry.Key.transform.position - transform.position).magnitude;
                if (popupEntryDist < minDist)
                {
                    minDist = popupEntryDist;
                    currentMinPopup = popupEntry.Value;
                }
            }
            if (currentMinPopup != null)
            {
                currentMinPopup.GetComponent<Popup>().LaunchMinigame();
            }
        }
    }

    public void RefreshUpgrades()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (upgradeSpriteObjects[i][j] != null)
                {
                    upgradeSpriteObjects[i][j].SetActive(GameManager.Instance.upgrades[i][j] != 0);
                    upgradeSpriteObjects[i][j].GetComponent<SpriteRenderer>().sprite = upgradeSprites[i].upgradeSprites[j].sprites[Mathf.Clamp(GameManager.Instance.upgrades[i][j] - 1, 0, upgradeSprites[i].upgradeSprites[j].sprites.Length - 1)];
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RandomTrash"))
        {
            string trashType = collision.GetComponent<RandomTrash>().trashType;
            print("Collected " + trashType);
            GameManager.Instance.trashCounts[trashType]++;
            floatingObjectsScript.objectsToRemove.Add(collision.gameObject);
            upgradeMenu.GetComponent<UpgradeMenu>().RefreshReadouts();
            //audioManager.PlaySFXAtLocation("Crinkle", collision.transform.position, 15);
            switch (collision.gameObject.GetComponent<RandomTrash>().trashType)
            {
                case "Plastic":
                    audioManager.PlaySFXAtLocation("Plastic", collision.transform.position, 15);
                    break;
                case "Metal":
                    audioManager.PlaySFXAtLocation("Metal", collision.transform.position, 15);
                    break;
                case "Glass":
                    audioManager.PlaySFXAtLocation("Glass", collision.transform.position, 15);
                    break;
                default:
                    break;
            }
        }
        else if (collision.CompareTag("Minigame"))
        {
            GameObject newPopup = Instantiate(popupPrefab, Camera.main.WorldToScreenPoint(collision.gameObject.transform.position), Quaternion.identity, popupsContainer);
            newPopup.GetComponent<Popup>().minigameMarker = collision.gameObject;
            newPopup.GetComponent<Popup>().trashType = collision.gameObject.GetComponent<MinigameMarker>().trashType;
            activePopups.Add(collision.gameObject, newPopup);
            audioManager.PlaySFXAtLocation("PopUp", collision.transform.position, 20);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Minigame"))
        {
            Destroy(activePopups[collision.gameObject]);
            activePopups.Remove(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Emergency"))
        {
            transform.position = Vector3.zero + (transform.position - collision.gameObject.transform.parent.position);
            string[] keys = new string[GameManager.Instance.trashCounts.Count];
            GameManager.Instance.trashCounts.Keys.CopyTo(keys, 0);
            for (int i = 0; i < GameManager.Instance.MaxRegion(); i++)
            {
                GameManager.Instance.trashCounts[keys[i]] += 30;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            switch (collision.transform.parent.name)
            {
                case "Eel":
                    if (GameManager.Instance.storyPoint == 1)
                    {
                        GameManager.Instance.LoadMinigame(TrashType.Plastic);
                    }
                    break;
                case "Crab":
                    if (GameManager.Instance.storyPoint == 5)
                    {
                        GameManager.Instance.storyPoint++;
                        GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
                    }
                    break;
                case "Whale":
                    if (GameManager.Instance.storyPoint == 9)
                    {
                        GameManager.Instance.storyPoint++;
                        GameObject.Find("UI/StoryVideo").GetComponent<VideoManager>().CheckCutscene();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void EditorUpdate()
    {

    }

    [System.Serializable]
    public struct UpgradeSprites
    {
        public NameSpriteArray[] upgradeSprites;
    }
}