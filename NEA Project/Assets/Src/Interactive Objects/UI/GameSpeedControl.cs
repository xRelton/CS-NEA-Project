using System;
using UnityEngine;

public class GameSpeedControl : MonoBehaviour {
    InteractiveComponents Interactions;
    string SpeedName;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        SpeedName = "Reg";
    }
    // Update is called once per frame
    void Update() {
        // Comment 2 below for efficiency
        transform.parent.GetComponent<UserInterfaceController>().SetUIObjectsPosition(Camera.main.transform.position.x, Camera.main.transform.position.y);
        transform.parent.GetComponent<UserInterfaceController>().SetUIObjectsScale(Math.Abs(Camera.main.transform.position.z) / 9);
        for (int i = 0; i < transform.childCount; i++) {
            GameObject UIObject = Interactions.GetC("Game Speed", i);
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
    }
}