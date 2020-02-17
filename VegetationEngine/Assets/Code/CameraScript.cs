using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

// Need this struct to communicate with SetCursorPos and GetCursorPos
public struct POINT {
    public int X;
    public int Y;
}

public class CameraScript : MonoBehaviour {
    public float zoomSensitivity = 50.0f;
    public float moveSensitivity = 50.0f;
    public float rotationSensitivity = 50.0f;


    int lastMouseX;
    int lastMouseY;
    Terrain worldTerrain;

    // Import the Windows cursor position getter/setter (from Windows dll's)
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0.0f && !Input.GetKey(KeyCode.LeftAlt)) {
            transform.localPosition += new Vector3(0.0f, 0.0f, 1.0f) * zoomSensitivity* mouseScroll *Time.deltaTime;
        }

        // Grab the player input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // If there is horizontal movement input, process it
        // Time.deltaTime is the amount of time that's hpassed between frames
        // Always multiple any movement or multi-frame operation by DeltaTime so as not to end up with Skyrim physics
        // The camera itself is attached to an object sitting at y = 0
        // transform.parent grabs that object - moving that instead of the camera itself makes things easier for RTS controls
        if (horizontal != 0.0f)
            transform.parent.position += transform.right * moveSensitivity * horizontal * Time.deltaTime;

        // If there is vertical movement input, process it
        // As this is an RTS camera, "forward" movement means moving "up" the screen,
        // i.e. don't move in the direction you're looking (into the ground)
        // and don't move just up the x- or y-axis (if the player has rotated, you want to move in that direction)
        // To do this, we do a cross-product of the camera's right-hand direction (as it's relevant to which way the camera itself is facing), and the "up" vector (x = 0, y = 1, z = 0)
        if (vertical != 0.0f)
            transform.parent.position += Vector3.Cross(transform.right, new Vector3(0.0f, 1.0f, 0.0f)) * moveSensitivity * vertical * Time.deltaTime;

        // Hard-coded input keys, BAD. But it'll do for testing purposes
        // in short, if the player is not rotating (hoilding down middle mouse button), store their mouse position
        // If they are rotating, rotate the camera and then reset their mouse position to the last one before they started rotating
        // (otherwise, they'll end up moving the mouse to the edge of the screen and stop rotation, AND it'll change the mouse position during rotation)
        if (Input.GetMouseButton(2)) {
            float deltaMouseX, deltaMouseY;
            POINT point;

            // Grab the current mouse position
            GetCursorPos(out point);
            deltaMouseX = (((float)point.Y - (float)lastMouseY) / 1920.0f);
            deltaMouseY = (((float)point.X - (float)lastMouseX) / 1080.0f);

            // Rotation has to be done in the right order - rotating around one axis will change how the object then rotates around another axis
            // Therefore, if we keep rotating around X and Y, it will make the camera go sideways eventually.
            // Thus, what we do instead is grab the current rotation as individual angles (x and y), add the new angles, and then rotate from scratch (from 0,0,0)
            float oldRotationX = transform.parent.rotation.eulerAngles.x;
            float oldRotationY = transform.parent.rotation.eulerAngles.y;

            float newRotationX = oldRotationX + deltaMouseX * rotationSensitivity;
            float newRotationY = oldRotationY + deltaMouseY * rotationSensitivity;

            // Make sure the player can't rotate too low or too high - clamp the x-axis rotation
            newRotationX = Mathf.Clamp(newRotationX, 10.0f, 75.0f);

            // As mentioned, reset the rotation, and then rotate the parent it from scratch
            transform.parent.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            transform.parent.Rotate(new Vector3(newRotationX, newRotationY, 0.0f));

            // Reset mouse position
            SetCursorPos(lastMouseX, lastMouseY);
        }
        else {
            POINT point;
            GetCursorPos(out point);
            lastMouseX = point.X;
            lastMouseY = point.Y;
        }
    }
}
