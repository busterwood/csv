using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var seq = Enumerable.Range(1, 1000000).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) }).ToDataSequence("test");
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
