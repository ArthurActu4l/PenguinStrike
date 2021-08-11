using UnityEngine;

public class playerModelScript : MonoBehaviour
{
    public Transform orientation;
    public Transform player;
    void Update()
    {
        transform.position = player.position;
        transform.rotation = orientation.rotation;
    }
}
