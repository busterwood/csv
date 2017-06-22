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
            var c1 = new Class1("fred", 1, DateTime.Now);
            var t1 = TypeExtender.Extend(typeof(Class1), new Column("Size", typeof(double)), new Column("Status", typeof(string)));
            var ext1 = Activator.CreateInstance(t1, c1, 2d, "bad");

            var t2 = TypeRestrictor.Restrict(ext1.GetType(), new string[] { "Id", "Size" });
            var ext2 = Activator.CreateInstance(t2, ext1);

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
