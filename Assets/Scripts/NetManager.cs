using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetManager : NetworkManager
{
    [SerializeField] private GameObject[] spawnPoints;
    private bool playerSpawned;
    private bool playerConnected;

    public void OnCreateCharacter(NetworkConnectionToClient conn, PosMessage message)
    {
        GameObject plGO = Instantiate(playerPrefab, message.vector2, Quaternion.identity); //�������� �� ������� ������� gameObject
        NetworkServer.AddPlayerForConnection(conn, plGO); //������������ gameObject � ���� ������� �������� � ���������� ���������� �� ���� ��������� �������
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); //���������, ����� struct ������ ������ �� ������, ����� ���������� �����
    }

    public void ActivatePlayerSpawn()
    {
        var randInd = Random.Range(0, spawnPoints.Length);
        Vector3 pos = spawnPoints[randInd].transform.position;
        PosMessage m = new PosMessage() { vector2 = pos }; //������� struct ������������� ����, ����� ������ ����� � ���� ��� ������ ���������
        NetworkClient.Send(m); //�������� ��������� �� ������ � ������������ ������
        playerSpawned = true;
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect(); // param "conn" was here. i do not know why//
        ActivatePlayerSpawn();
        playerConnected = true;
    }
}

public struct PosMessage : NetworkMessage //����������� �� ���������� NetworkMessage, ����� ������� ������ ����� ������ �����������
{
    public Vector2 vector2; //������ ������������ Property
}