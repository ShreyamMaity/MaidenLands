using UnityEngine;


[System.Serializable]
public class ThirdPersonCamera : CameraBase
{
    [System.Serializable]
    public class PositionSettings
    {
        public Vector3 positionOffset;
        public float moveSmooth = 30f;
        public float xAngleTilt = 3f;
        public float yAngleTilt = 1.5f;
    }
    public PositionSettings posSettings = new PositionSettings();
    Quaternion targetRotation;

    public override void Start()
    {
        base.Start();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void Update()
    {
        if (InputManager.reload_btn)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public override void LateUpdate()
    {
        UpdatePosition();
        UpdateRotation();
    }

    public override void UpdatePosition()
    {
        // calculate position.
        targetPos = target.position - (target.forward * posSettings.positionOffset.z);
        targetPos += (target.right * posSettings.positionOffset.x);
        targetPos += (target.up * posSettings.positionOffset.y);

        transform.position = targetPos;
    }

    public override void UpdateRotation()
    {
        Quaternion lookRotation = Quaternion.LookRotation((target.position) - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, posSettings.moveSmooth * Time.deltaTime);
    }
}
