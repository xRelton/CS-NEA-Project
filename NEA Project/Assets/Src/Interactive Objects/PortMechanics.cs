using System;
using System.Collections.Generic;
using UnityEngine;

public class PortMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    public Dictionary<string, PortInfo> Ports;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        Ports = new Dictionary<string, PortInfo>();
        Ports.Add("Alexandria", new PortInfo(200000 + 17891, 4));
        Ports.Add("Athens", new PortInfo(100000 - 802, 0));
        Ports.Add("Byzantium", new PortInfo(250000 - 28751, 2));
        Ports.Add("Carthage", new PortInfo(100000 + 41741, 3));
        Ports.Add("Jaffa", new PortInfo(120000 - 4950, 1));
        Ports.Add("Rome", new PortInfo(180000 + 1553, 0));

        Ports.Add("Caralis", new PortInfo(16000 + 496, 0));
        Ports.Add("New Carthage", new PortInfo(20000 - 237, 1));
        Ports.Add("Cyrene", new PortInfo(14000 + 171, 0));
        Ports.Add("Iol", new PortInfo(13000 - 211, 0));
        Ports.Add("Itanus", new PortInfo(12000 - 329, 0));
        Ports.Add("Leptis", new PortInfo(30000 - 465, 0));

        Ports.Add("Massalia", new PortInfo(54000 + 450, 2));
        Ports.Add("Salamis", new PortInfo(25000 + 329, 0));
        Ports.Add("Salona", new PortInfo(30000 + 122, 3));
        Ports.Add("Sparta", new PortInfo(50000 - 206, 0));
        Ports.Add("Syracuse", new PortInfo(60000 - 111, 0));
        Ports.Add("Tarraco", new PortInfo(20000 + 162, 3));
    }
    // Update is called once per frame
    void Update() {
        for (int i = 0; i < transform.childCount; i++) { // Iterates through major and then minor ports
            for (int j = 0; j < transform.GetChild(i).childCount; j++) { // Iterates through ports
                GameObject PortObject = transform.GetChild(i).GetChild(j).gameObject;
                GameObject UIScreen = GameObject.Find("User Interface").transform.GetChild(1).gameObject;
                List<GameObject> Ships = GameObject.Find("Ship").GetComponent<ShipMechanics>().PlayerShips;
                if (Interactions.MouseOnObject(PortObject)) {
                    if (UIScreen.activeSelf == false) {
                        if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                            PortObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                        }
                        if (Input.GetMouseButtonDown(0)) { // Creates UI screen for port when clicked on
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
                            List<Tuple<string, string, Vector2, int[]>> PortButtons = new List<Tuple<string, string, Vector2, int[]>>();
                            int yFix = 0;
                            if (ShipsAway.Count != 0) {
                                yFix--;
                                PortButtons.Add(new Tuple<string, string, Vector2, int[]>("Send ship", "SetPort", new Vector2(), ShipsAway.ToArray()));
                            }
                            if (ShipsHome.Count != 0) {
                                PortButtons.Add(new Tuple<string, string, Vector2, int[]>("Open market", "OpenMarket", new Vector2(0, yFix), ShipsHome.ToArray()));
                            }
                            List<Tuple<string, Vector2>> Title = new List<Tuple<string, Vector2>> { new Tuple<string, Vector2>(PortObject.name, new Vector2(0, 3)) };
                            GameObject.Find("User Interface").GetComponent<UserInterfaceController>().CreateScreen(Title, PortButtons, true, true);
                        }
                    }
                } else if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Interactions.MyGrey && UIScreen.activeSelf == false) {
                    PortObject.GetComponent<Renderer>().material.SetColor("_Color", Interactions.MyGrey);
                }
            }
        }
    }
}
public class PortInfo {
    int population;
    int climate;
    public PortInfo(double population, int climate) {
        this.population = (int)population;
        this.climate = climate;
    }

    public int Population { get => population; }
    public int Climate { get => climate; }
}