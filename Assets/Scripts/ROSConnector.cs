using System;
using System.Net.Sockets;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;
using RosMessageTypes.Moveit;
using UnityEngine.UI;
using TMPro;

public class ROSConnector : MonoBehaviour
{
    // ------------ TCP Connection ------------
    [SerializeField] string serverIP = "127.0.0.1"; // Qtアプリケーションが動作しているIPアドレス
    [SerializeField] int serverPort = 10000;        // Qtアプリケーションのポート番号
    private TcpClient client;
    private NetworkStream stream;

    // ------------ ROS Message ------------
    private MessageDeserializer messageDeserializer;
    private MessageSerializer messageSerializer;
    private JointStateMsg jointStateMsg;
    private RobotTrajectoryMsg robotTrajectoryMsg;

    // ------------ Event ------------
    public delegate void GetRobotTrajectoryMsgDelegate(RobotTrajectoryMsg robotTrajectoryMsg);
    public static event GetRobotTrajectoryMsgDelegate onRobotTrajectoryMsgReceived;

    // ------------ UI ------------
    [SerializeField] TextMeshProUGUI connectionText;

    void Start()
    {
        JointStatePublisher.OnJointStateUpdated += Publish;

        ConnectToServer().Forget();
    }

    void Update()
    {
        if(client.Connected)
        {
            connectionText.text = "TCP : <color=green>Connected!</color>";
        }
        else
        {
            connectionText.text = "TCP : <color=red>Disconnected...</color>";
            Reconnect();
        }
    }

    public void Reconnect()
    {
        if(!client.Connected)
        {
            ConnectToServer().Forget();
        }
    }

    async UniTaskVoid ConnectToServer()
    {
        client = new TcpClient();
        messageDeserializer = new MessageDeserializer();

        try
        {
            await client.ConnectAsync(serverIP, serverPort);
        }
        catch(Exception ex)
        {
            //Debug.Log(ex.Message + "\nMake sure you are running the Qt application");
            return;
        }

        if (client.Connected)
        {
            Debug.Log("Connected to server!");
            stream = client.GetStream();
            ReceiveData().Forget();
        }
        else
        {
            Debug.Log("Failed to connect to server.");
        }

        return;
    }

    async UniTaskVoid ReceiveData()
    {
        byte[] buffer = new byte[1024];

        // 接続している間、受信し続けたい
        // await を使用しているため、無限ループには陥らない
        while (client.Connected)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
            	Debug.Log(bytesRead);
            	
                messageDeserializer.InitWithBuffer(buffer);

                robotTrajectoryMsg = RobotTrajectoryMsg.Deserialize(messageDeserializer);
                
                Debug.Log(robotTrajectoryMsg.ToString());
                
                onRobotTrajectoryMsgReceived?.Invoke(robotTrajectoryMsg);
            }
        }
    }

    void Publish(JointStateMsg jointStateMsg)
    {
        if(!client.Connected) return;

        messageSerializer = new MessageSerializer();

        messageSerializer.SerializeMessage(jointStateMsg);

        stream.Write(messageSerializer.GetBytes());
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        if(client.Connected) client.Close();
    }
}

