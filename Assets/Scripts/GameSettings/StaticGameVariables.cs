using System.Net;
using System.Net.Sockets;
using SimpleFactoryServerLib.Server;
using UnityEngine;

public class StaticGameVariables : MonoBehaviour
{
    public static StaticGameVariables Instance;
    public static readonly float GRIDSIZE = 1;
    public static readonly float ASSEMBLY_LINE_SIZE = 1;
    public static bool INVENTORY_VISIBLE = false;

    private string _serverIpAdress;
    private IPEndPoint _serverEp;
    private Socket _socket;
    private Socket _udpClient;
    private byte[] _buffer;
    private Server _server;
    private IPEndPoint _ip;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if (Instance == null) {
            Instance = this;
        } else {
            DestroyObject(gameObject);
        }
    }

    public string ServerIpAdress
    {
        get => _serverIpAdress;
        set => _serverIpAdress = value;
    }

    public IPEndPoint ServerEp
    {
        get => _serverEp;
        set => _serverEp = value;
    }

    public Socket Socket
    {
        get => _socket;
        set => _socket = value;
    }

    public Socket UdpClient
    {
        get => _udpClient;
        set => _udpClient = value;
    }

    public byte[] Buffer
    {
        get => _buffer;
        set => _buffer = value;
    }

    public Server Server
    {
        get => _server;
        set => _server = value;
    }

    public IPEndPoint Ip
    {
        get => _ip;
        set => _ip = value;
    }
    
    
}