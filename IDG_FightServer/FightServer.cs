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
        public List<byte> FrameList
        {
            get
            {
                lock (_frameList)
                {
                    return _frameList;
                }
            }
        }
        protected List<byte> _frameList;
        private IndexObjectPool<Connection> _clientPool;
        
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
            _clientPool = new IndexObjectPool<Connection>(maxServerCount);
            _frameList = new List<byte>();
            Listener.NoDelay = true;
            timer = new Timer(100);
            timer.AutoReset = true;
            timer.Elapsed += SendStepAll;
            timer.Enabled = true;
            Listener.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            Listener.Listen(maxServerCount);
            Listener.BeginAccept(AcceptCallBack, Listener);
            stepMessage = new byte[1];
            //ClientPool.Clear();
            
            Console.WriteLine("服务器启动成功");
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
                stepMessage = new byte[ClientPool.ActiveCount];
                con.socket = client;
                SendIninInfo((byte)con.clientId);
                if(FrameList.Count>0)SendToClientAllFrame(index);
                con.socket.BeginReceive(con.readBuff, 0, Connection.buffer_size, SocketFlags.None, ReceiveCallBack, con);
            }
            else
            {
                Console.WriteLine("服务器人数达到上限");
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
                    Console.WriteLine("receive:" + length);
                    if (length <= 0)
                    {
                        Console.WriteLine("客户端断开连接：" + ClientPool[con.clientId].socket.LocalEndPoint + "ClientID:" + con.clientId);
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
                Console.WriteLine("客户端异常终止连接：" + ClientPool[con.clientId].socket.LocalEndPoint + "ClientID:" + con.clientId);
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
            ParseMessage(message);

            //Send(connection, str);
            int count = connection.length - connection.msgLength - sizeof(Int32);
            Array.Copy(connection.readBuff, sizeof(Int32) + connection.msgLength, connection.readBuff, 0, count);
            connection.length = count;
            if (connection.length > 0)
            {
                ProcessData(connection);
            }
        }
        protected void ParseMessage(ProtocolBase protocol)
        {
            lock (stepMessage)
            {
                switch ((MessageType)protocol.getByte())
                {
                    case MessageType.Frame:
                        byte t1 = protocol.getByte(), t2 = protocol.getByte();
                        stepMessage[t1] = t2;
                        ClientPool[t1].SetActive();
                        //  Console.WriteLine(DateTime.Now.ToLongTimeString() + "[" + t1 + "]=>" + t2);
                        break;
                    case MessageType.ClientReady:
                        break;
                    default:
                        return;
                        //break;
                }

            }
            if (protocol.Length > 0)
            {
                ParseMessage(protocol);
            }
        }
        protected void SendToClient(int clientId,byte[] bytes)
        {
           byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] temp = length.Concat(bytes).ToArray();
            Console.WriteLine("send:" + temp.Length);
            ClientPool[clientId].socket.BeginSend(temp, 0, temp.Length, SocketFlags.None, null, null);
                //Console.WriteLine(DateTime.Now.ToLongTimeString() + "clientId：" + clientId + " mesagge " + bytes[0]+" length "+bytes.Length);
            
           
        }
        protected void SendIninInfo(byte clientId)
        {
            ProtocolBase protocol = new ByteProtocol();
            protocol.push((byte)MessageType.Init);
            
            protocol.push(clientId);
            
            SendToClient(clientId, protocol.GetByteStream());
            Console.WriteLine("客户端连接成功：" + ClientPool[clientId].socket.LocalEndPoint + "ClientID:" + clientId);
        }
        protected void SendToClientAllFrame(int clientId)
        {
            byte[] list= FrameList.ToArray();
            Console.WriteLine("中途加入 发送历史帧："+ list.Length);
            SendToClient(clientId, list);
        }
        protected void SendStepAll(object sender, ElapsedEventArgs e)
        {
            if (ClientPool.ActiveCount <= 0)
            {
                if (FrameList.Count > 0)
                {
                    Console.WriteLine(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "\t所有客户端退出游戏 战斗结束！！！");
                    FrameList.Clear();
                }
                return;
            }
            
            if (FrameList.Count == 0)
            {
                Console.WriteLine(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "\t玩家进入服务器 战斗开始！！！");
            }
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
            FrameList.AddRange(temp);
            ClientPool.Foreach((con) => { SendToClient(con.clientId, temp);
                if (!con.ActiveCheck())
                {
                    Console.WriteLine("客户端断线 中止连接：" + ClientPool[con.clientId].socket.LocalEndPoint + "ClientID:" + con.clientId);
                    con.socket.Close();
                    ClientPool.Recover(con.clientId);
                }

            });
            Console.WriteLine(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString()+ "\t帧同步["+ FrameList.Count/temp.Length+"]");
            stepMessage = new byte[ClientPool.Count];
            
            
        }
    }
    public enum MessageType : byte
    {
        Init = 0,
        Frame = 1,
        ClientReady=2,
    }
}
