﻿using UnityEngine;
using UnityEngine.UI;

public class WeatherMechanics : MonoBehaviour { // Creates weather and controls compass movement
    public Weather WorldWeather;
    void Start() { // Start is called before the first frame update
        WorldWeather = new Weather();
    }

    void Update() { // Update is called once per frame
        if (!WorldWeather.AtGoal()) {
            WorldWeather.MoveToGoal();
        }
    }
}
public class Weather { // Controls weather randomisation and compass displaying
    int windSpeed;
    Vector2 windDirection;
    float angleGoal;
    float anglePosition = 0;
    public Vector2 Wind { get => windSpeed * windDirection/windDirection.magnitude/7500; } // Outputs the vector of the wind and its power multiplied
    public void UpdateMonth() { // Sets the new wind direction then calculates bearing angle from due north for the compass to be set at
        do {
            windDirection = new Vector2(Random.Range(-3, 4), Random.Range(-3, 4));
        } while (windDirection.magnitude == 0);
        int quadrant = 0;
        if (windDirection.x < 0 || (windDirection.x == 0 && windDirection.y < 0)) {
            quadrant = 2;
        }
        if (windDirection.x * windDirection.y < 0 || windDirection.y == 0) {
            quadrant++;
        }
        float angle = 0;
        if (windDirection.x * windDirection.y != 0) {
            if (windDirection.x * windDirection.y < 0) {
                angle = Mathf.Atan(Mathf.Abs(windDirection.y / windDirection.x));
            } else {
                angle = Mathf.Atan(Mathf.Abs(windDirection.x / windDirection.y));
            }
        }
        angleGoal = (360 - ((quadrant * 90) + (180 * angle) / Mathf.PI)) % 360;
        windSpeed = Random.Range(1, 9);
        GameObject.Find("Wind Speed").GetComponent<Text>().text = "Wind Speed: " + windSpeed;
    }
    public bool AtGoal() { // Checks if the compass arrow is in the right position and stops its movement
        if (Mathf.Round(anglePosition) == Mathf.Round(angleGoal)) {
            if (anglePosition != angleGoal) {
                angleGoal = (angleGoal + 360) % 360;
                GameObject.Find("Arrow").transform.rotation = new Quaternion();
                GameObject.Find("Arrow").transform.Rotate(0, 0, angleGoal);
                anglePosition = angleGoal;
            }
            return true;
        }
        return false;
    }
    public void MoveToGoal() { // Moves compass arrow to the angleGoal so there is a smooth on-screen movement
        float TimeDilation = GameObject.Find("Interactive Object").GetComponent<InteractiveComponents>().TimeDilation * 30;
        if (Mathf.Abs(anglePosition - (angleGoal + 360)) < Mathf.Abs(anglePosition - angleGoal)) {
            angleGoal += 360;
        } else if (Mathf.Abs(anglePosition - (angleGoal - 360)) < Mathf.Abs(anglePosition - angleGoal)) {
            angleGoal -= 360;
        }
        if (anglePosition > angleGoal) {
            GameObject.Find("Arrow").transform.Rotate(0, 0, -TimeDilation);
            anglePosition -= TimeDilation;
        } else {
            GameObject.Find("Arrow").transform.Rotate(0, 0, TimeDilation);
            anglePosition += TimeDilation;
        }
    }
}