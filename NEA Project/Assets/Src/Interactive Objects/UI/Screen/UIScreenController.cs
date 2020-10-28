﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
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
                GameObject.Find("UI Screen").SetActive(false);
                if (buttonScript.Action == "SetInitialPort") {
                    ShipMechanics ShipMechs = GameObject.Find("Ship").GetComponent<ShipMechanics>();
                    ShipMechs.PlayerShips.Add(ShipMechs.ShipTypes.NewShip(ShipMechs.ShipNames[5], GameObject.Find("Ship").transform));
                    ShipMechs.PlayerShips[0].transform.position = GameObject.Find(buttonScript.Reference).transform.position;
                } else if (buttonScript.Action == "SetPort") {
                    GameObject.Find("Ship").GetComponent<ShipMechanics>().PlayerShips[0].transform.position = GameObject.Find(buttonScript.Reference).transform.position;
                }
            }
        }
    }
    public GameObject NewButton(string text, string action, Vector2 position) {
        GameObject Button = Instantiate(transform.GetChild(1).GetChild(1).gameObject);
        Button.SetActive(true);
        Button.transform.parent = transform.GetChild(1);
        Button.transform.name = string.Format("Button {0}", text);
        Button.transform.GetChild(0).GetComponent<Text>().text = text;
        Button.transform.position = position;
        Button.AddComponent<UIButton>();
        Button.GetComponent<UIButton>().Action = action;
        Button.GetComponent<UIButton>().Reference = text;
        return Button;
    }
    public GameObject NewButton(string text, string action, string reference, Vector2 position) {
        GameObject Button = NewButton(text, action, position);
        Button.GetComponent<UIButton>().Reference = reference;
        return Button;
    }
}