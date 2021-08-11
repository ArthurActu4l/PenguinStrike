using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static bool isConnected = false;
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        GameManager.instance.Indicator("Connected To Server!");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        isConnected = true;
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void PlayerMovement(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        Quaternion _cameraRotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.position = Vector3.Lerp(GameManager.players[_id].transform.position, _position, 20f * Time.smoothDeltaTime);
        GameManager.players[_id].transform.rotation = Quaternion.Lerp(GameManager.players[_id].transform.rotation, _rotation, 20f * Time.smoothDeltaTime);
        GameManager.players[_id].playerCamera.localRotation = Quaternion.Lerp(GameManager.players[_id].playerCamera.localRotation, _cameraRotation, 20f * Time.smoothDeltaTime);

    }

    public static void SelectWeapon(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int selectedWeapon = _packet.ReadInt();

        GameManager.players[_id].weaponManager.SwitchWeapon(selectedWeapon);
    }

    public static void GetGunRotationAndPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int gunId = _packet.ReadInt();
        Vector3 position = _packet.ReadVector3();
        Quaternion rotation = _packet.ReadQuaternion();
        Quaternion boneRotation = _packet.ReadQuaternion();

        GameManager.players[_id].weaponManager.guns[gunId].transform.localPosition = position;
        GameManager.players[_id].weaponManager.guns[gunId].transform.localRotation = rotation;
        GameManager.players[_id].weaponManager.guns[gunId].bone.transform.localRotation = boneRotation;
    }

    public static void SendGunSounds(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int gunId = _packet.ReadInt();
        int soundEffectId = _packet.ReadInt();

        GameManager.players[_id].weaponManager.guns[gunId].InstantiateGunEffects(soundEffectId);
    }

    public static void GetBulletHitPoint(Packet _packet)
    {
        int id = _packet.ReadInt();
        int gunId = _packet.ReadInt();
        Vector3 bulletHitPoint = _packet.ReadVector3();
        float bulletForce = _packet.ReadFloat();
        Vector3 decalNormal = _packet.ReadVector3();

        GameManager.players[id].weaponManager.guns[gunId].InstantiateBullet(bulletHitPoint, bulletForce, decalNormal);
    }
}
