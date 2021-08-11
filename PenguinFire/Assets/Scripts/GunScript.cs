using EZCameraShake;
using UnityEngine;

public class GunScript : MonoBehaviour
{
    public Transform mainCamera;
    private Animator gunAnimator;
    private int currentAmmo;
    private bool isReloading = false;
    private Vector3 offset;
    public Transform bone;
    [HideInInspector]
    public bool isShooting = false;

    [Header("Different for each Gun")]
    public Transform shellPos;
    public Transform bulletPos;
    public int maxAmmo = 8;
    public GameObject buletPrefab;
    public float recoil = 5f;
    public float recoilRoughness = 10f;
    public float recoilSmoothness = 30f;
    public bool auto = false, canChangeFireMode = false;
    [Header("Number of bullets fired per shot")]
    public int shotCount = 5;
    public float bulletForce = 10f;
    public GameObject[] bulletholeDecal;
    public GameObject shootSoundEffect, bulletShell, reloadSoundEffect;

    [Header("Procedural Animations Recoil")]
    public float knockBack = 0.4f;
    public float SmoothTime = 5f;
    public float repositioningSmoothTime = 1f;
    public float knockBackRotation = -5f;
    public float RotationSmoothTime = 5f;
    public float angularRepositionnigSmoothTime= 1f;

    [Header("Bullet spread in various situations")]
    public float inAirBulletSpreadFactor = 5f; 
    public float movingBulletSpread = 2f;
    public float refBulletSpread = 0.01f;
    public float BulletSpread = 0.05f;
    public float crouchingBulletSpreadFactor = 0.04f;


    [Header("Default location of the gun")]
    public Vector3 defaultLocation;
    RaycastHit[] hit;

    [Header("Debug")]
    public float reticleMultiplier = 100f;

    private void Awake()
    {
        hit = new RaycastHit[shotCount];
    }

    void Start()
    {
        gunAnimator = GetComponent<Animator>();
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        if (GameManager.instance.gameIsPaused) return;
        SendPositionsAndRotationsToServer();
        CrosshairAndRecoilStuff();
        ReloadingAndShooting();
        ChangeFireMode();
    }
    void SendPositionsAndRotationsToServer()
    {
        ClientSend.SendGunPositionAndRotation(transform.localPosition, transform.localRotation, transform.GetSiblingIndex(), bone.localRotation);
    }
    void ChangeFireMode()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (canChangeFireMode)
            {
                if (auto)
                {
                    GameManager.instance.Indicator("Fire Mode Changed To Single");
                }
                else
                {
                    GameManager.instance.Indicator("Fire Mode Changed To Auto");
                }
                auto = !auto;
            }
            else
            {
                GameManager.instance.Indicator("Cannot Change Fire Mode For Current Weapon");
            }
        }
    } 

    void CrosshairAndRecoilStuff()
    {
        Crosshair.instance.spreadValue = refBulletSpread * reticleMultiplier;
        transform.localPosition = Vector3.Lerp(transform.localPosition, defaultLocation, repositioningSmoothTime * Time.smoothDeltaTime);
        Quaternion lookRotation = Quaternion.Inverse(transform.parent.rotation) * Quaternion.LookRotation(RigidBodyPlayerMovement.instance.gunLookAt.position - transform.position);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, lookRotation, angularRepositionnigSmoothTime * Time.smoothDeltaTime);
        GameManager.instance.ammoText.text = currentAmmo.ToString("0") + "/" + maxAmmo.ToString("0");
    }

    void ReloadingAndShooting()
    {
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
        {
            isReloading = true;
            Reload();
            return;
        }

        if (currentAmmo <= 0 && !isReloading)
        {
            isReloading = true;
            Reload();
            return;
        }
        
        if (!isReloading && !isShooting)
        {
            if (!auto)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    gunAnimator.SetBool("Shoot", true);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    gunAnimator.SetBool("Shoot", true);
                }
            }
        }
    }

    public void Shoot()
    {
        isShooting = true;
        gunAnimator.SetBool("Shoot", false);
        for (int i = 0; i < shotCount; i++)
        {
            offset = new Vector3(Random.Range(-refBulletSpread, refBulletSpread), Random.Range(-refBulletSpread, refBulletSpread), 0f);
            Physics.Raycast(mainCamera.position, mainCamera.forward + offset, out hit[i]);
            GameObject bullet = Instantiate(buletPrefab, bulletPos.position, Quaternion.identity);
            bullet.transform.LookAt(hit[i].point);
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletForce);

            //send bulet to server
            ClientSend.SendBulletHitPoint(hit[i].point, transform.GetSiblingIndex(), bulletForce, hit[i].normal);

            bulletScript bulletScript = bullet.GetComponent<bulletScript>();
            bulletScript.decalPosition = hit[i].point;
            bulletScript.decalRotation = Quaternion.LookRotation(hit[i].normal);
            bulletScript.bulletHoleDecal = bulletholeDecal[Random.Range(0, bulletholeDecal.Length)];
            Debug.DrawLine(mainCamera.position, hit[i].point, Color.red);
        }
        RigidBodyPlayerMovement.instance.recoilCamera.localRotation = Quaternion.Lerp(RigidBodyPlayerMovement.instance.recoilCamera.localRotation, Quaternion.Euler(new Vector3(recoil, 0f, 0f)), recoilRoughness * Time.smoothDeltaTime);
        RigidBodyPlayerMovement.instance.recoilRoughness = recoilSmoothness;
        currentAmmo--;
        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(defaultLocation.x, defaultLocation.y, defaultLocation.z - knockBack), SmoothTime * Time.smoothDeltaTime);
        Quaternion lookRotation = Quaternion.Inverse(transform.parent.rotation) * Quaternion.LookRotation(RigidBodyPlayerMovement.instance.gunLookAt.position - transform.position);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(lookRotation.eulerAngles.x + knockBackRotation, lookRotation.eulerAngles.x, lookRotation.eulerAngles.x), RotationSmoothTime * Time.smoothDeltaTime);
        GameObject shootSound = Instantiate(shootSoundEffect, transform.position, Quaternion.identity, RigidBodyPlayerMovement.instance.transform);
        shootSound.GetComponent<AudioSource>().PlayDelayed(0f);
        CameraShaker.Instance.ShakeOnce(2f, 4f, 0.1f, 0.5f);
        ClientSend.SendGunSoundEffects(transform.GetSiblingIndex(), 0);
    }

    public void SetAmmo()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    public void Reload()
    {
        gunAnimator.SetTrigger("Reload");
    }

    public void InstantiateShells()
    {
        GameObject shell = Instantiate(bulletShell, shellPos.position, shellPos.rotation) as GameObject;
        shell.GetComponent<Rigidbody>().AddForce(shellPos.right * 10f);
        shell.GetComponent<Rigidbody>().AddTorque(shellPos.up * 5000f);
    }

    public void IncreaseBulletSpread(float factor) 
    {
        refBulletSpread = BulletSpread * factor;
    }

    public void ReloadSoundEffect()
    {
        Instantiate(reloadSoundEffect, transform.position, Quaternion.identity, RigidBodyPlayerMovement.instance.transform);
        ClientSend.SendGunSoundEffects(transform.GetSiblingIndex(), 1);
    }

    public void IsShooting()
    {
        isShooting = false;
    }

    private void OnDisable()
    {
        WeaponHolder.instance.lastWeapon = transform.GetSiblingIndex();
    }
    private void OnEnable()
    {
        isReloading = false;
        isShooting = false;
    }
}
