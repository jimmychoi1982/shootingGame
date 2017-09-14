using UnityEngine;
using System;

public class AirplaneBase : MonoBehaviour
{
    private new string name;

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    private int id;

    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    private float hp;

    public float Hp
    {
        get { return hp; }
        set { hp = value; }
    }

    private float mp;

    public float Mp
    {
        get { return mp; }
        set { mp = value; }
    }
}
