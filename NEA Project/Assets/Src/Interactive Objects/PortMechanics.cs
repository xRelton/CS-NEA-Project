using System.Collections.Generic;
using UnityEngine;

public class PortMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }
    // Update is called once per frame
    void Update() {
        for (int i = 0; i < transform.childCount; i++) { // Iterates through major and then minor ports
            for (int j = 0; j < Interactions.GetC("Port", i).transform.childCount; j++) { // Iterates through ports
                GameObject PortObject = Interactions.GetC("Port", i, j);
                ShipMechanics ShipMechs = GameObject.Find("Ship").GetComponent<ShipMechanics>();
                if (Interactions.MouseOnObject(PortObject)) {
                    if (Interactions.GetC("User Interface", 1).activeSelf == false) {
                        if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                            PortObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                        }
                        if (Input.GetMouseButtonDown(0)) { // Creates UI screen for port when clicked on
                            List<GameObject> PortButtons = new List<GameObject>();
                            if (GameObject.Find("Ship").GetComponent<ShipMechanics>().PlayerShips[0].GetComponent<ShipInfo>().Docked()) {
                                PortButtons.Add(Interactions.GetC("User Interface", 1).GetComponent<UIScreenController>().NewButton("Send ship", "SetPort", PortObject.name, new Vector2()));
                            }
                            GameObject.Find("User Interface").GetComponent<UserInterfaceController>().CreateScreen("Market", PortObject.name, true, PortButtons);
                        }
                    }
                } else if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Interactions.MyGrey && Interactions.GetC("User Interface", 1).activeSelf == false) {
                    PortObject.GetComponent<Renderer>().material.SetColor("_Color", Interactions.MyGrey);
                }
            }
        }
    }
}