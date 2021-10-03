using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

namespace CapeServer
{
    public class Settings
    {
        public long MaxCapeSize = 2000000;
        public List<string> UploadWhitelist = new List<string>();
        public Dictionary<string, string> RedirectList = new Dictionary<string, string>();
    }
    class Program
    {
        static HttpListener listener = new HttpListener();
        static Settings settings;

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

                if (settings.RedirectList.ContainsKey(re))
                {
                    re = settings.RedirectList[re];
                    Console.WriteLine("Redirect To " + re);
                }
                if (string.IsNullOrEmpty(req.Url.LocalPath) || req.Url.LocalPath == "/")
                {
                    byte[] bytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html"));
                    resp.ContentType = "text/html";
                    resp.StatusCode = 200;
                    resp.ContentLength64 = bytes.LongLength;
                    await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    resp.Close();
                }
                // TO disable Comment Out this block
                else if (re.StartsWith("/Upload") && req.HttpMethod == "POST")
                {

                    byte[] bytes;
                    string user = re.Split('?')[1];

                    if (settings.MaxCapeSize != -1 && req.ContentLength64 > settings.MaxCapeSize)//2 MB
                    {
                        bytes = Encoding.UTF8.GetBytes(string.Format("{0} is too large keep it under {1}", req.ContentLength64, settings.MaxCapeSize));
                        resp.ContentType = "text/html";
                        resp.StatusCode = 403;
                        resp.ContentLength64 = bytes.LongLength;
                        await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        resp.Close();
                    }
                    else if (settings.UploadWhitelist.Count > 0 && !settings.UploadWhitelist.Contains(user))
                    {
                        bytes = Encoding.UTF8.GetBytes(string.Format("Your Username isn't in the whitelist"));
                        resp.ContentType = "text/html";
                        resp.StatusCode = 403;
                        resp.ContentLength64 = bytes.LongLength;
                        await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        resp.Close();
                    }
                    else
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int count = 0;
                            do
                            {
                                byte[] buf = new byte[1024];
                                count = req.InputStream.Read(buf, 0, 1024);
                                ms.Write(buf, 0, count);
                            } while (req.InputStream.CanRead && count > 0);
                            File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "capes", user + ".png"), ms.ToArray());
                        }
                        bytes = Encoding.UTF8.GetBytes(string.Format("Success"));
                        resp.ContentType = "text/html";
                        resp.StatusCode = 200;
                        resp.ContentLength64 = bytes.LongLength;
                        await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        resp.Close();
                    }
                }
                else
                {
                    if(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, re.Substring(1))))
                    {
                        byte[] capedata = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, re.Substring(1)));
                        if (re.ToLower().EndsWith(".png")) resp.ContentType = "image/png";
                        else if (req.AcceptTypes.Length > 0) resp.ContentType = req.AcceptTypes[0];
                        resp.ContentLength64 = capedata.LongLength;
                        await resp.OutputStream.WriteAsync(capedata, 0, capedata.Length);
                        resp.Close();
                    }
                    else
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
        }
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

            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "capes"));
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json")))
            {
                settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json")));
            }
            else
            {
                settings = new Settings();
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json"), Newtonsoft.Json.JsonConvert.SerializeObject(settings,Newtonsoft.Json.Formatting.Indented));
            }
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
