using System;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenController : MonoBehaviour {
    public string PortName;
    public List<GameObject> Buttons;
    public List<GameObject> Texts;
    // Update is called once per frame
    void Update() {
        foreach (GameObject button in Buttons) {
            UIButton buttonScript = button.GetComponent<UIButton>();
            if (buttonScript.ButtonPressed()) {
                ShipMechanics ShipMechs = GameObject.Find("Ship").GetComponent<ShipMechanics>();
                GameObject.Find("UI Screen").SetActive(false);
                if (buttonScript.Action == "SetInitialPort") {
                    ShipMechs.PlayerShips.Add(ShipMechs.NewShip(string.Format("{0} 1", ShipMechs.ShipNames[5]), ShipMechs.ShipNames[5], GameObject.Find("Ship").transform));
                    ShipMechs.PlayerShips[0].GetComponent<ShipInfo>().Port = buttonScript.name.Replace("Button ", "");
                    ShipMechs.PlayerShips[0].GetComponent<ShipInfo>().Dock();
                    ShipMechs.PlayerShips.Add(ShipMechs.NewShip(string.Format("{0} 1", ShipMechs.ShipNames[7]), ShipMechs.ShipNames[7], GameObject.Find("Ship").transform));
                    ShipMechs.PlayerShips[1].GetComponent<ShipInfo>().Port = "Rome";
                    ShipMechs.PlayerShips[1].GetComponent<ShipInfo>().Dock();
                } else if (buttonScript.Action == "SetPort") {
                    if (buttonScript.References.Length == 1) {
                        ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = PortName;
                    } else {
                        List<Tuple<string, string, Vector2, int[]>> ShipChoiceButtons = new List<Tuple<string, string, Vector2, int[]>>();
                        for (int i = 0; i < buttonScript.References.Length; i++) {
                            ShipChoiceButtons.Add(new Tuple<string, string, Vector2, int[]>(ShipMechs.PlayerShips[buttonScript.References[i]].name, "ShipSelect", new Vector2(0, -i), new int[] { i }));
                        }
                        List<Tuple<string, Vector2>> Title = new List<Tuple<string, Vector2>> { new Tuple<string, Vector2>("Select Ship to Send", new Vector2(0, 3)) };
                        transform.GetComponentInParent<UserInterfaceController>().CreateScreen(Title, ShipChoiceButtons, true);
                    }
                } else if (buttonScript.Action == "OpenMarket") {
                    if (buttonScript.References.Length == 1) {
                        CreateMarketScreen(PortName, buttonScript.References[0]);
                    } else {
                        List<Tuple<string, string, Vector2, int[]>> ShipChoiceButtons = new List<Tuple<string, string, Vector2, int[]>>();
                        for (int i = 0; i < buttonScript.References.Length; i++) {
                            ShipChoiceButtons.Add(new Tuple<string, string, Vector2, int[]>(ShipMechs.PlayerShips[buttonScript.References[i]].name, "ShipSelect", new Vector2(0, -i), new int[] { i }));
                        }
                        List<Tuple<string, Vector2>> Title = new List<Tuple<string, Vector2>> { new Tuple<string, Vector2>("Select Ship to Trade with", new Vector2(0, 3)) };
                        transform.GetComponentInParent<UserInterfaceController>().CreateScreen(Title, ShipChoiceButtons, true);
                    }
                } else if (buttonScript.Action == "ShipSelect") {
                    if (ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port == PortName) { // Market
                        CreateMarketScreen(PortName, buttonScript.References[0]);
                    } else { // Send ship
                        ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = PortName;
                    }
                } else if (buttonScript.Action == "ItemPage") {
                    Debug.Log(buttonScript.References[1]);
                    CreateMarketScreen(PortName, buttonScript.References[0], buttonScript.References[1]);
                }
            }
        }
    }
    void CreateMarketScreen(string portName, int shipNum, int item = -1) {
        MarketSimulator MarketSim = GameObject.Find("Port").GetComponent<MarketSimulator>();
        PortInfo Port = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[portName];
        List<Tuple<string, string, Vector2, Vector2, int[]>> ItemsButtons = new List<Tuple<string, string, Vector2, Vector2, int[]>>();
        int move = 0;
        for (int i = 0; i < MarketSim.Items.Count; i++) {
            if (MarketSim.ItemInPort(i, portName)) {
                ItemsButtons.Add(new Tuple<string, string, Vector2, Vector2, int[]>(MarketSim.Items[i].Name, "ItemPage", new Vector2(-5, move), new Vector2(0.6f, 1), new int[] { shipNum, i }));
                move--;
            }
        }
        List<Tuple<string, Vector2>> PageInfo = new List<Tuple<string, Vector2>>();
        PageInfo.Add(new Tuple<string, Vector2>(portName + " Market", new Vector2(0, 3)));
        if (item == -1) {
            PageInfo.Add(new Tuple<string, Vector2>("Population: " + Port.Population, new Vector2(2, 0.5f)));
            PageInfo.Add(new Tuple<string, Vector2>("Climate: " + MarketSim.Climates[Port.Climate], new Vector2(2, -0.5f)));
        } else { // If the user has chosen an item to buy / sell
            float[] PriceAndQuantity = MarketSim.GetPriceAndQuantity(item, portName);
            PageInfo.Add(new Tuple<string, Vector2>("Price: " + PriceAndQuantity[0], new Vector2(2, 2)));
            PageInfo.Add(new Tuple<string, Vector2>("Quantity: " + PriceAndQuantity[1], new Vector2(2, 1)));
            ItemsButtons.Add(new Tuple<string, string, Vector2, Vector2, int[]>("Buy", "BuyItem", new Vector2(2, -2), new Vector2(0.6f, 1), new int[] { shipNum, item }));
            ItemsButtons.Add(new Tuple<string, string, Vector2, Vector2, int[]>("Sell", "SellItem", new Vector2(2, -3), new Vector2(0.6f, 1), new int[] { shipNum, item }));
        }
        transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, ItemsButtons, true);
    }
}