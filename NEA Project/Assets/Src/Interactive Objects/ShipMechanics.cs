using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using System.Collections;
using System.Net;

public class ShipMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    public string[] ShipNames = new string[] { "barque", "brig", "carrack", "frigate", "full-rigged ship", "schooner", "ship of the line", "sloop of war" };
    public ShipType ShipTypes = new ShipType();
    public List<GameObject> PlayerShips = new List<GameObject>();
    List<Vector2> Nodes = new List<Vector2>();
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        ShipTypes.Interactions = Interactions;
        ShipTypes.SetStat(ShipNames[0], 0.2f, 1, 2); // Sets Barque size, speed and strength
        ShipTypes.SetStat(ShipNames[1], 0.2f, 1, 2); // Sets Brig size, speed and strength
        ShipTypes.SetStat(ShipNames[2], 0.2f, 1, 2); // Sets Carrack size, speed and strength
        ShipTypes.SetStat(ShipNames[3], 0.2f, 1, 2); // Sets Frigate size, speed and strength
        ShipTypes.SetStat(ShipNames[4], 0.2f, 1, 2); // Sets Full-rigged Ship size, speed and strength
        ShipTypes.SetStat(ShipNames[5], 0.2f, 1, 2); // Sets Schooner size, speed and strength
        ShipTypes.SetStat(ShipNames[6], 0.2f, 1, 2); // Sets Ship of the Line size, speed and strength
        ShipTypes.SetStat(ShipNames[7], 0.2f, 1, 2); // Sets Sloop of War size, speed and strength
        for (float x = -8; x <= 10; x+=0.5f) {
            for (float y = -2; y <= 3; y+=0.5f) {
                Vector2 NewNode = new Vector2(x, y);
                if (!Interactions.OnLand(NewNode)) {
                    Nodes.Add(NewNode);
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {
        foreach (GameObject PlayerShip in PlayerShips) {
            ShipInfo PlayerShipInfo = PlayerShip.GetComponent<ShipInfo>();
            if (!PlayerShipInfo.Docked()) {
                if (PlayerShipInfo.Route.Any()) {
                    // If the new position is not on land
                    PlayerShip.transform.position += ShipTypes.GetSpeed(PlayerShip.name)/10 * Interactions.TimeDilation * Interactions.PerfectMove(PlayerShip.transform.position, PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1]);
                    // Else navigate around land until new position not land
                    if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1], 0.01f)) {
                        PlayerShipInfo.Route.Remove(PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1]);
                    }
                } else {
                    SetShipRoute(PlayerShip.transform.position, PlayerShipInfo);
                }
                if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.GetPortPos(true), 0.01f)) {
                    PlayerShipInfo.Dock();
                    PlayerShipInfo.Route.Clear();
                }
            }
        }
    }
    void DrawNode(Vector2 node, int color) {
        Color[] Colors = new Color[] { Color.white, Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow, Color.gray, Color.black };
        Color setColor;
        if (color > Colors.Length - 1) {
            setColor = Colors[Colors.Length - 1];
        } else {
            setColor = Colors[color];
        }
        Debug.DrawLine(node - new Vector2(0.1f, 0), node + new Vector2(0.1f, 0), setColor, 15);
        Debug.DrawLine(node - new Vector2(0, 0.1f), node + new Vector2(0, 0.1f), setColor, 15);
    }
    void SetShipRoute(Vector3 shipPosition, ShipInfo shipInfo) {
        shipInfo.Route.Add(shipInfo.GetPortPos(true));
        shipInfo.Route.Add(shipInfo.GetPortSeaPos(true));
        if (!TargetHitBeforeLand(shipInfo.GetPortSeaPos(false), shipInfo.GetPortSeaPos(true))) {
            shipInfo.Route.AddRange(BurrowForRoute(shipInfo));
        }
        shipInfo.Route.Add(shipInfo.GetPortSeaPos(false));
        /*for (int i = 0; i < shipInfo.Route.Count - 1; i++) {
            Debug.DrawLine(shipInfo.Route[i], shipInfo.Route[i + 1], Color.green, 15);
        }*/
    }
    List<Vector2> BurrowForRoute(ShipInfo shipInfo) {
        Dictionary<Vector2, int> NodeGroups = GetNodeGroups(shipInfo.Route[1]);
        List<Vector2> NodesHit = new List<Vector2>() { }; // Nodes from the lowest numbered group that have line of sight with starting port
        int CurrentGroup = -1;
        while (!NodesHit.Any()) { // Loops while no nodes from a node group are hit
            CurrentGroup++;
            foreach (Vector2 node in Nodes) {
                if (NodeGroups[node] == CurrentGroup && TargetHitBeforeLand(node, shipInfo.GetPortSeaPos(false))) {
                    NodesHit.Add(node); // Adds all nodes in current group that hit
                }
            }
        }
        List<Vector2> NodesInRoute = new List<Vector2>();
        for (int i = CurrentGroup; i > 0; i--) {
            NodesInRoute.Add(Interactions.ClosestVector(NodesHit, shipInfo.GetPortSeaPos(true)));
            NodesHit.Clear();
            foreach (Vector2 node in Nodes) {
                if (NodeGroups[node] == i && TargetHitBeforeLand(NodesInRoute[NodesInRoute.Count - 1], node)) {
                    //Debug.DrawLine(NodesInRoute[NodesInRoute.Count - 1], node, Color.yellow, 15);
                    NodesHit.Add(node); // Adds all nodes in current group that hit
                }
            }
        }
        NodesInRoute.Reverse();
        return NodesInRoute;
    }
    Dictionary<Vector2, int> GetNodeGroups(Vector2 centrePos) {
        Dictionary<Vector2, int> NodeGroups = new Dictionary<Vector2, int>() { };
        List<Vector2> UnassignedNodes = Nodes.ToList();
        List<Vector2> PreviousNodes = new List<Vector2>() { centrePos };
        int GroupNum = 1;
        while (UnassignedNodes.Any()) { // Loop runs while there are still unassigned nodes left
            List<Vector2> NewNodes = new List<Vector2>();
            foreach (Vector2 node in UnassignedNodes.ToList()) {
                foreach (Vector2 previousNode in PreviousNodes) {
                    if (TargetHitBeforeLand(previousNode, node)) {
                        //DrawNode(node, GroupNum - 1);
                        NodeGroups.Add(node, GroupNum);
                        NewNodes.Add(node);
                        UnassignedNodes.Remove(node);
                        break;
                    }
                }
            }
            PreviousNodes = NewNodes;
            GroupNum++;
        }
        return NodeGroups;
    }
    bool TargetHitBeforeLand(Vector2 start, Vector2 target) { // Checks if the straight line between the start and target point is unobstructed
        bool ReturnVal = true;
        RaycastHit2D[] hitsUp = Physics2D.RaycastAll(transform.position, Interactions.PerfectMove(start, target), Vector2.Distance(start, target));
        RaycastHit2D[] hitsDown = Physics2D.RaycastAll(transform.position, Interactions.PerfectMove(target, start), Vector2.Distance(start, target));
        foreach (RaycastHit2D hit in hitsUp) {
            if (Interactions.LandColliders.Any(collider => collider == hit.collider.name)) {
                ReturnVal = false;
            }
        }
        foreach (RaycastHit2D hit in hitsDown) {
            if (Interactions.LandColliders.Any(collider => collider == hit.collider.name) && ReturnVal == false) {
                return false;
            }
        }
        return true;
    }
}
public class ShipInfo : MonoBehaviour {
    InteractiveComponents interactions;
    string previousPort; // Name of last port the ship docked at
    string targetPort; // Name of port being headed to if the ship is travelling
    List<Vector2> route = new List<Vector2>(); // List of points the ship must travel between to reach targetPort
    public InteractiveComponents Interactions { set { interactions = value; } }
    public string TargetPort { set { targetPort = value; } }
    public List<Vector2> Route { set { route = value; } get { return route; } }
    string WhichPort(bool portIsTarget) {
        if (portIsTarget) {
            return targetPort;
        } else {
            return previousPort;
        }
    }
    public Vector3 GetPortPos(bool portIsTarget) {
        return GameObject.Find(WhichPort(portIsTarget)).transform.position;
    }
    public Vector2 GetPortSeaPos(bool portIsTarget) {
        Vector2 PortPos = GameObject.Find(WhichPort(portIsTarget)).transform.position;
        List<Vector2> SurroundingArea = new List<Vector2>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2 SeaPos = PortPos + new Vector2(i, j)/3.3f;
                if (!interactions.OnLand(SeaPos)) {
                    SurroundingArea.Add(SeaPos);
                }
            }
        }
        return interactions.ClosestVector(SurroundingArea, GetPortPos(!portIsTarget)); ;
    }
    public void Dock() {
        previousPort = targetPort;
        transform.position = GetPortPos(true);
    }
    public bool Docked() {
        return previousPort == targetPort;
    }
}
public class ShipType {
    InteractiveComponents interactions;
    Dictionary<string, float[]> ShipStats = new Dictionary<string, float[]>();
    public InteractiveComponents Interactions { set { interactions = value; } }
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
        NewShip.GetComponent<ShipInfo>().Interactions = interactions;
        NewShip.transform.localScale = new Vector2(ShipStats[name][0], ShipStats[name][0]);
        return NewShip;
    }
}
