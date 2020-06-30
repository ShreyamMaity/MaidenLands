using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    public List<AudioClip> rifleSounds = new List<AudioClip>();
}
