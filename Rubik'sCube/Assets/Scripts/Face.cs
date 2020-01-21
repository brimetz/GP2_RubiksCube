using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face
{
    public GameObject _gameObject = null;

    public List<Cube> _cubeInFace = null;

    public Face()
    {
        _cubeInFace = new List<Cube>();
    }

    public void Reset()
    {
        _gameObject.transform.localRotation = Quaternion.identity;
        _cubeInFace.Clear();
    }

    public void SetCubesToChildren()
    {
        for (int i = 0; i < _cubeInFace.Count; ++i)
        {
            _cubeInFace[i].transform.parent = _gameObject.transform;
        }
    }

    public void SetCubesToParent(Transform parent)
    {
        for (int i = 0; i < _cubeInFace.Count; ++i)
            _cubeInFace[i].transform.parent = parent;
    }

    // Rotate the face around the World(0,0,0)
    public void RotateAxisAngle(Vector3 axis, float angle)
    {
        // Get the quaternion of rotation
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);

        // Rotate the parent gameObject to rotate the whole face
        _gameObject.transform.localRotation = rotation * _gameObject.transform.localRotation;
        
        //***** OLD *****// Rotation all the cubes around the center 
        //Transform currTransform;
        //// Rotate the cube
        //for (int i = 0; i < _cubeInFace.Count; i++)
        //{
        //    currTransform = _cubeInFace[i].transform;
        //    currTransform.localPosition = rotation * currTransform.localPosition;
        //    currTransform.localRotation = rotation * currTransform.localRotation;
        //}
    }

    public bool CheckFaceDone()
    {
        List<Transform> SpriteRot = new List<Transform>();
        List<Sprite> SpritePossible = new List<Sprite>();

        /* Take the center approximative of the face */
        int index = TakeIndexOfCubeInCenterOfFace();

        Cube currentCube = _cubeInFace[index];
        /* take all sprite on the cube center approximative of the face with the rotation associated */
        for (int i = 0; i < currentCube._spriteList.Count; i++)
        {
            SpritePossible.Add(currentCube._spriteList[i].sprite);
            SpriteRot.Add(currentCube._spriteList[i].transform);
        }

        for (int i = 0; i < _cubeInFace.Count; i++)
        {
            currentCube = _cubeInFace[i];

            int j = 0;
            /* check if one sprite on current cube can be here */
            if (!SpriteIsPossible(currentCube, SpritePossible, ref j))
            {
                return false;
            }

            /* check if cube is at his right place */
            if (!TestIsFinishCube(currentCube, SpritePossible, SpriteRot))
            {
                return false;
            }
        }
        return true;
    }

    public bool SpriteIsPossible(Cube currentCube, List<Sprite> SpritePossible, ref int index)
    {
        bool isPossible = false;

        // Search if a sprite on the current cube is possible to complete the face
        for (int i = 0; i < currentCube._spriteList.Count; i++)
        {
            for (int j = 0; j < SpritePossible.Count; j++)
            {
                if (currentCube._spriteList[i].sprite == SpritePossible[j])
                {
                    // one sprite could complete face
                    isPossible = true;
                    index = i;
                    break;
                }
            }
        }
        // none sprite could complete face
        return isPossible;
    }

    public int TakeIndexOfCubeInCenterOfFace()
    {
        // Take index of a cube in center of the face (with only one sprite)
        int index = 0;
        if (_cubeInFace.Count % 2 == 1)
            index = ((_cubeInFace.Count - 1) / 2);
        else
            index = ((_cubeInFace.Count) / 2) + 1;
        return index;
    }

    public bool TestIsFinishCube(Cube currentCube, List<Sprite> SpritePossible, List<Transform> SpriteRot)
    {
        /* Check for each Sprite of cube if it is the same than one sprite possible for the face,
            then, check if rotation of sprite is egual in x and y */

       double epsilon = 1.0;
       SpriteRenderer currentSpriteRenderer = null;
       for (int j = 0; j < currentCube._spriteList.Count; j++)
       {
           currentSpriteRenderer = currentCube._spriteList[j];
           for (int k = 0; k < SpritePossible.Count; k++)
           {
                // Check if current sprite is possible in face
                if (currentSpriteRenderer.sprite == SpritePossible[k])
                {
                    if (!(currentSpriteRenderer.transform.rotation.eulerAngles.y < SpriteRot[k].rotation.eulerAngles.y + epsilon &&
                        currentSpriteRenderer.transform.rotation.eulerAngles.y > SpriteRot[k].rotation.eulerAngles.y - epsilon &&
                        currentSpriteRenderer.transform.rotation.eulerAngles.x < SpriteRot[k].rotation.eulerAngles.x + epsilon &&
                        currentSpriteRenderer.transform.rotation.eulerAngles.x > SpriteRot[k].rotation.eulerAngles.x - epsilon))
                        return false;
                }
           }
       }
        return true;
    }
}
