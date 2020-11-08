using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class ShipMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    public string[] ShipNames = new string[] { "barque", "brig", "carrack", "frigate", "full-rigged ship", "schooner", "ship of the line", "sloop of war" };
    public ShipType ShipTypes = new ShipType();
    public List<GameObject> PlayerShips = new List<GameObject>();
    static int NumDirs = 5;
    List<Vector3> TestDirections = new List<Vector3>();
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        ShipTypes.SetStat(ShipNames[0], 0.2f, 1, 2 ); // Sets Barque size, speed and strength
        ShipTypes.SetStat(ShipNames[1], 0.2f, 1, 2 ); // Sets Brig size, speed and strength
        ShipTypes.SetStat(ShipNames[2], 0.2f, 1, 2 ); // Sets Carrack size, speed and strength
        ShipTypes.SetStat(ShipNames[3], 0.2f, 1, 2 ); // Sets Frigate size, speed and strength
        ShipTypes.SetStat(ShipNames[4], 0.2f, 1, 2 ); // Sets Full-rigged Ship size, speed and strength
        ShipTypes.SetStat(ShipNames[5], 0.2f, 1, 2 ); // Sets Schooner size, speed and strength
        ShipTypes.SetStat(ShipNames[6], 0.2f, 1, 2 ); // Sets Ship of the Line size, speed and strength
        ShipTypes.SetStat(ShipNames[7], 0.2f, 1, 2); // Sets Sloop of War size, speed and strength
        for (int i = -NumDirs; i <= NumDirs; i++) {
            for (int j = -NumDirs; j <= NumDirs; j++) {
                if (i != 0 && j != 0) {
                    TestDirections.Add(new Vector3(i, j));
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {
        foreach (GameObject PlayerShip in PlayerShips) {
            ShipInfo PlayerShipInfo = PlayerShip.GetComponent<ShipInfo>();
            if (!PlayerShipInfo.Docked()) {
                if (PlayerShipInfo.OnSea(PlayerShipInfo.PerfectMove() * Time.deltaTime)) {
                    PlayerShip.transform.position += PlayerShipInfo.PerfectMove() * Interactions.TimeDilation;
                } else {
                    Vector3 BestDirection = new Vector3();
                    float BestWeight = -1;
                    foreach (Vector3 Direction in TestDirections) {
                        if (PlayerShipInfo.OnSea(Direction * Interactions.TimeDilation) && PlayerShipInfo.GetWeight(Direction) > BestWeight && !PlayerShipInfo.illegalDirections.Contains(Direction)) {
                            BestDirection = Direction;
                            BestWeight = PlayerShipInfo.GetWeight(Direction);
                        }
                    }
                    // Make it so directions are made illegal when they go back and forth without moving (use periodic loop to check if the current position is in the same area as the previous one then track
                    // directions made during that period and ban them until out of the area range plus a little bit)
                    //PlayerShipInfo.illegalDirections.Remove(PlayerShipInfo.illegalDirections[0]);
                    //PlayerShipInfo.illegalDirections.Add(BestDirection);
                    PlayerShip.transform.position += BestDirection * Interactions.TimeDilation;
                }
                if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.GetPort().transform.position, 0.1f)) {
                    PlayerShipInfo.Dock();
                }
            }
        }
    }
}
public class ShipInfo : MonoBehaviour {
    string previousPort; // Set to the last port the ship docked at
    string targetPort; // Set to the port being headed to if the ship is travelling
    public List<Vector3> illegalDirections = new List<Vector3>();
    public ShipInfo() {}
    public string TargetPort {
        set { targetPort = value; }
    }
    public GameObject GetPort() {
        return GameObject.Find(targetPort);
    }
    public void Dock() {
        previousPort = targetPort;
        transform.position = GetPort().transform.position;
    }
    public bool Docked() {
        return previousPort == targetPort;
    }
    public bool OnSea(Vector3 posChange) {
        return transform.GetComponentInParent<InteractiveComponents>().PosOnObject(transform.position + posChange, GameObject.Find("Sea Collider"));
    }
    public Vector3 PerfectMove() {
        float xChange = (GetPort().transform.position.x - GameObject.Find(previousPort).transform.position.x) / 3; // (+/-) depends on direction of ship, adjusted to make vertical and horizontal journeys similar speeds
        float yChange = xChange * (GetPort().transform.position.y - transform.position.y) / (GetPort().transform.position.x - transform.position.x); // Gradient value
        return new Vector3(xChange, yChange);
    }
    public float GetWeight(Vector3 posChange) {
        return posChange.y / PerfectMove().y;
    }
}
public class ShipType {
    Dictionary<string, float[]> ShipStats = new Dictionary<string, float[]>();
    public ShipType() {}
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
        NewShip.AddComponent<ShipInfo>();
        NewShip.transform.localScale = new Vector2(ShipStats[name][0], ShipStats[name][0]);
        return NewShip;
    }
}