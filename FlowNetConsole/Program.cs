using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowNet;

namespace FlowNetConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "FlowNet by Ulysses";
            OfController controller = new OfController(6633);
            controller.LoadPlugins(Environment.CurrentDirectory);
            controller.LoadLogConfigsFromFile("FlowNet.log4net");
            controller.Start();
            Console.WriteLine("FlowNet Started!");
            Console.ReadLine();
            controller.Stop();
            Console.WriteLine("FlowNet Stoped!");
            Console.ReadLine();
        }
    }
}
