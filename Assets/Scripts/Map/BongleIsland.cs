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

    [HideInInspector]
    public bool isInputEnabled = false;
    [HideInInspector]
    public Dictionary<GameObject, GameObject> activePopups = new Dictionary<GameObject, GameObject>();
    [HideInInspector]
    public List<GameObject> pathObjects = new List<GameObject>();

    private Rigidbody2D rb2D;
    private Vector3 lastPathPos, pathOffset;
    private FloatingObjects floatingObjectsScript;
    private Transform popupsContainer;
    private AudioManager audioManager;
    private bool prevFlipX;
    private float sailZ;

    // Start is called before the first frame update

    void Start()
    {
        sailZ = sailAnimator.gameObject.transform.position.z;
        audioManager = GameObject.Find("SoundManager").GetComponent<AudioManager>();
        popupsContainer = GameObject.Find("UI/PopupsContainer").transform;
        rb2D = GetComponent<Rigidbody2D>();
        pathOffset = new Vector3(0, pathYOffset, 0);
        floatingObjectsScript = GameObject.Find("Map").GetComponent<FloatingObjects>();
        transform.position = new Vector3(PlayerPrefs.GetFloat("posX", 0), PlayerPrefs.GetFloat("posY", 0), PlayerPrefs.GetFloat("posZ", 0));
        lastPathPos = transform.position;
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
        if (Input.GetAxis("Jump") > 0)
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RandomTrash"))
        {
            string trashType = collision.GetComponent<RandomTrash>().trashType;
            print("Collected " + trashType);
            PlayerPrefs.SetInt(trashType, PlayerPrefs.GetInt(trashType, 0) + 1);
            floatingObjectsScript.objectsToRemove.Add(collision.gameObject);
            upgradeMenu.GetComponent<UpgradeMenu>().RefreshReadouts();
            upgradeMenu.GetComponent<UpgradeMenu>().RefreshStoryPanel();
            audioManager.PlaySFXAtLocation("Crinkle", collision.transform.position);
        }
        else if (collision.CompareTag("Minigame"))
        {
            GameObject newPopup = Instantiate(popupPrefab, Camera.main.WorldToScreenPoint(collision.gameObject.transform.position), Quaternion.identity, popupsContainer);
            newPopup.GetComponent<Popup>().minigameMarker = collision.gameObject;
            newPopup.GetComponent<Popup>().trashType = collision.gameObject.GetComponent<MinigameMarker>().trashType;
            activePopups.Add(collision.gameObject, newPopup);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Minigame"))
        {
            Destroy(activePopups[collision.gameObject]);
            activePopups.Remove(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            switch (collision.transform.parent.name)
            {
                case "Eel":
                    if (PlayerPrefs.GetInt("storyPoint", 1) == 1)
                    {
                        GameObject newLoadScreen = Instantiate(loadScreen, new Vector3(960, 540, 0), Quaternion.identity);
                        DontDestroyOnLoad(newLoadScreen);
                        SceneManager.LoadScene("VerticalScroller");
                    }
                    break;
                default:
                    break;
            }
            print("Story Point : " + PlayerPrefs.GetInt("storyPoint", 0));
        }
    }

    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            print("Plastic : " + PlayerPrefs.GetInt("Plastic", 0));
            print("Metal : " + PlayerPrefs.GetInt("Metal", 0));
            print("Glass : " + PlayerPrefs.GetInt("Glass", 0));
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            print("Current Story Point : " + PlayerPrefs.GetInt("storyPoint"));
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("posX", transform.position.x);
        PlayerPrefs.SetFloat("posY", transform.position.y);
        PlayerPrefs.SetFloat("posZ", transform.position.z);
    }
}