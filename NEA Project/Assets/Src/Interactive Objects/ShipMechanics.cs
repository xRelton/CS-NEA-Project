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
        for (float i = -13; i < 13; i++) {
            for (float j = -7; j < 7; j++) {
                Vector2 NewNode = new Vector2(i, j);
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
                    PlayerShip.transform.position += Interactions.PerfectMove(PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1], PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 2]) * Interactions.TimeDilation;
                    if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 2], 0.1f)) {
                        PlayerShipInfo.Route.Remove(PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1]);
                    }
                } else {
                    SetShipRoute(PlayerShip.transform.position, PlayerShipInfo);
                }
                if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.GetPortPos(true), 0.1f)) {
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
        Debug.DrawLine(node - new Vector2(0.1f, 0), node + new Vector2(0.1f, 0), setColor, 10);
        Debug.DrawLine(node - new Vector2(0, 0.1f), node + new Vector2(0, 0.1f), setColor, 10);
    }
    void SetShipRoute(Vector3 shipPosition, ShipInfo shipInfo) {
        shipInfo.Route.Add(shipInfo.GetPortPos(true));
        shipInfo.Route.Add(shipInfo.GetPortSeaPos(true));
        //shipInfo.Route.AddRange(BurrowForRoute(shipInfo));
        shipInfo.Route.Add(shipInfo.GetPortSeaPos(false));
        shipInfo.Route.Add(shipPosition);
        for (int i = 0; i < shipInfo.Route.Count - 1; i++) {
            Debug.DrawLine(shipInfo.Route[i], shipInfo.Route[i + 1], Color.green, 100);
        }
    }
    List<Vector2> BurrowForRoute(ShipInfo shipInfo) {
        Dictionary<Vector2, int> NodeGroups = GetNodeGroups(shipInfo.Route[1]);
        List<Vector2> NodesHit = new List<Vector2>(); // Nodes from the lowest numbered group that have line of sight with starting port
        int CurrentGroup = -1;
        bool FinalRound = false;
        for (int i = 0; i < 10; i++) { // Loop until any number of nodes from a node group is hit
            CurrentGroup++;
            foreach (Vector2 node in Nodes) {
                if (NodeGroups[node] == CurrentGroup && TargetHitBeforeLand(shipInfo.GetPortSeaPos(false), node)) {
                    NodesHit.Add(node); // Adds all nodes in current group that hit
                    FinalRound = true;
                }
            }
            if (FinalRound == true) {
                break;
            }
        }
        List<Vector2> NodesInRoute = new List<Vector2>() { Interactions.ClosestVector(NodesHit, shipInfo.GetPortSeaPos(false)) };
        for (int i = CurrentGroup; i > 0; i--) {
            NodesHit.Clear();
            foreach (Vector2 node in Nodes) {
                if (NodeGroups[node] == i && TargetHitBeforeLand(NodesInRoute[NodesInRoute.Count - 1], node)) {
                    NodesHit.Add(node); // Adds all nodes in current group that hit
                }
            }
            NodesInRoute.Add(Interactions.ClosestVector(NodesHit, NodesInRoute[NodesInRoute.Count - 1]));
        }
        NodesInRoute.Reverse();
        return NodesInRoute;
    }
    Dictionary<Vector2, int> GetNodeGroups(Vector2 centrePos) {
        Dictionary<Vector2, int> NodeGroups = new Dictionary<Vector2, int>() { { centrePos, 0 } };
        List<Vector2> UnassignedNodes = Nodes;
        List<Vector2> PreviousNodes = new List<Vector2>() { centrePos };
        DrawNode(centrePos, 0);
        for (int GroupNum = 1; GroupNum < 10; GroupNum++) { // Loop runs while there are still unassigned nodes left
            List<Vector2> NewNodes = new List<Vector2>();
            foreach (Vector2 node in UnassignedNodes.ToList()) {
                foreach (Vector2 previousNode in PreviousNodes) {
                    if (TargetHitBeforeLand(node, previousNode)) {
                        DrawNode(node, GroupNum); // Debug draw ---------------------------------------------------------------------------------
                        NodeGroups.Add(node, GroupNum);
                        NewNodes.Add(node);
                        UnassignedNodes.Remove(node);
                        foreach (Vector2 uNode in UnassignedNodes) {
                            DrawNode(uNode, 0);
                        }
                        break;
                    }
                }
            }
            PreviousNodes = NewNodes;
        }
        return NodeGroups;
    }
    bool TargetHitBeforeLand(Vector2 start, Vector2 target) { // Checks if the straight line between the start and target point is obstructed
        float DistToTarget = Math.Abs(Vector2.Distance(start, target));
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Interactions.PerfectMove(start, target), DistToTarget); // TODO: Ensure that gradient is picked based on port to port movement
        foreach (RaycastHit2D hit in hits) {
            if (hit.distance < DistToTarget && hit.collider.name == "Sea Collider") {
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