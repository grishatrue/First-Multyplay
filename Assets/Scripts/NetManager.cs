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
        GameObject plGO = Instantiate(playerPrefab, message.vector2, Quaternion.identity); //локально на сервере создаем gameObject
        NetworkServer.AddPlayerForConnection(conn, plGO); //присоеднияем gameObject к пулу сетевых объектов и отправляем информацию об этом остальным игрокам
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); //указываем, какой struct должен прийти на сервер, чтобы выполнился свапн
    }

    public void ActivatePlayerSpawn()
    {
        var randInd = Random.Range(0, spawnPoints.Length);
        Vector3 pos = spawnPoints[randInd].transform.position;
        PosMessage m = new PosMessage() { vector2 = pos }; //создаем struct определенного типа, чтобы сервер понял к чему эти данные относятся
        NetworkClient.Send(m); //отправка сообщения на сервер с координатами спавна
        playerSpawned = true;
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect(); // param "conn" was here. i do not know why//
        ActivatePlayerSpawn();
        playerConnected = true;
    }
}

public struct PosMessage : NetworkMessage //наследуемся от интерфейса NetworkMessage, чтобы система поняла какие данные упаковывать
{
    public Vector2 vector2; //нельзя использовать Property
}