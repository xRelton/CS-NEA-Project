using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveComponents : MonoBehaviour {
    public float TimeDilation;
    public Color MyGrey;
    public string[] LandColliders;
    void Start() {
        TimeDilation = Time.deltaTime;
        MyGrey = new Color(0.7f, 0.7f, 0.7f);
        LandColliders = new string[] { "continents", "west islands", "italy", "greece", "east islands" };
    }
    public Vector2 GetMousePos() {
        float zoomAbs = Math.Abs(Camera.main.transform.position.z);
        Vector2 screenCentreInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosOnScreen = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePosOnScreen.x = (2 * mousePosOnScreen.x - 1);
        mousePosOnScreen.y = (2 * mousePosOnScreen.y - 1);
        mousePosOnScreen.x = (zoomAbs / -78.50297938f) * (-104.0847188f * mousePosOnScreen.x - 0.01859821175f) + (mousePosOnScreen.x / -78.50297938f);
        mousePosOnScreen.y = (zoomAbs / -22.73010161f) * (-12.87099021f * mousePosOnScreen.y - 0.01777780835f) + (mousePosOnScreen.y / -22.73010161f);
        return screenCentreInWorld + mousePosOnScreen;
    }
    public bool PosOnObject(Vector2 pos, GameObject objChecked) {
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
    public bool OnLand(Vector3 pos) {
        bool ReturnVal = false;
        foreach (string collider in LandColliders) {
            ReturnVal |= PosOnObject(pos, GameObject.Find(collider));
        }
        return ReturnVal;
    }
    public Vector3 PerfectMove(Vector2 start, Vector2 target) {
        return (target - start) / Vector2.Distance(start, target);
    }
    public bool MouseOnObject(GameObject objChecked) {
        return PosOnObject(GetMousePos(), objChecked);
    }
    public GameObject GetC(string parentName, int cNum) { // Gets from immediate child of gameobject with name parentName
        GameObject allChildren = GameObject.Find(parentName);
        return allChildren.transform.GetChild(cNum).gameObject;
    }

    public GameObject GetC(string parentName, int cNumA, int cNumB) { // Override method gets the child of the initial child
        GameObject firstChild = GetC(parentName, cNumA);
        return firstChild.transform.GetChild(cNumB).gameObject;
    }
    public bool InVectDomain(Vector2 vect1, Vector2 vect2, float domain) {
        return (vect1.x < vect2.x + domain && vect1.x > vect2.x - domain && vect1.y < vect2.y + domain && vect1.y > vect2.y - domain) ;
    }
    public Vector2 ClosestVector(List<Vector2> vects, Vector2 goal) {
        Vector2 ClosestVect = vects[0];
        for (int i = 1; i < vects.Count - 1; i++) {
            if (Vector2.Distance(vects[i], goal) < Vector2.Distance(ClosestVect, goal)) {
                ClosestVect = vects[i];
            }
        }
        return ClosestVect;
    }
}