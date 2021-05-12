using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceController : MonoBehaviour { // Controls creation of UI screens and matching UI position to camera position
    UIScreenController UIScreen;
    // Start is called before the first frame update
    void Start() {
        UIScreen = gameObject.GetComponentInChildren<UIScreenController>(); // Creates the UI screen that may pop up when buttons are pressed
        List<ButtonUIObject> FirstButtons = new List<ButtonUIObject>();
        Transform MajorPorts = GameObject.Find("Major").transform;
        for (int i = 0; i < MajorPorts.childCount; i++) {
            FirstButtons.Add(new ButtonUIObject(MajorPorts.GetChild(i).name, "SetInitialPort", new Vector2(0, -i))); // Creates list of buttons (major ports) that the player can start at
        }
        List<TextUIObject> Title = new List<TextUIObject> { new TextUIObject("Choose a port to start in", new Vector2(0, 3)) };
        CreateScreen(Title, FirstButtons, false); // Creates first screen where the starting port is chosen
    }
    // Update is called once per frame
    void Update() {
        MatchObjPositionToCamera(transform);
    }
    public void MatchObjPositionToCamera(Transform obj) { // Moves all UI objects to sit in-front of the camera perfectly
        obj.position = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
        obj.localScale = new Vector2(Math.Abs(Camera.main.transform.position.z) / 9, Math.Abs(Camera.main.transform.position.z) / 9);
    }
    public void CreateScreen(List<TextUIObject> texts, List<ButtonUIObject> buttons, bool closeButton, bool portNameIsTitle = false) { // Used to create user interface screens that are interacted with by the player without sliders
        UIScreen.gameObject.SetActive(true);
        if (portNameIsTitle) {
            UIScreen.PortID = Array.FindIndex(GameObject.Find("Port").GetComponent<PortMechanics>().Ports, element => element.Name == texts[0].Contents);
        }
        List<GameObject> AllObjects = new List<GameObject>(UIScreen.Texts.Count + UIScreen.Sliders.Count + UIScreen.Buttons.Count);
        AllObjects.AddRange(UIScreen.Texts);
        AllObjects.AddRange(UIScreen.Sliders);
        AllObjects.AddRange(UIScreen.Buttons);
        foreach (GameObject objToDelete in AllObjects) { // Destroys all buttons, texts and sliders from the previous screen except the close and back buttons
            if (objToDelete.transform.parent.name == "UI Screen Canvas") {
                Destroy(objToDelete);
            }
        }
        List<GameObject> TextObjects = new List<GameObject>();
        List<GameObject> ButtonObjects = new List<GameObject>();
        GameObject CloseButton = UIScreen.transform.GetChild(0).gameObject;
        CloseButton.SetActive(closeButton); // Close 'X' button appears in the corner if 'closeButton' is true
        if (CloseButton.activeSelf) {
            ButtonObjects.Add(CloseButton);
        }
        foreach (TextUIObject text in texts) {
            TextObjects.Add(text.NewText());
        }
        foreach (ButtonUIObject button in buttons) {
            ButtonObjects.Add(button.NewButton(Instantiate(GameObject.Find("UI Screen Canvas").transform.GetChild(0))));
        }
        UIScreen.Texts = TextObjects;
        UIScreen.Sliders = new List<GameObject>();
        UIScreen.Buttons = ButtonObjects;
    }
    public void CreateScreen(List<TextUIObject> texts, List<SliderUIObject> sliders, List<ButtonUIObject> buttons, bool closeButton, bool portNameIsTitle = false) { // Used to create user interface screens that are interacted with by the player including sliders
        CreateScreen(texts, buttons, closeButton, portNameIsTitle);
        List<GameObject> SliderObjects = new List<GameObject>();
        foreach (SliderUIObject slider in sliders) {
            SliderObjects.Add(slider.NewSlider(Instantiate(GameObject.Find("UI Screen Canvas").transform.GetChild(1))));
        }
        UIScreen.Sliders = SliderObjects;
    }
}
public class UIObject { // Stores data for each new UI object, each inheriting object adding data specific to its type
    protected UIObject(string contents, Vector2 position, Vector2 size) {
        Contents = contents;
        Position = position;
        if (size == new Vector2()) {
            Size = new Vector2(1, 1);
        } else {
            Size = size;
        }
    }
    public string Contents { get; }
    public Vector2 Position { get; }
    public Vector2 Size { get; }
    protected GameObject NewUIObject(string type, GameObject baseObject = null) { // Creates UI object of a certain type and assigns basic values to it
        GameObject UIObject = baseObject ?? new GameObject(string.Format("{0} {1}", type, Contents));
        UIObject.transform.position += new Vector3(Position.x, Position.y, 0);
        UIObject.transform.localScale *= new Vector2(Size.x, Size.y);
        if (baseObject != null) {
            UIObject.name = string.Format("{0} {1}", type, Contents);
        }
        UIObject.transform.SetParent(GameObject.Find("UI Screen Canvas").transform, false);
        return UIObject;
    }
}
public class TextUIObject : UIObject {
    public TextUIObject(string contents, Vector2 position, Vector2 size = new Vector2()) : base(contents, position, size) { }
    public GameObject NewText() { // Creates text UI object that shows text
        GameObject text = NewUIObject("Text");
        text.transform.localScale *= new Vector2(0.05f, 0.05f) * Size;
        text.AddComponent<Text>();
        text.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.GetComponent<Text>().text = Contents;
        text.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 60);
        return text;
    }
}
public class SliderUIObject : UIObject {
    private int maxValue;
    public SliderUIObject(string contents, Vector2 position, Vector2 size, int maxValue) : base(contents, position, size) {
        this.maxValue = maxValue;
    }
    public GameObject NewSlider(Transform sliderBase) { // Creates slider UI object that has an interactable slider
        GameObject slider = NewUIObject("Slider", sliderBase.gameObject);
        slider.GetComponent<Slider>().maxValue = maxValue;
        slider.SetActive(true);
        return slider;
    }
}
public class ButtonUIObject : UIObject {
    public ButtonUIObject(string contents, string action, Vector2 position, int[] references = null, Vector2 size = new Vector2()) : base(contents, position, size) {
        Action = action;
        References = references ?? new int[0];
    }
    public string Action { get; }
    public int[] References { get; }
    public GameObject NewButton(Transform buttonBase) { // Creates button UI object that can be pressed to reach a new point
        GameObject button = NewUIObject("Button", buttonBase.gameObject);
        button.SetActive(true);
        button.transform.GetChild(0).localScale /= Size;
        button.GetComponentInChildren<Text>().text = Contents;
        button.AddComponent<UIButton>();
        button.GetComponent<UIButton>().Action = Action;
        button.GetComponent<UIButton>().References = References;
        return button;
    }
}