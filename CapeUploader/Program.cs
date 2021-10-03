using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.IO;


namespace CapeUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                WebClient c = new WebClient();
                byte[] cape = File.ReadAllBytes(args[0]);
                c.UploadData("http://s.optifine.net/Upload?" + args[1], cape);


            }
            else if (args.Length == 3)
            {
                WebClient c = new WebClient();
                byte[] cape = File.ReadAllBytes(args[0]);
                c.UploadData(string.Format("http://{1}/Upload?{0}", args[1],args[2]), cape);
            }
            else
            {
                Console.WriteLine(
                    "Incorect Number of Args\n" +
                    "Usage:\n" +
                    "CapeUploader.exe \"Path/to/Cape.png\" USERNAME\n" +
                    "CapeUploader.exe \"Path/to/Cape.png\" USERNAME \"Address\"");
                Console.Read();
                Process.GetCurrentProcess().Close();
            }
            
        }
    }
}
