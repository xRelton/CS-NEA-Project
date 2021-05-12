using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveComponents : MonoBehaviour { // Controls overarching variables and stores functions applicable to many other classes
    public float TimeDilation;
    public Color MyGrey;
    public string[] LandColliders;
    void Start() { // Sets universal initial important values
        TimeDilation = Time.deltaTime;
        MyGrey = new Color(0.7f, 0.7f, 0.7f);
        LandColliders = new string[] { "continents", "west islands", "italy", "greece", "east islands" };
    }
    public Vector2 GetMousePos() { // Gets the mouse position relative to the camera zoom
        float zoomAbs = Math.Abs(Camera.main.transform.position.z);
        Vector2 screenCentreInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosOnScreen = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePosOnScreen.x = (2 * mousePosOnScreen.x - 1);
        mousePosOnScreen.y = (2 * mousePosOnScreen.y - 1);
        mousePosOnScreen.x = (zoomAbs / -78.50297938f) * (-104.0847188f * mousePosOnScreen.x - 0.01859821175f) + (mousePosOnScreen.x / -78.50297938f);
        mousePosOnScreen.y = (zoomAbs / -22.73010161f) * (-12.87099021f * mousePosOnScreen.y - 0.01777780835f) + (mousePosOnScreen.y / -22.73010161f);
        return screenCentreInWorld + mousePosOnScreen;
    }
    public bool PosOnObject(Vector2 pos, GameObject objChecked) { // Uses raycasting to check if a position on-screen is in-line with a named object
        RaycastHit2D[] objsHit = Physics2D.RaycastAll(pos, Vector2.zero);
        if (objsHit != null) {
            foreach (RaycastHit2D objHit in objsHit) {
                if (objHit.collider.name == objChecked.name) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool OnLand(Vector3 pos) { // Checks if a position is on a land object
        bool ReturnVal = false;
        foreach (string collider in LandColliders) {
            ReturnVal |= PosOnObject(pos, GameObject.Find(collider));
        }
        return ReturnVal;
    }
    public Vector3 PerfectMove(Vector2 start, Vector2 target) { // Gets vector between two position vector points and divides it by the distance between them
        return (target - start) / Vector2.Distance(start, target);
    }
    public bool MouseOnObject(GameObject objChecked) { // Checks if the mouse is on a named object
        return PosOnObject(GetMousePos(), objChecked);
    }
    public bool InVectDomain(Vector2 vect1, Vector2 vect2, float domain) { // Checks if two position vectors exist in each other's detecting circle of 'domain' radius
        return (vect1.x < vect2.x + domain && vect1.x > vect2.x - domain && vect1.y < vect2.y + domain && vect1.y > vect2.y - domain);
    }
    public Vector2 ClosestVector(List<Vector2> vects, Vector2 goal) { // Checks which of a list of position vectors is closest to the vector goal
        Vector2 ClosestVect = vects[0];
        for (int i = 1; i < vects.Count - 1; i++) {
            if (Vector2.Distance(vects[i], goal) < Vector2.Distance(ClosestVect, goal)) {
                ClosestVect = vects[i];
            }
        }
        return ClosestVect;
    }
    public void DrawPoint(Vector2 node, int color) { // Draws a cross of an input colour as a point on the screen
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
}