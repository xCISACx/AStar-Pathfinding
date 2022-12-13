using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Vector3 DefaultPosition;
    public Vector3 DefaultScale;

    private void Awake()
    {
        DefaultPosition = transform.position;
        DefaultScale = transform.localScale;
    }
}
