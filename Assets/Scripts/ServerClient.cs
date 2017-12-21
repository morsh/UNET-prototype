using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ServerClient
{
    public int connectionId;
    public string playerName;
    public Vector3 position;

    public string ToStateString()
    {
        return connectionId.ToString() + "%" + position.x.ToString() + "%" + position.y.ToString() + "%" + position.z.ToString();
    }

    static public ServerClient LoadPosition(string state)
    {
        var data = state.Split('%');
        return new ServerClient
        {
            connectionId = int.Parse(data[0]),
            position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]))
        };
    }
}