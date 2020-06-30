using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsFile : MonoBehaviour
{
    public string humanCharacterLayer = "";

    public static LayerMask humanCharacterMask;

    void Awake()
    {
        humanCharacterMask = LayerMask.GetMask(humanCharacterLayer);
    }
}
