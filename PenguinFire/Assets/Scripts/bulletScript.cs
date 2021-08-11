using UnityEngine;

public class bulletScript : MonoBehaviour
{
    [HideInInspector]
    public GameObject bulletHoleDecal;
    [HideInInspector]
    public Vector3 decalPosition;
    [HideInInspector]
    public Quaternion decalRotation;
    private int collisionCount;
    public float destroyTime;
    public GameObject bulletFlyBy;

    private void OnCollisionEnter(Collision collision)
    {
        collisionCount++;
        if(collisionCount <= 1)
        {
            Instantiate(bulletHoleDecal, decalPosition, decalRotation);
        }
        Destroy(gameObject, destroyTime);
    }
}
