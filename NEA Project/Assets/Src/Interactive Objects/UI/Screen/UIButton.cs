using UnityEngine;

public class UIButton : MonoBehaviour { // Controls colour changing of highlighted buttons and receiving click inputs
    InteractiveComponents Interactions;
    public string Action;
    public int[] References;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }

    // Update is called once per frame
    public bool ButtonPressed() { // Returns true if the mouse button is pressed while the mouse hovers over the button
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