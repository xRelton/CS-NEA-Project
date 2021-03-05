using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenController : MonoBehaviour {
    public int PortID;
    public List<GameObject> Texts;
    public List<GameObject> Sliders;
    public List<GameObject> Buttons;
    ShipMechanics ShipMechs;
    UserInterfaceController UIController;
    PortInfo CurrentPort;
    MarketSimulator MarketSim;
    GameObject[] PlayerShips;
    void Start() {
        ShipMechs = GameObject.Find("Ship").GetComponent<ShipMechanics>();
        UIController = transform.GetComponentInParent<UserInterfaceController>();
        MarketSim = GameObject.Find("Port").GetComponent<MarketSimulator>();
    }
    // Update is called once per frame
    void Update() {
        CurrentPort = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[PortID];
        PlayerShips = Array.FindAll(ShipMechs.Ships.ToArray(), element => element.GetComponent<ShipInfo>().Owner == -1);
        foreach (GameObject button in Buttons) {
            UIButton Button = button.GetComponent<UIButton>();
            if (Button.ButtonPressed()) {
                GameObject.Find("UI Screen").SetActive(false);
                switch (Button.Action) {
                    case ("SetInitialPort"):
                        int SelectedPortID = Array.FindIndex(GameObject.Find("Port").GetComponent<PortMechanics>().Ports, element => element.Name == Button.name.Replace("Button ", ""));
                        ShipMechs.Ships.Add(ShipMechs.NewShip(-1, 5));
                        ShipMechs.Ships[ShipMechs.Ships.Count - 1].GetComponent<ShipInfo>().Port = SelectedPortID;
                        ShipMechs.Ships[ShipMechs.Ships.Count - 1].GetComponent<ShipInfo>().Dock();
                        break;
                    case ("ShipRequest"):
                        RequestShip(Button.References, Button.transform.GetComponentInChildren<Text>().text);
                        break;
                    case ("ShipSelect"):
                        SelectedShipOptions(Button.References[1], Button.References[0]);
                        break;
                    case ("ShipMarket"):
                        List<TextUIObject> Title = new List<TextUIObject>();
                        Title.Add(new TextUIObject(CurrentPort.Name + ": Ship Market", new Vector2(0, 3)));
                        List<ButtonUIObject> ShipChoiceButtons = new List<ButtonUIObject>();
                        string BuyText, SellText;
                        if (CurrentPort.ShipsSold.Length == 1) {
                            BuyText = string.Format("{0} ({1})", ShipMechs.ShipTypes[CurrentPort.ShipsSold[0]].Name, CurrentPort.GetMinShipValue()); 
                        } else {
                            BuyText = "a ship";
                        }
                        if (Button.References.Length == 1) {
                            SellText = string.Format("docked ship ({0})", ShipMechs.ShipTypes[PlayerShips[Button.References[0]].GetComponent<ShipInfo>().Type].GetValue(true));
                        } else {
                            SellText = "a docked ship";
                        }
                        if (MarketSim.PlayerCoins >= CurrentPort.GetMinShipValue() && PlayerShips.Length < 6) {
                            ShipChoiceButtons.Add(new ButtonUIObject("Buy " + BuyText, "ShipRequest", new Vector2(), CurrentPort.ShipsSold, new Vector2(1.2f, 1)));
                        }
                        if (PlayerShips.Length > 1) {
                            ShipChoiceButtons.Add(new ButtonUIObject("Sell " + SellText, "ShipRequest", new Vector2(0, -1), Button.References, new Vector2(1.2f, 1)));
                        }
                        UIController.CreateScreen(Title, ShipChoiceButtons, true);
                        break;
                    case ("ItemPage"):
                        CreateMarketScreen(Button.References[0], Button.References[1]);
                        break;
                    case ("ManageItem"):
                        CreateMarketScreen(Button.References[0], Button.References[1], Button.References[2], true);
                        break;
                    case ("BuyItem"): case ("SellItem"):
                        foreach (GameObject slider in Sliders) {
                            if (slider.name.Split()[1].Contains("Sales")) {
                                MarketSim.ItemInteraction(Button.Action, PlayerShips[Button.References[0]], Button.References[1], PortID, (int)slider.GetComponent<Slider>().value);
                                break;
                            }
                        }
                        break;
                }
            }
        }
    }
    void RequestShip(int[] references, string text) {
        int Option = Array.FindIndex(new string[] { "Send", "Open", "Buy", "Sell" }, element => element == text.Split(' ')[0]);
        if (references.Length == 1) {
            SelectedShipOptions(Option, references[0]);
        } else {
            List<TextUIObject> Title = new List<TextUIObject>();
            List<ButtonUIObject> ShipChoiceButtons = new List<ButtonUIObject>();
            Title.Add(new TextUIObject(CurrentPort.Name + ": " + new string[] { "Send Ship", "Trade with Ship", "Buy Ship", "Sell Ship" }[Option], new Vector2(0, 3)));
            for (int i = 0; i < references.Length; i++) {
                if (Option == 2) {
                    ShipType CurrentShipType = ShipMechs.ShipTypes[references[i]];
                    if (MarketSim.PlayerCoins >= CurrentShipType.GetValue(false)) {
                        ShipChoiceButtons.Add(new ButtonUIObject(string.Format("{0} ({1})", CurrentShipType.Name, CurrentShipType.GetValue(false)), "ShipSelect", new Vector2(0, -i), new int[] { references[i], Option }, new Vector2(1.2f, 1)));
                    }
                } else if (Option == 3) {
                    ShipType CurrentShipType = ShipMechs.ShipTypes[PlayerShips[references[i]].GetComponent<ShipInfo>().Type];
                    ShipChoiceButtons.Add(new ButtonUIObject(string.Format("{0} ({1})", PlayerShips[references[i]].name, CurrentShipType.GetValue(true)), "ShipSelect", new Vector2(0, -i), new int[] { references[i], Option }, new Vector2(1.2f, 1)));
                } else {
                    ShipChoiceButtons.Add(new ButtonUIObject(PlayerShips[references[i]].name, "ShipSelect", new Vector2(0, -i), new int[] { references[i], Option }));
                }
            }
            UIController.CreateScreen(Title, ShipChoiceButtons, true);
        }
    }
    void SelectedShipOptions(int option, int shipID) {
        switch (option) {
            case 0: // Send ship
                PlayerShips[shipID].GetComponent<ShipInfo>().Port = PortID;
                break;
            case 1: // Market
                CreateMarketScreen(shipID);
                break;
            case 2: // Buy ship
                MarketSim.PlayerCoins -= ShipMechs.ShipTypes[shipID].GetValue(false);
                ShipMechs.Ships.Add(ShipMechs.NewShip(-1, shipID));
                ShipMechs.Ships[ShipMechs.Ships.Count - 1].GetComponent<ShipInfo>().Port = PortID;
                ShipMechs.Ships[ShipMechs.Ships.Count - 1].GetComponent<ShipInfo>().Dock();
                break;
            case 3: // Sell ship
                MarketSim.PlayerCoins += ShipMechs.ShipTypes[PlayerShips[shipID].GetComponent<ShipInfo>().Type].GetValue(true);
                Destroy(PlayerShips[shipID]);
                ShipMechs.Ships.RemoveAt(Array.FindIndex(ShipMechs.Ships.ToArray(), element => element == PlayerShips[shipID]));
                break;
        }
    }
    void CreateMarketScreen(int shipID, int itemID = -1, int buyOrSell = -1, bool saleSlider = false) {
        ShipInfo Ship = PlayerShips[shipID].GetComponent<ShipInfo>();
        List<ButtonUIObject> ItemsButtons = new List<ButtonUIObject>();
        int move = 0;
        for (int i = 0; i < MarketSim.Items.Count; i++) {
            ItemsButtons.Add(new ButtonUIObject(MarketSim.Items[i].Name, "ItemPage", new Vector2(-5, move), new int[] { shipID, i }, new Vector2(0.6f, 1)));
            move--;
        }
        List<TextUIObject> PageInfo = new List<TextUIObject>();
        PageInfo.Add(new TextUIObject(string.Format("Ship slots: {0}/{1}", Ship.GetUsedSlots(), ShipMechs.ShipTypes[Ship.Type].Slots), new Vector2(6, -2.75f), new Vector2(0.65f, 0.65f)));
        if (itemID == -1) {
            PageInfo.Add(new TextUIObject(CurrentPort.Name + " Market", new Vector2(0, 3)));
            PageInfo.Add(new TextUIObject("Population: " + CurrentPort.Population, new Vector2(2, 0.5f)));
            PageInfo.Add(new TextUIObject("Climate: " + MarketSim.Climates[CurrentPort.Climate], new Vector2(2, -0.5f)));
            UIController.CreateScreen(PageInfo, ItemsButtons, true);
        } else { // If the user has chosen an item to buy / sell
            int BuyPrice = MarketSim.GetBuyPrice(CurrentPort.Inventory[itemID][1]);
            PageInfo.Add(new TextUIObject(CurrentPort.Name + " Market: " + MarketSim.Items[itemID].Name, new Vector2(0, 3)));
            if (saleSlider == false) {
                PageInfo.Add(new TextUIObject("Amount in ship: " + Ship.Inventory[itemID], new Vector2(2, 2.1f), new Vector2(0.9f, 0.9f)));
                PageInfo.Add(new TextUIObject("Price (Denarii): " + BuyPrice, new Vector2(2, 1.3f), new Vector2(0.9f, 0.9f)));
                PageInfo.Add(new TextUIObject("Port Quantity: " + CurrentPort.Inventory[itemID][0], new Vector2(2, 0.5f), new Vector2(0.9f, 0.9f)));
                if (MarketSim.PlayerCoins >= BuyPrice && CurrentPort.Inventory[itemID][0] > 0 && Ship.GetUsedSlots() < ShipMechs.ShipTypes[Ship.Type].Slots) {
                    ItemsButtons.Add(new ButtonUIObject("Buy", "ManageItem", new Vector2(2, -2.5f), new int[] { shipID, itemID, 0 }, new Vector2(0.6f, 1)));
                }
                if (Ship.Inventory[itemID] > 0) {
                    ItemsButtons.Add(new ButtonUIObject("Sell", "ManageItem", new Vector2(2, -3.5f), new int[] { shipID, itemID, 1 }, new Vector2(0.6f, 1)));
                }
                PageInfo.Add(new TextUIObject("Disclaimer: Price listed is buy price, sell price is marked down.", new Vector2(2, -2.75f), new Vector2(0.65f, 0.65f)));
                UIController.CreateScreen(PageInfo, ItemsButtons, true);
            } else {
                List<SliderUIObject> SliderInfo = new List<SliderUIObject>();
                PageInfo.Add(new TextUIObject("Price (Denarii): ", new Vector2(2, 1.5f)));
                PageInfo.Add(new TextUIObject("Quantity: ", new Vector2(2, 0.5f)));
                int SliderMax = CurrentPort.Inventory[itemID][0];
                if (buyOrSell == 0) {
                    if (SliderMax > ShipMechs.ShipTypes[Ship.Type].Slots - Ship.GetUsedSlots()) {
                        SliderMax = ShipMechs.ShipTypes[Ship.Type].Slots - Ship.GetUsedSlots();
                    }
                    ItemsButtons.Add(new ButtonUIObject("Buy", "BuyItem", new Vector2(2, -4), new int[] { shipID, itemID }, new Vector2(0.6f, 1)));
                } else {
                    SliderMax = Ship.Inventory[itemID];
                    ItemsButtons.Add(new ButtonUIObject("Sell", "SellItem", new Vector2(2, -4), new int[] { shipID, itemID }, new Vector2(0.6f, 1)));
                }
                SliderInfo.Add(new SliderUIObject("Sales", new Vector2(2, -0.5f), new Vector2(1, 1), SliderMax));
                UIController.CreateScreen(PageInfo, SliderInfo, ItemsButtons, true);
            }
        }
    }
}