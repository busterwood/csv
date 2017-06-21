using BusterWood.Data;
using NUnit.Framework;
using System;
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
        public void can_iterate_large_number_of_objects_without_data_sequence()
        {
            var seq = Enumerable.Range(1, 100000).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) });
            foreach (var item in seq)
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.OptId);
                GC.KeepAlive(item.When);
            }
        }

        [Test]
        public void can_iterate_large_number_of_objects()
        {
            var seq = Enumerable.Range(1, 100000).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) }).ToDataSequence("test");
            foreach (var item in seq)
            {
                GC.KeepAlive(item.String("Text"));
                GC.KeepAlive(item.Int("Id"));
                GC.KeepAlive(item.Get("OptId"));
                GC.KeepAlive(item.DateTime("When"));
            }
        }

        class NoPublics { }
    }
}
