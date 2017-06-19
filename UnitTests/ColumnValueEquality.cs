using BusterWood.Data;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ColumnValueEquality
    {
        [Test]
        public void equal_if_column_and_value_are_equal()
        {
            var c1 = new Column("fred", typeof(string));
            var cv1 = new ColumnValue(c1, "a");
            var cv2 = new ColumnValue(c1, "a");
            Assert.AreEqual(cv1, cv2);
        }

        [Test]
        public void not_equal_if_colum_differs()
        {
            var c1 = new Column("fred", typeof(string));
            var c2 = new Column("fred", typeof(int));
            var cv1 = new ColumnValue(c1, "a");
            var cv2 = new ColumnValue(c2, "a");
            Assert.AreNotEqual(cv1, cv2);
        }

        [Test]
        public void not_equal_if_value_differs()
        {
            var c1 = new Column("fred", typeof(string));
            var cv1 = new ColumnValue(c1, "a");
            var cv2 = new ColumnValue(c1, "b");
            Assert.AreNotEqual(cv1, cv2);
        }
    }
}
