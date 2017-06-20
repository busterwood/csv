using BusterWood.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void can_extend_exist_sequence()
        {
            var orig = new { Text = "hello" }.ToDataSequence();
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
            var orig = new { Hello = "hello", World="world" }.ToDataSequence();

            var result = orig.Project("world");
            Assert.AreEqual(1, result.Schema.Count);
            Assert.AreEqual(typeof(string), result.Schema["world"].Type);
        }

        [Test]
        public void can_projectAway_to_remove_columns()
        {
            var orig = new { Hello = "hello", World="world" }.ToDataSequence();

            var result = orig.ProjectAway("world");
            Assert.AreEqual(1, result.Schema.Count);
            Assert.AreEqual(typeof(string), result.Schema["hello"].Type);
        }
    }
}
