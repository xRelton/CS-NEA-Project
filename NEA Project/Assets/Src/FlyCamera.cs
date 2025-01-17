﻿using System;
using UnityEngine;

public class FlyCamera : MonoBehaviour { // Controls user camera movement and response to keyboard inputs
    float mainSpeed = 10.0f; // Regular speed
    float shiftAdd = 20.0f; // Multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; // Maximum speed when holdin gshift
    float[][] bounds = new float[][] {
        new float[] { -8.06f, 8.06f },
        new float[] { -3.92f, 3.92f },
        new float[] { -9.0f, -3.0f }
    };

    void Update() {
        if (GameObject.Find("Interactive Object").transform.GetChild(0).GetChild(1).gameObject.activeSelf == false) {
            Vector3 p = GetCameraControlInputs(); // Keyboard commands
            if (Input.GetKey(KeyCode.LeftAlt)) { // Speeds up camera movement when shift left alt is pressed
                p = p * shiftAdd;
                p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
                p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
                p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
            } else {
                p = p * mainSpeed;
            }
            Vector3 oldPosition = transform.position;
            transform.Translate(p * Time.deltaTime);
            float zoomAbs = Math.Abs(transform.position.z);
            bounds[0][1] = 12.065f - (0.05625f / zoomAbs) - (1.32875f * zoomAbs); // I used simultaneous equations on the bounds to calculate the values for the equation so the camera boundaries expand to match its zoom
            bounds[1][1] = 5.72f - (0.16875f / zoomAbs) - (0.58125f * zoomAbs);
            bounds[0][0] = -bounds[0][1];
            bounds[1][0] = -bounds[1][1];
            if (GetCurrentBound('x') != 'i') {
                SetTransform('x', oldPosition.x);
                if (transform.position.z < -3.02 && transform.position.z > -8.955) {
                    BoundCorrect('x', zoomAbs);
                }
            }
            if (GetCurrentBound('y') != 'i') {
                SetTransform('y', oldPosition.y);
                if (transform.position.z < -3.02 && transform.position.z > -8.98) {
                    BoundCorrect('y', zoomAbs);
                }
            }
            if (GetCurrentBound('z') != 'i') {
                SetTransform('z', oldPosition.z);
            }
        }
    }

    private char OutBounds(float val, float[] bound) { // Outputs if an inputted value is out of an array of 2 bounds
        if (val <= bound[0]) {
            return '<';
        } else if (val >= bound[1]) {
            return '>';
        }
        return 'i'; // Stands for "in bounds"
    }

    private void SetTransform(char choice, float val) { // Sets the camera position's x, y or z to inputted value
        if (choice == 'x') {
            transform.position = new Vector3(val, transform.position.y, transform.position.z);
        } else if (choice == 'y') {
            transform.position = new Vector3(transform.position.x, val, transform.position.z);
        } else if (choice == 'z') {
            transform.position = new Vector3(transform.position.x, transform.position.y, val);
        }
    }

    private void AddTransform(char choice, float val) { // Adds the inputted value to the camera position's x, y or z 
        if (choice == 'x') {
            transform.position += new Vector3(val, 0, 0);
        } else if (choice == 'y') {
            transform.position += new Vector3(0, val, 0);
        } else if (choice == 'z') {
            transform.position += new Vector3(0, 0, val);
        }
    }

    private char GetCurrentBound(char choice) { // Checks if the x, y or z is out of bounds
        if (choice == 'x') {
            return OutBounds(transform.position.x, bounds[0]);
        } else if (choice == 'y') {
            return OutBounds(transform.position.y, bounds[1]);
        } else if (choice == 'z') {
            return OutBounds(transform.position.z, bounds[2]);
        }
        return 'n'; // stands for "no correct choice given"
    }

    private void BoundCorrect(char choice, float zoomEffector) { // Moves camera position back into bounds if it is out
        char currentBound = GetCurrentBound(choice);
        if (choice == 'x') {
            zoomEffector /= 3;
        }
        if (currentBound == '<') {
            AddTransform(choice, (float)Math.Pow(1.29155f, -zoomEffector));
        } else if (currentBound == '>') {
            AddTransform(choice, -(float)Math.Pow(1.29155f, -zoomEffector));
        }
    }

    private Vector3 GetCameraControlInputs() { // Returns the basic values, if it's 0 then it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W)) {
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.S)) {
            p_Velocity += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.A)) {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            p_Velocity += new Vector3(1, 0, 0);
        }
        p_Velocity += new Vector3(0, 0, 100*Input.GetAxis("Mouse ScrollWheel"));
        return p_Velocity;
    }
}