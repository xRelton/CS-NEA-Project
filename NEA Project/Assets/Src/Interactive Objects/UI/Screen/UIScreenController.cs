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
                switch (Button.Action) {
                    case ("SetInitialPort"):
                        List<PortInfo> AllPorts = GameObject.Find("Port").GetComponent<PortMechanics>().Ports;
                        int SelectedPortNum = 0;
                        for (int i = 0; i < AllPorts.Count; i++) {
                            if (AllPorts[i].Name == Button.name.Replace("Button ", "")) {
                                SelectedPortNum = i;
                                break;
                            }
                        }
                        ShipMechs.Ships.Add(ShipMechs.NewShip("player", 5, GameObject.Find("Ship").transform));
                        ShipMechs.Ships[0].GetComponent<ShipInfo>().Port = SelectedPortNum;
                        ShipMechs.Ships[0].GetComponent<ShipInfo>().Dock();
                        ShipMechs.Ships.Add(ShipMechs.NewShip("player", 7, GameObject.Find("Ship").transform));
                        ShipMechs.Ships[1].GetComponent<ShipInfo>().Port = 5;
                        ShipMechs.Ships[1].GetComponent<ShipInfo>().Dock();
                        break;
                    case ("ShipRequest"):
                        RequestShip(Button.References, Button.transform.GetComponentInChildren<Text>().text);
                        break;
                    case ("ShipSelect"):
                        if (ShipMechs.Ships[Button.References[0]].GetComponent<ShipInfo>().Port == PortNum) { // Market
                            CreateMarketScreen(Button.References[0]);
                        } else { // Send ship
                            ShipMechs.Ships[Button.References[0]].GetComponent<ShipInfo>().Port = PortNum;
                        }
                        break;
                    case ("ItemPage"):
                        CreateMarketScreen(Button.References[0], Button.References[1]);
                        break;
                    case ("ManageItem"):
                        CreateMarketScreen(Button.References[0], Button.References[1], Button.References[2], true);
                        break;
                    case ("BuyItem"): case ("SellItem"):
                        foreach (GameObject slider in Sliders) {
                            int numItems = (int)slider.GetComponent<Slider>().value;
                            if (Button.Action == "SellItem") { numItems = -numItems; }
                            if (slider.name.Split()[1].Contains("Sales")) {
                                ShipMechs.Ships[Button.References[0]].GetComponent<ShipInfo>().Inventory[Button.References[1]] += numItems;
                                GameObject.Find("Port").GetComponent<PortMechanics>().Ports[PortNum].Inventory[Button.References[1]] -= numItems;
                                GameObject.Find("Port").GetComponent<MarketSimulator>().PlayerCoins -= Button.References[2] * numItems;
                                break;
                            }
                        }
                        break;
                }
            }
        }
    }
    void RequestShip(int[] references, string text) {
        if (references.Length == 1) {
            if (text == "Send ship") {
                ShipMechs.Ships[references[0]].GetComponent<ShipInfo>().Port = PortNum;
            } else {
                CreateMarketScreen(references[0]);
            }
        } else {
            List<ButtonUIObject> ShipChoiceButtons = new List<ButtonUIObject>();
            for (int i = 0; i < references.Length; i++) {
                ShipChoiceButtons.Add(new ButtonUIObject(ShipMechs.Ships[references[i]].name, "ShipSelect", new Vector2(0, -i), new int[] { i }));
            }
            List<TextUIObject> Title = new List<TextUIObject>();
            string PortName = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[PortNum].Name;
            if (text == "Send ship") {
                Title.Add(new TextUIObject(PortName + ": Send Ship", new Vector2(0, 3)));
            } else {
                Title.Add(new TextUIObject(PortName + ": Trade with Ship", new Vector2(0, 3)));
            }
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(Title, ShipChoiceButtons, true);
        }
    }
    void CreateMarketScreen(int shipNum, int itemNum = -1, int buyOrSell = -1, bool saleSlider = false) {
        MarketSimulator MarketSim = GameObject.Find("Port").GetComponent<MarketSimulator>();
        PortInfo Port = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[PortNum];
        ShipInfo Ship = ShipMechs.Ships[shipNum].GetComponent<ShipInfo>();
        List<ButtonUIObject> ItemsButtons = new List<ButtonUIObject>();
        int move = 0;
        for (int i = 0; i < MarketSim.Items.Count; i++) {
            ItemsButtons.Add(new ButtonUIObject(MarketSim.Items[i].Name, "ItemPage", new Vector2(-5, move), new int[] { shipNum, i }, new Vector2(0.6f, 1)));
            move--;
        }
        List<TextUIObject> PageInfo = new List<TextUIObject>();
        if (itemNum == -1) {
            PageInfo.Add(new TextUIObject(Port.Name + " Market", new Vector2(0, 3)));
            PageInfo.Add(new TextUIObject("Population: " + Port.Population, new Vector2(2, 0.5f)));
            PageInfo.Add(new TextUIObject("Climate: " + MarketSim.Climates[Port.Climate], new Vector2(2, -0.5f)));
            transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, ItemsButtons, true);
        } else { // If the user has chosen an item to buy / sell
            int[] PriceAndQuantity = MarketSim.GetPriceAndQuantity(itemNum, PortNum);
            int BuyPrice = (int)Mathf.Round(PriceAndQuantity[0] * 1.2f);
            PageInfo.Add(new TextUIObject(Port.Name + " Market: " + MarketSim.Items[itemNum].Name, new Vector2(0, 3)));
            if (saleSlider == false) {
                PageInfo.Add(new TextUIObject("Amount in ship: " + Ship.Inventory[itemNum], new Vector2(2, 2.1f), new Vector2(0.8f, 0.8f)));
                PageInfo.Add(new TextUIObject("Price (Roman Coins): " + BuyPrice, new Vector2(2, 1.3f), new Vector2(0.8f, 0.8f)));
                PageInfo.Add(new TextUIObject("Port Quantity: " + Port.Inventory[itemNum], new Vector2(2, 0.5f), new Vector2(0.8f, 0.8f)));
                if (PriceAndQuantity[1] > 0 && MarketSim.PlayerCoins >= BuyPrice && Port.Inventory[itemNum] > 0 && Ship.GetUsedSlots() < ShipMechs.ShipTypes[Ship.Type].Slots) {
                    ItemsButtons.Add(new ButtonUIObject("Buy", "ManageItem", new Vector2(2, -2.5f), new int[] { shipNum, itemNum, 0 }, new Vector2(0.6f, 1)));
                }
                if (Ship.Inventory[itemNum] > 0) {
                    ItemsButtons.Add(new ButtonUIObject("Sell", "ManageItem", new Vector2(2, -3.5f), new int[] { shipNum, itemNum, 1 }, new Vector2(0.6f, 1)));
                }
                PageInfo.Add(new TextUIObject("Disclaimer: Price listed is buy price, sell price is marked down.", new Vector2(2, -2.75f), new Vector2(0.4f, 0.4f)));
                transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, ItemsButtons, true);
            } else {
                List<SliderUIObject> SliderInfo = new List<SliderUIObject>();
                PageInfo.Add(new TextUIObject("Price (Roman Coins): ", new Vector2(2, 1.5f)));
                PageInfo.Add(new TextUIObject("Quantity: ", new Vector2(2, 0.5f)));
                int SliderMax = Port.Inventory[itemNum];
                if (buyOrSell == 0) {
                    if (SliderMax > ShipMechs.ShipTypes[Ship.Type].Slots - Ship.GetUsedSlots()) {
                        SliderMax = ShipMechs.ShipTypes[Ship.Type].Slots - Ship.GetUsedSlots();
                    }
                    ItemsButtons.Add(new ButtonUIObject("Buy", "BuyItem", new Vector2(2, -4), new int[] { shipNum, itemNum, BuyPrice }, new Vector2(0.6f, 1)));
                } else {
                    SliderMax = Ship.Inventory[itemNum];
                    ItemsButtons.Add(new ButtonUIObject("Sell", "SellItem", new Vector2(2, -4), new int[] { shipNum, itemNum, PriceAndQuantity[0] }, new Vector2(0.6f, 1)));
                }
                SliderInfo.Add(new SliderUIObject("Sales", new Vector2(2, -0.5f), new Vector2(1, 1), SliderMax));
                transform.GetComponentInParent<UserInterfaceController>().CreateScreen(PageInfo, SliderInfo, ItemsButtons, true);
            }
        }
    }
}