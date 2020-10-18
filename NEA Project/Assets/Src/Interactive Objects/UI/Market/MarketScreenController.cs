using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketScreenController : MonoBehaviour {
    InteractiveComponents Interactions;
    public string PortName;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }

    // Update is called once per frame
    void Update() {
        Interactions.GetCOChild("Market Screen", 1, 0).GetComponent<Text>().text = PortName;
    }
}
