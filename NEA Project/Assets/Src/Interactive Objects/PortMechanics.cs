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
                GameObject UIScreen = Interactions.GetC("User Interface", 1);
                List<GameObject> Ships = GameObject.Find("Ship").GetComponent<ShipMechanics>().PlayerShips;
                if (Interactions.MouseOnObject(PortObject)) {
                    if (UIScreen.activeSelf == false) {
                        if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                            PortObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                        }
                        if (Input.GetMouseButtonDown(0)) { // Creates UI screen for port when clicked on
                            List<GameObject> PortButtons = new List<GameObject>();
                            List<int> ShipsAway = new List<int>();
                            List<int> ShipsHome = new List<int>();
                            for (int k = 0; k < Ships.Count; k++) {
                                if (Ships[k].GetComponent<ShipInfo>().Docked()) {
                                    if (Ships[k].GetComponent<ShipInfo>().Port == PortObject.name) {
                                        ShipsHome.Add(k);
                                    } else {
                                        ShipsAway.Add(k);
                                    }
                                }
                            }
                            int yFix = 0;
                            if (ShipsAway.Count != 0) {
                                yFix--;
                                PortButtons.Add(UIScreen.GetComponent<UIScreenController>().NewButton("Send ship", "SetPort", PortObject.name, ShipsAway, new Vector2()));
                            }
                            if (ShipsHome.Count != 0) {
                                PortButtons.Add(UIScreen.GetComponent<UIScreenController>().NewButton("Open market", "OpenMarket", PortObject.name, ShipsHome, new Vector2(0, yFix)));
                            }
                            GameObject.Find("User Interface").GetComponent<UserInterfaceController>().CreateScreen("PortServices", PortObject.name, true, PortButtons);
                        }
                    }
                } else if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Interactions.MyGrey && Interactions.GetC("User Interface", 1).activeSelf == false) {
                    PortObject.GetComponent<Renderer>().material.SetColor("_Color", Interactions.MyGrey);
                }
            }
        }
    }
}