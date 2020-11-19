using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenController : MonoBehaviour {
    InteractiveComponents Interactions;
    public string ScreenType;
    public string Title;
    public List<GameObject> Buttons;
    // Start is called before the first frame update
    void Start() {
        Interactions = transform.GetComponentInParent<InteractiveComponents>();
    }
    // Update is called once per frame
    void Update() {
        Interactions.GetC("UI Screen Canvas", 0).GetComponent<Text>().text = Title;
        foreach (GameObject button in Buttons) {
            UIButton buttonScript = button.GetComponent<UIButton>();
            if (buttonScript.ButtonPressed()) {
                ShipMechanics ShipMechs = GameObject.Find("Ship").GetComponent<ShipMechanics>();
                GameObject.Find("UI Screen").SetActive(false);
                if (buttonScript.Action == "SetInitialPort") {
                    ShipMechs.PlayerShips.Add(ShipMechs.NewShip(string.Format("{0} 1", ShipMechs.ShipNames[5]), ShipMechs.ShipNames[5], GameObject.Find("Ship").transform));
                    ShipMechs.PlayerShips[0].GetComponent<ShipInfo>().Port = buttonScript.PortName;
                    ShipMechs.PlayerShips[0].GetComponent<ShipInfo>().Dock();
                    ShipMechs.PlayerShips.Add(ShipMechs.NewShip(string.Format("{0} 1", ShipMechs.ShipNames[7]), ShipMechs.ShipNames[7], GameObject.Find("Ship").transform));
                    ShipMechs.PlayerShips[1].GetComponent<ShipInfo>().Port = "Rome";
                    ShipMechs.PlayerShips[1].GetComponent<ShipInfo>().Dock();
                } else if (buttonScript.Action == "SetPort") {
                    if (buttonScript.References.Count == 1) {
                        ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = buttonScript.PortName;
                    } else {
                        List<GameObject> ShipChoiceButtons = new List<GameObject>();
                        for (int i = 0; i < buttonScript.References.Count; i++) {
                            ShipChoiceButtons.Add(NewButton(ShipMechs.PlayerShips[buttonScript.References[i]].name, "ShipSelect", buttonScript.PortName, new List<int> {i}, new Vector2(0, -i)));
                        }
                        GameObject.Find("User Interface").GetComponent<UserInterfaceController>().CreateScreen("ShipSelection", "Select Ship to Send", true, ShipChoiceButtons);
                    }
                } else if (buttonScript.Action == "OpenMarket") {
                    if (buttonScript.References.Count == 1) {
                        ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = buttonScript.PortName;
                    } else {
                        List<GameObject> ShipChoiceButtons = new List<GameObject>();
                        for (int i = 0; i < buttonScript.References.Count; i++) {
                            ShipChoiceButtons.Add(NewButton(ShipMechs.PlayerShips[buttonScript.References[i]].name, "ShipSelect", buttonScript.PortName, new List<int> { i }, new Vector2(0, -i)));
                        }
                        GameObject.Find("User Interface").GetComponent<UserInterfaceController>().CreateScreen("ShipSelection", "Select Ship to Trade with", true, ShipChoiceButtons);
                    }
                } else if (buttonScript.Action == "ShipSelect") {
                    if (ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port == buttonScript.PortName) { // Market

                    } else { // Send ship
                        ShipMechs.PlayerShips[buttonScript.References[0]].GetComponent<ShipInfo>().Port = buttonScript.PortName;
                    }
                }
            }
        }
    }
    public GameObject NewButton(string text, string action, string portName, Vector2 position) {
        Transform PrimaryButton = transform.GetChild(1).GetChild(1);
        GameObject Button = Instantiate(PrimaryButton.gameObject);
        Button.SetActive(true);
        Button.transform.parent = transform.GetChild(1);
        Button.transform.name = string.Format("Button {0}", text);
        Button.transform.GetChild(0).GetComponent<Text>().text = text;
        Button.transform.position = PrimaryButton.position + new Vector3(position.x, position.y, 0);
        Button.transform.localScale = PrimaryButton.localScale;
        Button.AddComponent<UIButton>();
        Button.GetComponent<UIButton>().Action = action;
        Button.GetComponent<UIButton>().PortName = portName;
        Button.GetComponent<UIButton>().References = new List<int>();
        return Button;
    }
    public GameObject NewButton(string text, string action, string portName, List<int> references, Vector2 position) {
        GameObject Button = NewButton(text, action, portName, position);
        Button.GetComponent<UIButton>().References = references;
        return Button;
    }
}