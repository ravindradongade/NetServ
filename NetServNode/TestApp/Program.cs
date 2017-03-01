using NetServNode;
using NetServNodeEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] actors = { "a" };
            NodeConfiguration nodeConfiguration = new NodeConfiguration("tesing", "10.85.130.175", 3344, true, NetServNodeEntity.Enums.StorageType.SQL, "data source=localhost;initial catalog=NetServ;user id=sa;password=P@ssw0rd;MultipleActiveResultSets=True;App=EntityFramework", 50, 90, false, null,actors);
            NetServManager manager = new NetServManager();
            manager.StartNode(nodeConfiguration);
            Console.ReadLine();
        }
    }
}
