using System;
using System.Globalization;
using System.Text;
using System.Device.I2c;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using BBB;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using ThreadUtils;
using Sqlite;

namespace BBB
{
    /// <summary>
    /// Active object containing a Lcd display object
    /// </summary>
    public class LcdWorker : ActiveObject
    {
        private Display lcd;

        public LcdWorker()
        {
            lcd = new Display(new I2cConnectionSettings(2, 0x3C));
        }

        public string GetNumEvents()
        {
              return new SqliteEventWriter("GpioEvents.db").ReadAllEvents().Count.ToString();          
        }

        List<string> GetIps()
        {
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);

            var ips = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)  
                        {
                            ips.Add(Convert.ToString(ip.Address));
                        }
                    }
                }
            }
            return ips;
        }


        public override void DoWork()
        {
            while (!_shouldStop)
            {
                lcd.ClearScreen();
                var sb = new StringBuilder();
                // CultureInfo.CreateSpecificCulture("fr-FR")
                sb.AppendLine(DateTime.Now.ToString("HH:mm:ss"));

                foreach(var ip in GetIps())
                    if(!ip.StartsWith("127"))
                        sb.AppendLine(ip);
                
                sb.AppendLine(GetNumEvents() + " entries");

                lcd.WriteLine(sb.ToString());
                Thread.Sleep(1000);
            }
            Console.WriteLine("Exiting Display thread");
        }



        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && lcd != null)
            {
                lcd.Dispose();
                lcd = null;
            }
        }
    }
}