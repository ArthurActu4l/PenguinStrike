using UnityEngine;

public class GunManager : MonoBehaviour
{
    public Transform bone;
    public GameObject[] audioPrefab;
    public GameObject bulletPrefab;
    public GameObject bulletShell;
    public Transform shellPos;
    public Transform bulletPosition;
    public GameObject[] bulletholeDecal;

    public void InstantiateGunEffects(int soundEffectID)
    {
        GameObject instantiatedPrefab = Instantiate(audioPrefab[soundEffectID], transform.position, Quaternion.identity, transform.parent.parent) as GameObject;
        instantiatedPrefab.GetComponent<AudioSource>().spatialBlend = 1f;
        instantiatedPrefab.GetComponent<AudioSource>().PlayDelayed(0f);
    }

    public void InstantiateBullet(Vector3 bullletHitPoint, float bulletForce, Vector3 decalNormal)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletPosition.position, Quaternion.identity) as GameObject;
        bullet.transform.LookAt(bullletHitPoint);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletForce);

        bulletScript bulletScript = bullet.GetComponent<bulletScript>();
        bulletScript.decalPosition = bullletHitPoint;
        bulletScript.decalRotation = Quaternion.LookRotation(decalNormal);
        bulletScript.bulletHoleDecal = bulletholeDecal[Random.Range(0, bulletholeDecal.Length)];
        InstantiateShells();
    }

    private void InstantiateShells()
    {
        GameObject shell = Instantiate(bulletShell, shellPos.position, shellPos.rotation) as GameObject;
        shell.GetComponent<Rigidbody>().AddForce(shellPos.right * 10f);
        shell.GetComponent<Rigidbody>().AddTorque(shellPos.up * 5000f);
    }
}
