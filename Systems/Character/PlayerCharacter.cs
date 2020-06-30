using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class PlayerUIHud
{
    [System.Serializable]
    public struct UI_Texts
    {
        public Text currentHealth;
        public Text currentArmor;
        public Text maxBullets;
        public Text bulletsInMag;
        public Text currentBudget;
    }


    public UI_Texts uiTexts = new UI_Texts();
    public Transform crossHair;
    public Transform pickItemText;

    [HideInInspector]
    public bool onItem = false; // when play is standing very close to an item
                                // this is set to true

    public void Start()
    {
        crossHair.gameObject.SetActive(true);
        pickItemText.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (onItem)
        {
            crossHair.gameObject.SetActive(false);
            pickItemText.gameObject.SetActive(true);
            // if (Input.GetKeyDown(KeyCode.E)) {  }
        }
        else
        {
            crossHair.gameObject.SetActive(true);
            pickItemText.gameObject.SetActive(false);
        }
    }
}


public class PlayerCharacter : HumanCharacter
{
    public PlayerUIHud playerHud = new PlayerUIHud();
    public WeaponHandler weaponsHandler = new WeaponHandler();

    public override void Start()
    {
        base.Start();
        playerHud.Start();
        weaponsHandler.SwitchWeapon(weaponsHandler.weapons[1]);

        // update player HUD
        playerHud.uiTexts.maxBullets.text = weaponsHandler.currentWeapon.ammoCapacity.ToString();
        playerHud.uiTexts.bulletsInMag.text = weaponsHandler.currentWeapon.currentAmmo.ToString();
    }

    public void LateUpdate()
    {
        playerHud.Update();
        weaponsHandler.Update();

        // update player HUD
        playerHud.uiTexts.maxBullets.text = weaponsHandler.currentWeapon.ammoCapacity.ToString();
        playerHud.uiTexts.bulletsInMag.text = weaponsHandler.currentWeapon.currentAmmo.ToString();
    }

    public override void OnAttack()
    {
    }

    public override void CreateAttack(HumanCharacter other)
    {
    }
}