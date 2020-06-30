using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WeaponHandler
{
    public int maxWeaponsCount = 4;
    public float weaponSwitchTime = 0;
    public GameObject parent; // weapon will spawn under agent
    public Weapon currentWeapon = null;

    public Weapon[] weapons = new Weapon[4];

    int currentWeaponIndex;
    int currentWeaponsCount;
    float currentTime = 0;

    public void Start()
    {
    }

    public void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            NextWeapon();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            PreviousWeapon();
        }
    }


    public void AddWeapon(Weapon weapon)
    {
        if (currentWeaponsCount < maxWeaponsCount)
        {
            weapons[currentWeaponIndex] = weapon;
            currentWeaponsCount++;
        }
    }

    public void SwitchWeapon(Weapon newWeapon)
    {
        if(currentWeapon != newWeapon || !currentWeapon.gameObject.activeSelf)
        {
            currentWeapon.gameObject.SetActive(false);
            currentWeapon = newWeapon;
            currentWeapon.gameObject.SetActive(true);
        }
    }

    public void NextWeapon()
    {
        currentWeaponIndex++;
        if (currentWeaponIndex > maxWeaponsCount-1) currentWeaponIndex = 0;
        Weapon newWeapon = weapons[currentWeaponIndex];
        SwitchWeapon(newWeapon);
    }

    public void PreviousWeapon()
    {
        currentWeaponIndex--;
        if (currentWeaponIndex < 0) currentWeaponIndex = maxWeaponsCount-1;
        Weapon newWeapon = weapons[currentWeaponIndex];
        SwitchWeapon(newWeapon);
    }
}
