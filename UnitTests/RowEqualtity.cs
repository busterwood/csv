using BusterWood.Data;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class RowEqualtity
    {
        [Test]
        public void equal_when_all_column_are_equal()
        {
            var c1 = new Column("hello", typeof(string));
            var c2 = new Column("world", typeof(int));
            var s = new Schema("", c1, c2);
            var row1 = new ArrayRow(s, c1.Value("fred"), c2.Value(1));
            var row2 = new ArrayRow(s, c1.Value("fred"), c2.Value(1));
            Assert.AreEqual(row1, row2);
        }

        [Test]
        public void equal_when_schema_differs()
        {
            var c1 = new Column("hello", typeof(string));
            var c2 = new Column("world", typeof(int));
            var row1 = new ArrayRow(new Schema("", c1, c2), c1.Value("fred"), c2.Value(1));
            var row2 = new ArrayRow(new Schema("", c2, c1), c2.Value(1), c1.Value("fred"));
            Assert.AreEqual(row1, row2);
        }
    }
}
