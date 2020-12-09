using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceController : MonoBehaviour {
    UIScreenController UIScreen;
    // Start is called before the first frame update
    void Start() {
        UIScreen = gameObject.GetComponentInChildren<UIScreenController>();
        List<Tuple<string, string, Vector2, int[]>> FirstButtons = new List<Tuple<string, string, Vector2, int[]>>();
        Transform MajorPorts = GameObject.Find("Major").transform;
        for (int i = 0; i < MajorPorts.childCount; i++) {
            FirstButtons.Add(new Tuple<string, string, Vector2, int[]>(MajorPorts.GetChild(i).name, "SetInitialPort", new Vector2(0, -i), new int[0])); // Creates list of buttons (major ports) that the player can start at
        }
        List<Tuple<string, Vector2>> Title = new List<Tuple<string, Vector2>> { new Tuple<string, Vector2>("Choose a port to start in", new Vector2(0, 3)) };
        CreateScreen(Title, FirstButtons, false); // Creates first screen where the starting port is chosen
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
    public void CreateScreen(List<Tuple<string, Vector2>> texts, List<Tuple<string, string, Vector2, int[]>> buttons, bool closeButton, bool portNameIsTitle = false) { // Used to create user interface screens that are interacted with by the player
        List<GameObject> ButtonObjects = CleanUpForNewScreen(texts, closeButton, portNameIsTitle);
        foreach (Tuple<string, string, Vector2, int[]> button in buttons) {
            ButtonObjects.Add(NewButton(button));
        }
        UIScreen.Buttons = ButtonObjects;
    }
    public void CreateScreen(List<Tuple<string, Vector2>> texts, List<Tuple<string, string, Vector2, Vector2, int[]>> buttons, bool closeButton, bool portNameIsTitle = false) { // Used to create user interface screens that are interacted with by the player
        List<GameObject> ButtonObjects = CleanUpForNewScreen(texts, closeButton, portNameIsTitle);
        foreach (Tuple<string, string, Vector2, Vector2, int[]> button in buttons) {
            ButtonObjects.Add(NewButton(button));
        }
        UIScreen.Buttons = ButtonObjects;
    }
    List<GameObject> CleanUpForNewScreen(List<Tuple<string, Vector2>> texts, bool closeButton, bool portNameIsTitle) {
        UIScreen.gameObject.SetActive(true);
        if (portNameIsTitle) {
            UIScreen.PortName = texts[0].Item1;
        }
        List<GameObject> TextObjects = new List<GameObject>();
        foreach (Tuple<string, Vector2> text in texts) {
            TextObjects.Add(NewText(text));
        }
        foreach (GameObject text in UIScreen.Texts) { // Destroys all texts from the previous screen
            Destroy(text);
        }
        UIScreen.Texts = TextObjects;
        foreach (GameObject button in UIScreen.Buttons) { // Destroys all buttons from the previous screen except the close button
            if (button.name != "Close Button" && button.name != "Back Button") {
                Destroy(button);
            }
        }
        GameObject CloseButton = UIScreen.transform.GetChild(0).gameObject;
        List<GameObject> ButtonObjects = new List<GameObject>();
        CloseButton.SetActive(closeButton); // Close 'X' button in the corner appears if 'closeButton' is true
        if (CloseButton.activeSelf) {
            ButtonObjects.Add(CloseButton);
        }
        return ButtonObjects;
    }
    GameObject NewButton(Tuple<string, string, Vector2, int[]> buttonInfo) { // Tuple consists of string text, string action, Vector2 position, int[] references
        Transform PrimaryButton = GameObject.Find("UI Screen Canvas").transform.GetChild(0);
        GameObject Button = Instantiate(PrimaryButton.gameObject);
        Button.SetActive(true);
        Button.transform.parent = GameObject.Find("UI Screen Canvas").transform;
        Button.transform.name = string.Format("Button {0}", buttonInfo.Item1);
        Button.transform.position = PrimaryButton.position + new Vector3(buttonInfo.Item3.x, buttonInfo.Item3.y, 0);
        Button.transform.GetChild(0).GetComponent<Text>().text = buttonInfo.Item1;
        Button.AddComponent<UIButton>();
        Button.GetComponent<UIButton>().Action = buttonInfo.Item2;
        Button.GetComponent<UIButton>().References = buttonInfo.Item4;
        return Button;
    }
    GameObject NewButton(Tuple<string, string, Vector2, Vector2, int[]> buttonInfo) { // Tuple consists of an extra Vector2 button size
        GameObject Button = NewButton(new Tuple<string, string, Vector2, int[]> (buttonInfo.Item1, buttonInfo.Item2, buttonInfo.Item3, buttonInfo.Item5));
        Button.transform.localScale *= buttonInfo.Item4;
        Button.transform.GetChild(0).localScale /= buttonInfo.Item4;
        return Button;
    }
    GameObject NewText(Tuple<string, Vector2> textInfo) { // Tuple consists of string text, Vector2 position
        GameObject Text = new GameObject(textInfo.Item1);
        Text.transform.parent = GameObject.Find("UI Screen Canvas").transform;
        Text.transform.name = string.Format("Text {0}", textInfo.Item1);
        Text.transform.position = new Vector2(textInfo.Item2.x, textInfo.Item2.y);
        Text.transform.localScale = new Vector2(0.05f, 0.05f);
        Text.AddComponent<Text>();
        Text.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        Text.GetComponent<Text>().text = textInfo.Item1;
        Text.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        Text.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 30);
        return Text;
    }
}