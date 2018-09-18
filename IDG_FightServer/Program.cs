using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDG;
using IDG.FightServer;
namespace IDG_FightServer
{
    class Program
    {
       
        static void Main(string[] args)
        {
            //Ratio a = new Ratio(-1,2);
            //Ratio b = new Ratio(-1,2);

            //Console.WriteLine(a.ToString());
            //a.Sub(b);
            //Console.WriteLine(a.ToString());
            //a.Division(b);
            //Console.WriteLine(a.ToString());
            //ProtocolBase protocol = new ByteProtocol();
            //protocol.push(1);//4
            //protocol.push("123123");//12 +2
            //protocol.push((byte)3);//1
            //protocol.push(new Ratio(9, 20));
            //protocol.push(true);//1
            //protocol.push("卧槽");//4 +2

            //protocol.InitMessage(protocol.GetByteStream());

            //Console.WriteLine(protocol.getInt32());
            //Console.WriteLine(protocol.getString());
            //Console.WriteLine(protocol.getByte());
            //Console.WriteLine(protocol.getRatio());
            //Console.WriteLine(protocol.getBoolean());
            //Console.WriteLine(protocol.getString());

          
            
            FightServer fightServer = new FightServer();
            fightServer.StartServer("127.0.0.1", 12345, 5);
            while (true)
            {
                Console.ReadLine();
            }

        }
        
    }
}
