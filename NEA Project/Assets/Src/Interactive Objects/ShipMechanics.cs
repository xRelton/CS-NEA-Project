using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShipMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    List<Vector2> Nodes = new List<Vector2>();
    public string[] ShipNames = new string[] { "barque", "brig", "carrack", "frigate", "full-rigged ship", "schooner", "ship of the line", "sloop of war" };
    public ShipType ShipTypes = new ShipType();
    public List<GameObject> PlayerShips = new List<GameObject>();
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
        for (float x = -8; x <= 10; x += 0.5f) {
            for (float y = -2; y <= 3; y += 0.5f) {
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
                    Vector2 NextRouteTarget = PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1];
                    Vector3 NextMove = ShipTypes.GetSpeed(PlayerShipInfo.Type) / 10 * Interactions.TimeDilation * Interactions.PerfectMove(PlayerShip.transform.position, NextRouteTarget);
                    // If the new position is not on land
                    PlayerShip.transform.position += NextMove;
                    // Else navigate around land until new position not land
                    if (Interactions.InVectDomain(PlayerShip.transform.position, NextRouteTarget, 0.01f)) {
                        PlayerShipInfo.Route.Remove(NextRouteTarget);
                    }
                } else {
                    PlayerShipInfo.SetShipRoute(PlayerShip.transform.position, Nodes);
                }
                if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.GetPortPos(true), 0.01f)) {
                    PlayerShipInfo.Dock();
                    PlayerShipInfo.Route.Clear();
                }
            }
        }
    }
    public GameObject NewShip(string name, string type, Transform parent) {
        GameObject NewShip = new GameObject(type);
        NewShip.name = name;
        NewShip.transform.parent = parent;
        NewShip.AddComponent<SpriteRenderer>();
        NewShip.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(string.Format("Sprites/Ships/{0}", type));
        NewShip.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        NewShip.GetComponent<SpriteRenderer>().sortingOrder = 1;
        NewShip.AddComponent<BoxCollider2D>();
        NewShip.AddComponent<ShipInfo>();
        NewShip.GetComponent<ShipInfo>().Type = type;
        NewShip.GetComponent<ShipInfo>().Interactions = Interactions;
        NewShip.transform.localScale = new Vector2(ShipTypes.GetSize(type), ShipTypes.GetSize(type));
        return NewShip;
    }
}
public class ShipInfo : MonoBehaviour {
    InteractiveComponents interactions;
    string previousPort; // Name of last port the ship docked at
    string targetPort; // Name of port being headed to if the ship is travelling
    List<Vector2> route = new List<Vector2>(); // List of points the ship must travel between to reach targetPort
    public InteractiveComponents Interactions { set { interactions = value; } }
    public string Type { set; get; }
    public string Port { set { targetPort = value; } get { return previousPort; } }
    public List<Vector2> Route { get { return route; } }
    public void Dock() {
        previousPort = targetPort;
        transform.position = GetPortPos(true);
    }
    public bool Docked() {
        return previousPort == targetPort;
    }
    public Vector3 GetPortPos(bool portIsTarget) {
        if (portIsTarget) {
            return GameObject.Find(targetPort).transform.position;
        } else {
            return GameObject.Find(previousPort).transform.position;
        }
    }
    Vector2 GetSeaPos(Vector2 shipPos, Vector2 targetPos) {
        List<Vector2> SurroundingArea = new List<Vector2>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2 SeaPos = shipPos + new Vector2(i, j) / 3.3f;
                if (!interactions.OnLand(SeaPos)) {
                    SurroundingArea.Add(SeaPos);
                }
            }
        }
        return interactions.ClosestVector(SurroundingArea, targetPos);
    }
    public void SetShipRoute(Vector3 shipPosition, List<Vector2> nodes) {
        Vector2 PreviousPortSeaPos = GetSeaPos(shipPosition, GetPortPos(true));
        Vector2 TargetPortSeaPos = GetSeaPos(GetPortPos(true), GetPortPos(false));
        route.Add(GetPortPos(true));
        route.Add(TargetPortSeaPos);
        if (!TargetHitBeforeLand(PreviousPortSeaPos, TargetPortSeaPos)) {
            route.AddRange(BurrowForRoute(PreviousPortSeaPos, TargetPortSeaPos, nodes));
        }
        route.Add(PreviousPortSeaPos);
        for (int i = 0; i < route.Count - 1; i++) {
            //Debug.DrawLine(route[i], route[i + 1], Color.green, 15);
        }
    }
    bool TargetHitBeforeLand(Vector2 start, Vector2 target) { // Checks if the straight line between the start and target point is unobstructed
        bool ReturnVal = true;
        RaycastHit2D[] hitsUp = Physics2D.RaycastAll(start, interactions.PerfectMove(start, target), Vector2.Distance(start, target));
        RaycastHit2D[] hitsDown = Physics2D.RaycastAll(start, interactions.PerfectMove(target, start), Vector2.Distance(start, target));
        foreach (RaycastHit2D hit in hitsUp) {
            if (interactions.LandColliders.Any(collider => collider == hit.collider.name)) {
                ReturnVal = false;
            }
        }
        foreach (RaycastHit2D hit in hitsDown) {
            if (interactions.LandColliders.Any(collider => collider == hit.collider.name) && ReturnVal == false) {
                return false;
            }
        }
        return true;
    }
    List<Vector2> BurrowForRoute(Vector2 startPos, Vector2 targetPos, List<Vector2> nodes) {
        Dictionary<Vector2, int> NodeGroups = GetNodeGroups(Route[1], nodes);
        List<Vector2> NodesHit = new List<Vector2>() { }; // Nodes from the lowest numbered group that have line of sight with starting port
        int CurrentGroup = -1;
        while (!NodesHit.Any()) { // Loops while no nodes from a node group are hit
            CurrentGroup++;
            foreach (Vector2 node in nodes) {
                if (NodeGroups[node] == CurrentGroup && TargetHitBeforeLand(startPos, node)) {
                    NodesHit.Add(node); // Adds all nodes in current group that hit
                }
            }
        }
        List<Vector2> NodesInRoute = new List<Vector2>();
        for (int i = CurrentGroup; i > 0; i--) {
            NodesInRoute.Add(interactions.ClosestVector(NodesHit, targetPos));
            NodesHit.Clear();
            foreach (Vector2 node in nodes) {
                if (NodeGroups[node] == i && TargetHitBeforeLand(NodesInRoute[NodesInRoute.Count - 1], node)) {
                    //Debug.DrawLine(NodesInRoute[NodesInRoute.Count - 1], node, Color.yellow, 15);
                    NodesHit.Add(node); // Adds all nodes in current group that hit
                }
            }
        }
        NodesInRoute.Reverse();
        return NodesInRoute;
    }
    Dictionary<Vector2, int> GetNodeGroups(Vector2 centrePos, List<Vector2> nodes) {
        Dictionary<Vector2, int> NodeGroups = new Dictionary<Vector2, int>() { };
        List<Vector2> UnassignedNodes = nodes.ToList();
        List<Vector2> PreviousNodes = new List<Vector2>() { centrePos };
        int GroupNum = 1;
        while (UnassignedNodes.Any()) { // Loop runs while there are still unassigned nodes left
            List<Vector2> NewNodes = new List<Vector2>();
            foreach (Vector2 node in UnassignedNodes.ToList()) {
                foreach (Vector2 previousNode in PreviousNodes) {
                    if (TargetHitBeforeLand(previousNode, node)) {
                        //Interactions.DrawPoint(node, GroupNum - 1);
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
}
public class ShipType {
    InteractiveComponents interactions;
    Dictionary<string, float[]> shipStats = new Dictionary<string, float[]>();
    public InteractiveComponents Interactions { set { interactions = value; } }
    public void SetStat(string name, float size, float speed, float strength) {
        shipStats.Add(name, new float[] { size, speed, strength });
    }
    public float GetSize(string name) { return (shipStats[name][0]); }
    public float GetSpeed(string name) { return (shipStats[name][1]); }
    public float GetStrength(string name) { return (shipStats[name][2]); }
}
