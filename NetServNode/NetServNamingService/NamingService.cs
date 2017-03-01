using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNamingService
{
    public class NamingService
    {
        public void StartNamingService(int port)
        {
            try
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    throw new InvalidOperationException("Network not available");
                WebApp.Start<Startup>(url: "http://+:" + port);
                Console.WriteLine("Naming service started at {0}", "http://localhost:" + port);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
