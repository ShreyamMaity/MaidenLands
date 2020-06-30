using System;
using Message;
using UnityEngine;


public class HumanCharacter : Entity
{
    public float maxHealth;
    public float currentHealth;

    [HideInInspector]
    public CharacterData characterData;

    [HideInInspector]
    public AudioSource audioSource;

    public virtual void Start()
    {
        characterData = GameObject.FindGameObjectWithTag("DataHolder").GetComponent<GameData>().characterData;
        audioSource = GetComponent<AudioSource>();
    }

    public override bool HandleMessage(Telegram msg)
    {
        throw new NotImplementedException();
    }

    public virtual void OnAttack() { }

    public virtual void CreateAttack(HumanCharacter other) { }
}
