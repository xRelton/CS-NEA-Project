using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class ShipMechanics : MonoBehaviour {
    string[] ShipNames = new string[] { "barque", "brig", "carrack", "frigate", "full-rigged ship", "schooner", "ship of the line", "sloop of war" };
    Ships ShipTypes = new Ships();
    List<GameObject> PlayerShips = new List<GameObject>();
    // Start is called before the first frame update
    void Start() {
        ShipTypes.SetStat(ShipNames[0], 0.2f, 1, 2 ); // Sets Barque size, speed and strength
        ShipTypes.SetStat(ShipNames[1], 0.2f, 1, 2 ); // Sets Brig size, speed and strength
        ShipTypes.SetStat(ShipNames[2], 0.2f, 1, 2 ); // Sets Carrack size, speed and strength
        ShipTypes.SetStat(ShipNames[3], 0.2f, 1, 2 ); // Sets Frigate size, speed and strength
        ShipTypes.SetStat(ShipNames[4], 0.2f, 1, 2 ); // Sets Full-rigged Ship size, speed and strength
        ShipTypes.SetStat(ShipNames[5], 0.2f, 1, 2 ); // Sets Schooner size, speed and strength
        ShipTypes.SetStat(ShipNames[6], 0.2f, 1, 2 ); // Sets Ship of the Line size, speed and strength
        ShipTypes.SetStat(ShipNames[7], 0.2f, 1, 2 ); // Sets Sloop of War size, speed and strength
        PlayerShips.Add(ShipTypes.NewShip(ShipNames[5], transform));
    }

    // Update is called once per frame
    void Update() {}
}

public class Ships {
    Dictionary<string, float[]> ShipStats = new Dictionary<string, float[]>();
    public Ships() {}
    public void SetStat(string name, float size, float speed, float strength) {
        ShipStats.Add(name, new float[] { size, speed, strength });
    }
    public float GetSpeed(string name) {
        return (ShipStats[name][1]);
    }
    public float GetStrength(string name) {
        return (ShipStats[name][2]);
    }
    public GameObject NewShip(string name, Transform parent) {
        GameObject NewShip = new GameObject(name);
        NewShip.transform.parent = parent;
        NewShip.AddComponent<SpriteRenderer>();
        NewShip.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(string.Format("Sprites/Ships/{0}", name));
        NewShip.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        NewShip.GetComponent<SpriteRenderer>().sortingOrder = 1;
        NewShip.AddComponent<BoxCollider2D>();
        NewShip.transform.localScale = new Vector2(ShipStats[name][0], ShipStats[name][0]);
        return NewShip;
    }
}
