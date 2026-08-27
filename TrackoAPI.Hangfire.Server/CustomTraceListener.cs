using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService
{
    public class CustomTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            WriteLog(message);
        }


        public override void WriteLine(string message)
        {
            WriteLog(message);
        }

        private void WriteLog(string message)
        {
            string file_name = "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            string server_path = "\\logs\\";
            string wl_path = System.Threading.Thread.GetDomain().BaseDirectory + server_path;
            if (!Directory.Exists(wl_path))
                Directory.CreateDirectory(wl_path); //如果没有该目录，则创建
            StreamWriter sw = new StreamWriter(wl_path + file_name, true, Encoding.UTF8);
            sw.WriteLine("【"+DateTime.Now.ToString() + "】" + message);
            sw.Close();
        }

    }
}
