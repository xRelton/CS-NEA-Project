using System;
using System.Collections.Generic;
using UnityEngine;

public class PortMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    public PortInfo[] Ports;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        Ports = new PortInfo[] {
            new PortInfo("Alexandria", 200000 + 17891, 4, new int[] { 0 }),
            new PortInfo("Athens", 100000 - 802, 0, new int[] { 1 }),
            new PortInfo("Byzantium", 250000 - 28751, 2, new int[] { 2 }),
            new PortInfo("Carthage", 100000 + 41741, 3, new int[] { 3 }),
            new PortInfo("Jaffa", 120000 - 4950, 1, new int[] { 4, 5 }),
            new PortInfo("Rome", 180000 + 1553, 0, new int[] { 6, 7 }),

            new PortInfo("Caralis", 16000 + 496, 0),
            new PortInfo("New Carthage", 20000 - 237, 1),
            new PortInfo("Cyrene", 14000 + 171, 0),
            new PortInfo("Iol", 13000 - 211, 0),
            new PortInfo("Itanus", 12000 - 329, 0),
            new PortInfo("Leptis", 30000 - 465, 0),

            new PortInfo("Massalia", 54000 + 450, 2),
            new PortInfo("Salamis", 25000 + 329, 0),
            new PortInfo("Salona", 30000 + 122, 3),
            new PortInfo("Sparta", 50000 - 206, 0),
            new PortInfo("Syracuse", 60000 - 111, 0),
            new PortInfo("Tarraco", 20000 + 162, 3)
        };
    }
    // Update is called once per frame
    void Update() {
        for (int i = 0; i < transform.childCount; i++) { // Iterates through major and then minor ports
            for (int j = 0; j < transform.GetChild(i).childCount; j++) { // Iterates through ports
                GameObject PortObject = transform.GetChild(i).GetChild(j).gameObject;
                GameObject UIScreen = GameObject.Find("User Interface").transform.GetChild(1).gameObject;
                GameObject[] Ships = GameObject.Find("Ship").GetComponent<ShipMechanics>().Ships.ToArray();
                if (Interactions.MouseOnObject(PortObject)) {
                    if (UIScreen.activeSelf == false) {
                        if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                            PortObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                        }
                        if (Input.GetMouseButtonDown(0)) { // Creates UI screen for port when clicked on
                            List<int> ShipsAway = new List<int>();
                            List<int> ShipsHome = new List<int>();
                            GameObject[] PlayerShips = Array.FindAll(Ships, element => element.GetComponent<ShipInfo>().Owner == -1);
                            for (int k = 0; k < PlayerShips.Length; k++) {
                                if (PlayerShips[k].GetComponent<ShipInfo>().Docked()) {
                                    int PortID = 0;
                                    for (int l = 0; l < Ports.Length; l++) {
                                        if (Ports[l].Name == PortObject.name) {
                                            PortID = l;
                                            break;
                                        }
                                    }
                                    if (PlayerShips[k].GetComponent<ShipInfo>().Port == PortID) {
                                        ShipsHome.Add(k);
                                    } else {
                                        ShipsAway.Add(k);
                                    }
                                }
                            }
                            List<ButtonUIObject> PortButtons = new List<ButtonUIObject>();
                            int yFix = 0;
                            if (ShipsAway.Count != 0) {
                                PortButtons.Add(new ButtonUIObject("Send ship", "ShipRequest", new Vector2(), ShipsAway.ToArray()));
                                yFix--;
                            }
                            if (ShipsHome.Count != 0) {
                                PortButtons.Add(new ButtonUIObject("Open market", "ShipRequest", new Vector2(0, yFix), ShipsHome.ToArray()));
                                yFix--;
                                if (i == 0 && (transform.GetComponent<MarketSimulator>().PlayerCoins >= Array.Find(Ports, element => element.Name == PortObject.name).GetMinShipValue() ||
                                        PlayerShips.Length > 1)) {
                                    PortButtons.Add(new ButtonUIObject("Ship market", "ShipMarket", new Vector2(0, yFix), ShipsHome.ToArray()));
                                    //yFix--;
                                }
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
    Dictionary<int, int[]> inventory = new Dictionary<int, int[]>(); // Includes item id then number of items, price, demand and supply
    int population;
    int climate;
    int[] shipsSold;
    public PortInfo(string name, double population, int climate, int[] shipsSold = null) {
        this.name = name;
        this.population = (int)population;
        this.climate = climate;
        this.shipsSold = shipsSold;
    }
    public string Name { get => name; }
    public Dictionary<int, int[]> Inventory { get => inventory; set => inventory = value; }
    public int Population { get => population; }
    public int Climate { get => climate; }
    public int[] ShipsSold { get => shipsSold; set => shipsSold = value; }
    public int GetMinShipValue() {
        List<ShipType> ShipTypes = GameObject.Find("Ship").GetComponent<ShipMechanics>().ShipTypes;
        int MinShipValue = ShipTypes[ShipsSold[0]].GetValue(false);
        for (int i = 1; i < ShipsSold.Length; i++) {
            if (ShipTypes[ShipsSold[1]].GetValue(false) < MinShipValue) {
                MinShipValue = ShipTypes[ShipsSold[1]].GetValue(false);
            }
        }
        return MinShipValue;
    }
}