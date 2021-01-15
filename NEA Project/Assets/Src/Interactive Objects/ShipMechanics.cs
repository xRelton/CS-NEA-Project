using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShipMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    List<Vector2> Nodes = new List<Vector2>();
    public List<ShipType> ShipTypes = new List<ShipType>();
    public List<GameObject> Ships = new List<GameObject>();
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        ShipTypes.Add(new ShipType("Barque", 0.2f, 1, 2, 10)); // Sets Barque info
        ShipTypes.Add(new ShipType("Brig", 0.2f, 1, 2, 10)); // Sets Brig info
        ShipTypes.Add(new ShipType("Carrack", 0.2f, 1, 2, 10)); // Sets Carrack info
        ShipTypes.Add(new ShipType("Frigate", 0.2f, 1, 2, 10)); // Sets Frigate info
        ShipTypes.Add(new ShipType("Full-rigged Ship", 0.2f, 1, 2, 10)); // Sets Full-rigged Ship info
        ShipTypes.Add(new ShipType("Schooner", 0.2f, 1, 2, 10)); // Sets Schooner info
        ShipTypes.Add(new ShipType("Ship of the Line", 0.2f, 1, 2, 10)); // Sets Ship of the Line info
        ShipTypes.Add(new ShipType("Sloop of War", 0.2f, 1, 2, 10)); // Sets Sloop of War info
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
        foreach (GameObject ship in Ships) {
            ShipInfo shipInfo = ship.GetComponent<ShipInfo>();
            if (!shipInfo.Docked()) {
                if (shipInfo.Route.Any()) {
                    Vector2 NextRouteTarget = shipInfo.Route[shipInfo.Route.Count - 1];
                    Vector3 NextMove = ShipTypes[shipInfo.Type].Speed / 10 * Interactions.TimeDilation * Interactions.PerfectMove(ship.transform.position, NextRouteTarget);
                    ship.transform.position += NextMove;
                    if (Interactions.InVectDomain(ship.transform.position, NextRouteTarget, 0.01f)) {
                        shipInfo.Route.Remove(NextRouteTarget);
                    }
                } else {
                    shipInfo.SetShipRoute(ship.transform.position, Nodes);
                }
                if (Interactions.InVectDomain(ship.transform.position, shipInfo.GetPortPos(true), 0.01f)) {
                    shipInfo.Dock();
                    shipInfo.Route.Clear();
                }
            }
        }
    }
    public GameObject NewShip(string owner, int type, Transform parent) {
        int shipNum = 1;
        foreach (GameObject ship in Ships) {
            if (ship.GetComponent<ShipInfo>().Type == type) {
                shipNum++;
            }
        }
        GameObject NewShip = new GameObject(ShipTypes[type].Name + " " + shipNum);
        NewShip.transform.parent = parent;
        NewShip.AddComponent<SpriteRenderer>();
        NewShip.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(string.Format("Sprites/Ships/{0}", ShipTypes[type].Name.ToLower()));
        NewShip.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        NewShip.GetComponent<SpriteRenderer>().sortingOrder = 1;
        NewShip.AddComponent<BoxCollider2D>();
        NewShip.AddComponent<ShipInfo>();
        NewShip.GetComponent<ShipInfo>().Owner = owner;
        NewShip.GetComponent<ShipInfo>().Type = type;
        NewShip.GetComponent<ShipInfo>().Interactions = Interactions;
        NewShip.transform.localScale = new Vector2(ShipTypes[type].Size, ShipTypes[type].Size);
        return NewShip;
    }
}
public class ShipInfo : MonoBehaviour {
    InteractiveComponents interactions;
    int previousPort; // Number of last port the ship docked at
    int targetPort; // Number of port being headed to if the ship is travelling
    List<Vector2> route = new List<Vector2>(); // List of points the ship must travel between to reach targetPort
    int[] inventory; // Includes item id and number of items
    public InteractiveComponents Interactions {
        set {
            interactions = value; inventory = new int[GameObject.Find("Port").GetComponent<MarketSimulator>().Items.Count];
        }
    }
    public string Owner { set; get; }
    public int Type { set; get; }
    public int Port { set => targetPort = value; get => previousPort; }
    public int[] Inventory { set => inventory = value; get => inventory; }
    public int GetUsedSlots() {
        int usedSlots = 0;
        foreach (int item in inventory) {
            usedSlots += item;
        }
        return usedSlots;
    }
    public List<Vector2> Route { get => route; }
    public void Dock() {
        previousPort = targetPort;
        transform.position = GetPortPos(true);
    }
    public bool Docked() {
        return previousPort == targetPort;
    }
    public Vector3 GetPortPos(bool portIsTarget) {
        List<PortInfo> AllPorts = GameObject.Find("Port").GetComponent<PortMechanics>().Ports;
        if (portIsTarget) {
            return GameObject.Find(AllPorts[targetPort].Name).transform.position;
        } else {
            return GameObject.Find(AllPorts[previousPort].Name).transform.position;
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
    string name;
    float size;
    float speed;
    float strength;
    int slots;
    public ShipType(string name, float size, float speed, float strength, int slots) {
        this.name = name;
        this.size = size;
        this.speed = speed;
        this.strength = strength;
        this.slots = slots;
    }
    public string Name { get => name; }
    public float Size { get => size; }
    public float Speed { get => speed; }
    public float Strength { get => strength; }
    public int Slots { get => slots; }
}