using System;
using BusterWood.Data;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class SchemaTests
    {
        [Test]
        public void cannot_create_schema_with_zero_columns()
        {
            Assert.Throws<ArgumentException>(() => new Schema("second", new BusterWood.Data.Column[0]));
        }

        [Test]
        public void cannot_create_schema_with_duplicate_columns()
        {
            var c1 = new BusterWood.Data.Column("name", typeof(string));
            Assert.Throws<ArgumentException>(() => new Schema("second", new[] { c1, c1 }));
        }

        //[Test]
        //public void merge_does_not_include_duplicates()
        //{
        //    var c1 = new Column("name", typeof(string));
        //    var c2 = new Column("fred", typeof(string));
        //    var s1 = new Schema("first", c1, c2);
        //    var s2 = new Schema("second", c2, c1);
        //    var result = Schema.Merge(s1, s2);
        //    Assert.AreEqual(s1, result);
        //}

        //[Test]
        //public void merge_includes_all_columns_from_left()
        //{
        //    var c1 = new Column("name", typeof(string));
        //    var c2 = new Column("fred", typeof(string));
        //    var s1 = new Schema("first", c1);
        //    var s2 = new Schema("second", c2);
        //    var result = Schema.Merge(s1, s2);
        //    Assert.AreEqual(new Schema("", c1, c2), result);
        //}

        //[Test]
        //public void merge_includes_extra_columns_from_right()
        //{
        //    var c1 = new Column("name", typeof(string));
        //    var c2 = new Column("fred", typeof(string));
        //    var s1 = new Schema("first", c1);
        //    var s2 = new Schema("second", c2);
        //    var result = Schema.Merge(s1, s2);
        //    Assert.AreEqual(new Schema("", c1, c2), result);
        //}

    }
}