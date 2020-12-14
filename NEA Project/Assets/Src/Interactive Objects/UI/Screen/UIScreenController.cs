using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenController : MonoBehaviour {
    public string PortName;
    public List<GameObject> Texts;
    public List<GameObject> Sliders;
    public List<GameObject> Buttons;
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
                } else if (buttonScript.Action == "ShipRequest") {
                    if (buttonScript.References.Length == 1) {
                        if (buttonScript.transform.GetComponentInChildren<Text>().text == "Send ship") {
                            ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = PortName;
                        } else {
                            CreateMarketScreen(PortName, buttonScript.References[0]);
                        }
                    } else {
                        List<ButtonUIObject> ShipChoiceButtons = new List<ButtonUIObject>();
                        for (int i = 0; i < buttonScript.References.Length; i++) {
                            ShipChoiceButtons.Add(new ButtonUIObject(ShipMechs.PlayerShips[buttonScript.References[i]].name, "ShipSelect", new Vector2(0, -i), new int[] { i }));
                        }
                        List<TextUIObject> Title = new List<TextUIObject>();
                        if (buttonScript.transform.GetComponentInChildren<Text>().text == "Send ship") {
                            Title.Add(new TextUIObject("Select Ship to Send", new Vector2(0, 3)));
                        } else {
                            Title.Add(new TextUIObject("Select Ship to Trade with", new Vector2(0, 3)));
                        }
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
                } else if (buttonScript.Action == "ManageItem") {
                    if (buttonScript.References[2] == 0) { // Item is being bought
                        CreateMarketScreen(PortName, buttonScript.References[0], buttonScript.References[1], true);
                    } else { // Item is being sold

                    }
                }
            }
        }
    }
    void CreateMarketScreen(string portName, int shipNum, int item = -1, bool saleSlider = false) {
        MarketSimulator MarketSim = GameObject.Find("Port").GetComponent<MarketSimulator>();
        PortInfo Port = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[portName];
        List<ButtonUIObject> ItemsButtons = new List<ButtonUIObject>();
        int move = 0;
        for (int i = 0; i < MarketSim.Items.Count; i++) {
            if (MarketSim.ItemInPort(i, portName)) {
                ItemsButtons.Add(new ButtonUIObject(MarketSim.Items[i].Name, "ItemPage", new Vector2(-5, move), new int[] { shipNum, i }, new Vector2(0.6f, 1)));
                move--;
            }
        }
        List<TextUIObject> PageInfo = new List<TextUIObject>();
        PageInfo.Add(new TextUIObject(portName + " Market", new Vector2(0, 3)));
        if (saleSlider == false) {
            if (item == -1) {
                PageInfo.Add(new TextUIObject("Population: " + Port.Population, new Vector2(2, 0.5f)));
                PageInfo.Add(new TextUIObject("Climate: " + MarketSim.Climates[Port.Climate], new Vector2(2, -0.5f)));
            } else { // If the user has chosen an item to buy / sell
                float[] PriceAndQuantity = MarketSim.GetPriceAndQuantity(item, portName);
                PageInfo.Add(new TextUIObject("Price: " + PriceAndQuantity[0], new Vector2(2, 2)));
                PageInfo.Add(new TextUIObject("Quantity: " + PriceAndQuantity[1], new Vector2(2, 1)));
                ItemsButtons.Add(new ButtonUIObject("Buy", "ManageItem", new Vector2(2, -2), new int[] { shipNum, item, 0 }, new Vector2(0.6f, 1)));
                ItemsButtons.Add(new ButtonUIObject("Sell", "ManageItem", new Vector2(2, -3), new int[] { shipNum, item, 1 }, new Vector2(0.6f, 1)));
            }
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, ItemsButtons, true);
        } else {
            List<SliderUIObject> SliderInfo = new List<SliderUIObject>();
            SliderInfo.Add(new SliderUIObject("Sales", new Vector2(2, 0), new Vector2(1, 1)));
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, SliderInfo, ItemsButtons, true);
        }
        
    }
}