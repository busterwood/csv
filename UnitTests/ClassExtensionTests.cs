using BusterWood.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;

namespace UnitTests
{
    [TestFixture]
    public class ObjectsTests
    {
        [Test]
        public void cannot_create_schema_for_primative_type()
        {
            Assert.Throws<ArgumentException>(() => Objects.ToSchema<int>());
        }

        [Test]
        public void cannot_create_schema_for_type_with_no_public_fields_or_properties()
        {
            Assert.Throws<ArgumentException>(() => Objects.ToSchema<NoPublics>());
        }

        [Test]
        public void can_iterate_large_number_of_objects()
        {            
            var sw = new Stopwatch();
            sw.Start();
            var seq = Enumerable.Range(1, 100000).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) }).ToList();
            foreach (var item in seq.Distinct())
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.Id + 1);
                GC.KeepAlive(item.OptId);
                GC.KeepAlive(item.When);
            }
            sw.Stop();
            Console.WriteLine($"took {sw.ElapsedMilliseconds:N1}ms");            
        }

        [Test]
        public void can_iterate_large_number_of_objects_dynamic()
        {            
            var sw = new Stopwatch();
            sw.Start();
            var seq = Enumerable.Range(1, 100000).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) }).ToList();
            foreach (dynamic item in seq.Distinct())
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.OptId);
                GC.KeepAlive(item.When);
            }
            sw.Stop();
            Console.WriteLine($"took {sw.ElapsedMilliseconds:N1}ms");            
        }

        [Test]
        public void can_iterate_large_number_of_objects_dynamic_expando()
        {            
            var sw = new Stopwatch();
            sw.Start();
            var seq = Enumerable.Range(1, 100000).Select(i => { dynamic obj = new ExpandoObject(); obj.Text = "hello"; obj.Id = i; obj.OptId = (int?)i; obj.When = new DateTime(i); return obj; }).ToList();
            foreach (dynamic item in seq.Distinct())
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.OptId);
                GC.KeepAlive(item.When);
            }
            sw.Stop();
            Console.WriteLine($"took {sw.ElapsedMilliseconds:N1}ms");            
        }

        [Test]
        public void can_iterate_large_number_of_objects_in_dictionary()
        {            
            var sw = new Stopwatch();
            sw.Start();
            var seq = Enumerable.Range(1, 100000).Select(i => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Text", "hello" }, { "Id", i }, { "OptId", (int?)i }, { "When", new DateTime(i) } }).ToList();

            foreach (var item in seq)
            {
                GC.KeepAlive((string)item["Text"]);
                GC.KeepAlive((int)item["Id"]);
                GC.KeepAlive((int?)item["OptId"]);
                GC.KeepAlive((DateTime)item["When"]);
            }
            sw.Stop();
            Console.WriteLine($"took {sw.ElapsedMilliseconds:N1}ms");            
        }

        [Test]
        public void can_iterate_large_number_of_objects_with_data_sequence()
        {            
            var sw = new Stopwatch();
            sw.Start();
            var seq = Enumerable.Range(1, 100000).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) }).ToList();
            foreach (var item in seq.ToDataSequence("test"))
            {
                GC.KeepAlive(item.String("Text"));
                GC.KeepAlive(item.Int("Id"));
                GC.KeepAlive(item.Get("OptId"));
                GC.KeepAlive(item.DateTime("When"));
            }
            sw.Stop();
            Console.WriteLine($"took {sw.ElapsedMilliseconds:N1}ms");
        }

        [SetUp, TearDown]
        public void RunGc()
        {
            GC.Collect();
        }
        class NoPublics { }
    }
}
