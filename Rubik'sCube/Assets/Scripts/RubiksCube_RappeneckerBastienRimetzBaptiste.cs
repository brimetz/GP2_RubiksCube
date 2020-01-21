using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubiksCube_RappeneckerBastienRimetzBaptiste : MonoBehaviour
{
    private RubiksCubeModel _model  = null;
    private System.Random   _random = null;
    private Face            _face   = null;

    [SerializeField] private Camera _camera = null;

    #region Cube Control Parameters
    [Header("User Rubik's Cube Control")]
    [SerializeField] private float      _rubiksAcceleration = 100.0f;
    [SerializeField] private float      _rubiksMaxSpeed     = 200.0f;
    [SerializeField] private float      _durationToStop     = 0.1f;
    [SerializeField] private float      _faceRotationSpeed  = 10.0f;
    [SerializeField] private float      _lockingTreshold    = 0.1f;

    private Vector2                     _rubiksSpeed        = Vector2.zero;
    private Vector2                     _rubiksLastSpeed    = Vector2.zero;
    private float                       _rubiksLerpProgress = 0.0f;
    #endregion

    #region Camera Control Parameters
    [Header("User Camera Control")]
    [SerializeField] private Transform  _maxCameraPosition = null;
    [SerializeField] private Transform  _minCameraPosition = null;
    [SerializeField] private float      _cameraZoomSpeed = 0.2f;

    private Vector3                     _newCameraPos = Vector3.zero;
    private float                       _currZoom = 0.5f;
    #endregion

    #region Smooth Control parameters
    [Header("Smooth Control")]
    [SerializeField] private float _faceToNeutralRotationSpeed = 145.0f;
    [SerializeField] private float _scrambleSpeed = 1300.0f;
    #endregion

    private GameObject  _clickedCube = null;
    private Vector3     _normal = Vector3.zero;
    private Vector3     _localNormal = Vector3.zero;
    private Vector3     _localDirection1 = Vector3.zero;
    private Vector3     _localDirection2 = Vector3.zero;
    private Vector3     _rotation = Vector3.zero;
    private Vector3     _mousePosInWorld   = Vector3.zero;
    private Vector2     _mousePosOnScreen  = Vector2.zero;
    private Vector2     _screenRotationDir = Vector2.zero;
    private float       _userAngle = 0.0f;

    private bool _isUserInputLocked = false;
    private bool _isRotationLocked = false; 

    public delegate void NoParamDelegate();
    public NoParamDelegate onCompletition;
    public NoParamDelegate onScrambleEnd;

    void Start()
    {
        // Get the model
        _model = GetComponent<RubiksCubeModel>();

        // Instantiate the base values
        _isUserInputLocked = false;
        _isRotationLocked  = false;

        // Instantiate the random
        _random = new System.Random();

        // Create the face for the rotations
        _face = new Face();
        _face._gameObject = new GameObject("Face");
        _face._gameObject.transform.parent = transform;

        // Set the camera to a base position
        _newCameraPos = Vector3.Slerp(_minCameraPosition.position, _maxCameraPosition.position, _currZoom);
        _camera.transform.position = _newCameraPos;
    }

    public void CreateCubes(int numCubes)
    {
        _face.Reset();
        _model.CreateCubes(numCubes);
    }

    #region Scramble
    public bool ScrambleCube(int iterations)
    {
        if (!_isUserInputLocked)
        {
            _isUserInputLocked = true;
            StartCoroutine(ScrambleCubeCoroutine(iterations));

            return true;
        }

        return false;
    }

    IEnumerator ScrambleCubeCoroutine(int iterations)
    {
        int idXCube, idYCube, idZCube, numTurn;
        Vector3 rotation;
        // Scramble the number of given iteration
        for (int i = 0; i < iterations; ++i)
        {
            // Ge tteh values for the current scramble
            GetScrambleValues(out idXCube, out idYCube, out idZCube, out rotation, out numTurn);

            // Get the corresponding face
            _model.GetFace(ref _face, idXCube, idYCube, idZCube, rotation);

            // Update the face in the model
            _model.UpdateCubePosInRubiksCubeArray(numTurn, rotation);

            // Set the face children for the rotation
            _face.SetCubesToChildren();

            // Get the rotation to reach and rotate
            Quaternion endQuat = Quaternion.AngleAxis(90 * numTurn, rotation);
            yield return StartCoroutine(RotateBackToNormal(Quaternion.identity, endQuat, _scrambleSpeed, true));
        }

        _isUserInputLocked = false;
        onScrambleEnd();
    }

    void GetScrambleValues(out int idXCube, out int idYCube, out int idZCube, out Vector3 rotation, out int numTurn)
    {
        // Get random id of the cube to rotate
        idXCube = _random.Next(0, _model.NumCubes);
        idYCube = _random.Next(0, _model.NumCubes);
        idZCube = _random.Next(0, _model.NumCubes);

        // Get a random rotation
        int idRotation = _random.Next(0, 3);
        switch (idRotation)
        {
            case 0: rotation = Vector3.up; break;
            case 1: rotation = Vector3.right; break;
            default: rotation = Vector3.forward; break;
        }

        // Get a random between -2 and 2 while excluding 0
        do
        {
            numTurn = _random.Next(-2, 3);
        } while (numTurn == 0);
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        UpdateCameraMovement();

        UpdateRotateCube();
    }

    #region Camera Control
    void UpdateCameraMovement()
    {
        // If the mouse wheel moved
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            // Get the mouse wheel movement
            _currZoom += Input.GetAxis("Mouse ScrollWheel") * _cameraZoomSpeed;
            _currZoom = Mathf.Clamp(_currZoom, 0, 1);

            // Get the new position
            _newCameraPos = Vector3.Slerp(_minCameraPosition.position, _maxCameraPosition.position, _currZoom);
        }

        // Lerp the camera to the new position
        _camera.transform.position = Vector3.Slerp(_camera.transform.position, _newCameraPos, 0.2f);
    }
    #endregion

    #region Rubik's Rotation
    void UpdateRotateCube()
    {
        UpdateRubiksRotation();

        UpdateFacesRotation();
    }

    void UpdateRubiksRotation()
    {
        // Only update if we aren't pressing left
        if (!Input.GetMouseButton(0))
        {
            // Check for the right mouse click
            if (Input.GetMouseButton(1))
            {
                UpdateRightMouse();
            }
            // If the right click is released
            if (Input.GetMouseButtonUp(1))
            {
                SlowRubiksSpeed();
            }
        }

        // If we need to lerp the speed to reach zero
        if (_rubiksLerpProgress > 0)
        {
            UpdateCubeRotation();
        }
    }

    void UpdateRightMouse()
    {
        // Get the movement for the horizontal and vertical axis
        float hMove = -Input.GetAxis("Mouse X") * _rubiksAcceleration;
        float yMove =  Input.GetAxis("Mouse Y") * _rubiksAcceleration;

        if (!Mathf.Approximately(hMove, 0) || !Mathf.Approximately(yMove, 0))
        {
            // Update the speed 
            _rubiksSpeed.x += hMove;
            _rubiksSpeed.y += yMove;

            _rubiksLastSpeed = _rubiksSpeed;

            // Reset the lerp progress to keep the user movement while the right click is pressed
            _rubiksLerpProgress = 1;
        }
        else
        {
            // If we are not moving the cube try to slow the speed
            SlowRubiksSpeed();
        }
    }

    void SlowRubiksSpeed()
    {
        // If the cube is rotating faster than the max speed
        if (_rubiksSpeed.sqrMagnitude > _rubiksMaxSpeed * _rubiksMaxSpeed)
        {
            // Set the speed to max speed (+ the direction)
            _rubiksSpeed = _rubiksSpeed.normalized * _rubiksMaxSpeed;
            _rubiksLastSpeed = _rubiksSpeed;
        }
    }

    void UpdateCubeRotation()
    {
        // Get quaternions for the rotation
        // Rotate around up for the horizontal axis and right for the vertical axis
        Quaternion quatH = Quaternion.AngleAxis(_rubiksSpeed.x * Time.deltaTime, Vector3.up);
        Quaternion quatV = Quaternion.AngleAxis(_rubiksSpeed.y * Time.deltaTime, Vector3.right);

        // Rotate the cube
        transform.rotation = (quatH * quatV) * transform.rotation;

        // Reduce the progression
        _rubiksLerpProgress -= Time.deltaTime / _durationToStop;
        if (_rubiksLerpProgress < 0)
        {
            _rubiksSpeed = Vector2.zero;
            _rubiksLerpProgress = 0;
        }
        // Lerp the speed to reach zero
        _rubiksSpeed = Vector2.Lerp(Vector2.zero, _rubiksLastSpeed, _rubiksLerpProgress);
    }
    #endregion

    #region Faces Rotation
    void UpdateFacesRotation()
    {
        // Do not update if a face is rotating back to a neutral position
        if (!_isUserInputLocked)
        {
            // Check for left mouse click
            if (Input.GetMouseButtonDown(0))
            {
                OnLeftMouseDown();
            }
            // If we release the left click or press the right click
            else if (Input.GetMouseButtonUp(0) || Input.GetMouseButton(1))
            {
                OnLeftMouseUp();
            }

            // Check for the duration that the left click is hold 
            if (Input.GetMouseButton(0))
            {
                UpdateLeftMouse();
            }
        }
    }

    void OnLeftMouseDown()
    {
        // Get the raycast of the item we are clicking in the world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // If we are clicking on the a cube
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Get the game object we are clicking
            _clickedCube = hit.transform.gameObject;
            _mousePosOnScreen = Input.mousePosition;

            _normal = hit.normal;
            _mousePosInWorld = hit.point;

            GetPossibleDirection();
        }
    }

    void GetPossibleDirection()
    {
        // Get the two possible direction that the player can try to rotate to

        // Get up and right if the nomal is in the same direction forward
        float dotProduct = Vector3.Dot(transform.forward, _normal);
        if (Mathf.Approximately(dotProduct, 1.0f) || Mathf.Approximately(dotProduct, -1.0f))
        {
            _localNormal =     Vector3.forward * Mathf.Sign(dotProduct);
            _localDirection1 = Vector3.right;
            _localDirection2 = Vector3.up;
        }
        // Get forward and up if the nomal is in the same direction right
        dotProduct = Vector3.Dot(transform.right, _normal);
        if (Mathf.Approximately(dotProduct, 1.0f) || Mathf.Approximately(dotProduct, -1.0f))
        {
            _localNormal =     Vector3.right * Mathf.Sign(dotProduct);
            _localDirection1 = Vector3.up;
            _localDirection2 = Vector3.forward;
        }
        // Get forward and right if the nomal is in the same direction up
        dotProduct = Vector3.Dot(transform.up, _normal);
        if (Mathf.Approximately(dotProduct, 1.0f) || Mathf.Approximately(dotProduct, -1.0f))
        {
            _localNormal =     Vector3.up * Mathf.Sign(dotProduct);
            _localDirection1 = Vector3.forward;
            _localDirection2 = Vector3.right;
        }
    }

    void OnLeftMouseUp()
    {
        if (_isRotationLocked && _clickedCube)
        {
            LeftMouseUpStartAutoRotation();
        }
        else
            _isRotationLocked = false;

        // Change the color
        _mousePosInWorld = Vector3.zero;
        _localDirection1 = Vector3.zero;
        _localDirection2 = Vector3.zero;

    }

    void LeftMouseUpStartAutoRotation()
    {
        // Update the model with the number of turn the player did
        _model.UpdateCubePosInRubiksCubeArray(Mathf.RoundToInt(_userAngle / 90) % 4, _rotation);

        // Get the current rotation
        Quaternion baseFaceRotation = _face._gameObject.transform.localRotation;
        // Get rotation to the closest "90 degree angle"
        Vector3 newRotationToReach = FindClosestAngle(baseFaceRotation.eulerAngles);

        // Create a Quaternion from the rotation
        Quaternion rotationToReach = Quaternion.Euler(newRotationToReach);

        // Start the coroutine of going back
        StartCoroutine(RotateBackToNormal(baseFaceRotation, rotationToReach, _faceToNeutralRotationSpeed));

        _isUserInputLocked = true;
    }

    void UpdateLeftMouse()
    {
        // Check that we have clicked a cube
        if (!_clickedCube)
            return;
        
        if (!_isRotationLocked)
            UpdateToLockRotation();
        else
            UpdateLockedRotation();
    }

    void UpdateToLockRotation()
    {
        // Get the raycast of the item we are clicking in the world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // If we are clicking on the a cube
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Get the hit point
            Vector3 newHitPoint = hit.point;
            Vector3 playerDirection = newHitPoint - _mousePosInWorld;

            // If the player has traveled more than a threshold given
            if (playerDirection.magnitude > _lockingTreshold)
                LockRotation(ref playerDirection, ref newHitPoint);
        }
    }

    void LockRotation(ref Vector3 playerDirection, ref Vector3 positionHit)
    {
        // Lock the rotation for the face to start rotating
        LockGetRotationAndScreenForward(ref playerDirection);
        _isRotationLocked = true;

        // Reset the values of the value that the user has moved
        _userAngle = 0.0f;
        
        // Get the clicked cube
        Cube cubeClicked = _clickedCube.GetComponent<Cube>();

        // Get the face from the model using the clicked cube and the rotation
        _model.FillFace(ref _face, ref cubeClicked, _rotation);
        // The all the cubes' parents to the face
        _face.SetCubesToChildren();
    }

    void LockGetRotationAndScreenForward(ref Vector3 playerDirection)
    {
        // Get the two dot products to know in which direction the player has the most moved
        Vector3 worldDirection1 = transform.localToWorldMatrix * _localDirection1;
        Vector3 worldDirection2 = transform.localToWorldMatrix * _localDirection2;
        float dotPlayerDir1 = Mathf.Abs(Vector3.Dot(playerDirection, worldDirection1));
        float dotPlayerDir2 = Mathf.Abs(Vector3.Dot(playerDirection, worldDirection2));

        Vector3 worldDirection;
        // If the player is moving towards direction 1
        if (dotPlayerDir1 > dotPlayerDir2)
        {
            _rotation = Vector3.Cross(_localDirection1, _localNormal);
            worldDirection = worldDirection1;
        }
        // If the player is moving towards direction 2
        else
        {
            _rotation = Vector3.Cross(_localDirection2, _localNormal);
            worldDirection = worldDirection2;
        }
        _rotation.Normalize();

        // Get the point from the first mouse hit to the direction given
        Vector3 pointForward = _mousePosInWorld + worldDirection;

        // Get the direction on screen to get which direction is forward to rotate the cube
        Vector3 pointForwardOnScreen = _camera.WorldToScreenPoint(pointForward);
        _screenRotationDir = _mousePosOnScreen - new Vector2(pointForwardOnScreen.x, pointForwardOnScreen.y);
        _screenRotationDir.Normalize();
    }

    void UpdateLockedRotation()
    {
        // Get the movement vector of the mouse during the frame
        Vector2 frameDir = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Get the projection of the movement on the direction
        float dotFrameScreenDir = Vector2.Dot(frameDir, _screenRotationDir);
        Vector2 frameMovement = dotFrameScreenDir * _screenRotationDir;

        // Get the horizontal movement using the projection
        float hMove = frameMovement.magnitude * Mathf.Sign(dotFrameScreenDir) * _faceRotationSpeed;

        // Update the amount of rotation that the user has moved
        _userAngle += hMove;

        // Rotate the cube
        _face.RotateAxisAngle(_rotation, hMove);
    }

    Vector3 FindClosestAngle(Vector3 currRotation)
    {
        // Scale the current rotation by the axis be are rotating around to reset only
        Vector3 rotationDirection = Vector3.Scale(currRotation, _rotation);

        if (!Mathf.Approximately(rotationDirection.x, 0))
            currRotation.x = GetClosestAngle(currRotation.x);
        else if (!Mathf.Approximately(rotationDirection.y, 0))
            currRotation.y = GetClosestAngle(currRotation.y);
        else if (!Mathf.Approximately(rotationDirection.z, 0))
            currRotation.z = GetClosestAngle(currRotation.z);

        return currRotation;
    }

    float GetClosestAngle(float angle)
    {
        float absAngle = Mathf.Abs(angle);
        if (absAngle < 45.0f)
            angle = 0;
        else if (absAngle < 135.0f)
            angle = 90;
        else if (absAngle < 225.0f)
            angle = 180;
        else if (absAngle < 315.0f)
            angle = -90;
        else
            angle = 0;

        return angle;
    }

    IEnumerator RotateBackToNormal(Quaternion qStart, Quaternion qEnd, float rotationSpeed, bool keepUserLocked = false)
    {
        float progress = 0.0f;
        float step = rotationSpeed / Quaternion.Angle(qStart, qEnd);
        while (progress < 1)
        {
            progress += step * Time.deltaTime;

            // Lerp between the base and end result
            Quaternion rotation = Quaternion.Lerp(qStart, qEnd, progress);

            // Rotate the face
            _face._gameObject.transform.localRotation = rotation;

            yield return 0;
        }

        // Set the rotation to the exact given rotation
        _face._gameObject.transform.localRotation = qEnd;

        ResetAfter(keepUserLocked);
    }

    void ResetAfter(bool keepUserLocked)
    {
        _face.SetCubesToParent(transform);
        _face.Reset();
        _userAngle = 0;

        _isRotationLocked = false;
        if (!keepUserLocked)
            _isUserInputLocked = false;

        // Reset the object that we were clicking
        _clickedCube = null;

        // Check if the cube is completed
        if (_model.CheckWinCondition())
            onCompletition();
    }
    #endregion
}

