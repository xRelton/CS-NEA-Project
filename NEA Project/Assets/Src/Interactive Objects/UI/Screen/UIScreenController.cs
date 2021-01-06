using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenController : MonoBehaviour {
    public int PortNum;
    public List<GameObject> Texts;
    public List<GameObject> Sliders;
    public List<GameObject> Buttons;
    ShipMechanics ShipMechs;
    void Start() {
        ShipMechs = GameObject.Find("Ship").GetComponent<ShipMechanics>();
    }
    // Update is called once per frame
    void Update() {
        foreach (GameObject button in Buttons) {
            UIButton Button = button.GetComponent<UIButton>();
            if (Button.ButtonPressed()) {
                GameObject.Find("UI Screen").SetActive(false);
                if (Button.Action == "SetInitialPort") {
                    List<PortInfo> AllPorts = GameObject.Find("Port").GetComponent<PortMechanics>().Ports;
                    int PortNum = 0;
                    for (int i = 0; i < AllPorts.Count; i++) {
                        if (AllPorts[i].Name == Button.name.Replace("Button ", "")) {
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
                } else if (Button.Action == "ShipRequest") {
                    if (Button.References.Length == 1) {
                        if (Button.transform.GetComponentInChildren<Text>().text == "Send ship") {
                            ShipMechs.PlayerShips[Button.References[0]].GetComponent<ShipInfo>().Port = PortNum;
                        } else {
                            CreateMarketScreen(Button.References[0]);
                        }
                    } else {
                        List<ButtonUIObject> ShipChoiceButtons = new List<ButtonUIObject>();
                        for (int i = 0; i < Button.References.Length; i++) {
                            ShipChoiceButtons.Add(new ButtonUIObject(ShipMechs.PlayerShips[Button.References[i]].name, "ShipSelect", new Vector2(0, -i), new int[] { i }));
                        }
                        List<TextUIObject> Title = new List<TextUIObject>();
                        string PortName = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[PortNum].Name;
                        if (Button.transform.GetComponentInChildren<Text>().text == "Send ship") {
                            Title.Add(new TextUIObject(PortName + ": Send Ship", new Vector2(0, 3)));
                        } else {
                            Title.Add(new TextUIObject(PortName + ": Trade with Ship", new Vector2(0, 3)));
                        }
                        transform.GetComponentInParent<UserInterfaceController>().CreateScreen(Title, ShipChoiceButtons, true);
                    }
                } else if (Button.Action == "ShipSelect") {
                    if (ShipMechs.PlayerShips[Button.References[0]].GetComponent<ShipInfo>().Port == PortNum) { // Market
                        CreateMarketScreen(Button.References[0]);
                    } else { // Send ship
                        ShipMechs.PlayerShips[Button.References[0]].GetComponent<ShipInfo>().Port = PortNum;
                    }
                } else if (Button.Action == "ItemPage") {
                    CreateMarketScreen(Button.References[0], Button.References[1]);
                } else if (Button.Action == "ManageItem") {
                    CreateMarketScreen(Button.References[0], Button.References[1], Button.References[2], true);
                } else if (Button.Action == "BuyItem" || Button.Action == "SellItem") {
                    foreach (GameObject slider in Sliders) {
                        int numItems = (int)slider.GetComponent<Slider>().value;
                        if (Button.Action == "SellItem") { numItems = -numItems; }
                        if (slider.name.Split()[1].Contains("Sales")) {
                            ShipMechs.PlayerShips[Button.References[0]].GetComponent<ShipInfo>().Inventory[Button.References[1]] += numItems;
                            GameObject.Find("Port").GetComponent<PortMechanics>().Ports[PortNum].Inventory[Button.References[1]] -= numItems;
                            GameObject.Find("Port").GetComponent<MarketSimulator>().PlayerCoins -= Button.References[2] * numItems;
                            break;
                        }
                    }
                }
            }
        }
    }
    void CreateMarketScreen(int shipNum, int itemNum = -1, int buyOrSell = -1, bool saleSlider = false) {
        MarketSimulator MarketSim = GameObject.Find("Port").GetComponent<MarketSimulator>();
        PortInfo Port = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[PortNum];
        ShipInfo Ship = ShipMechs.PlayerShips[shipNum].GetComponent<ShipInfo>();
        List<ButtonUIObject> ItemsButtons = new List<ButtonUIObject>();
        int move = 0;
        for (int i = 0; i < MarketSim.Items.Count; i++) {
            ItemsButtons.Add(new ButtonUIObject(MarketSim.Items[i].Name, "ItemPage", new Vector2(-5, move), new int[] { shipNum, i }, new Vector2(0.6f, 1)));
            move--;
        }
        List<TextUIObject> PageInfo = new List<TextUIObject>();
        if (saleSlider == false) {
            if (itemNum == -1) {
                PageInfo.Add(new TextUIObject(Port.Name + " Market", new Vector2(0, 3)));
                PageInfo.Add(new TextUIObject("Population: " + Port.Population, new Vector2(2, 0.5f)));
                PageInfo.Add(new TextUIObject("Population: " + Port.Population, new Vector2(2, 0.5f)));
                PageInfo.Add(new TextUIObject("Climate: " + MarketSim.Climates[Port.Climate], new Vector2(2, -0.5f)));
            } else { // If the user has chosen an item to buy / sell
                int[] PriceAndQuantity = MarketSim.GetPriceAndQuantity(itemNum, PortNum);
                PageInfo.Add(new TextUIObject(Port.Name + " Market: " + MarketSim.Items[itemNum].Name, new Vector2(0, 3)));
                PageInfo.Add(new TextUIObject("Amount in ship: " + Ship.Inventory[itemNum], new Vector2(2, 2.1f), new Vector2(0.8f, 0.8f)));
                PageInfo.Add(new TextUIObject("Price (Roman Coins): " + (int)(PriceAndQuantity[0] * 1.2f), new Vector2(2, 1.3f), new Vector2(0.8f, 0.8f)));
                PageInfo.Add(new TextUIObject("Port Quantity: " + Port.Inventory[itemNum], new Vector2(2, 0.5f), new Vector2(0.8f, 0.8f)));
                if (PriceAndQuantity[1] > 0 && MarketSim.PlayerCoins >= (int)(PriceAndQuantity[0] * 1.2f) && Port.Inventory[itemNum] > 0) {
                    ItemsButtons.Add(new ButtonUIObject("Buy", "ManageItem", new Vector2(2, -2.5f), new int[] { shipNum, itemNum, 0 }, new Vector2(0.6f, 1)));
                }
                if (PriceAndQuantity[0] > 0 && Ship.Inventory[itemNum] > 0) {
                    ItemsButtons.Add(new ButtonUIObject("Sell", "ManageItem", new Vector2(2, -3.5f), new int[] { shipNum, itemNum, 1 }, new Vector2(0.6f, 1)));
                }
                PageInfo.Add(new TextUIObject("Disclaimer: Price listed is buy price, sell price is marked down.", new Vector2(2, -2.75f), new Vector2(0.4f, 0.4f)));
            }
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, ItemsButtons, true);
        } else {
            int[] PriceAndQuantity = MarketSim.GetPriceAndQuantity(itemNum, PortNum);
            List<SliderUIObject> SliderInfo = new List<SliderUIObject>();
            PageInfo.Add(new TextUIObject(Port.Name + " Market: " + MarketSim.Items[itemNum].Name, new Vector2(0, 3)));
            PageInfo.Add(new TextUIObject("Price (Roman Coins): ", new Vector2(2, 1.5f)));
            PageInfo.Add(new TextUIObject("Quantity: ", new Vector2(2, 0.5f)));
            if (buyOrSell == 0) {
                SliderInfo.Add(new SliderUIObject("Sales", new Vector2(2, -0.5f), new Vector2(1, 1), Port.Inventory[itemNum]));
                ItemsButtons.Add(new ButtonUIObject("Buy", "BuyItem", new Vector2(2, -4), new int[] { shipNum, itemNum, (int)(PriceAndQuantity[0] * 1.2f) }, new Vector2(0.6f, 1)));
            } else {
                SliderInfo.Add(new SliderUIObject("Sales", new Vector2(2, -0.5f), new Vector2(1, 1), Ship.Inventory[itemNum]));
                ItemsButtons.Add(new ButtonUIObject("Sell", "SellItem", new Vector2(2, -4), new int[] { shipNum, itemNum, PriceAndQuantity[0] }, new Vector2(0.6f, 1)));
            }
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, SliderInfo, ItemsButtons, true);
        }
    }
}