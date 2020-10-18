using System;
using UnityEngine;

public class UserInterfaceController : MonoBehaviour {
    // Update is called once per frame
    void Update() {
        SetUIObjectsPosition(Camera.main.transform.position.x, Camera.main.transform.position.y);
        SetUIObjectsScale(Math.Abs(Camera.main.transform.position.z) / 9);
    }
    public void SetUIObjectsPosition(float xPos, float yPos) {
        transform.position = new Vector3(xPos, yPos, 0);
    }
    public void SetUIObjectsScale(float scale) {
        transform.localScale = new Vector3(scale, scale, 1);
    }
}