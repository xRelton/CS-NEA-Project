using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSpeedControl : MonoBehaviour {
    InteractiveComponents Interactions;
    string SpeedName;
    Dictionary<string, float> SpeedVals = new Dictionary<string, float>();
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        SpeedName = "Reg";
        /*for (int i = 0; i < transform.childCount; i++) {
            SpeedVals.Add(Interactions.GetC("Game Speed", i).name, i);
        }*/
        SpeedVals.Add("Pause", 0);
        SpeedVals.Add("Reg", 1);
        SpeedVals.Add("Fast", 3);
        SpeedVals.Add("Fastest", 6);
    }
    // Update is called once per frame
    void Update() {
        // Comment 2 below for efficiency
        transform.parent.GetComponent<UserInterfaceController>().SetUIObjectsPosition(Camera.main.transform.position.x, Camera.main.transform.position.y);
        transform.parent.GetComponent<UserInterfaceController>().SetUIObjectsScale(Math.Abs(Camera.main.transform.position.z) / 9);
        for (int i = 0; i < transform.childCount; i++) {
            GameObject UIObject = transform.GetChild(i).gameObject;
            Material UIObjectMaterial = UIObject.GetComponent<Renderer>().material;
            if (Interactions.MouseOnObject(UIObject)) {
                if (Input.GetMouseButtonDown(0)) {
                    SpeedName = UIObject.name;
                }
            }
            if (UIObject.name != SpeedName && UIObjectMaterial.GetColor("_Color") != Color.white) {
                UIObjectMaterial.SetColor("_Color", Color.white);
            } else if (UIObject.name == SpeedName && UIObjectMaterial.GetColor("_Color") != Color.green) {
                UIObjectMaterial.SetColor("_Color", Color.green);
            }
        }
        Interactions.TimeDilation = Time.deltaTime * SpeedVals[SpeedName];
    }
}