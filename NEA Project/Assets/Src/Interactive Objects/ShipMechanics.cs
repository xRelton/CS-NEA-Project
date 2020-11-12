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
    }

    // Update is called once per frame
    void Update() {
        foreach (GameObject PlayerShip in PlayerShips) {
            ShipInfo PlayerShipInfo = PlayerShip.GetComponent<ShipInfo>();
            if (!PlayerShipInfo.Docked()) {
                if (PlayerShipInfo.Route.Any()) {
                    PlayerShip.transform.position += PlayerShipInfo.PerfectMove(PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1], PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 2]) * Interactions.TimeDilation;
                    if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 2], 0.1f)) {
                        PlayerShipInfo.Route.Remove(PlayerShipInfo.Route[PlayerShipInfo.Route.Count - 1]);
                    }
                } else {
                    SetShipRoute(PlayerShip.transform.position, PlayerShipInfo);
                }
                if (Interactions.InVectDomain(PlayerShip.transform.position, PlayerShipInfo.GetTargetPortPos(), 0.1f)) {
                    PlayerShipInfo.Dock();
                    PlayerShipInfo.Route.Clear();
                }
            }
        }
    }
    void SetShipRoute(Vector3 shipPosition, ShipInfo shipInfo) {
        List<Point> Points = new List<Point>();
        Points.Add(new Point(shipPosition, 0, -1));
        Points.Add(new Point(shipInfo.GetPreviousPortSeaPos(), 1, 0));
        if (!Interactions.InVectDomain(shipInfo.NextLines(shipInfo.GetPreviousPortSeaPos(), false)[0], shipInfo.GetTargetPortPos(), 0.1f)) {
            Points = GetIndirectRoute(Points, shipInfo);
        } else {
            Points.Add(new Point(shipInfo.GetTargetPortPos(), 2, 1));
        }
        shipInfo.Route = BurrowForRoute(Points);
    }
    List<Point> GetIndirectRoute(List<Point> points, ShipInfo shipInfo) {
        int Generation = 2;
        while (true) {
            int PrePointCount = points.Count;
            for (int i = 1; i < PrePointCount; i++) {
                if (points[i].Generation == Generation) {
                    foreach (Vector3 lineEnd in shipInfo.NextLines(points[i].Position)) {
                        if (Interactions.InVectDomain(lineEnd, shipInfo.GetTargetPortPos(), 0.1f)) {
                            points.Add(new Point(lineEnd, Generation, i));;
                            return points;
                        } else {
                            foreach (Vector3 newPoint in shipInfo.NextPoints(points[i].Position, lineEnd)) {
                                points.Add(new Point(newPoint, Generation, i));
                                points[points.Count - 1].SetPosition(points);
                            }
                        }
                    }
                }
            }
            Generation++;
        }
    }
    List<Vector3> BurrowForRoute(List<Point> points) {
        List<Point> PointRoute = new List<Point>();
        PointRoute.Add(points[points.Count - 1]);
        for (int i = 1; i <= PointRoute[0].Generation; i++) {
            PointRoute.Add(points[PointRoute[i - 1].Parent]);
        }
        List<Vector3> VectRoute = new List<Vector3>();
        foreach (Point point in PointRoute) {
            VectRoute.Add(point.Move);
        }
        return VectRoute;
    }
}
public class ShipInfo : MonoBehaviour {
    InteractiveComponents interactions;
    string previousPort; // Name of last port the ship docked at
    string targetPort; // Name of port being headed to if the ship is travelling
    List<Vector3> route = new List<Vector3>(); // List of points the ship must travel between to reach targetPort
    public InteractiveComponents Interactions { set { interactions = value; } }
    public string TargetPort { set { targetPort = value; } }
    public List<Vector3> Route { set { route = value; } get { return route; } }
    public Vector3 GetTargetPortPos() {
        return GameObject.Find(targetPort).transform.position;
    }
    public void Dock() {
        previousPort = targetPort;
        transform.position = GetTargetPortPos();
    }
    public bool Docked() {
        return previousPort == targetPort;
    }
    public bool OnLand(Vector3 posChange) {
        return interactions.PosOnObject(transform.position + posChange, GameObject.Find("Sea Collider"));
    }
    float GradShipToPoint(Vector3 target) {
        return (target.y - transform.position.y) / (target.x - transform.position.x); // Gradient value
    }
    public Vector3 PerfectMove(Vector3 start, Vector3 target) {
        float xChange = (target.x - start.x) / 3; // (+/-) depends on direction of ship, adjusted to make vertical and horizontal journeys similar speeds
        float yChange = xChange * GradShipToPoint(target);
        return new Vector3(xChange, yChange);
    }
    Vector3 GetReciprocal(Vector3 lineEnd, bool reverse) {
        Vector3 Reciprocal = new Vector3(lineEnd.x, -(float)Math.Pow(lineEnd.x,2) / lineEnd.y);
        if (reverse) {
            return -Reciprocal;
        }
        return Reciprocal;
    }
    public Vector3 GetPreviousPortSeaPos() {
        Vector3 PreviousPortPos = GameObject.Find(previousPort).transform.position;
        /*Vector3 TargetPortPos = GetTargetPortPos();
        int xDistance = (int)Math.Floor(TargetPortPos.x - PreviousPortPos.x);
        if (xDistance > 0) {
            for (float x = 0; x < xDistance; x += 0.02f) {
                Vector3 SeaTest = new Vector3(x, x * GradShipToPoint(GetTargetPortPos()));
                if (!OnLand(SeaTest)) { // If PerfectMove() is obstructed
                    return SeaTest;
                }
            }
        } else {
            for (float x = 0; x > xDistance; x -= 0.02f) {
                Vector3 SeaTest = new Vector3(x, x * GradShipToPoint(GetTargetPortPos()));
                if (!OnLand(SeaTest)) { // If PerfectMove() is obstructed
                    return SeaTest;
                }
            }
        }
        int yDistance = (int)Math.Floor(TargetPortPos.y - PreviousPortPos.y);
        return new Vector3(PreviousPortPos.x + 0.1f * xDistance / Math.Abs(xDistance), PreviousPortPos.y + 0.1f * yDistance / Math.Abs(yDistance));*/
        List<Vector3> SeaSides = new List<Vector3>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector3 SeaPos = new Vector3(i, j);
                if (!OnLand(SeaPos)){
                    SeaSides.Add(PreviousPortPos + SeaPos);
                }
            }
        }
        float SmallestDistance = -1;
        Vector3 ClosestSide = PreviousPortPos;
        foreach (Vector3 side in SeaSides) {
            if (SmallestDistance == -1 || side.magnitude - GetTargetPortPos().magnitude < SmallestDistance) {
                SmallestDistance = side.magnitude - GetTargetPortPos().magnitude;
                ClosestSide = side;
            }
        }
        return ClosestSide;
    }
    Vector3 LineLandIntersect(Vector2 start, Vector2 move) { // Checks if the straight line between the start and point moved to is obstructed
        float SmallestDistance = Math.Abs(start.magnitude - move.magnitude);
        Vector3 ClosestPoint = move;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, PerfectMove(start, start + move), SmallestDistance); // TODO: Ensure that gradient is picked based on port to port movement
        foreach (RaycastHit2D hit in hits) {
            if (hit.distance < SmallestDistance) {
                Debug.Log(string.Format("{0} collision happens in port: {1}", hit.collider.name, interactions.InVectDomain(hit.point, GameObject.Find(previousPort).transform.position, 0.01f)));
                if (hit.collider.name == targetPort) {
                    SmallestDistance = hit.distance;
                    ClosestPoint = move;
                    Debug.Log(move == start - hit.point);
                } else if (hit.collider.name == "Sea Collider" && !interactions.InVectDomain(hit.point, GameObject.Find(previousPort).transform.position, 0.01f)) {
                    SmallestDistance = hit.distance;
                    ClosestPoint = start - hit.point;
                }
            }
        }
        return ClosestPoint;
    }
    public Vector3[] NextLines(Vector3 point, bool perpendicular = true) {
        Vector3 DistToLand = LineLandIntersect(point, GetTargetPortPos());
        if (perpendicular && interactions.InVectDomain(DistToLand, GetTargetPortPos(), 0.1f)) {
            return new Vector3[] { GetReciprocal(DistToLand, false), GetReciprocal(DistToLand, true) }; // For each POINT: fractal into 2 perpendicular LINES from point
        } else {
            return new Vector3[] { DistToLand }; // Set LINE to final point if there is no obstruction || set LINE to obstruction point if on the first generation
        }
    }
    public Vector2[] NextPoints(Vector2 point, Vector2 direction) { // For each LINE: split into 3 equidescent points between start point and obstruction point
        Vector2 DistToLand = LineLandIntersect(point, direction);
        Vector2[] EquiPoints = new Vector2[3];
        EquiPoints[0] = DistToLand * (1 / 4);
        EquiPoints[1] = DistToLand * (1 / 2);
        EquiPoints[2] = DistToLand * (3 / 4);
        return EquiPoints;
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

public class Point {
    Vector3 move;
    Vector3 position;
    int generation;
    int parent;
    public Point(Vector3 move, int generation, int parent) {
        this.move = move;
        this.position = move;
        this.generation = generation;
        this.parent = parent;
    }
    public void SetPosition(List<Point> Points) {
        position = Points[parent].position + move;
    }
    public Vector3 Move { get { return move; } }
    public Vector3 Position { get { return position; } }
    public int Generation { get { return generation; } }
    public int Parent { get { return parent; } }
}