using BusterWood.Data;
using NUnit.Framework;
using System.Linq;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void can_extend_existing_sequence()
        {
            var orig = Objects.ToDataSequence(new { Text = "hello" });
            Assert.AreEqual(1, orig.Schema.Count);
            Assert.AreEqual(typeof(string), orig.Schema["text"].Type);

            var extended = orig.Extend("Length", r => r.String("text").Length);

            Assert.AreEqual(2, extended.Schema.Count);
            Assert.AreEqual(typeof(string), extended.Schema["text"].Type);
            Assert.AreEqual(typeof(int), extended.Schema["length"].Type);
            Assert.AreEqual(1, extended.Count(), "count was wrong");

            foreach (var row in extended)
            {
                Assert.AreEqual(5, row.Int("length"));
            }
        }

        [Test]
        public void can_project_to_remove_columns()
        {
            var orig = Objects.ToDataSequence(new { Hello = "hello", World="world" });

            var result = orig.Project("world");
            Assert.AreEqual(1, result.Schema.Count);
            Assert.AreEqual(typeof(string), result.Schema["world"].Type);
        }

        [Test]
        public void can_create_data_sequence()
        {
            var orig = new Temp[] { new Temp { Text = "hello", Size = 1 }, new Temp { Text = "hello", Size = 2 } }.ToDataSequence();
            Assert.AreEqual(2, orig.Schema.Count);
            Assert.AreEqual(typeof(string), orig.Schema["TEXT"].Type);
            Assert.AreEqual(typeof(int), orig.Schema["SIZE"].Type);
        }

        [Test]
        public void project_removes_duplicate_rows()
        {
            var orig = new Temp[] { new Temp { Text = "hello", Size = 1 }, new Temp { Text = "hello", Size = 2 } }.ToDataSequence();
            var result = orig.Project("text");
            Assert.AreEqual(1, result.Count());
        }

        class Temp { public string Text; public int Size; };

        [Test]
        public void can_projectAway_to_remove_columns()
        {
            var orig = Objects.ToDataSequence(new { Hello = "hello", World="world" });

            var result = orig.ProjectAway("world");
            Assert.AreEqual(1, result.Schema.Count);
            Assert.AreEqual("Hello", result.Schema.First().Name);
            Assert.AreEqual(typeof(string), result.Schema["hello"].Type);
        }

        [Test]
        public void can_natural_join_on_one_column()
        {
            var left = Objects.ToDataSequence(new { Hello = "hello", World = "world" });
            var right = Objects.ToDataSequence(new { Hello = "hello", Name = "fred" });
            var result = left.NaturalJoin(right);
            Assert.AreEqual(3, result.Schema.Count);
            Assert.AreEqual(1, result.Count());
            var first = result.First();
            Assert.AreEqual("hello", first.Get("hello"));
            Assert.AreEqual("world", first.Get("world"));
            Assert.AreEqual("fred", first.Get("name"));
        }
        
        [Test]
        public void can_natural_join_on_multiple_column()
        {
            var left = Objects.ToDataSequence(new { Hello = "hello", World = "world", First="one" });
            var right = Objects.ToDataSequence(new { Hello = "hello", World = "world", Second = "two" });
            var result = left.NaturalJoin(right);
            Assert.AreEqual(4, result.Schema.Count);
            Assert.AreEqual(1, result.Count());
            var first = result.First();
            Assert.AreEqual("hello", first.Get("hello"));
            Assert.AreEqual("world", first.Get("world"));
            Assert.AreEqual("one", first.Get("first"));
            Assert.AreEqual("two", first.Get("second"));
        }

        [Test]
        public void can_natural_one_left_to_multiple_right()
        {
            var left = Objects.ToDataSequence(new { Hello = "hello", World = "world" });
            var right = Objects.ToDataSequence((IEnumerable<Hellos>)new[] { new Hellos { Hello = "hello", Name = "fred" }, new Hellos { Hello = "hello", Name = "bloggs" } });
            var result = left.NaturalJoin(right);
            Assert.AreEqual(3, result.Schema.Count);
            Assert.AreEqual(2, result.Count());

            var first = result.First();
            Assert.AreEqual("hello", first.Get("hello"));
            Assert.AreEqual("world", first.Get("world"));
            Assert.AreEqual("fred", first.Get("name"));

            var second = result.ElementAt(1);
            Assert.AreEqual("hello", second.Get("hello"));
            Assert.AreEqual("world", second.Get("world"));
            Assert.AreEqual("bloggs", second.Get("name"));
        }

        class Hellos
        {
            public string Hello { get; set; }
            public string Name { get; set; }
        }
    }
}
