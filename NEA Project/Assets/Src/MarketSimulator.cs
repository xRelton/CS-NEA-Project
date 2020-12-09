using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarketSimulator : MonoBehaviour {
    public string[] Climates; // Named after wikipedia biome page classifications https://upload.wikimedia.org/wikipedia/commons/e/e4/Vegetation.png
    public List<ItemInfo> Items;
    // Start is called before the first frame update
    void Start() {
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

    }
    public bool ItemInPort(int itemNum, string portName) {
        float[] PriceAndQuantity = GetPriceAndQuantity(itemNum, portName);
        if (PriceAndQuantity[0] > 0 && PriceAndQuantity[1] > 0) {
            return true;
        }
        return false;
    }
    public float[] GetPriceAndQuantity(int itemNum, string portName) {
        ItemInfo Item = Items[itemNum];
        PortInfo Port = transform.GetComponent<PortMechanics>().Ports[portName];
        int ClimateAbundance = Item.ClimateAbundance[Climates[Port.Climate]];
        float PED = -1; // (demand gradient m) horizontal = elastic, vertical = inelastic
        float PES = 1; // (supply gradient m) horizontal = elastic, vertical = inelastic
        float demandShift = -PED * ((float)System.Math.Log10(Port.Population)); // c = -m * dShift
        float supplyShift = -PES * (ClimateAbundance); // c = -m * sShift

        float quantity = (supplyShift - demandShift) / (PED - PES); // x = (c2 - c1) / (m1 - m2)
        float price = PED * quantity + demandShift; // y = mx + c
        Debug.DrawLine(new Vector2(0, 0), new Vector2(0, 5), Color.black, 15);
        Debug.DrawLine(new Vector2(0, 0), new Vector2(5, 0), Color.black, 15);
        Debug.DrawRay(new Vector2(0, demandShift), new Vector2(5, 5*PED), Color.red, 15);
        Debug.DrawRay(new Vector2(0, supplyShift), new Vector2(5, 5*PES), Color.green, 15);
        transform.GetComponentInParent<InteractiveComponents>().DrawPoint(new Vector2(quantity, price), 4);
        return new float[] { price, quantity };
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