using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenController : MonoBehaviour {
    InteractiveComponents Interactions;
    public string ScreenType;
    public string Title;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }

    // Update is called once per frame
    void Update() {
        Interactions.GetCOC("UI Screen", 1, 0).GetComponent<Text>().text = Title;
    }
}
