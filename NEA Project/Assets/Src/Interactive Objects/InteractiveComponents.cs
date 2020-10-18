using System;
using UnityEngine;

public class InteractiveComponents : MonoBehaviour {
    public Color MyGrey;
    public void Start() {
        MyGrey = new Color(0.7f, 0.7f, 0.7f);
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

    public bool MouseOnObject(GameObject objChecked) {
        Collider2D objHit = Physics2D.Raycast(GetMousePos(), Vector2.zero).collider;
        if (objHit != null) {
            return (objHit.name == objChecked.name);
        }
        return false;
    }

    public GameObject GetFChild(string parentName, int cNum) { // Gets from first set of children
        GameObject allChildren = GameObject.Find(parentName);
        return allChildren.transform.GetChild(cNum).gameObject;
    }

    public GameObject GetCOChild(string parentName, int cNumA, int cNumB) { // Gets the child of the initial child
        GameObject firstChild = GetFChild(parentName, cNumA);
        return firstChild.transform.GetChild(cNumB).gameObject;
    }
}