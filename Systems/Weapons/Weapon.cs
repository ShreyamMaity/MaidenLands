using UnityEngine;


[System.Serializable]
public class CrossHair
{
    public float idleSize;
    public float walkSize;
    public float sizeChangeSpeed;

    RectTransform rectTransform; // The RecTransform of reticle UI element.
    float currentSize;
    bool isMoving;

    public void Init(RectTransform rect, Vector3 rectPos)
    {
        rectTransform = rect;
        // rectTransform.transform.localPosition = rectPos;
        isMoving = false;
    }

    public void Update(Vector3 newPos)
    {
        rectTransform.transform.localPosition = newPos;

        // Check if player is currently moving and Lerp currentSize to the appropriate value.
        if (isMoving)
        {
            currentSize = Mathf.Lerp(currentSize, walkSize, Time.deltaTime * sizeChangeSpeed);
        }
        else
        {
            currentSize = Mathf.Lerp(currentSize, idleSize, Time.deltaTime * sizeChangeSpeed);
        }

        // Set the reticle's size to the currentSize value.
        rectTransform.sizeDelta = new Vector2(currentSize, currentSize);
    }
}


[System.Serializable]
public class Weapon_Recoil_Script
{
    [Header("Recoil_Transform")]
    public Transform positionTranform;
    public Transform rotationTranform;

    [Space(10)]
    [Header("Recoil_Settings")]
    public float PositionDampTime;
    public float RotationDampTime;
    [Space(10)]
    public float Recoil1;
    public float Recoil2;
    public float Recoil3;
    public float Recoil4;
    [Space(10)]
    public Vector3 RecoilRotation;
    public Vector3 RecoilKickBack;

    public Vector3 RecoilRotation_Aim;
    public Vector3 RecoilKickBack_Aim;



    Vector3 currentRecoil1;
    Vector3 currentRecoil2;
    Vector3 currentRecoil3;
    Vector3 currentRecoil4;

    Vector3 rotationOutput;

    public bool aim;

    public void Update()
    {
        currentRecoil1 = Vector3.Lerp(currentRecoil1, Vector3.zero, Recoil1 * Time.deltaTime);
        currentRecoil2 = Vector3.Lerp(currentRecoil2, currentRecoil1, Recoil2 * Time.deltaTime);
        currentRecoil3 = Vector3.Lerp(currentRecoil3, Vector3.zero, Recoil3 * Time.deltaTime);
        currentRecoil4 = Vector3.Lerp(currentRecoil4, currentRecoil3, Recoil4 * Time.deltaTime);

        positionTranform.localPosition = Vector3.Slerp(positionTranform.localPosition, currentRecoil3, PositionDampTime * Time.fixedDeltaTime);
        rotationOutput = Vector3.Slerp(rotationOutput, currentRecoil1, RotationDampTime * Time.fixedDeltaTime);
        rotationTranform.localRotation = Quaternion.Euler(rotationOutput);
    }

    public void Fire()
    {
        if (aim == true)
        {
            currentRecoil1 += new Vector3(RecoilRotation_Aim.x, Random.Range(-RecoilRotation_Aim.y, RecoilRotation_Aim.y), Random.Range(-RecoilRotation_Aim.z, RecoilRotation_Aim.z));
            currentRecoil3 += new Vector3(Random.Range(-RecoilKickBack_Aim.x, RecoilKickBack_Aim.x), Random.Range(-RecoilKickBack_Aim.y, RecoilKickBack_Aim.y), RecoilKickBack_Aim.z);
        }
        if (aim == false)
        {
            currentRecoil1 += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
            currentRecoil3 += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x), Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);
        }
    }
}


[System.Serializable]
public class WeaponEffects
{
    [System.Serializable]
    public struct Shell
    {
        public Transform shell;
        public Transform shellSpawnPos;
    }

    public Shell shellSettings = new Shell();

    public void Init(Transform weapon)
    {
    }
}

[System.Serializable]
public struct Toggles
{
    public bool shouldRecoil;
    public bool effectsPlay;
    public bool playSounds;
    public bool lazer;
}

public class Weapon : MonoBehaviour
{
    [Header("General")]
    public HumanCharacter owner = null;
    public float delayBeforeFire = 0.0f;  // An optional delay that causes the weapon to fire a specified amount of time after it normally would (0 for no delay)
    public Vector3 weaponOrigonalPos = Vector3.zero;
    MuzzleFlash muzzleFlash;
    Camera playerCamera;

    public Toggles toggles = new Toggles();

    // Rate of Fire
    [Header("Fire Settings")]
    public float range = 9999.0f;   // How far this weapon can shoot (for raycast and beam)
    public float rateOfFire = 10;   // The number of rounds this weapon fires per second.
    private float actualROF;        // The frequency between shots based on the rateOfFire.( frquency is rate at which something occurs over particulr period of time )
    private float fireTimer;        // Timer used to fire at a set frequency.

    // Damage
    [Header("Attack Settings")]
    public float damagePerBullet = 10f;

    // Accuracy
    [Header("Accuracy")]
    [Range(0,100)] public short accuracy = 80;                   // How accurate this weapon is on a scale of 0 to 100
    [Range(0, 1.0f)] public float accuracyDropPerShot = 1.0f;    // How much the accuracy will decrease on each shot
    [Range(0, 1.0f)] public float accuracyRecoverRate = 0.1f;    // How quickly the accuracy recovers after each shot (value between 0 and 1)
    private float currentAccuracy;                               // Holds the current accuracy.  Used for varying accuracy based on speed, etc.

    // Ammunation
    [Header("Ammunation")]
    public bool infiniteAmmo;
    public int ammoCapacity;
    public int currentAmmo;
    public int shotPerRound = 1;  // The number of "bullets" that will be fired on each round.  Usually this will be 1, but set to a higher number for things like shotguns with spread

    [Header("Hud Settings")]
    public RectTransform crossHairRect;

    public Weapon_Recoil_Script weaponRecoil = new Weapon_Recoil_Script();
    public CrossHair crossHair = new CrossHair();
    public WeaponEffects effects = new WeaponEffects();

    void Start()
    {
        playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<Camera>();
        if(playerCamera == null) { Debug.LogError("Camera with tag player camera not found"); }

        muzzleFlash = GetComponent<MuzzleFlash>();

        // init all classes
        effects.Init(transform);

        // Calculate the actual ROF to be used in the weapon systems.  The rateOfFire variable is
        // designed to make it easier on the user - it represents the number of rounds to be fired
        // per second.  Here, an actual ROF decimal value is calculated that can be used with timers.
        // e.g if rateOfFire = 0.5 then num bullets shot per second = (1.0 / 0.5) = 2 bullets per second.
        if (rateOfFire != 0)
            actualROF = 1.0f / rateOfFire;
        else
            actualROF = 0.01f;

        // Make sure the fire timer starts at 0
        fireTimer = 0.0f;
    }

    void Update()
    {
        // suppose a weapon 
        // ROF = 500 rpm or 8.333.. or simply 8 bullets per second
        // magazine size = 30 rounds per magazine
        // reload time = 0.7s

        // damage per bullet = 4

        // damage = damage per bullet * bullets in magazine ( 4 * 30 = 120 )
        // so for health 100 = ( 100 / 4 = 25 ) bullets

        // Calculate the current accuracy for this weapon
        currentAccuracy = Mathf.Lerp(currentAccuracy, accuracy, accuracyRecoverRate * Time.deltaTime);

        // Update the fireTimer
        fireTimer += Time.deltaTime;

        if (InputManager.fire_btn && fireTimer >= actualROF)
        {
            CreateAttack();
        }

        // Recoil Recovery
        if (toggles.shouldRecoil)
        {
            weaponRecoil.Update();
        }
    }

    void OnDrawGizmos()
    {
    }

    void CreateAttack()
    {
        // Reset the fireTimer to 0 (for ROF)
        fireTimer = 0.0f;

        // First make sure there is ammo
        if (currentAmmo <= 0 && !infiniteAmmo)
        {
            DryFire();
            return;
        }

        // Subtract 1 from the current ammo
        if (!infiniteAmmo)
            currentAmmo--;

        for (int i = 0; i < shotPerRound; i++)
        {
            // Calculate accuracy for this shot
            float accuracyVary = (100 - currentAccuracy) / 1000;

            Vector3 direction = playerCamera.transform.forward;
            direction.x += Random.Range(-accuracyVary, accuracyVary);
            direction.y += Random.Range(-accuracyVary, accuracyVary);
            direction.z += Random.Range(-accuracyVary, accuracyVary);

            currentAccuracy -= accuracyDropPerShot;
            if (currentAccuracy <= 0.0f)
                currentAccuracy = 0.0f;

            // The ray that will be used for this shot
            var x = Screen.width / 2;
            var y = Screen.height / 2;
            var ray = playerCamera.ScreenPointToRay(new Vector3(x, y, 0));
            // Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameSettingsFile.humanCharacterMask))
            {
                HumanCharacter target = hit.collider.GetComponent<HumanCharacter>();
                Debug.Log(hit.transform.name);
            }
        }

        // Recoil 
        if (toggles.shouldRecoil)
        {
            weaponRecoil.Fire();
        }

        // firing sound
        if (toggles.playSounds)
        {
            AudioClip clip = owner.characterData.rifleSounds[UnityEngine.Random.Range(0, owner.characterData.rifleSounds.Count)];
            GetComponent<AudioSource>().PlayOneShot(clip);
        }

        // special effects
        if (toggles.effectsPlay)
        {
            Transform shell =
                Instantiate(effects.shellSettings.shell, effects.shellSettings.shellSpawnPos.position, effects.shellSettings.shellSpawnPos.rotation);
            muzzleFlash.Activate();
        }


    }

    void DryFire()
    {

    }
}
