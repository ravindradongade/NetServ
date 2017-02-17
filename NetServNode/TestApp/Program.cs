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
            NodeConfiguration nodeConfiguration = new NodeConfiguration("tesing", "10.85.130.175", 3344, true, NetServNodeEntity.Enums.StorageType.InMemory, "", 50, 90, false, null,actors,"10.23.45.46",3344);
            NetServManager manager = new NetServManager();
            manager.StartNode(nodeConfiguration);
            Console.ReadLine();
        }
    }
}
