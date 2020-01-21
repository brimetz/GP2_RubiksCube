using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubiksCubeModel : MonoBehaviour
{
    [SerializeField] private GameObject _cube = null;

    Cube[,,] _listCube = null;

    private int _numCubes = 3;
    private Vector3 _positionOfClickedCube = Vector3.zero;

    [Header("Faces")]
    [SerializeField] Sprite _white;
    [SerializeField] Sprite _red;
    [SerializeField] Sprite _orange;
    [SerializeField] Sprite _blue;
    [SerializeField] Sprite _green;
    [SerializeField] Sprite _yellow;
    [SerializeField] Color _colorInsideCube = new Color(0, 0, 0, 1);

    public int NumCubes { get => _numCubes; set => _numCubes = value; }

    public void CreateCubes(int numCubes)
    {
        InitNewCubes(numCubes);

        // Get the scale for each cube
        Vector3 scale = _cube.transform.localScale;
        scale /= (float)_numCubes;
        // Also create the padding for the cubes so they're centered with their parent
        float paddingX = scale.x * (_numCubes - 1) / 2.0f;
        float paddingY = scale.y * (_numCubes - 1) / 2.0f;
        float paddingZ = scale.z * (_numCubes - 1) / 2.0f;

        int idTmp = 0;

        // Create n*n*n cubes
        for (int i = 0; i < _numCubes; ++i)
        {
            for (int j = 0; j < _numCubes; ++j)
            {
                for (int k = 0; k < _numCubes; ++k)
                {
                    // Get the position of the current cube
                    float xPos = i * scale.x - paddingX;
                    float yPos = j * scale.y - paddingY;
                    float zPos = k * scale.z - paddingZ;

                    // Create the cube at its location and set its parent its scale and name
                    GameObject newObject = Instantiate(_cube, transform.position + new Vector3(xPos, yPos, zPos), Quaternion.identity);
                    newObject.transform.parent = transform;
                    newObject.transform.localScale = scale;
                    newObject.name = "Cube" + i + j + k;

                    Cube cube = newObject.GetComponent<Cube>();
                    if (cube)
                    {
                        cube.Set(transform.position + new Vector3(xPos, yPos, zPos), ++idTmp, _colorInsideCube);
                        SetFaces(ref cube, i, j, k, _numCubes - 1);

                        _listCube[i, j, k] = cube;
                    }
                }
            }
        }
    }

    void InitNewCubes(int numCubes)
    {
        // Remove all the created cubes
        if (_listCube != null)
            for (int i = 0; i < _numCubes; i++)
                for (int j = 0; j < _numCubes; j++)
                    for (int k = 0; k < _numCubes; k++)
                        Destroy(_listCube[i, j, k].gameObject);

        // change the number of cubes
        _numCubes = numCubes;
        // Create the list
        _listCube = new Cube[_numCubes, _numCubes, _numCubes];
        // Reset the rotation
        transform.rotation = Quaternion.identity;
    }

    void SetFaces(ref Cube cube, int i, int j, int k, int lastRow)
    {
        /* Add sprite to init cube according to his position*/

        if (i == 0)
            AddRed(ref cube);
        else if (i == lastRow)
            AddOrange(ref cube);

        if (j == 0)
            AddWhite(ref cube);
        else if (j == lastRow)
            AddYellow(ref cube);

        if (k == 0)
            AddBlue(ref cube);
        else if (k == lastRow)
            AddGreen(ref cube);
    }

    void AddWhite(ref Cube cube)
    {
        GameObject gameObject = new GameObject();
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        gameObject.transform.parent = cube.transform;
        cube._spriteList.Add(sr);

        SetSprite(ref sr, ref _white, "White Face", new Vector3(0, -0.51f, 0), new Vector3(90, 0, 0));
    }

    void AddYellow(ref Cube cube)
    {
        GameObject gameObject = new GameObject();
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.transform.SetParent(cube.transform);
        cube._spriteList.Add(sr);

        SetSprite(ref sr, ref _yellow, "Yellow Face", new Vector3(0, 0.51f, 0), new Vector3(90, 0, 0));
    }

    void AddOrange(ref Cube cube)
    {
        GameObject gameObject = new GameObject();
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.transform.SetParent(cube.transform);
        cube._spriteList.Add(sr);

        SetSprite(ref sr, ref _orange, "Orange Face", new Vector3(0.51f, 0, 0), new Vector3(0, 90, 0));
    }

    void AddRed(ref Cube cube)
    {
        GameObject gameObject = new GameObject();
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.transform.SetParent(cube.transform);
        cube._spriteList.Add(sr);

        SetSprite(ref sr, ref _red, "Red Face", new Vector3(-0.51f, 0, 0), new Vector3(0, 90, 0));
    }

    void AddBlue(ref Cube cube)
    {
        GameObject gameObject = new GameObject();
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.transform.SetParent(cube.transform);
        cube._spriteList.Add(sr);

        SetSprite(ref sr, ref _blue, "Blue Face", new Vector3(0, 0, -0.51f), new Vector3(0, 0, 0));
    }

    void AddGreen(ref Cube cube)
    {
        GameObject gameObject = new GameObject();
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.transform.SetParent(cube.transform);
        cube._spriteList.Add(sr);

        SetSprite(ref sr, ref _green, "Green Face", new Vector3(0, 0, 0.51f), new Vector3(0, 0, 0));
    }

    void SetSprite(ref SpriteRenderer sr, ref Sprite sprite, string name, Vector3 locPosition, Vector3 rotation)
    {
        sr.sprite = sprite;
        sr.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        sr.transform.rotation = Quaternion.Euler(rotation);
        sr.transform.localPosition = locPosition;
        sr.name = name;
    }

    public void GetFace(ref Face face, int idXCube, int idYCube, int idZCube, Vector3 rotationVector)
    {
        FillFace(ref face, ref _listCube[idXCube, idYCube, idZCube], rotationVector);
    }

    public void FillFace(ref Face face, ref Cube cube, Vector3 rotationVector)
    {
        int lastRow = _numCubes - 1;

        face._cubeInFace.Clear();

        // need to know where is the clicked cube in rubiks cube's third dimension array 
        _positionOfClickedCube = GetPosOfCubeInListOfCube(ref cube);

        if (rotationVector == Vector3.right || rotationVector == -Vector3.right)
        {
            //Take all cube in the face
            for (int j = 0; j < _numCubes; j++)
                for (int k = 0; k < _numCubes; k++)
                    face._cubeInFace.Add(_listCube[(int)_positionOfClickedCube.x, j, k]);
        }
        else if (rotationVector == Vector3.up || rotationVector == -Vector3.up)
        {
            //Take all cube in the face
            for (int i = 0; i < _numCubes; i++)
                for (int k = 0; k < _numCubes; k++)
                    face._cubeInFace.Add(_listCube[i, (int)_positionOfClickedCube.y, k]);
        }
        else if (rotationVector == Vector3.forward || rotationVector == -Vector3.forward)
        {
            //Take all cube in the face
            for (int i = 0; i < _numCubes; i++)
                for (int j = 0; j < _numCubes; j++)
                    face._cubeInFace.Add(_listCube[i, j, (int)_positionOfClickedCube.z]);
        }
    }

    public Vector3 GetPosOfCubeInListOfCube(ref Cube cube)
    {
        int lastRow = _numCubes - 1;

        /* Optimisation : Check only cube on the outside of the rubiks cube */

        /* browse on the face up and down */
        for (int j = 0; j < _numCubes; ++j)
        {
            for (int k = 0; k < _numCubes; ++k)
            {
                if (cube == _listCube[0, j, k])
                    return new Vector3(0, j, k);
                else if (cube == _listCube[lastRow, j, k])
                    return new Vector3(lastRow, j, k);
            }
        }

        /* browse on the face right and left */
        for (int i = 0; i < _numCubes; ++i)
        {
            for (int k = 0; k < _numCubes; ++k)
            {
                if (cube == _listCube[i, 0, k])
                    return new Vector3(i, 0, k);
                else if (cube == _listCube[i, lastRow, k])
                    return new Vector3(i, lastRow, k);
            }
        }

        /* browse on the face front and back */
        for (int i = 0; i < _numCubes; ++i)
        {
            for (int j = 0; j < _numCubes; ++j)
            {
                if (cube == _listCube[i, j, 0])
                    return new Vector3(i, j, 0);
                else if (cube == _listCube[i, j, lastRow])
                    return new Vector3(i, j, lastRow);
            }
        }

        // Cube isn't in the rubiks cube
        return new Vector3(0, 0, 0);
    }

    public void UpdateCubePosInRubiksCubeArray(int numTurn, Vector3 rotation)
    {
        if (numTurn == 0)
            return;

        Vector3 currRot = rotation * Mathf.Sign(numTurn);

        numTurn = Mathf.Abs(numTurn);

        /* Search Which rotation have been done, in order to Update The Rube Array */
        if (currRot == Vector3.right)
            TurnAroundRight(numTurn, false);
        else if (currRot == -Vector3.right)
            TurnAroundRight(numTurn, true);
        else if (currRot == Vector3.up)
            TurnAroundUp(numTurn, true);
        else if (currRot == -Vector3.up)
            TurnAroundUp(numTurn, false);
        else if (currRot == Vector3.forward)
            TurnAroundForward(numTurn, false);
        else if (currRot == -Vector3.forward)
            TurnAroundForward(numTurn, true);
    }

    void Swap4Cube(ref Cube cube1, ref Cube cube2, ref Cube cube3, ref Cube cube4, bool neg)
    {
        if (!neg)
        {
            /* swap 4 four in counterclockwise */
            Cube tmp = cube4;
            cube4 = cube3;
            cube3 = cube2;
            cube2 = cube1;
            cube1 = tmp;
        }
        else
        {
            /* swap 4 four in clockwise */
            Cube tmp = cube1;
            cube1 = cube2;
            cube2 = cube3;
            cube3 = cube4;
            cube4 = tmp;
        }
    }

    public bool CheckWinCondition()
    {
        Face up = new Face();
        Face down = new Face();
        Face right = new Face();
        Face left = new Face();
        Face back = new Face();
        Face front = new Face();

        int lastRow = _numCubes - 1;

        /* Fill the 6 faces of the cube in order to check if it's finish */
        for (int i = 0; i <= lastRow; i++)
        {
            for (int j = 0; j <= lastRow; j++)
            {
                up._cubeInFace.Add(_listCube[i, j, 0]);
                down._cubeInFace.Add(_listCube[i, j, lastRow]);
            }
        }

        for (int i = 0; i <= lastRow; i++)
        {
            for (int k = 0; k <= lastRow; k++)
            {
                right._cubeInFace.Add(_listCube[i, 0, k]);
                left._cubeInFace.Add(_listCube[i, lastRow, k]);
            }
        }

        for (int j = 0; j <= lastRow; j++)
        {
            for (int k = 0; k <= lastRow; k++)
            {
                back._cubeInFace.Add(_listCube[0, j, k]);
                front._cubeInFace.Add(_listCube[lastRow, j, k]);
            }
        }

        /* check if each face are done */
        if (up.CheckFaceDone() &&
            down.CheckFaceDone() &&
            right.CheckFaceDone() &&
            left.CheckFaceDone() &&
            back.CheckFaceDone() &&
            front.CheckFaceDone())
            return true;

        return false;
    }

    void TurnAroundRight(int numTurn, bool neg)
    {
        int lastRow = _numCubes - 1;
        int firstCube = 0;

        int index = 0;
        int TempNumOfCubePerSide = _numCubes;

        /* Swap all cube in a face during a turn around right */

        while (TempNumOfCubePerSide > 1)
        {
            // check if current cube is before the lastRow 
            if (firstCube + index != lastRow)
            {
                // take 4 cubes per 4 cubes in each line to swap these cubes
                for (int i = 0; i < numTurn; i++)
                {
                    Swap4Cube(ref _listCube[(int)_positionOfClickedCube.x, firstCube + index, firstCube],
                              ref _listCube[(int)_positionOfClickedCube.x, lastRow, firstCube + index],
                              ref _listCube[(int)_positionOfClickedCube.x, lastRow - index, lastRow],
                              ref _listCube[(int)_positionOfClickedCube.x, firstCube, lastRow - index], neg);
                }
                index++;
            }
            else
            {
                // change step (take face more in the middle)
                TempNumOfCubePerSide -= 2;
                firstCube++;
                lastRow--;
                index = 0;
            }
        }

    }

    void TurnAroundUp(int numTurn, bool neg)
    {
        int lastRow = _numCubes - 1;
        int firstCube = 0;

        int index = 0;
        int TempNumOfCubePerSide = _numCubes;

        /* Swap all cube in a face during a turn around up */

        while (TempNumOfCubePerSide > 1)
        {
            // check if current cube is before the lastRow 
            if (firstCube + index != lastRow)
            {
                // take 4 cubes per 4 cubes in each line to swap these cubes
                for (int i = 0; i < numTurn; i++)
                {
                    Swap4Cube(ref _listCube[firstCube + index, (int)_positionOfClickedCube.y, firstCube],
                               ref _listCube[lastRow, (int)_positionOfClickedCube.y, firstCube + index],
                                ref _listCube[lastRow - index, (int)_positionOfClickedCube.y, lastRow],
                                ref _listCube[firstCube, (int)_positionOfClickedCube.y, lastRow - index], neg);
                }

                index++;
            }
            else
            {
                // change step (take face more in the middle)
                TempNumOfCubePerSide -= 2;
                firstCube++;
                lastRow--;
                index = 0;
            }
        }

    }

    void TurnAroundForward(int numTurn, bool neg)
    {
        int lastRow = _numCubes - 1;
        int firstCube = 0;

        int index = 0;
        int TempNumOfCubePerSide = _numCubes;

        /* Swap all cube in a face during a turn around forward */

        while (TempNumOfCubePerSide > 1)
        {
            // check if current cube is before the lastRow 
            if (firstCube + index != lastRow)
            {
                // take 4 cubes per 4 cubes in each line to swap these cubes
                for (int i = 0; i < numTurn; i++)
                {
                    Swap4Cube(ref _listCube[firstCube + index, firstCube, (int)_positionOfClickedCube.z],
                                ref _listCube[lastRow, firstCube + index, (int)_positionOfClickedCube.z],
                               ref _listCube[lastRow - index, lastRow, (int)_positionOfClickedCube.z],
                                ref _listCube[firstCube, lastRow - index, (int)_positionOfClickedCube.z], neg);
                }
                index++;
            }
            else
            {
                // change step (take face more in the middle)
                TempNumOfCubePerSide -= 2;
                firstCube++;
                lastRow--;
                index = 0;
            }
        }
    }
}
