using UnityEngine;

public class CloseButton : MonoBehaviour {
    InteractiveComponents Interactions;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }

    // Update is called once per frame
    void Update() {
        if (Interactions.MouseOnObject(transform.gameObject)) {
            if (transform.gameObject.GetComponent<Renderer>().material.GetColor("_Color") != Color.white) {
                transform.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }
            if (Input.GetMouseButtonDown(0)) {
                transform.parent.gameObject.SetActive(false);
            }
        } else if (transform.gameObject.GetComponent<Renderer>().material.GetColor("_Color") != Interactions.MyGrey) {
            transform.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Interactions.MyGrey);
        }
    }
}
