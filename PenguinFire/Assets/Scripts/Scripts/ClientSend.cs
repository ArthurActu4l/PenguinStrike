using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    /// <summary>Lets the server know that the welcome message was received.</summary>
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    public static void PlayerMovement(Vector3 playerPosition, Quaternion playerRotation, Quaternion cameraRotation)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(playerPosition);
            _packet.Write(playerRotation);
            _packet.Write(cameraRotation);

            SendTCPData(_packet);
        }
    }
    
    public static void SendSelectedWeaponNumber(int selectedWeapon)
    {
        using (Packet _packet = new Packet((int)ClientPackets.weapons))
        {
            _packet.Write(selectedWeapon);
            SendTCPData(_packet);
        }
    }
    
    public static void SendGunPositionAndRotation(Vector3 gunPosition, Quaternion gunRotation, int gunId, Quaternion boneRotation)
    {
        using (Packet _packet = new Packet((int)ClientPackets.gunPositionAndRotation))
        {
            _packet.Write(gunId);
            _packet.Write(gunPosition);
            _packet.Write(gunRotation);
            _packet.Write(boneRotation);

            SendTCPData(_packet);
        }
    }
    
    public static void SendGunSoundEffects(int gunId, int soundEffectID)
    {
        using (Packet _packet = new Packet((int)ClientPackets.gunSounds))
        {
            _packet.Write(gunId);
            _packet.Write(soundEffectID);

            SendTCPData(_packet);
        }
    }
    
    
    public static void SendBulletHitPoint(Vector3 hitPoint, int gunId, float bulletForce, Vector3 decalNormal)
    {
        using (Packet _packet = new Packet((int)ClientPackets.gunBullets))
        {
            _packet.Write(gunId);
            _packet.Write(hitPoint);
            _packet.Write(bulletForce);
            _packet.Write(decalNormal);

            SendTCPData(_packet);
        }
    }
    #endregion
}
