using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
namespace IDG
{
    public class Connection
    {
        protected static readonly int ActiveNum=30;
        public readonly static int buffer_size = 1024;
        public int clientId;
        public  byte[] readBuff = new byte[buffer_size];
        public byte[] lenBytes = new byte[4];
        public Socket socket;
        public int msgLength = 0;
        public int length=0;
        protected byte[] tempBuff;
        protected int active;
        public int BuffRemain { get { return buffer_size - length; } }
        public byte[] ReceiveBytes { get { tempBuff = new byte[msgLength]; Array.Copy(readBuff, 4, tempBuff, 0, msgLength); return tempBuff; } }
        public bool ActiveCheck()
        {
            return (--active >= 0);  
        }
        public void SetActive()
        {
            active = ActiveNum;
        }
    }
}
