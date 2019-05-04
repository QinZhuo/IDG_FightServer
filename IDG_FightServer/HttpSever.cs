using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using LiteDB;
namespace IDG
{
    public class HttpServer
    {
        HttpListener listener;
      //  string ip;
        public Func<string, string> InfoParse;
        public HttpServer()
        {
            listener = new HttpListener();
        }
        public void Listen(string ipPort)
        {
            listener.Prefixes.Add("http://"+ ipPort + "/");
            listener.Start();
            listener.BeginGetContext(AsyncContext, listener);
            Console.WriteLine("Http服务启动成功");
        }
        public void AsyncContext(IAsyncResult ar)
        {
            var listener = ar.AsyncState as HttpListener;
         
            listener.BeginGetContext(AsyncContext, listener);
            var context = listener.EndGetContext(ar);
            var request = context.Request;

            string receiveInfo = "";
            if (request.HttpMethod == "POST" && request.InputStream != null)
            {
                StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                receiveInfo = reader.ReadToEnd();
            }
            else if(request.HttpMethod == "GET")
            {
                receiveInfo = request.RawUrl.Remove(0,1);
            }
            

            var response = context.Response;
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=UTF-8";
            response.ContentEncoding = Encoding.UTF8;
            response.AppendHeader("Content-Type", "application/json;charset=UTF-8");
            
            using (StreamWriter writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
            {
                if (InfoParse != null)
                {
                    writer.Write(InfoParse(receiveInfo));
                }
                writer.Close();
                response.Close();
            }
           
        }
        
    }
    public class DataHttpServer
    {
        HttpServer http;
        string dbName;
        public DataHttpServer()
        {
            http = new HttpServer();
            http.InfoParse = InfoParse;
        }
        public virtual void Start(string ipPort, string dbName)
        {
            this.dbName = dbName;
            http.Listen(ipPort);
        }
        private string InfoParse(string receive)
        {
            return ParseReceive(new KeyValueProtocol(receive)).GetString();
        }
        public virtual KeyValueProtocol ParseReceive(KeyValueProtocol protocol)
        {
            protocol["time"] = DateTime.Now.ToString();
            return protocol;
        }
        public void Data(Action<LiteDatabase> action)
        {
            using (var db = new LiteDatabase(dbName))
            {
                action(db);
            }
        }
    }
}
