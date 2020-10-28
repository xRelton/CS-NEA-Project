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
                bool MouseOnPosition = Interactions.MouseOnObject(PortObject); // Checks if the mouse is on the port
                if (MouseOnPosition == false) {
                    foreach (GameObject ship in ShipMechs.PlayerShips) {
                        if (Interactions.MouseOnObject(ship) && ship.transform.position == PortObject.transform.position) { // Checks if the mouse is on any ships docked at the port
                            MouseOnPosition = true;
                        }
                    }
                }
                if (MouseOnPosition) {
                    if (Interactions.GetC("User Interface", 1).activeSelf == false) {
                        if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                            PortObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                        }
                        if (Input.GetMouseButtonDown(0)) { // Creates UI screen for port when clicked on
                            List<GameObject> PortButtons = new List<GameObject>();
                            PortButtons.Add(Interactions.GetC("User Interface", 1).GetComponent<UIScreenController>().NewButton("Send ship", "SetPort", PortObject.name, new Vector2(0, 2)));
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