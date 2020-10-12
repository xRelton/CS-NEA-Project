using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortMechanics : MonoBehaviour {
    InteractiveComponents interactions;
    // Update is called once per frame
    void Update() {
        interactions = transform.parent.GetComponent<InteractiveComponents>();
        Vector2 mousePos = interactions.GetMousePos();
        for (int i = 0; i < transform.childCount; i++) {
            for (int j = 0; j < interactions.GetFChild("port", i).transform.childCount; j++) {
                if (interactions.PointOnObject(mousePos, interactions.GetCOChild("port", i, j))) {
                    interactions.GetCOChild("port", i, j).GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                } else {
                    interactions.GetCOChild("port", i, j).GetComponent<Renderer>().material.SetColor("_Color", new Color(0.7f, 0.7f, 0.7f));
                }
            }
        }
    }
}
