using TMPro;
using UnityEngine;
public class NameTagScript : MonoBehaviour
{
    public PlayerManager player;
    public TextMeshPro text;
    private void Start()
    {
        text.text = player.username;
    }

    private void Update()
    {
        transform.LookAt(RigidBodyPlayerMovement.instance.transform);
    }
}
