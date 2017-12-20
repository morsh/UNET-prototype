using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Client : MonoBehaviour {

    private const int MAX_CONNECTIONS = 100;

    private int port = 5701;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private int selfClientId = -1;
    private int connectionId;

    private float connectionTime;
    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string playerName;

    public void Connect()
    {
        // Does the player has a name
        string playerNameInput = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (playerNameInput == "")
        {
            Debug.Log("You must enter a name");
            return;
        }

        playerName = playerNameInput;

        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();

        // Client and server need to have the same connection types
        reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
        unreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(connectionConfig, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, 0);
        connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", port, 0, out error);

        connectionTime = Time.time;
        isConnected = true;
    }

    private void Update()
    {
        if (!isConnected) { return; }

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:       //3
                string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving : " + message);

                var parts = message.Split('|');

                switch (parts.Length > 0 ? parts[0] : "")
                {
                    case "ASKNAME":
                        OnAskName(parts);
                        break;

                    case "UPD":
                        break;

                    case "DC":
                        break;

                    default:
                        Debug.Log("Invalid message : " + message);
                        break;
                }
                break;
        }
    }

    private void OnAskName(string[] data)
    {
        // Set self client's id
        selfClientId = int.Parse(data[1]);

        // Send self name to server
        Send("NAMEIS|" + playerName, reliableChannel);

        // Create all the other players
        for (int i = 2; i < data.Length - 1; i++)
        {
            string otherClientId = data[i].Substring(0, data[i].IndexOf('%'));
            string otherClientName = data[i].Substring(data[i].IndexOf('%') + 1);
            SpawnPlayer(otherClientId, otherClientName);
        }
    }

    private void Send(string message, int channelId)
    {
        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);

    }

    private void SpawnPlayer(string id, string name)
    {

    }
}
