﻿using UnityEngine;
using UnityEngine.Networking;

public class CGSNetManager : NetworkManager
{
    public const string PlayerCountMessage = "Number of connected players: ";
    public const string HostIpMessage = "Host Ip: ";

    public static CGSNetManager Instance => (CGSNetManager)singleton;
    public CGSNetPlayer LocalPlayer { get; set; }
    public CGSNetData Data { get; set; }

    public GameObject cardModelPrefab;
    public PlayMode playController;
    public PointsCounter pointsDisplay;

    void Start()
    {
        customConfig = true;
        connectionConfig.NetworkDropThreshold = 90;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        CardGameManager.Instance.Discovery.StartAsHost();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
        if (Data == null) {
            Data = Instantiate(spawnPrefabs[0]).GetOrAddComponent<CGSNetData>();
            NetworkServer.Spawn(Data.gameObject);
        }
        Data.RegisterScore(conn.playerControllers[playerControllerId].gameObject, CardGameManager.Current.GameStartPointsCount);
        playController.netText.text = PlayerCountMessage + NetworkServer.connections.Count.ToString();
    }

    public override void OnStartClient(NetworkClient netClient)
    {
        base.OnStartClient(netClient);
        ClientScene.RegisterSpawnHandler(cardModelPrefab.GetComponent<NetworkIdentity>().assetId, SpawnCard, UnSpawnCard);
        playController.netText.text = HostIpMessage + netClient.serverIp;
    }

    public GameObject SpawnCard(Vector3 position, NetworkHash128 assetId)
    {
        GameObject newCardGO = Instantiate(cardModelPrefab, playController.playAreaContent);
        CardModel cardModel = newCardGO.GetComponent<CardModel>();
        cardModel.transform.localPosition = cardModel.LocalPosition;
        cardModel.transform.rotation = cardModel.Rotation;
        cardModel.HideHighlight();
        playController.SetPlayActions(playController.playAreaContent.GetComponent<CardStack>(), cardModel);
        return newCardGO;
    }

    public void UnSpawnCard(GameObject spawned)
    {
        CardModel cardModel = spawned?.GetComponent<CardModel>();
        if (cardModel != null && !cardModel.hasAuthority)
            Destroy(spawned);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (CardGameManager.Instance.Discovery.running)
            CardGameManager.Instance.Discovery.StopBroadcast();
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        //base.OnServerError(conn, errorCode);
        Debug.Log("Server error:" + errorCode);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //base.OnServerDisconnect(conn);
        Debug.Log("Player disconnected");
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        //base.OnClientError(conn, errorCode);
        Debug.Log("Client error:" + errorCode);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //base.OnClientDisconnect(conn);
        Debug.Log("Client Disconnected");
    }
}