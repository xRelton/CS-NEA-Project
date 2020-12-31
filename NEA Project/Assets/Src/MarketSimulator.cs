using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MarketSimulator : MonoBehaviour {
    public string[] Climates; // Named after wikipedia biome page classifications https://upload.wikimedia.org/wikipedia/commons/e/e4/Vegetation.png
    public List<ItemInfo> Items;
    UIScreenController UIScreen;
    // Start is called before the first frame update
    void Start() {
        UIScreen = transform.parent.GetComponentInChildren<UIScreenController>();
        Items = new List<ItemInfo>();
        Climates = new string[] {
            "Mediterranean",
            "Semi-Arid",
            "Temperate",
            "Montane",
            "Steppe"
        };
        Items.Add(new ItemInfo("Fish", Climates.ToDictionary(x => x, x => Random.Range(0, 5))));
    }

    // Update is called once per frame
    void Update() {
        foreach (GameObject slider in UIScreen.Sliders) {
            if (slider.name.Split()[1].Contains("Sales")) {
                foreach (GameObject text in UIScreen.Texts) {
                    if (text.name.Split()[1].Contains("Price")) {
                        foreach (GameObject button in UIScreen.Buttons) {
                            if (button.name.Contains("Buy") || button.name.Contains("Sell")) {
                                text.GetComponent<Text>().text = "Price(Roman Coins): " + slider.GetComponent<Slider>().value * button.GetComponent<UIButton>().References[0];
                            }
                        }
                    }
                    if (text.name.Split()[1].Contains("Quantity")) {
                        text.GetComponent<Text>().text = "Quantity: " + slider.GetComponent<Slider>().value;
                    }
                }
            }
        }
    }
    public int[] GetPriceAndQuantity(int itemNum, int portNum) {
        ItemInfo Item = Items[itemNum];
        PortInfo Port = transform.GetComponent<PortMechanics>().Ports[portNum];
        int ClimateAbundance = Item.ClimateAbundance[Climates[Port.Climate]];
        float PED = -1; // (demand gradient m) horizontal = elastic, vertical = inelastic
        float PES = 1; // (supply gradient m) horizontal = elastic, vertical = inelastic
        float demandShift = -PED * ((float)System.Math.Log10(Port.Population)); // c = -m * dShift
        float supplyShift = -PES * (ClimateAbundance); // c = -m * sShift

        int quantity = (int)Mathf.Round((supplyShift - demandShift) / (PED - PES)); // x = (c2 - c1) / (m1 - m2)
        int price = (int)Mathf.Round(PED * quantity + demandShift) + 1; // y = mx + c
        Debug.DrawLine(new Vector2(0, 0), new Vector2(0, 5), Color.black, 15);
        Debug.DrawLine(new Vector2(0, 0), new Vector2(5, 0), Color.black, 15);
        Debug.DrawRay(new Vector2(0, demandShift), new Vector2(5, 5*PED), Color.red, 15);
        Debug.DrawRay(new Vector2(0, supplyShift), new Vector2(5, 5*PES), Color.green, 15);
        transform.GetComponentInParent<InteractiveComponents>().DrawPoint(new Vector2(quantity, price), 4);
        return new int[] { price, quantity };
    }
}
public class ItemInfo {
    string name;
    Dictionary<string, int> climateAbundance = new Dictionary<string, int>(); // Climate name, abundance of resource in climate
    public string Name { get => name; }
    public Dictionary<string, int> ClimateAbundance { get => climateAbundance; }

    public ItemInfo(string name, Dictionary<string, int> climateAbundance) {
        this.name = name;
        this.climateAbundance = climateAbundance;
    }
}