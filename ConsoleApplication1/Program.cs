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
        static void Main(string[] args)
        {
            var c1 = new Class1("fred", 1, DateTime.Now);
            var t1 = TypeExtender.Extend(typeof(Class1), new Column("Size", typeof(double)), new Column("Status", typeof(string)));
            var ext1 = (IHasSchema)Activator.CreateInstance(t1, c1, 2d, "bad");
            foreach (var cv in ext1)
            {
                Console.WriteLine(cv);
            }
            Console.WriteLine(ext1.Equals(ext1));

            //var t2 = TypeRestrictor.Restrict(ext1.GetType(), new string[] { "Id", "Size" });
            //var ext2 = Activator.CreateInstance(t2, ext1);

            var sw = new Stopwatch();
            sw.Start();
            var seq = Enumerable.Range(1, 100000)
                .Select(i => new Class1("hello", i, new DateTime(i)))
                .Select(c => Activator.CreateInstance(t1, c, c.Id * 2.5d, "bad"));
            foreach (dynamic item in seq)
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.When);
                GC.KeepAlive(item.Size);
            }
            sw.Stop();
            Console.WriteLine($"took {sw.ElapsedMilliseconds} MS");
        }
    }
}
