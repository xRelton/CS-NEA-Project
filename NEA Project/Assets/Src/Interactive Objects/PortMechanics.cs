using UnityEngine;

public class PortMechanics : MonoBehaviour {
    InteractiveComponents Interactions;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }
    // Update is called once per frame
    void Update() {
        for (int i = 0; i < transform.childCount; i++) {
            for (int j = 0; j < Interactions.GetC("Port", i).transform.childCount; j++) {
                GameObject PortObject = Interactions.GetCOC("Port", i, j);
                if (Interactions.MouseOnObject(PortObject)) {
                    if (Interactions.GetC("User Interface", 1).activeSelf == false) {
                        if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                            PortObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                        }
                        if (Input.GetMouseButtonDown(0)) {
                            transform.parent.GetComponentInChildren<UserInterfaceController>().CreateScreen("Market", PortObject.name);
                        }
                    }
                } else if (PortObject.GetComponent<Renderer>().material.GetColor("_Color") != Interactions.MyGrey && Interactions.GetC("User Interface", 1).activeSelf == false) {
                    PortObject.GetComponent<Renderer>().material.SetColor("_Color", Interactions.MyGrey);
                }
            }
        }
    }
}