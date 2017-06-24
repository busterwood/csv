using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var c1 = new Class1("fred", 1, DateTime.UtcNow);
            var t1 = TypeExtender.Extend(typeof(Class1), new Column("Size", typeof(double)), new Column("Status", typeof(string)));
            var ext1 = (IHasSchema)Activator.CreateInstance(t1, c1, 2d, "bad");
            foreach (var cv in ext1)
            {
                Console.WriteLine(cv);
            }
            Console.WriteLine(ext1.Equals(ext1));
            Console.WriteLine(ext1.Equals((object)ext1));
            Console.WriteLine(ext1.GetHashCode());

            //var t2 = TypeRestrictor.Restrict(ext1.GetType(), new string[] { "Id", "Size" });
            //var ext2 = Activator.CreateInstance(t2, ext1);

            //TODO: factory method - if fac just in Extend(....)
            var p1 = Expression.Parameter(typeof(Class1));
            var p2 = Expression.Parameter(typeof(double));
            var p3 = Expression.Parameter(typeof(string));
            var lambda = Expression.Lambda<Func<Class1, double, string, object>>(Expression.New(t1.GetConstructor(new[] { typeof(Class1), typeof(double), typeof(string) }), p1, p2, p3), p1, p2, p3);
            var factory = lambda.Compile();

            var seq = Enumerable.Range(1, 10000000)
                .Select(i => new Class1("hello", i, new DateTime(i)))
                .Select(c => factory(c, c.Id * 2.5d, "bad")) 
                ;
            var sw = new Stopwatch();
            sw.Start();
            foreach (dynamic item in seq)
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.When);
                GC.KeepAlive(item.Size);
            }
            sw.Stop();
            Console.WriteLine($"took {sw.ElapsedMilliseconds} MS");
            Console.ReadLine();
        }
    }
}
