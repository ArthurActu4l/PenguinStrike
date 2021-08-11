using UnityEngine;

public class WeaponSwitchinManager : MonoBehaviour
{
    public GunManager[] guns;

    private void Start()
    {
        guns = new GunManager[transform.childCount];

        for (int i = 0; i < guns.Length; i++)
        {
            guns[i] = transform.GetChild(i).GetComponent<GunManager>();
        }
    }
    public int selectedWeapon = 0;
    public void SwitchWeapon(int selectedweapon)
    {
        selectedWeapon = selectedweapon;
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }
}
