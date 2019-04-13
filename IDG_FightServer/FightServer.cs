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
        public IndexObjectPool<Connection> ClientPool
        {
            get
            {
                lock (_clientPool)
                {
                    return _clientPool;
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
        public List<byte[]> FrameList
        {
            get
            {
                lock (_frameList)
                {
                    return _frameList;
                }
            }
        }
        public Byte[][] StepMessage
        {
            get
            {
                lock(_stepMessage)
                {
                    return _stepMessage;
                }
                
            }
            set {
                lock (_stepMessage)
                {
                    _stepMessage = value;
                }
            }
        }
        protected List<byte[]> _frameList;
       
        private IndexObjectPool<Connection> _clientPool;
        
        public Socket _serverListener;
        public Timer timer;
        //private int framSize;
        public Byte[][] _stepMessage;
       // public byte keyInfo;
        public FightServer()
        {
            
        }
        public void StartServer(string host,int port,int maxServerCount)
        {
            _serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientPool = new IndexObjectPool<Connection>(maxServerCount);
            _frameList = new List<byte[]>();
            Listener.NoDelay = true;
            timer = new Timer(100);
            timer.AutoReset = true;
            timer.Elapsed += SendStepAll;
            timer.Enabled = true;
            Listener.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            Listener.Listen(maxServerCount);
            Listener.BeginAccept(AcceptCallBack, Listener);
            _stepMessage = new byte[maxServerCount][];
           // framSize = 0;
            //for (int i = 0; i < _stepMessage.Length; i++)
            //{
            //    _stepMessage[i] = new byte[framSize];
            //}
            //ClientPool.Clear();

            ServerLog.LogServer("服务器启动成功",0);
        }
        protected void AcceptCallBack(IAsyncResult ar)
        {
            
                
             Socket client = Listener.EndAccept(ar);
            int index= ClientPool.Get();  
            if (index >=0)
            {
                ClientPool[index].SetActive();
                Connection con = ClientPool[index];
                con.clientId = index;
                StepMessage = new byte[ClientPool.Count][];
                //for (int i = 0; i < StepMessage.Length; i++)
                //{
                //    StepMessage[i] = new byte[framSize];
                //}
                con.socket = client;
                SendIninInfo((byte)con.clientId);
                if(FrameList.Count>0)SendToClientAllFrame(index);
                con.socket.BeginReceive(con.readBuff, 0, Connection.buffer_size, SocketFlags.None, ReceiveCallBack, con);
            }
            else
            {
                ServerLog.LogServer("服务器人数达到上限",0);
            }
            Listener.BeginAccept(AcceptCallBack, Listener);
        }
       
        protected void ReceiveCallBack(IAsyncResult ar)
        {
            
            Connection con = (Connection)ar.AsyncState;
            if (!con.ActiveCheck()) return;
            try
            {
                lock (con)
                {
                    int length = con.socket.EndReceive(ar);
                    ServerLog.LogClient("receive:" + length,1,con.clientId);
                    if (length <= 0)
                    {
                        ServerLog.LogClient("客户端断开连接：" + ClientPool[con.clientId].socket.LocalEndPoint + "ClientID:" + con.clientId, 0, con.clientId);
 
                        con.socket.Close();
                        ClientPool.Recover(con.clientId);

                        return;
                    }
                    con.length += length;
                    //Console.WriteLine("bool isNew = bufferList[con]：" + ClientPool[con.clientId].socket.LocalEndPoint + "ClientID:" + con.clientId);
                    //bool isNew = bufferList[con] == null;
                    ProcessData(con);
                   
                    //{
                    //    //MessageList.Push(message);

                    //    bufferList[con] = null;
                    //}
                    //else
                    //{
                    //    bufferList[con] = message;
                    ////}
                    //Console.WriteLine("接收信息：" + message.Length + "ClientID:" + con.clientId);



                    con.socket.BeginReceive(con.readBuff, con.length, con.BuffRemain, SocketFlags.None, ReceiveCallBack, con);
                }
            }
            catch (Exception)
            {
                ServerLog.LogClient("客户端异常终止连接：" + ClientPool[con.clientId].socket.LocalEndPoint + "ClientID:" + con.clientId, 0, con.clientId);
 
                con.socket.Close();
                ClientPool.Recover(con.clientId);
                //throw;
            }
        }
        private void ProcessData(Connection connection)
        {

            if (connection.length < sizeof(Int32))
            {
               // Debug.Log("获取不到信息大小重新接包解析：" + connection.length.ToString());
                return;
            }
            Array.Copy(connection.readBuff, connection.lenBytes, sizeof(Int32));
            connection.msgLength = BitConverter.ToInt32(connection.lenBytes, 0);

            if (connection.length < connection.msgLength + sizeof(Int32))
            {
                //Debug.Log("信息大小不匹配重新接包解析：" + connection.msgLength.ToString());
                return;
            }
            //ServerDebug.Log("接收信息大小：" + connection.msgLength.ToString(), 1);
            // string str = Encoding.UTF8.GetString(connection.readBuff, sizeof(Int32), connection.length);
            ProtocolBase message = new ByteProtocol();
            message.InitMessage(connection.ReceiveBytes);
            ParseMessage(connection, message);

            //Send(connection, str);
            int count = connection.length - connection.msgLength - sizeof(Int32);
            Array.Copy(connection.readBuff, sizeof(Int32) + connection.msgLength, connection.readBuff, 0, count);
            connection.length = count;
            if (connection.length > 0)
            {
                ProcessData(connection);
            }
        }
        protected void ParseMessage(Connection con,ProtocolBase protocol)
        {
            
                switch ((MessageType)protocol.getByte())
                {
                    case MessageType.Frame:
                        byte t1 = protocol.getByte();byte[] t2 = protocol.getLastBytes();
                        //if (framSize != t2.Length) { framSize = t2.Length;
                        //    for (int i = 0; i < StepMessage.Length; i++)
                        //    {
                        //        StepMessage[i] = new byte[framSize];
                        //    }
                        //}
                        StepMessage[con.clientId] = t2;
                            ClientPool[t1].SetActive();
                        ServerLog.LogClient("Key:[" + t2.Length + "]", 3, t1);
                        break;
                    case MessageType.ClientReady:
                        break;
                    default:
                        return;
                        //break;
                }

            
            if (protocol.Length > 0)
            {
                ServerLog.LogServer("剩余未解析" + protocol.Length, 1);
                //ParseMessage(con,protocol);
            }
        }
        protected void SendToClient(int clientId,byte[] bytes)
        {
            //int sendLength = bytes.Length;
            //int index=0;
            //int unitLength=0;
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] temp = new byte[4 + bytes.Length];
            Array.Copy(length, temp, 4);
            Array.Copy(bytes, 0, temp, 4, bytes.Length);
            ServerLog.LogClient("send:" + temp.Length, 2, clientId);
            ClientPool[clientId].socket.BeginSend(temp, 0, temp.Length, SocketFlags.None, null, null);
            //Console.WriteLine(DateTime.Now.ToLongTimeString() + "clientId：" + clientId + " mesagge " + bytes[0]+" length "+bytes.Length);
            
           
        }

        protected void SendIninInfo(byte clientId)
        {
            ProtocolBase protocol = new ByteProtocol();
            protocol.push((byte)MessageType.Init);
            
            protocol.push(clientId);
            protocol.push((byte)MessageType.end);
            SendToClient(clientId, protocol.GetByteStream());
            ServerLog.LogClient("客户端连接成功：" + ClientPool[clientId].socket.LocalEndPoint + "ClientID:" + clientId, 0, clientId);
      
        }
        protected void SendToClientAllFrame(int clientId)
        {
           
            byte[][] list= FrameList.ToArray();
            ServerLog.LogClient("中途加入 发送历史帧："+ list.Length, 3, clientId);
            foreach (var item in list)
            {
                SendToClient(clientId, item);
            }
            
        }
        protected void SendStepAll(object sender, ElapsedEventArgs e)
        {
          
            if (ClientPool.ActiveCount <= 0)
            {
                if (FrameList.Count > 0)
                {
                    ServerLog.LogServer("所有客户端退出游戏 战斗结束！！！", 1); 
                    FrameList.Clear();
                }
                return;
            }
           
            if (FrameList.Count == 0)
            {
                ServerLog.LogServer("玩家进入服务器 战斗开始！！！",1);
            }
            ServerLog.LogServer("0[" + FrameList.Count + "]", 1);
            
            byte[][] temp = StepMessage;
            int length = temp.Length;
            ProtocolBase protocol = new ByteProtocol();
            protocol.push((byte)MessageType.Frame);
            protocol.push((byte)length);
            //ServerLog.LogServer("获取[" + FrameList.Count + "]", 1);
            for (int i = 0; i < length; i++)
            {
                protocol.push(temp[i] != null);
                protocol.push(temp[i]);
            }
            if (FrameList.Count == 0)
            {
                protocol.push((byte)MessageType.RandomSeed);
                Random rand = new Random();
                protocol.push(rand.Next(10000));
            }
            protocol.push((byte)MessageType.end);
            ServerLog.LogServer("生成帧信息[" + length+ "]", 1);
            byte[] temp2 = protocol.GetByteStream();

            FrameList.Add(temp2);
          
            ClientPool.Foreach((con) => { SendToClient(con.clientId, temp2);
                if (!con.ActiveCheck())
                {
                    ServerLog.LogClient("客户端断线 中止连接：" + ClientPool[con.clientId].socket.LocalEndPoint + "ClientID:" + con.clientId, 0, con.clientId);
    
                    con.socket.Close();
                    ClientPool.Recover(con.clientId);
                }

            });
           
            ServerLog.LogServer("帧同步["+ FrameList.Count+"]",2);
            //StepMessage = new byte[ClientPool.Count][];
            //for (int i = 0; i < StepMessage.Length; i++)
            //{
            //    StepMessage[i] = new byte[framSize];
            //}


        }
        
    }
    public enum MessageType : byte
    {
        Init =11,
        Frame = 12,
        ClientReady=13,
        RandomSeed=14,
        end=200,
    }
}
