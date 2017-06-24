using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        private const int Max = 1000000;

        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            RunObject();
            sw.Stop();
            Console.WriteLine($"{nameof(RunObject)}took {sw.ElapsedMilliseconds:N0} MS");

            sw.Restart();
            RunDynamicObj();
            sw.Stop();
            Console.WriteLine($"{nameof(RunDynamicObj)}took {sw.ElapsedMilliseconds:N0} MS");

            sw.Restart();
            RunDataSequence();
            sw.Stop();
            Console.WriteLine($"{nameof(RunDataSequence)}took {sw.ElapsedMilliseconds:N0} MS");
        }

        private static void RunObject()
        {
            var seq = Enumerable.Range(1, Max).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) });
            foreach (var item in seq)
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.OptId);
                GC.KeepAlive(item.When);
            }
        }


        private static void RunDynamicObj()
        {
            var seq = Enumerable.Range(1, Max).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) });
            foreach (dynamic item in seq)
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.OptId);
                GC.KeepAlive(item.When);
            }
        }

        private static void RunDataSequence()
        {
            var seq = Enumerable.Range(1, Max).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) }).ToDataSequence("test");
            foreach (var item in seq)
            {
                GC.KeepAlive(item.String("Text"));
                GC.KeepAlive(item.Int("Id"));
                GC.KeepAlive(item.Get("OptId"));
                GC.KeepAlive(item.DateTime("When"));
            }
        }
    }
}
