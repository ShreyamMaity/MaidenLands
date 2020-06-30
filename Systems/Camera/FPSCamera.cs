using UnityEngine;


/// <summary>
/// This is just a variation of FixedPosCamera which is more useful for a third person camera context,
/// this script handles First person style camera.
/// </summary>
public class FPSCamera : CameraBase
{

    [System.Serializable]
    public class PositionSettings
    {
        public Vector3 positionOffset = Vector3.zero;
    }

    public PositionSettings posSettings = new PositionSettings();
    public Transform xRotationTarget;
    public Transform yRotationTarget;
    Quaternion targetRotation;

    public override void Start()
    {
    }

    public override void LateUpdate()
    {
        UpdatePosition();
        UpdateRotation();
    }

    public override void UpdatePosition()
    {
        // in fps camera camera remains at fix position, 
        transform.position = target.position + posSettings.positionOffset; 
    }

    public override void UpdateRotation()
    {
        targetRotation = Quaternion.Euler(xRotationTarget.localEulerAngles.x, yRotationTarget.localEulerAngles.y, 0);
        transform.rotation = targetRotation;
    }
}
