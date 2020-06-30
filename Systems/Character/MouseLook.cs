using UnityEngine;


public class MouseLook : MonoBehaviour
{
    new public Camera camera = null;

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -60F;
    public float maximumX = 60F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public float offsetY = 0F;
    public float offsetX = 0F;

    public float smoothVel = 0.3f;

    public float rotationX = 0F;
    public float rotationY = 0F;

    public bool lockOnHorizontalInput = true;
    public bool lockOnIdle = true;

    Quaternion originalRotation;
    Quaternion xRotation, yRotation;
    Quaternion targetRotation;

    void Start()
    {
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
        originalRotation = transform.localRotation;

        xRotation = Quaternion.identity;
        yRotation = Quaternion.identity;
    }

    void Update()
    {
        // if (Cursor.lockState == CursorLockMode.None) return;

        if (camera == null) return;

        // sometimes when using this script with a character controller ,
        // you want to limit the turning e.g when character is already
        // moving horizontally or when it is idle 
        if (lockOnHorizontalInput && (InputManager.keyboard_axis.x > 0 || InputManager.keyboard_axis.x < 0)) return;
        if (lockOnIdle && InputManager.keyboard_axis.x == 0 && InputManager.keyboard_axis.y == 0) return;


        if (axes == RotationAxes.MouseXAndY)
        {
            // Read the mouse input axis
            rotationX += (Input.GetAxis("Mouse X") * sensitivityX / 30 * camera.fieldOfView + offsetX);
            rotationY += (Input.GetAxis("Mouse Y") * sensitivityY / 30 * camera.fieldOfView + offsetY);

            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            xRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            yRotation = Quaternion.AngleAxis(rotationY, Vector3.left);
        }
        else if (axes == RotationAxes.MouseX)
        {
            rotationX += (Input.GetAxis("Mouse X") * sensitivityX / 60 * camera.fieldOfView + offsetX);
            rotationX = ClampAngle(rotationX, minimumX, maximumX);

            xRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        }
        else
        {
            rotationY += (Input.GetAxis("Mouse Y") * sensitivityY / 60 * camera.fieldOfView + offsetY);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            yRotation = Quaternion.AngleAxis(rotationY, Vector3.left);
        }

        offsetY = 0F;
        offsetX = 0F;

        targetRotation = originalRotation * xRotation * yRotation;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, smoothVel);
    }

    // clamp angle to pi, -pi range
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}