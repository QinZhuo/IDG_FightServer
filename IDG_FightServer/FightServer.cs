using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Timers;
namespace IDG.FightServer
{
    class FightServer
    {
        public List<Connection> ClientList
        {
            get
            {
                lock (_clientList)
                {
                    return _clientList;
                }
            }
        }

        public Socket Listener
        {
            get
            {
                lock (_serverListener)
                {
                    return _serverListener;
                }
            }
        }
        
        private List<Connection> _clientList = new List<Connection>();
        public Socket _serverListener;
        public Timer timer;
        public Byte[] stepMessage;
       // public byte keyInfo;
        public FightServer()
        {
            
        }
        public void StartServer(string host,int port,int maxServerCount)
        {
            _serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.NoDelay = true;
            timer = new Timer(100);
            timer.AutoReset = true;
            timer.Elapsed += SendStepAll;
            timer.Enabled = true;
            Listener.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            Listener.Listen(maxServerCount);
            Listener.BeginAccept(AcceptCallBack, Listener);
            stepMessage = new byte[1];
            ClientList.Clear();
            
            Console.WriteLine("服务器启动成功");
        }
        protected void AcceptCallBack(IAsyncResult ar)
        {
            
                
             Socket client = Listener.EndAccept(ar);
             Connection con = new Connection();

            
         
             con.clientId =ClientList.Count;
              ClientList.Add(con);
              stepMessage = new byte[ClientList.Count];
            con.socket = client;
            SendIninInfo((byte)con.clientId);
            con.socket.BeginReceive(con.readBuff,0,Connection.buffer_size, SocketFlags.None, ReceiveCallBack, con);
            Listener.BeginAccept(AcceptCallBack, Listener);
        }
        protected void ReceiveCallBack(IAsyncResult ar)
        {
           
            Connection con = (Connection)ar.AsyncState;
            lock (con)
            {
                con.length= con.socket.EndReceive(ar);


                ProtocolBase protocol = new ByteProtocol();
               
                protocol.InitMessage(con.ReceiveBytes);
                lock (stepMessage)
                {
                    switch ((MessageType)protocol.getByte())
                    {
                        case MessageType.Frame:
                            byte t1 = protocol.getByte(), t2 = protocol.getByte();
                            stepMessage[t1] = t2;
                            Console.WriteLine(DateTime.Now.ToLongTimeString() + "[" + t1 + "]=>" + t2);
                            break;
                        case MessageType.ClientReady:
                            break;
                        default:
                            break;
                    }
                
                }
            
            con.socket.BeginReceive(con.readBuff, 0, Connection.buffer_size, SocketFlags.None, ReceiveCallBack, con);
            }
        }
        protected void SendToClient(int clientId,byte[] bytes)
        {
          
                ClientList[clientId].socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, null, null);
                //Console.WriteLine(DateTime.Now.ToLongTimeString() + "clientId：" + clientId + " mesagge " + bytes[0]+" length "+bytes.Length);
            
           
        }
        protected void SendIninInfo(byte clientId)
        {
            ProtocolBase protocol = new ByteProtocol();
            protocol.push((byte)MessageType.Init);
            
            protocol.push(clientId);
            
            SendToClient(clientId, protocol.GetByteStream());
            Console.WriteLine("客户端连接成功：" + ClientList[clientId].socket.LocalEndPoint + "ClientID:" + clientId);
        }
        protected void SendStepAll(object sender, ElapsedEventArgs e)
        {
            if (ClientList.Count < 0) return;
            byte[] temp = stepMessage.ToArray();
            int length = stepMessage.Length;
            ProtocolBase protocol = new ByteProtocol();
            protocol.push((byte)MessageType.Frame);
            protocol.push((byte)length);
            for (int i = 0; i < length; i++)
            {
                protocol.push(temp[i]);
            }
            temp = protocol.GetByteStream();
           
                for (int i = 0; i < ClientList.Count; i++)
                {
                    SendToClient(i, temp);
                }
                stepMessage = new byte[ClientList.Count];
           
            
        }
    }
    public enum MessageType : byte
    {
        Init = 0,
        Frame = 1,
        ClientReady=2,
    }
}
