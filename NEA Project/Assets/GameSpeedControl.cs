using System;
using UnityEngine;

public class GameSpeedControl : MonoBehaviour {
    InteractiveComponents interactions;
    string speedName;
    // Start is called before the first frame update
    void Start() {
        interactions = transform.parent.GetComponent<InteractiveComponents>();
        speedName = "reg";
    }
    // Update is called once per frame
    void Update() {
        float zoomAbs = Math.Abs(Camera.main.transform.position.z)/9;
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
        transform.localScale = new Vector3(zoomAbs, zoomAbs, 1);
        Vector2 mousePos = interactions.GetMousePos();
        for (int i = 0; i < transform.childCount; i++) {
            if (interactions.PointOnObject(mousePos, interactions.GetFChild("game-speed", i))) {
                if (Input.GetMouseButtonDown(0)) {
                    speedName = interactions.GetFChild("game-speed", i).name;
                    interactions.GetFChild("game-speed", i).GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                }
            }
            if (interactions.GetFChild("game-speed", i).name != speedName) {
                interactions.GetFChild("game-speed", i).GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }
        }
    }
}