using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RubiksCubeView : MonoBehaviour
{
    enum ETimerState
    {
        STOPPED,
        STARTED,
        PAUSED,
        VICTORY
    }

    [Header("UI Objects")]
    [SerializeField] private Slider _sliderRubkisCubeSize = null;
    [SerializeField] private Button _setNewSizeButton = null;
    [SerializeField] private Text   _textRubkisCubeSize = null;

    [SerializeField] private Slider _scrambleSlider = null;
    [SerializeField] private Button _scrambleButton = null;
    [SerializeField] private Text   _scrambleText = null;
    [SerializeField] private InputField _scrambleInput = null;

    [SerializeField] private Button _startPauseButton = null;
    [SerializeField] private Button _resetButton = null;
    [SerializeField] private Text   _timerText = null;
    [SerializeField] private Text   _timerStartText = null;
    [SerializeField] private Text   _endText = null;

    [SerializeField] private Button _exitButton = null;

    [SerializeField] private ParticleSystem _firework = null;

    [Header("Strings")]
    [SerializeField] private String _startString = "Start";
    [SerializeField] private String _pauseString = "Pause";
    [SerializeField] private String _resumeString = "Resume";
    [SerializeField] private String _resetString = "Reset Timer";

    private int _rubkisCubeSize = 3;
    private int _scrambleDepth = 10;
    private ETimerState _timerState = ETimerState.STOPPED;
    private float _timerDuration;

    RubiksCube_RappeneckerBastienRimetzBaptiste _rubiksCube = null;
    // Start is called before the first frame update
    void Start()
    {

        _rubiksCube = GetComponent<RubiksCube_RappeneckerBastienRimetzBaptiste>();
        _rubiksCube.onCompletition += OnCompletition;
        _rubiksCube.onScrambleEnd += OnScrambleEnd;

        // Add listeners
        _sliderRubkisCubeSize.onValueChanged.AddListener(OnRubiksCubeSizeChanged);
        _setNewSizeButton.onClick.AddListener(OnNewSize);

        _scrambleButton.onClick.AddListener(OnScramble);
        _scrambleSlider.onValueChanged.AddListener(OnScrambleDepthChangedSlider);
        _scrambleInput.onValueChanged.AddListener(OnScrambleDepthChangedInput);

        _startPauseButton.onClick.AddListener(OnStart);
        _resetButton.onClick.AddListener(OnReset);

        _exitButton.onClick.AddListener(OnExit);

        _timerText.text = "";
        _timerStartText.text = _startString;
        _endText.text        = _resetString;

        _firework.Stop();

        SetActiveUIScramble(false);

        // Update the texts
        SetNumberOfCubes();
        SetScrambleDepth();
        UpdateTimerText();
    }

    void OnRubiksCubeSizeChanged(float value)
    {
        // Get the value
        _rubkisCubeSize = (int)value;
        // Unhide the create button
        _setNewSizeButton.gameObject.SetActive(true);

        // Set the text
        SetNumberOfCubes();
    }

    void SetNumberOfCubes()
    {
        _textRubkisCubeSize.text = "Cubes : " + _rubkisCubeSize.ToString();
    }

    void OnNewSize()
    {
        // Call the function to create the new rubiks cube
        _rubiksCube?.CreateCubes(_rubkisCubeSize);

        // Hide itself
        _setNewSizeButton.gameObject.SetActive(false);

        SetActiveUIScramble(true);
    }

    void OnScramble()
    {
        _rubiksCube?.ScrambleCube(_scrambleDepth);

        SetInteractableUIOnScramble(false);
    }

    void OnScrambleEnd()
    {
        SetInteractableUIOnScramble(true);
    }

    void OnScrambleDepthChangedSlider(float value)
    {
        _scrambleDepth = Mathf.FloorToInt(value);
        SetScrambleDepth();
    }

    void OnScrambleDepthChangedInput(string value)
    {
        _scrambleDepth = int.Parse(value);
        SetScrambleDepth();
    }

    void SetScrambleDepth()
    {
        _scrambleSlider.value = _scrambleDepth;
        _scrambleInput.text = _scrambleDepth.ToString();
    }

    void SetActiveUIScramble(bool flag)
    {
        _scrambleSlider.gameObject.SetActive(flag);
        _scrambleButton.gameObject.SetActive(flag);
        _scrambleInput .gameObject.SetActive(flag);
        _scrambleText  .gameObject.SetActive(flag);
    }

    void SetInteractableUIOnScramble(bool flag)
    {
        _setNewSizeButton.interactable = flag;
        _scrambleSlider.interactable = flag;
        _scrambleButton.interactable = flag;
        _scrambleInput .interactable = flag;
        //_scrambleText  .interactable = flag;

    }

    void OnStart()
    {
        // Reset the timer if the state is victory
        if (_timerState == ETimerState.VICTORY)
        {
            _timerDuration = 0.0f;
            StopAllCoroutines();
            ResetAnimation();
        }

        // Pause the timer
        if (_timerState == ETimerState.STARTED)
        {
            _timerState = ETimerState.PAUSED;
            _timerStartText.text = _resumeString;
        }
        // Start or resume the timer
        else
        {
            _timerState = ETimerState.STARTED;
            _timerStartText.text = _pauseString;
        }
    }

    void OnReset()
    {
        // Reset the timer if the state is victory
        if (_timerState == ETimerState.VICTORY)
        {
            StopAllCoroutines();
            ResetAnimation();
        }

        // Reset the time
        _timerDuration = 0.0f;

        // Set the timer state
        _timerStartText.text = _startString;
        _timerState = ETimerState.PAUSED;

        // Reset the text
        UpdateTimerText();
    }

    void OnCompletition()
    {
        // Start the animation vicotry only if the timer is started
        if (_timerState == ETimerState.STARTED)
        {
            _timerState = ETimerState.VICTORY;
            _timerStartText.text = _startString;

            _firework.Play();

            StartCoroutine(VictoryAnimation());
        }
    }

    IEnumerator VictoryAnimation()
    {
        float speed = 2.0f;
        float maxGrowth = 2;
        float minGrowth = 1;
        float currentGrowth = minGrowth - 1;
        int growingDir = 1;

        float rotationSpeed = 15.0f;
        float minRotation = -20;
        float maxRotation = 20;
        float currMinRotation = 0;
        float currMaxRotation = 0;
        float currRotation = 0;
        float randSizeRotation = 5;
        int rotationDir = 1;

        System.Random rand = new System.Random();

        while (true)
        {
            if (currentGrowth > maxGrowth)
            {
                currentGrowth = maxGrowth;
                growingDir = -1;
            }
            else if (currentGrowth < minGrowth)
            {
                currentGrowth = minGrowth;
                growingDir = 1;
                
            }
            currentGrowth += speed * growingDir * Time.deltaTime;
            _timerText.rectTransform.localScale = Vector3.one * currentGrowth;

            if (currRotation > currMaxRotation)
            {
                currRotation = currMaxRotation;
                currMinRotation = minRotation + randSizeRotation * ((float)rand.NextDouble() - 0.5f);
                rotationDir = -1;
            }
            else if (currRotation < currMinRotation)
            {
                currRotation = currMinRotation;
                currMaxRotation = maxRotation + randSizeRotation * ((float)rand.NextDouble() - 0.5f);
                rotationDir = 1;
            }
            currRotation += rotationSpeed * rotationDir * Time.deltaTime;
            _timerText.rectTransform.rotation = Quaternion.AngleAxis(currRotation, Vector3.forward);

            yield return 0;
        }
    }

    void ResetAnimation()
    {
        _timerText.rectTransform.localScale = Vector3.one;
        _timerText.rectTransform.rotation = Quaternion.identity;

        _firework.Stop();
    }

    void OnExit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnStart();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            OnReset();
        }
        // Udpate the timer if it has started
        if (_timerState == ETimerState.STARTED)
        {
            _timerDuration += Time.deltaTime;
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        // Get the time span
        TimeSpan duration = new TimeSpan(0, 0, 0, 0, Mathf.FloorToInt(_timerDuration * 1000));

        _timerText.text = string.Format("{0:hh\\:mm\\:ss\\.ff}", duration);
    }
}
