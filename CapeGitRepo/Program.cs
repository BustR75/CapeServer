using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

namespace CapeGitRepo
{
    class Program
    {
        static HttpListener listener = new HttpListener();
        public static async Task HandleConnections()
        {
            WebClient Resender = new WebClient();
            int RequestCount = 0;
            for (; ; )
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                //Log
                Console.WriteLine("Request #{0}:", ++RequestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                string re = req.Url.PathAndQuery;
                Console.WriteLine(re);
                Console.WriteLine();

                try
                {
                    
                    byte[] capedata = Resender.DownloadData(string.Format("https://raw.githubusercontent.com/{1}/master{0}", re,Repo));
                    if (re.ToLower().EndsWith(".png")) resp.ContentType = "image/png";
                    else if (req.AcceptTypes.Length > 0) resp.ContentType = req.AcceptTypes[0];
                    resp.ContentLength64 = capedata.LongLength;
                    await resp.OutputStream.WriteAsync(capedata, 0, capedata.Length);
                    resp.Close();
                }
                catch
                {
                    byte[] data = null;
                    try
                    {
                        data = Resender.DownloadData("http://35.190.10.249" + re);
                        if (re.ToLower().EndsWith(".png")) resp.ContentType = "image/png";
                        else if (req.AcceptTypes.Length > 0) resp.ContentType = req.AcceptTypes[0];
                        resp.ContentLength64 = data.LongLength;
                    }
                    catch { }
                    if (data != null)
                    {
                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        Console.WriteLine("Redirected {0} of data, of type {1}", data.LongLength, resp.ContentType);
                        resp.Close();
                    }
                    else
                    {
                        resp.StatusCode = 404;
                        resp.Close();
                    }
                }
                
            }
        }
        static string Repo = "BustR75/CapeServer";
        static void Main(string[] args)
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Console.WriteLine("Not Running As Admin, Please Restart As Admin");
                    Console.ReadLine();
                    Process.GetCurrentProcess().Close();
                }
            }
            if (args.Length >= 1) Repo = args[0];
            listener.Prefixes.Add("http://s.optifine.net/");
            listener.Prefixes.Add("https://s.optifine.net/");
            listener.Start();
            Console.WriteLine("Started Server");
            Task li = HandleConnections();
            li.GetAwaiter().GetResult();
            listener.Close();
        }
    }
}
