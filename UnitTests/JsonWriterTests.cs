using BusterWood.Data;
using BusterWood.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class JsonWriterTests
    {
        [Test]
        public void can_write_array_over_multiple_lines()
        {
            var expected = @"[ 
 { ""First"": 1 }
]";

            var sb = new StringBuilder();

            var src = Objects.ToDataSequence(new { Text = "hello" });
            src.WriteJson(new StringWriter(sb));
            Assert.AreEqual(expected, sb.ToString());
        }
    }
}
