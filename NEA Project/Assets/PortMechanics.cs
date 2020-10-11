using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;

public class PortMechanics : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePos = GetMousePos();
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 6 * (i + 1); j++) {
                    if (PointOnObject(mousePos, GetPort(i, j))) {
                        GetPort(i, j).GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                    }
                }
            }
        }
    }

    Vector2 GetMousePos() {
        float zoomAbs = Math.Abs(Camera.main.transform.position.z);
        Vector2 screenCentreInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosOnScreen = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePosOnScreen.x = (2 * mousePosOnScreen.x - 1);
        mousePosOnScreen.y = (2 * mousePosOnScreen.y - 1);
        mousePosOnScreen.x = (zoomAbs / -78.50297938f) * (-104.0847188f * mousePosOnScreen.x - 0.01859821175f) + (mousePosOnScreen.x / -78.50297938f);
        mousePosOnScreen.y = (zoomAbs / -22.73010161f) * (-12.87099021f * mousePosOnScreen.y - 0.01777780835f) + (mousePosOnScreen.y / -22.73010161f);
        return screenCentreInWorld + mousePosOnScreen;
    }

    bool PointOnObject(Vector2 Point, Transform objChecked) {
        Collider2D objHit = Physics2D.Raycast(Point, Vector2.zero).collider;
        if (objHit != null) {
            return (objHit.name == objChecked.name);
        }
        return false;
    }

    Transform GetPort(int portType, int portNum) {
        GameObject originalGameObject = GameObject.Find("port");
        GameObject child = originalGameObject.transform.GetChild(portType).gameObject;
        return child.transform.GetChild(portNum);
    }
}
