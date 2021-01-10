using System;
using System.Collections.Generic;
using UnityEngine;

public class PortMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    public List<PortInfo> Ports;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        Ports = new List<PortInfo>();
        Ports.Add(new PortInfo("Alexandria", 200000 + 17891, 4));
        Ports.Add(new PortInfo("Athens", 100000 - 802, 0));
        Ports.Add(new PortInfo("Byzantium", 250000 - 28751, 2));
        Ports.Add(new PortInfo("Carthage", 100000 + 41741, 3));
        Ports.Add(new PortInfo("Jaffa", 120000 - 4950, 1));
        Ports.Add(new PortInfo("Rome", 180000 + 1553, 0));

        Ports.Add(new PortInfo("Caralis", 16000 + 496, 0));
        Ports.Add(new PortInfo("New Carthage", 20000 - 237, 1));
        Ports.Add(new PortInfo("Cyrene", 14000 + 171, 0));
        Ports.Add(new PortInfo("Iol", 13000 - 211, 0));
        Ports.Add(new PortInfo("Itanus", 12000 - 329, 0));
        Ports.Add(new PortInfo("Leptis", 30000 - 465, 0));

        Ports.Add(new PortInfo("Massalia", 54000 + 450, 2));
        Ports.Add(new PortInfo("Salamis", 25000 + 329, 0));
        Ports.Add(new PortInfo("Salona", 30000 + 122, 3));
        Ports.Add(new PortInfo("Sparta", 50000 - 206, 0));
        Ports.Add(new PortInfo("Syracuse", 60000 - 111, 0));
        Ports.Add(new PortInfo("Tarraco", 20000 + 162, 3));
        foreach (PortInfo port in Ports) {
            for (int i = 0; i < transform.GetComponent<MarketSimulator>().Items.Count; i++) {
                port.Inventory.Add(i, 0);
            }
        }
    }
    // Update is called once per frame
    void Update() {
        for (int i = 0; i < transform.childCount; i++) { // Iterates through major and then minor ports
            for (int j = 0; j < transform.GetChild(i).childCount; j++) { // Iterates through ports
                GameObject PortObject = transform.GetChild(i).GetChild(j).gameObject;
                GameObject UIScreen = GameObject.Find("User Interface").transform.GetChild(1).gameObject;
                List<GameObject> Ships = GameObject.Find("Ship").GetComponent<ShipMechanics>().Ships;
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
                                    int PortNum = 0;
                                    for (int l = 0; l < Ports.Count; l++) {
                                        if (Ports[l].Name == PortObject.name) {
                                            PortNum = l;
                                            break;
                                        }
                                    }
                                    if (Ships[k].GetComponent<ShipInfo>().Port == PortNum) {
                                        ShipsHome.Add(k);
                                    } else {
                                        ShipsAway.Add(k);
                                    }
                                }
                            }
                            List<ButtonUIObject> PortButtons = new List<ButtonUIObject>();
                            int yFix = 0;
                            if (ShipsAway.Count != 0) {
                                yFix--;
                                PortButtons.Add(new ButtonUIObject("Send ship", "ShipRequest", new Vector2(), ShipsAway.ToArray()));
                            }
                            if (ShipsHome.Count != 0) {
                                PortButtons.Add(new ButtonUIObject("Open market", "ShipRequest", new Vector2(0, yFix), ShipsHome.ToArray()));
                            }
                            List<TextUIObject> Title = new List<TextUIObject> { new TextUIObject(PortObject.name, new Vector2(0, 3)) };
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
    string name;
    Dictionary<int, int> inventory = new Dictionary<int, int>(); // Includes item id and number of items
    int population;
    int climate;
    public PortInfo(string name, double population, int climate) {
        this.name = name;
        this.population = (int)population;
        this.climate = climate;
    }
    public string Name { get => name; }
    public Dictionary<int, int> Inventory { set => inventory = value; get => inventory; }
    public int Population { get => population; }
    public int Climate { get => climate; }
}