using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDG;
using Newtonsoft.Json;

namespace IDG.FightServer
{
    public class FightServerManager:DataHttpServer
    {
      
        public string ip;
        public int port;
        public List<FightServer> runingServers;
        public List<FightServer> waitServers;
        public override void Start(string ipPort, string dbName)
        {
            base.Start(ipPort, dbName);
            var strs = ipPort.Split(':');
            ip = strs[0];
            port = int.Parse(strs[1]) + 100;
            Console.WriteLine(ip + ":" + port);
            waitServers = new List<FightServer>();
            runingServers = new List<FightServer>();
        }
        public FightServer GetServer()
        {
            FightServer server = null;
            if (waitServers.Count > 0)
            {
                server = waitServers[0];
                waitServers.Remove(server);
             
            }
            else
            {
                server = new FightServer();
                server.StartServer(ip, port++, 10);
            }
            runingServers.Add(server);
            return server;
        }
        public override KeyValueProtocol ParseReceive(KeyValueProtocol receive)
        {
            var send = new KeyValueProtocol();
            Console.WriteLine("接收 " + receive.GetString());
            switch (receive["cmd"])
            {
                case "startGame":
                    Data((db) =>
                    {
                        var server = GetServer();
                        server.fightRoom = JsonConvert.DeserializeObject<FightRoom>(receive["fightRoom"]);
                        send["ip"] = server.ip;
                        send["port"] = server.port;
                    }
                    );
                    send["status"] = "成功";
                    break;
                default:
                    break;
            }
            return send;
        }
    }
}
