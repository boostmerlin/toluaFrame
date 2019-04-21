using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using LuaFramework;

public enum DisType {
    Exception,
    Disconnect,
}

public class SocketClient {
    private TcpClient client = null;
    private NetworkStream outStream = null;
    private MemoryStream memStream;
    private BinaryReader reader;

    private const int MAX_READ = 1024*128;
    private byte[] byteBuffer = new byte[MAX_READ];
    public static bool loggedIn = false;

    /// <summary>
    /// 注册代理
    /// </summary>
    public void OnRegister() {
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);
    }

    /// <summary>
    /// 移除代理
    /// </summary>
    public void OnRemove() {
        this.Close();
        reader.Close();
        memStream.Close();
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    void ConnectServer(string host, int port) {
        client = null;
        try {
            IPAddress[] address = Dns.GetHostAddresses(host);
            if (address.Length == 0) {
                Debug.LogError("host invalid");
                return;
            }
            if (address[0].AddressFamily == AddressFamily.InterNetworkV6) {
                client = new TcpClient(AddressFamily.InterNetworkV6);
            }
            else {
                client = new TcpClient(AddressFamily.InterNetwork);
            }
            client.SendTimeout = 1000;
            client.ReceiveTimeout = 1000;
            client.NoDelay = true;
            client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
        } catch (Exception e) {
            Close(); Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 连接上服务器
    /// </summary>
    void OnConnect(IAsyncResult asr) {
        if (!client.Connected)
        {
            OnDisconnected(DisType.Exception, "can't connect to server.");
            return;
        }

        outStream = client.GetStream();
        outStream.BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
        NetworkManager.AddEvent(Protocal.Connect, new ByteBuffer());
    }

    /// <summary>
    /// 写数据
    /// </summary>
    void WriteMessage(byte[] message) {
        {
            //ms.Position = 0;
            //BinaryWriter writer = new BinaryWriter(ms);
            //writer.Write(message);
            //writer.Flush();
            if (client != null && client.Connected) {
                //NetworkStream stream = client.GetStream();
                byte[] payload = message;
                outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
            } else {
                OnDisconnected(DisType.Disconnect, "Write bytes error for connected: false");
            }
        }
    }

    /// <summary>
    /// 读取消息
    /// </summary>
    void OnRead(IAsyncResult asr) {
        if(client == null)
        {
            Debug.LogWarning("OnRead client is null ?");
            return;
        }
        int bytesRead = 0;
        try {
            lock (client.GetStream()) {         //读取字节流到缓冲区
                bytesRead = client.GetStream().EndRead(asr);
            }
            if (bytesRead < 1) {                //包尺寸有问题，断线处理
                OnDisconnected(DisType.Disconnect, string.Format("Bytes read: {0}", bytesRead));
                return;
            }
            OnReceive(byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
            lock (client.GetStream()) {         //分析完，再次监听服务器发过来的新消息
                Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
                client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
            }
        } catch (Exception ex) {
            OnDisconnected(DisType.Exception, ex.Message);
        }
    }

    public void Disconnect()
    {
        this.OnDisconnected(DisType.Disconnect, "Manual disconnect..");
    }

    /// <summary>
    /// 丢失链接
    /// </summary>
    void OnDisconnected(DisType dis, string msg) {
        Close();   //关掉客户端链接
        int protocal = dis == DisType.Exception ?
        Protocal.Exception : Protocal.Disconnect;

        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteShort((ushort)protocal);
        NetworkManager.AddEvent(protocal, buffer);
        Debug.LogError("Connection was closed for: " + msg + " Distype:" + dis);
    }

    /// <summary>
    /// 打印字节
    /// </summary>
    /// <param name="bytes"></param>
    void PrintBytes() {
        string returnStr = string.Empty;
        for (int i = 0; i < byteBuffer.Length; i++) {
            returnStr += byteBuffer[i].ToString("X2");
        }
        Debug.LogError(returnStr);
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    void OnWrite(IAsyncResult r) {
        try {
            outStream.EndWrite(r);
        } catch (Exception ex) {
            OnDisconnected(DisType.Exception, "OnWrite--->>>" + ex.ToString());
            Debug.LogError("OnWrite--->>>" + ex.Message);
        }
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    void OnReceive(byte[] bytes, int length) {
        memStream.Seek(0, SeekOrigin.End);
        memStream.Write(bytes, 0, length);
        //Reset to beginning
        memStream.Seek(0, SeekOrigin.Begin);
        while (RemainingBytes() > 6) {
            int messageLen = reader.ReadInt32();
            messageLen = IPAddress.NetworkToHostOrder(messageLen);
            if (RemainingBytes() >= messageLen) {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                var tag = reader.ReadBytes(2);

                //check tag.
                writer.Write(reader.ReadBytes(messageLen-2));
                //ms.Seek(0, SeekOrigin.Begin);
                OnReceivedMessage(ms);
            } else {
                //Back up the position two bytes
                memStream.Position = memStream.Position - 4;
                break;
            }
        }
        //Create a new stream with any leftover bytes
        byte[] leftover = reader.ReadBytes((int)RemainingBytes());
        memStream.SetLength(0);     //Clear
        memStream.Write(leftover, 0, leftover.Length);
    }

    /// <summary>
    /// 剩余的字节
    /// </summary>
    private long RemainingBytes() {
        return memStream.Length - memStream.Position;
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    /// <param name="ms"></param>
    void OnReceivedMessage(MemoryStream ms) {
        byte[] message = ms.ToArray();
        //int msglen = message.Length;
        ByteBuffer buffer = new ByteBuffer(message);
        NetworkManager.AddEvent(Protocal.Message, buffer);
        ms.Close();
    }

    /// <summary>
    /// 会话发送
    /// </summary>
    void SessionSend(byte[] bytes) {
        WriteMessage(bytes);
    }

    /// <summary>
    /// 关闭链接
    /// </summary>
    public void Close() {
        if (client != null) {
            if (client.Connected) client.Close();
            client = null;
        }
        loggedIn = false;
    }

    /// <summary>
    /// 发送连接请求
    /// </summary>
    public void SendConnect() {
        ConnectServer(AppDef.SocketAddress, AppDef.SocketPort);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void SendMessage(ByteBuffer buffer) {
        SessionSend(buffer.ToBytes());
        buffer.Close();
    }
}
