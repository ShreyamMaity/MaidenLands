using UnityEngine;


[System.Serializable]
public class InputManager : MonoBehaviour
{
    public string VERTICAL = "Vertical";
    public string HORIZONTAL = "Horizontal";
    public string MOUSE_X = "Mouse X";
    public string MOUSE_Y = "Mouse Y";
    public string ZOOM = "Mouse ScrollWheel";
    public string JUMP_AXIS = "Jump";

    public KeyCode FIRE_BUTTON = KeyCode.Mouse0;
    public KeyCode AIM_BUTTON = KeyCode.Mouse1;
    public KeyCode RELOAD_BUTTON = KeyCode.R;
    public KeyCode RUN_BUTTON = KeyCode.LeftShift;

    public static Vector2 keyboard_axis = new Vector2(0, 0);
    public static Vector2 mouse_axis = new Vector2(0, 0);

    public static float zoom = 0.0f, jump_axis;

    public static bool mouse0, mouse1, mouse2;
    public static bool reload_btn, fire_btn, aim_btn, run_btn;

    void Update()
    {
        GetInput();
    }

    void GetInput()
    {
        keyboard_axis.x = Input.GetAxis(HORIZONTAL);
        keyboard_axis.y = Input.GetAxis(VERTICAL);

        mouse_axis.x = Input.GetAxis(MOUSE_X);
        mouse_axis.y = Input.GetAxis(MOUSE_Y);

        mouse0 = Input.GetKey(FIRE_BUTTON);
        mouse1 = Input.GetKey(AIM_BUTTON);

        zoom = Input.GetAxisRaw(ZOOM);
        jump_axis = Input.GetAxisRaw(JUMP_AXIS);

        reload_btn = Input.GetKeyDown(RELOAD_BUTTON);
        fire_btn = Input.GetKey(FIRE_BUTTON);
        aim_btn = Input.GetKey(AIM_BUTTON);
        run_btn = Input.GetKey(RUN_BUTTON);
    }
}
