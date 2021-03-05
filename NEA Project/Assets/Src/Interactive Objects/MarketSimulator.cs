using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketSimulator : MonoBehaviour {
    public int PlayerCoins;
    public float GameTime;
    public string[] Climates; // Named after wikipedia biome page classifications https://upload.wikimedia.org/wikipedia/commons/e/e4/Vegetation.png
    public List<ItemInfo> Items = new List<ItemInfo>();
    public List<AIOpponent> AI = new List<AIOpponent>();
    UIScreenController UIScreen;
    PortInfo[] AllPorts;
    bool MonthDone;
    // Start is called before the first frame update
    void Start() {
        PlayerCoins = 100;
        GameTime = 0;
        UIScreen = transform.parent.GetComponentInChildren<UIScreenController>();
        AllPorts = transform.GetComponent<PortMechanics>().Ports;
        MonthDone = false;
        Climates = new string[] {
            "Mediterranean",
            "Semi-Arid",
            "Temperate",
            "Montane",
            "Steppe"
        };
        Items.Add(new ItemInfo("Fish", new int[] { 5, 1, 3, 4, 2 }));
        Items.Add(new ItemInfo("Shells", new int[] { 5, 3, 1, 3, 1 }));
    }
    // Update is called once per frame
    void Update() {
        if (GameObject.Find("Ship").GetComponent<ShipMechanics>().Ships.Count > 0) {
            GameTime += transform.GetComponentInParent<InteractiveComponents>().TimeDilation;
        }
        GameObject.Find("Money").GetComponent<Text>().text = string.Format("Denarii: {0}", PlayerCoins);
        GameObject.Find("Time").GetComponent<Text>().text = string.Format("Time: {0}", (int)GameTime);
        if ((int)GameTime % 100 == 0) {
            if (!MonthDone) {
                MonthlyUpdate();
                MonthDone = true;
            }
        } else if (MonthDone) {
            MonthDone = false;
        }
        CorrectSliders();
    }
    void MonthlyUpdate() {
        for (int i = 0; i < AllPorts.Length; i++) {
            for (int j = 0; j < Items.Count; j++) {
                AllPorts[i].Inventory[j] = GetPriceAndQuantity(j, i)[1];
            }
        }
        int AIAmountChange = Random.Range(-2, 4);
        if (AIAmountChange < 0) {
            AIAmountChange = Mathf.Abs(AIAmountChange);
            if (AIAmountChange > AI.Count - 1) {
                AIAmountChange = AI.Count - 1;
            }
            List<GameObject> Ships = GameObject.Find("Ship").GetComponent<ShipMechanics>().Ships;
            for (int i = 0; i < AIAmountChange; i++) {
                GameObject ShipToRemove = Ships.Find(element => element.GetComponent<ShipInfo>().Owner == i);
                Destroy(ShipToRemove);
                Ships.Remove(ShipToRemove);
                AI.RemoveAt(i);
            }
            for (int i = 0; i < AI.Count; i++) {
                Ships.Find(element => element.GetComponent<ShipInfo>().Owner == i + AIAmountChange).GetComponent<ShipInfo>().Owner -= AIAmountChange;
            }
        } else if (AIAmountChange > 0) {
            if (AIAmountChange + AI.Count > 10) {
                AIAmountChange = 10 - AI.Count;
            }
            for (int i = 0; i < AIAmountChange; i++) {
                AI.Add(new AIOpponent(AI.Count));
            }
        }
        GameObject.Find("Compass").GetComponent<WeatherMechanics>().WorldWeather.UpdateMonth();
    }
    void CorrectSliders() {
        foreach (GameObject slider in UIScreen.Sliders) {
            Slider Slider = slider.GetComponent<Slider>();
            if (slider.name.Split()[1].Contains("Sales")) {
                foreach (GameObject text in UIScreen.Texts) {
                    if (text.name.Split()[1].Contains("Price")) {
                        foreach (GameObject button in UIScreen.Buttons) {
                            UIButton Button = button.GetComponent<UIButton>();
                            if (Button.Action == "BuyItem" || Button.Action == "SellItem") {
                                int Price = GetPriceAndQuantity(Button.References[1], UIScreen.PortID)[0];
                                if (Button.Action == "BuyItem") {
                                    Price = GetBuyPrice(Price);
                                    if (Slider.maxValue * Price > PlayerCoins) {
                                        Slider.maxValue = PlayerCoins / Price;
                                    }
                                }
                                text.GetComponent<Text>().text = "Price(Denarii): " + Slider.value * Price;
                                break;
                            }
                        }
                    }
                    if (text.name.Split()[1].Contains("Quantity")) {
                        text.GetComponent<Text>().text = "Quantity: " + Slider.value;
                        break;
                    }
                }
            }
            break;
        }
    }
    public int[] GetPriceAndQuantity(int itemID, int portID, bool drawDiagram = false) {
        float PED = -1; // (demand gradient m) horizontal = elastic, vertical = inelastic
        float PES = 1; // (supply gradient m) horizontal = elastic, vertical = inelastic
        float demandShift;
        float supplyShift;
        GetDemandAndSupplyShift(itemID, portID, out demandShift, out supplyShift);

        float demand = -PED * demandShift; // c = -m * dShift
        float supply = -PES * supplyShift; // c = -m * sShift

        int quantity = (int)Mathf.Round((supply - demand) / (PED - PES)); // x = (c2 - c1) / (m1 - m2)
        int price = (int)Mathf.Round(PED * quantity + demand) + 1; // y = mx + c
        if (drawDiagram) {
            DrawDiagram(demand, supply, PED, PES);
        }
        return new int[] { price, quantity };
    }
    public void GetDemandAndSupplyShift(int itemID, int portID, out float demandShift, out float supplyShift) {
        ItemInfo Item = Items[itemID];
        PortInfo Port = AllPorts[portID];
        demandShift = (float)System.Math.Log10(Port.Population) * 10;
        supplyShift = Item.ClimateAbundance[Port.Climate];
    }
    void DrawDiagram(float demand, float supply, float PED, float PES) {
        Debug.DrawLine(new Vector2(0, 0), new Vector2(0, 5), Color.black, 15);
        Debug.DrawLine(new Vector2(0, 0), new Vector2(5, 0), Color.black, 15);
        Debug.DrawRay(new Vector2(0, demand), new Vector2(5, 5 * PED), Color.red, 15);
        Debug.DrawRay(new Vector2(0, supply), new Vector2(5, 5 * PES), Color.green, 15);
        float quantity = (supply - demand) / (PED - PES);
        transform.GetComponentInParent<InteractiveComponents>().DrawPoint(new Vector2(quantity, PED * quantity + demand), 4);
    }
    public int GetBuyPrice(int sellPrice) {
        return 1 | (int)Mathf.Round(sellPrice * 1.2f);
    }
    public void ItemInteraction(string interaction, GameObject ship, int itemID, int portID, int numItems) {
        ShipInfo Ship = ship.GetComponent<ShipInfo>();
        int ItemValue = GetPriceAndQuantity(itemID, portID)[0];
        if (interaction == "BuyItem") {
            numItems = -numItems;
            ItemValue = GetBuyPrice(ItemValue);
        }
        Ship.Inventory[itemID] -= numItems;
        AllPorts[portID].Inventory[itemID] += numItems;
        if (Ship.Owner == -1) {
            PlayerCoins += ItemValue * numItems;
        }
    }
}
public class ItemInfo {
    string name;
    int[] climateAbundance; // Climate number, abundance of resource in climate
    public string Name { get => name; }
    public int[] ClimateAbundance { get => climateAbundance; }
    public ItemInfo(string name, int[] climateAbundance) {
        this.name = name;
        this.climateAbundance = climateAbundance;
    }
}
public class AIOpponent {
    ShipMechanics ShipMechs;
    MarketSimulator MarketSim;
    ShipInfo Ship;
    int[] InvBuyPrices;
    public AIOpponent(int id) {
        ShipMechs = GameObject.Find("Ship").GetComponent<ShipMechanics>();
        MarketSim = GameObject.Find("Port").GetComponent<MarketSimulator>();
        ShipMechs.Ships.Add(ShipMechs.NewShip(id, Random.Range(0, 9)));
        Ship = ShipMechs.Ships[ShipMechs.Ships.Count - 1].GetComponent<ShipInfo>();
        InvBuyPrices = new int[Ship.Inventory.Length];
        Ship.Port = Random.Range(0, 7);
        Ship.Dock();
        Ship.gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
    }
    public void PortDecisions() {
        MarketInteraction();
        SelectDestination();
    }
    void MarketInteraction() {
        for (int i = 0; i < InvBuyPrices.Length; i++) {
            if (Ship.Inventory[i] > 0 && InvBuyPrices[i] < MarketSim.GetPriceAndQuantity(i, Ship.Port)[0]) { // Sell item if ship has it and it's worth more than bought for
                MarketSim.ItemInteraction("SellItem", Ship.gameObject, i, Ship.Port, Ship.Inventory[i]);
                InvBuyPrices[i] = 0;
            } else if (ShipMechs.ShipTypes[Ship.Type].Slots - Ship.GetUsedSlots() > 0 && Random.Range(0, 2) == 1) { // Buy item if slots available and randomly chose to
                int AmountToBuy = (ShipMechs.ShipTypes[Ship.Type].Slots - Ship.GetUsedSlots()) / Random.Range(1, 4);
                int PortItemQuantity = GameObject.Find("Port").GetComponent<PortMechanics>().Ports[Ship.Port].Inventory[i];
                if (AmountToBuy > PortItemQuantity) {
                    AmountToBuy = PortItemQuantity;
                }
                InvBuyPrices[i] *= Ship.Inventory[i];
                MarketSim.ItemInteraction("BuyItem", Ship.gameObject, i, Ship.Port, AmountToBuy);
                InvBuyPrices[i] += MarketSim.GetBuyPrice(MarketSim.GetPriceAndQuantity(i, Ship.Port)[0]) * AmountToBuy;
                if (Ship.Inventory[i] != 0) {
                    InvBuyPrices[i] /= Ship.Inventory[i];
                }
            }
        }
    }
    void SelectDestination() { // AI Ship selects destination based on weather conditions, climate and population
        PortInfo[] Ports = GameObject.Find("Port").GetComponent<PortMechanics>().Ports;
        int SearchMode = -1; // Sell mode -> find ports with low supply and high demand
        if (Ship.GetUsedSlots() > 0) {
            SearchMode = 1; // Buy mode -> find ports with high supply and low demand
        }
        float HighestWeight = 0;
        int HighestWeightedPort = -1;
        for (int i = 0; i < Ports.Length; i++) {
            float CurrentWeight = 0;
            if (i != Ship.Port) {
                for (int j = 0; j < InvBuyPrices.Length; j++) { // For every item
                    float demandShift;
                    float supplyShift;
                    MarketSim.GetDemandAndSupplyShift(j, i, out demandShift, out supplyShift);

                    CurrentWeight += supplyShift; // Climate
                    CurrentWeight -= demandShift; // Population

                }
                CurrentWeight *= SearchMode;
                if (HighestWeightedPort == -1 || CurrentWeight > HighestWeight) {
                    HighestWeight = CurrentWeight;
                    HighestWeightedPort = i;
                }
            }
        }
        Ship.Port = HighestWeightedPort;
    }
}