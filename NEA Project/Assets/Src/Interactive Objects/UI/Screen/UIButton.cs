using UnityEngine;

public class UIButton : MonoBehaviour {
    InteractiveComponents Interactions;
    public string Action;
    public string Reference;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }

    // Update is called once per frame
    public bool ButtonPressed() {
        if (Interactions.MouseOnObject(transform.gameObject)) {
            if (transform.gameObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                transform.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }
            if (Input.GetMouseButtonDown(0)) {
                return true;
            }
        } else if (transform.gameObject.GetComponent<Renderer>().material.GetColor("_Color") != Interactions.MyGrey) {
            transform.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Interactions.MyGrey);
        }
        return false;
    }
}
