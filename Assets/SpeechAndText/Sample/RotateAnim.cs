﻿using UnityEngine;

public class RotateAnim : MonoBehaviour {
    public float speed = 1;
    void Update() {
        transform.Rotate(Vector3.forward * Time.deltaTime * speed);
    }
}
