using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MarketSimulator : MonoBehaviour {
    public int PlayerCoins;
    public float GameTime;
    public string[] Climates; // Named after wikipedia biome page classifications https://upload.wikimedia.org/wikipedia/commons/e/e4/Vegetation.png
    public List<ItemInfo> Items;
    UIScreenController UIScreen;
    // Start is called before the first frame update
    void Start() {
        PlayerCoins = 10;
        GameTime = 0;
        UIScreen = transform.parent.GetComponentInChildren<UIScreenController>();
        Items = new List<ItemInfo>();
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
        GameTime += transform.GetComponentInParent<InteractiveComponents>().TimeDilation;
        GameObject.Find("Money").GetComponent<Text>().text = string.Format("Roman Coins: {0}", PlayerCoins);
        GameObject.Find("Time").GetComponent<Text>().text = string.Format("Time: {0}", (int)GameTime);
        if ((int)GameTime % 100 == 0) {
            List<PortInfo> AllPorts = transform.GetComponent<PortMechanics>().Ports;
            for (int i = 0; i < AllPorts.Count; i++) {
                for (int j = 0; j < Items.Count; j++) {
                    AllPorts[i].Inventory[j] = GetPriceAndQuantity(j, i)[1];
                }
            }
        }
        foreach (GameObject slider in UIScreen.Sliders) {
            Slider Slider = slider.GetComponent<Slider>();
            if (slider.name.Split()[1].Contains("Sales")) {
                foreach (GameObject text in UIScreen.Texts) {
                    if (text.name.Split()[1].Contains("Price")) {
                        foreach (GameObject button in UIScreen.Buttons) {
                            UIButton Button = button.GetComponent<UIButton>();
                            if (Button.Action == "BuyItem" || Button.Action == "SellItem") {
                                if (Button.Action == "BuyItem") {
                                    if (Slider.maxValue * Button.References[2] > PlayerCoins) {
                                        Slider.maxValue = PlayerCoins / Button.References[2];
                                    }
                                }
                                text.GetComponent<Text>().text = "Price(Roman Coins): " + Slider.value * Button.References[2];
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
    public int[] GetPriceAndQuantity(int itemNum, int portNum) {
        ItemInfo Item = Items[itemNum];
        PortInfo Port = transform.GetComponent<PortMechanics>().Ports[portNum];
        int ClimateAbundance = Item.ClimateAbundance[Port.Climate];
        float PED = -1; // (demand gradient m) horizontal = elastic, vertical = inelastic
        float PES = 1; // (supply gradient m) horizontal = elastic, vertical = inelastic
        float demandShift = -PED * ((float)System.Math.Log10(Port.Population)); // c = -m * dShift
        float supplyShift = -PES * (ClimateAbundance); // c = -m * sShift

        int quantity = (int)Mathf.Round((supplyShift - demandShift) / (PED - PES)); // x = (c2 - c1) / (m1 - m2)
        int price = (int)Mathf.Round(PED * quantity + demandShift) + 1; // y = mx + c
        /*Debug.DrawLine(new Vector2(0, 0), new Vector2(0, 5), Color.black, 15);
        Debug.DrawLine(new Vector2(0, 0), new Vector2(5, 0), Color.black, 15);
        Debug.DrawRay(new Vector2(0, demandShift), new Vector2(5, 5*PED), Color.red, 15);
        Debug.DrawRay(new Vector2(0, supplyShift), new Vector2(5, 5*PES), Color.green, 15);*/
        transform.GetComponentInParent<InteractiveComponents>().DrawPoint(new Vector2(quantity, price), 4);
        return new int[] { price, quantity };
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