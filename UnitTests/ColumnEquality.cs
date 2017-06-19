using BusterWood.Data;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ColumnEquality
    {
        [Test]
        public void columnns_are_equal_when_name_and_type_match()
        {
            var c1 = new Column("name", typeof(string));
            var c2 = new Column("name", typeof(string));
            Assert.AreEqual(c1, c2);
        }

        [Test]
        public void columnns_are_equal_regardless_of_name_case()
        {
            var c1 = new Column("name", typeof(string));
            var c2 = new Column("NAME", typeof(string));
            Assert.AreEqual(c1, c2);
        }

        [Test]
        public void columnns_are_different_when_name_differs()
        {
            var c1 = new Column("fred", typeof(string));
            var c2 = new Column("NAME", typeof(string));
            Assert.AreNotEqual(c1, c2);
        }

        [Test]
        public void columnns_are_different_when_type_differs()
        {
            var c1 = new Column("fred", typeof(string));
            var c2 = new Column("fred", typeof(int));
            Assert.AreNotEqual(c1, c2);
        }
    }
}
