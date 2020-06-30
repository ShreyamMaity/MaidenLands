using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Most if not all objects will inherit from this class, mostly the scripts that need to
/// be attached to game objects.
/// </summary>
public abstract class Entity : MonoBehaviour
{
    // almost everything can have some stat attached to it even if it is an abstract thing e.g 
    // a weapon can have cooldown or warup before fire stat or enemy can have health stat and so on.
    // public List<Kryz.CharacterStats.StatBase> stats = new List<Kryz.CharacterStats.StatBase>();

    public abstract bool HandleMessage(Message.Telegram msg);
    public virtual void Die() { }
}

