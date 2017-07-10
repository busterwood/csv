using NUnit.Framework;
using System.Linq;
using BusterWood.Json;
using System.IO;

namespace UnitTests
{
    [TestFixture]
    public class JsonReaderTests
    {
        [Test]
        public void can_parse_empty_string()
        {
            var r = new JsonReader(new StringReader(""));
            Assert.IsFalse(r.Any());
        }

        [TestCase("[", JsonType.StartArray)]
        [TestCase("]", JsonType.EndArray)]
        [TestCase("{", JsonType.StartObject)]
        [TestCase("}", JsonType.EndObject)]
        [TestCase(":", JsonType.Colon)]
        [TestCase(",", JsonType.Comma)]
        [TestCase("true", JsonType.True)]
        [TestCase("false", JsonType.False)]
        [TestCase("null", JsonType.Null)]
        [TestCase("1", JsonType.Number)]
        [TestCase("1234", JsonType.Number)]
        [TestCase("1.1", JsonType.Number)]
        [TestCase("-1.1", JsonType.Number)]
        [TestCase("0.1234", JsonType.Number)]
        public void can_parse_single_token(string input, JsonType expected)
        {
            var r = new JsonReader(new StringReader(input));
            var toks = r.ToList();
            Assert.AreEqual(1, toks.Count, "count");
            var t = toks[0];
            Assert.AreEqual(expected, t.Type);
            Assert.AreEqual(input, t.Value);
        }

        [TestCase("", JsonType.String, "")]
        [TestCase("a", JsonType.String, "a")]
        [TestCase("abc", JsonType.String, "abc")]
        [TestCase(@"a\tbc", JsonType.String, "a\tbc")]
        [TestCase(@"a\nbc", JsonType.String, "a\nbc")]
        [TestCase(@"a\\bc", JsonType.String, @"a\bc")]
        //[TestCase("a\fbc", JsonType.String)]
        //[TestCase("a\nbc", JsonType.String)]
        //[TestCase("a\rbc", JsonType.String)]
        //[TestCase("a\"bc", JsonType.String)]
        public void can_parse_single_string_token(string input, JsonType expectedType, string expectedText)
        {
            var r = new JsonReader(new StringReader("\"" + input + "\""));
            var toks = r.ToList();
            Assert.AreEqual(1, toks.Count, "count");
            var t = toks[0];
            Assert.AreEqual(expectedType, t.Type);
            Assert.AreEqual(expectedText, t.Value);
        }

        [Test]
        public void can_read_multiple_tokens()
        {
            var r = new JsonReader(new StringReader(@"[
  {  ""one"": 1, ""hello"": ""world"" }
]"));
            var toks = r.ToList();
            int i = 0;
            Assert.AreEqual(new JsonToken(JsonType.StartArray, "["), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.StartObject, "{"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.String, "one"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.Colon, ":"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.Number, "1"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.Comma, ","), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.String, "hello"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.Colon, ":"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.String, "world"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.EndObject, "}"), toks[i++]);
            Assert.AreEqual(new JsonToken(JsonType.EndArray, "]"), toks[i++]);
            Assert.AreEqual(i, toks.Count);
        }
    }
}
