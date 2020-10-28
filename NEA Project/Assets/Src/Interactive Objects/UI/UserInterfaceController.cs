using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class UserInterfaceController : MonoBehaviour {
    InteractiveComponents Interactions;
    UIScreenController UIScreen;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
        UIScreen = gameObject.GetComponentInChildren<UIScreenController>();
        List<GameObject> FirstButtons = new List<GameObject>();
        for (int i = 0; i < GameObject.Find("Major").transform.childCount; i++) {
            FirstButtons.Add(UIScreen.NewButton(Interactions.GetC("Major", i).name, "SetInitialPort", new Vector2(0, 2 - i))); // Creates list of buttons (major ports) that the player can start at
        }
        CreateScreen("StartCity", "Choose a port to start in", false, FirstButtons); // Creates first screen where the starting port is chosen
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
    public void CreateScreen(string screenType, string title, bool closeButton, List<GameObject> buttons) { // Used to create user interface screens that are interacted with by the player
        Interactions.GetC("User Interface", 1).SetActive(true);
        Interactions.GetC("User Interface", 1, 0).SetActive(closeButton); // Close 'X' button in the corner appears if 'closeButton' is true
        if (Interactions.GetC("User Interface", 1, 0).activeSelf) {
            buttons.Add(Interactions.GetC("User Interface", 1, 0));
        }
        Interactions.GetC("UI Screen Canvas", 1).SetActive(false);
        UIScreen.ScreenType = screenType;
        UIScreen.Title = title;
        foreach (GameObject button in UIScreen.Buttons) { // Destroys all buttons from the previous screen except the close button
            if (button.name != "Close Button") {
                Destroy(button);
            }
        }
        UIScreen.Buttons = buttons;
    }
}