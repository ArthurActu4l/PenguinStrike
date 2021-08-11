using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public float smoothTime = 10f;
    public Transform mainCamera;
    public GunScript[] gun;
    public static WeaponHolder instance;
    public int selectedWeapon = 0;
    public int lastWeapon;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        gun = new GunScript[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            gun[i] = transform.GetChild(i).gameObject.GetComponent<GunScript>();
        }
        SelectWeapon();
    }

    private void Update()
    {
        int previousSelectedWeapon = selectedWeapon;
        //transform.position = mainCamera.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, mainCamera.rotation, smoothTime * Time.smoothDeltaTime);
        //transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, 0.86f, 0f), 20f * Time.smoothDeltaTime);

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedWeapon = lastWeapon;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedWeapon = 2;
        }
        if (previousSelectedWeapon != selectedWeapon)
        {
            for (int i = 0; i < gun.Length; i++)
            {
                if(gun[i].isShooting)
                return;
            }
            SelectWeapon();
        }
    }
    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
                //weapon.GetComponent<Animator>().enabled = true;
            }
            else
            {
                //weapon.GetComponent<Animator>().enabled = false;
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
        //send the wepon number from the weaponholder class
        ClientSend.SendSelectedWeaponNumber(selectedWeapon);
    }
}

