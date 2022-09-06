using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [HideInInspector]
    public string inputMethod;
    [HideInInspector] public Vector2 move;
    public event Action SwitchControlScheme, Jump, PrimaryAbility, SecondaryAbility, Menu, UpgradeMenu, Proceed, Move, Click;
    public event Action<int> Shoulder;
    [HideInInspector] public bool jump;
    [HideInInspector] public float zoom;
    [HideInInspector] public List<Button> backButtons;
    [HideInInspector] public List<GameObject> selectedButtons;
    public HashSet<VibrationData> vibrations = new();
    [HideInInspector] public PlayerInput playerInput;
    public AbilityPromptSet[] abilityPrompts;

    private static InputManager instance;
    private InputActionMap playerActionMap;
    private Dictionary<string, InputAction> inputActions = new Dictionary<string, InputAction>();
    private Vector2 savedMousePos = new Vector2(0, 0);

    public static InputManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        playerInput = GetComponent<PlayerInput>();
        playerActionMap = playerInput.actions.FindActionMap("Player");
        SceneManager.sceneLoaded += OnSceneLoad;
        foreach (InputAction inputAction in playerActionMap)
        {
            inputActions.Add(inputAction.name, inputAction);
        }
    }

    private void Start()
    {
        if (!GameManager.Instance.gameStarted && SceneManager.GetActiveScene().name == "Map")
        {
            selectedButtons.Add(EventSystem.current.firstSelectedGameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        move = inputActions["Move"].ReadValue<Vector2>();
        jump = inputActions["Jump"].ReadValue<float>() != 0;
        zoom = inputActions["Zoom"].ReadValue<float>();
    }

    private void LateUpdate()
    {
        string currentControlScheme = playerInput.currentControlScheme;
        Vector2 newMotorSpeeds = Vector2.zero;
        if (currentControlScheme == "Gamepad" &&  GameManager.Instance.allowVibration)
        {
            foreach (VibrationData vibration in vibrations)
            {
                if (vibration.intensity.x > newMotorSpeeds.x)
                {
                    newMotorSpeeds.x = vibration.intensity.x;
                }
                if (vibration.intensity.y > newMotorSpeeds.y)
                {
                    newMotorSpeeds.y = vibration.intensity.y;
                }
            }
        }
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(newMotorSpeeds.x, newMotorSpeeds.y);
        }
        if (currentControlScheme != inputMethod)
        {
            if (playerInput.currentActionMap.name == "UI")
            {
                if (currentControlScheme == "Gamepad" && inputMethod == "Keyboard&Mouse")
                {
                    GameObject mouseoverButton = MouseOverButton(true);
                    SetSelectedButton(mouseoverButton != null ? mouseoverButton : selectedButtons[^1]);
                    StartCoroutine(EnableNavigation(true));
                }
                else if (currentControlScheme == "Keyboard&Mouse" && inputMethod == "Gamepad")
                {
                    MouseOverButton(false);
                    EventSystem.current.SetSelectedGameObject(null);
                    playerInput.SwitchCurrentActionMap("Player");
                    StartCoroutine(EnableNavigation(false));
                }
                EnableCursor();
            }
            else if (playerInput.currentActionMap.name == "Player")
            {
                if (selectedButtons.Count > 0 && currentControlScheme == "Gamepad" && inputMethod == "Keyboard&Mouse")
                {
                    GameObject mouseoverButton = MouseOverButton(true);
                    SetSelectedButton(mouseoverButton != null ? mouseoverButton : selectedButtons[^1]);
                    EnableCursor(false);
                    EnableUIInput();
                }
                else if (SceneManager.GetActiveScene().name == "Map")
                {
                    EnableCursor();
                    EnableUIInput(false);
                }
            }
            inputMethod = currentControlScheme;
            SwitchControlScheme?.Invoke();
        }
        if (currentControlScheme == "Gamepad" && EventSystem.current.currentSelectedGameObject != null && (selectedButtons.Count == 0 || selectedButtons[^1] != EventSystem.current.currentSelectedGameObject) && playerInput.currentActionMap.name == "UI")
        {
            SetSelectedButton(EventSystem.current.currentSelectedGameObject);
        }
    }

    public void EnableUIInput(bool uIEnabled = true)
    {
        if (uIEnabled && playerInput.currentControlScheme == "Gamepad")
        {
            playerInput.SwitchCurrentActionMap("UI");
            StartCoroutine(EnableNavigation(true));
        }
        else
        {
            playerInput.SwitchCurrentActionMap("Player");
            StartCoroutine(EnableNavigation(false));
        }
    }

    private IEnumerator EnableNavigation(bool enabled)
    {
        while (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().magnitude >= 0.2f && enabled)
        {
            yield return null;
        }
        EventSystem.current.sendNavigationEvents = enabled;
    }

    private GameObject MouseOverButton(bool deselectButton = false)
    {
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        List<RaycastResult> hits = new();
        EventSystem.current.RaycastAll(pointerData, hits);
        if (deselectButton)
        {
            savedMousePos = Mouse.current.position.ReadValue();
            InputState.Change(Mouse.current.position, new Vector2(0, 0));
        }
        else
        {
            InputState.Change(Mouse.current.position, savedMousePos);
        }
        if (hits.Count != 0)
        {
            Button hitButton;
            Toggle hitToggle;
            foreach (RaycastResult hit in hits)
            {
                if (hitButton = hit.gameObject.GetComponent(typeof(Button)) as Button)
                {
                    if (deselectButton)
                    {
                        hitButton.OnPointerExit(pointerData);
                    }
                    return hit.gameObject;
                }
                else if (hitToggle = hit.gameObject.transform.parent.gameObject.GetComponent(typeof(Toggle)) as Toggle)
                {
                    if (deselectButton)
                    {
                        hitToggle.OnPointerExit(pointerData);
                    }
                    return hit.gameObject.transform.parent.gameObject;
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    public void CloseMenu(Transform menu)
    {
        RemoveHierarchyFromSelection(menu);
        List<Button> currentBackButtons = new(backButtons);
        foreach (Button button in currentBackButtons)
        {
            if (button.gameObject.transform.IsChildOf(menu))
            {
                backButtons.Remove(button);
            }
        }
        SetSelectedButton(null);
    }

    public void SetSelectedButton(GameObject button)
    {
        for (int i = selectedButtons.Count - 1; i >= 0; i--)
        {
            if (selectedButtons[i] == null)
            {
                selectedButtons.RemoveAt(i);
            }
        }
        if (button != null)
        {
            if (selectedButtons.Count > 0)
            {
                if (button.transform.parent == selectedButtons[^1].transform.parent)
                {
                    selectedButtons[^1] = button;
                }
                else
                {
                    selectedButtons.Add(button);
                }
            }
            else
            {
                selectedButtons.Add(button);
            }
            if (playerInput.currentControlScheme == "Gamepad")
            {
                EventSystem.current.SetSelectedGameObject(button);
            }
        }
        else if (playerInput.currentControlScheme == "Gamepad")
        {
            if (selectedButtons.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(selectedButtons[^1]);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        if (selectedButtons.Count <= 0)
        {
            EnableUIInput(false);
        }
    }

    public void RemoveHierarchyFromSelection(Transform button)
    {
        for (int i = selectedButtons.Count - 1; i >= 0; i--)
        {
            if (selectedButtons[i].transform.IsChildOf(button) || selectedButtons[i] == null)
            {
                selectedButtons.RemoveAt(i);
            }
        }
        if (inputMethod == "Gamepad" && selectedButtons.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(selectedButtons[^1]);
        }
    }

    public VibrationData Vibrate(float intensity = 0.5f, float duration = 0, AnimationCurve curve = null)
    {
        return Vibrate(intensity, intensity, duration, curve);
    }

    public VibrationData Vibrate(float leftIntensity, float rightIntensity, float duration, AnimationCurve curve = null)
    {
        VibrationData newVibration = new()
        {
            duration = duration,
            intensity = new Vector2(leftIntensity, rightIntensity),
            curve = curve ?? new AnimationCurve(new Keyframe(0, 1, 0, 0)),
        };
        vibrations.Add(newVibration);
        StartCoroutine(Vibration(newVibration));
        return newVibration;
    }

    private IEnumerator Vibration(VibrationData vibration)
    {
        Vector2 startIntensity = vibration.intensity;
        float elapsedTime = 0;
        while (elapsedTime <= vibration.duration)
        {
            if (vibration.duration != 0)
            {
                elapsedTime += Time.unscaledDeltaTime;
            }
            vibration.intensity = startIntensity * vibration.curve.Evaluate(elapsedTime / vibration.duration);
            yield return null;
        }
        vibrations.Remove(vibration);
    }

    public void SwitchControls()
    {
        SwitchControlScheme?.Invoke();
    }

    public void EnableCursor(bool enabled = true)
    {
        if (enabled && playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }
        

    public void SetBackButton(Button button)
    {
        backButtons.Add(button);
    }

    public void RemoveLatestBackButton()
    {
        backButtons.RemoveAt(backButtons.Count - 1);
    }

    private void OnJump()
    {
        Jump?.Invoke();
    }

    private void OnMove()
    {
        Move?.Invoke();
    }

    private void OnClick()
    {
        Click?.Invoke();
    }

    private void OnPrimaryAbility()
    {
        PrimaryAbility?.Invoke();
    }

    private void OnSecondaryAbility()
    {
        SecondaryAbility?.Invoke();
    }

    private void OnMenu()
    {
        Menu?.Invoke();
    }

    private void OnUpgradeMenu()
    {
        UpgradeMenu?.Invoke();
    }

    private void OnCancel()
    {
        if (backButtons.Count > 0)
        {
            backButtons[^1].onClick?.Invoke();
        }
    }

    private void OnShoulder(InputValue value)
    {
        int direction = (int)value.Get<float>();
        if (direction != 0)
        {
            Shoulder?.Invoke(direction);
        }
    }

    private void OnProceed()
    {
        Proceed?.Invoke();
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        selectedButtons?.Clear();
        backButtons?.Clear();
        vibrations?.Clear();
        if (GameManager.Instance.gameStarted)
        {
            EnableUIInput(false);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    public class VibrationData
    {
        public float duration;
        public Vector2 intensity;
        public AnimationCurve curve;

        public float Intensity
        {
            get
            {
                return (intensity.x + intensity.y) / 2;
            }
            set
            {
                intensity = new Vector2(value, value);
            }
        }
    }

    [System.Serializable]
    public struct AbilityPromptSet
    {
        public string name;
        public AbilityPrompt[] abilityPrompts;
    }

    [System.Serializable]
    public struct AbilityPrompt
    {
        public string name;
        public PromptType promptType;
        [HideInInspector] public string text;
        [HideInInspector] public Color colour;
        [HideInInspector] public Sprite sprite;
        public enum PromptType
        {
            text,
            image
        }
    }
}