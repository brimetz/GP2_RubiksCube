using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    Vector3 _position = new Vector3(0, 0, 0);

    public int _id = 0;

    public List<SpriteRenderer> _spriteList = null;

    public Cube()
    {
        _spriteList = new List<SpriteRenderer>();
    }

    public void Set(Vector3 pos, int id, Color col)
    {
        _position = pos;
        _id = id;

        GetComponent<Renderer>().material.color = col;
    }
}
