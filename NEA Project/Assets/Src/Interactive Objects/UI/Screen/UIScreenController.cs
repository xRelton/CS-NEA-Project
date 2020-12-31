using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenController : MonoBehaviour {
    public int PortNum;
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
                    List<PortInfo> AllPorts = GameObject.Find("Port").GetComponent<PortMechanics>().Ports;
                    int PortNum = 0;
                    for (int i = 0; i < AllPorts.Count; i++) {
                        if (AllPorts[i].Name == buttonScript.name.Replace("Button ", "")) {
                            PortNum = i;
                            break;
                        }
                    }
                    ShipMechs.PlayerShips.Add(ShipMechs.NewShip(string.Format("{0} 1", ShipMechs.ShipNames[5]), ShipMechs.ShipNames[5], GameObject.Find("Ship").transform));
                    ShipMechs.PlayerShips[0].GetComponent<ShipInfo>().Port = PortNum;
                    ShipMechs.PlayerShips[0].GetComponent<ShipInfo>().Dock();
                    ShipMechs.PlayerShips.Add(ShipMechs.NewShip(string.Format("{0} 1", ShipMechs.ShipNames[7]), ShipMechs.ShipNames[7], GameObject.Find("Ship").transform));
                    ShipMechs.PlayerShips[1].GetComponent<ShipInfo>().Port = 5;
                    ShipMechs.PlayerShips[1].GetComponent<ShipInfo>().Dock();
                } else if (buttonScript.Action == "ShipRequest") {
                    if (buttonScript.References.Length == 1) {
                        if (buttonScript.transform.GetComponentInChildren<Text>().text == "Send ship") {
                            ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = PortNum;
                        } else {
                            CreateMarketScreen(PortNum, buttonScript.References[0]);
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
                    if (ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port == PortNum) { // Market
                        CreateMarketScreen(PortNum, buttonScript.References[0]);
                    } else { // Send ship
                        ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = PortNum;
                    }
                } else if (buttonScript.Action == "ItemPage") {
                    CreateMarketScreen(PortNum, buttonScript.References[0], buttonScript.References[1]);
                } else if (buttonScript.Action == "ManageItem") {
                    CreateMarketScreen(PortNum, buttonScript.References[0], buttonScript.References[1], buttonScript.References[2], true);
                }
            }
        }
    }
    void CreateMarketScreen(int portNum, int shipNum, int itemNum = -1, int buyOrSell = -1, bool saleSlider = false) {
        MarketSimulator MarketSim = GameObject.Find("Port").GetComponent<MarketSimulator>();
        PortInfo Port = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[portNum];
        List<ButtonUIObject> ItemsButtons = new List<ButtonUIObject>();
        int move = 0;
        for (int i = 0; i < MarketSim.Items.Count; i++) {
            ItemsButtons.Add(new ButtonUIObject(MarketSim.Items[i].Name, "ItemPage", new Vector2(-5, move), new int[] { shipNum, i }, new Vector2(0.6f, 1)));
            move--;
        }
        List<TextUIObject> PageInfo = new List<TextUIObject>();
        PageInfo.Add(new TextUIObject(Port.Name + " Market", new Vector2(0, 3)));
        if (saleSlider == false) {
            if (itemNum == -1) {
                PageInfo.Add(new TextUIObject("Population: " + Port.Population, new Vector2(2, 0.5f)));
                PageInfo.Add(new TextUIObject("Climate: " + MarketSim.Climates[Port.Climate], new Vector2(2, -0.5f)));
            } else { // If the user has chosen an item to buy / sell
                int[] PriceAndQuantity = MarketSim.GetPriceAndQuantity(itemNum, portNum);
                PageInfo.Add(new TextUIObject("Price (Roman Coins): " + PriceAndQuantity[0], new Vector2(2, 1.5f)));
                PageInfo.Add(new TextUIObject("Quantity: " + PriceAndQuantity[1], new Vector2(2, 0.5f)));
                if (PriceAndQuantity[1] > 0) {
                    ItemsButtons.Add(new ButtonUIObject("Buy", "ManageItem", new Vector2(2, -2.5f), new int[] { shipNum, itemNum, 0 }, new Vector2(0.6f, 1)));
                }
                if (PriceAndQuantity[0] > 0) {
                    ItemsButtons.Add(new ButtonUIObject("Sell", "ManageItem", new Vector2(2, -3.5f), new int[] { shipNum, itemNum, 1 }, new Vector2(0.6f, 1)));
                }
                PageInfo.Add(new TextUIObject("Disclaimer: Price listed is buy price, sell price is marked down.", new Vector2(2, -2.75f), new Vector2(0.4f, 0.4f)));
            }
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, ItemsButtons, true);
        } else {
            int[] PriceAndQuantity = MarketSim.GetPriceAndQuantity(itemNum, portNum);
            List<SliderUIObject> SliderInfo = new List<SliderUIObject>();
            SliderInfo.Add(new SliderUIObject("Sales", new Vector2(2, -0.5f), new Vector2(1, 1), PriceAndQuantity[1]));
            PageInfo.Add(new TextUIObject("Price (Roman Coins): ", new Vector2(2, 1.5f)));
            PageInfo.Add(new TextUIObject("Quantity: ", new Vector2(2, 0.5f)));
            if (buyOrSell == 0) {
                ItemsButtons.Add(new ButtonUIObject("Buy", "BuyItem", new Vector2(2, -4), new int[] { PriceAndQuantity[0] }, new Vector2(0.6f, 1)));
            } else {
                ItemsButtons.Add(new ButtonUIObject("Sell", "SellItem", new Vector2(2, -4), new int[] { PriceAndQuantity[0] }, new Vector2(0.6f, 1)));
            }
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, SliderInfo, ItemsButtons, true);
        }
        
    }
}