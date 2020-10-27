using System;
using UnityEngine;

public class UserInterfaceController : MonoBehaviour {
    InteractiveComponents Interactions;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        gameObject.GetComponentInChildren<UIScreenController>().ScreenType = "StartCity";
        gameObject.GetComponentInChildren<UIScreenController>().Title = "Choose a city to start in";
    }
    // Update is called once per frame
    void Update() {
        SetUIObjectsPosition(Camera.main.transform.position.x, Camera.main.transform.position.y);
        SetUIObjectsScale(Math.Abs(Camera.main.transform.position.z) / 9);
    }
    public void SetUIObjectsPosition(float xPos, float yPos) {
        transform.position = new Vector2(xPos, yPos);
    }
    public void SetUIObjectsScale(float scale) {
        transform.localScale = new Vector2(scale, scale);
    }
    public void CreateScreen(string screenType, string title) {
        Interactions.GetC("User Interface", 1).SetActive(true);
        gameObject.GetComponentInChildren<UIScreenController>().ScreenType = screenType;
        gameObject.GetComponentInChildren<UIScreenController>().Title = title;
    }
}