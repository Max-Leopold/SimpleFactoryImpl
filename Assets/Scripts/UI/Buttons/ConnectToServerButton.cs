using System;
using System.Net;
using System.Net.Sockets;
using SimpleFactoryServerLib.Network.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectToServerButton : MonoBehaviour
{
    public InputField inputField;
    public Text infoText;
    public StaticGameVariables staticGameVariables;

    //private string serverIpAdress = "192.168.178.60";    // Needs to be the public ip of the host (router)


    private void Start()
    {
        string ip = PlayerPrefs.GetString("last-ip", "");
        inputField.text = ip;
        staticGameVariables = StaticGameVariables.Instance;
    }

    public void OnClickTryToConnect()
    {
        String ipAddress = inputField.text.Trim();
        if (string.IsNullOrEmpty(ipAddress))
        {
            infoText.text = "Can't connect to the Server";
            return;
        }
        
        try
        {
            staticGameVariables.ServerIpAdress = ipAddress;
            staticGameVariables.ServerEp = new IPEndPoint(IPAddress.Parse(ipAddress), Consts.PORT);
            staticGameVariables.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            staticGameVariables.UdpClient = new Socket(SocketType.Dgram, ProtocolType.Udp);
            
            staticGameVariables.UdpClient.Connect(staticGameVariables.ServerEp);
            Debug.Log("Local udp: " + staticGameVariables.UdpClient.LocalEndPoint);
            staticGameVariables.Ip = (IPEndPoint) staticGameVariables.UdpClient.LocalEndPoint;
            staticGameVariables.Socket.Connect(staticGameVariables.ServerEp);
            staticGameVariables.Socket.SendTimeout = 5000;
            // staticGameVariables.Socket.ReceiveTimeout = 5000;
            // staticGameVariables.Socket.ReceiveBufferSize = 16384;
            // staticGameVariables.Socket.SendBufferSize = 16384;
            // Send the Connection Message
            infoText.text = "Connected to Server";
            PlayerPrefs.SetString("last-ip", ipAddress);
            SceneManager.LoadScene(1);
        }
        catch (Exception e)
        {
            Debug.LogError("Network connect failed");
            Debug.LogError("Can't connect to " + ipAddress);
            Debug.LogError(e);
            infoText.text = "Can't connect to the Server";
        }
    }
}