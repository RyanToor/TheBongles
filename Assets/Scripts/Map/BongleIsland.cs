using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BongleIsland : MonoBehaviour
{
    public float acceleration, pathSeparation, pathYOffset;
    public int pathLength;
    public GameObject pathObject, pathContainer;
    
    public List<GameObject> pathObjects = new List<GameObject>();
    private Rigidbody2D rb2D;
    private bool isMovingRight;
    private Vector3 lastPathPos, pathOffset;
    private FloatingObjects floatingObjectsScript;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        pathOffset = new Vector3(0, pathYOffset, 0);
        floatingObjectsScript = GameObject.Find("Map").GetComponent<FloatingObjects>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            EditorUpdate();
        }
        float moveRight = Input.GetAxis("Horizontal");
        float moveUp = Input.GetAxis("Vertical");

        if (moveRight > 0)
        {
            isMovingRight = true;
        }
        else if (moveRight < 0)
        {
            isMovingRight = false;
        }

        rb2D.AddForce(new Vector2(moveRight * acceleration, moveUp * acceleration) * Time.deltaTime * 100);

        if (!isMovingRight)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            GetComponent<InputField>().Select();
            GetComponent<InputField>().ActivateInputField();
        }
    }

    private void LateUpdate()
    {
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("RandomTrash"))
        {
            string trashType = collision.GetComponent<RandomTrash>().trashType;
            print("Collected " + trashType);
            PlayerPrefs.SetInt(trashType, PlayerPrefs.GetInt(trashType, 0) + 1);
            floatingObjectsScript.objectsToRemove.Add(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Minigame"))
        {
            switch (collision.gameObject.GetComponent<MinigameMarker>().trashType)
            {
                case "Plastic":
                    SceneManager.LoadScene("VerticalScroller");
                    break;
                default:
                    break;
            }
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
    }
}