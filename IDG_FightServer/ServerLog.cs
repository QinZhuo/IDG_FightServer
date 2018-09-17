using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDG.FightServer
{
    public enum LogType{
        Server,
        Client,
    }
    public class ServerLog
    {
        protected static int[] _sizes = new int[2] { 3,5};
        protected static int serverSize
        {
            set
            {
                lock (_sizes)
                {
                    if (value + 2 > _sizes[0])
                    {
                     //   Console.Clear();
                        
                        _sizes[0] = value + 2;
                        Space(serverSize - 1);
                    }
                }
            }
            get
            {
                lock (_sizes)
                {
                    return _sizes[0];
                }
            }
        }
        //protected static int playerLength
        //{
        //    set
        //    {
        //        lock (_sizes)
        //        {
        //            if (value + 1 > _sizes[1])
        //            {
        //                //Console.Clear();
        //                _sizes[1] = value + 1;
                   
        //            }
        //        }
        //    }
        //    get
        //    {
        //        lock (_sizes)
        //        {
        //            return _sizes[1];
        //        }
        //    }
        //}
        protected static int playerSize
        {
            set
            {
                lock (_sizes)
                {
                   
                    if (value + 2 > _sizes[1])
                    {
                      //  Console.Clear();
                        _sizes[1] = value + 1;
                       
                    }
                }
            }
            get
            {
                lock (_sizes)
                {
                    return _sizes[1];
                }
            }
        }
        protected static string Time
        {
            get
            {
                return DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString()+"\t ";
            }
        }
        protected static void Log(string info,int top=-1)
        {
            lock (_sizes)
            {
                if (top != -1)
                {
                    Console.SetCursorPosition(0, top);
                }
                else
                {
                    Console.SetCursorPosition(0, 10);
                }
                string t = Time + info;
                string spacer = "\t\t\t\t\t\t\t";
                Console.WriteLine(t + spacer);
            }
        }
        protected static void Space(int top)
        {
            Console.SetCursorPosition(0, top);
            string spacer = "_______________________________________________________________________________________________\t\t\t\t\t";
            Console.WriteLine(spacer);
        }
        public static void LogServer(string info,int msgId)
        {
            
            serverSize = msgId;
           
            Log(info, msgId);
           
        }
        public static void LogClient(string info, int msgId,int clientId)
        {
           
           playerSize = msgId;
            
            Log("Player___["+clientId+"]___\t"+info, serverSize+clientId * playerSize + msgId);
            //Log("serverSize" + _sizes[0], 17);
            //Log("playerLength" + _sizes[1], 18);
            //Log("playerSize" + _sizes[2], 19);
        }
    }
}
