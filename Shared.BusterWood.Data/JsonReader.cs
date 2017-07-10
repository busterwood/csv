using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BusterWood.Json
{
    /// <summary>Allows enumeration of a string of JSON tokens</summary>
    /// <remarks>NOT safe for multiple or concurrent iteration</remarks>
    public class JsonReader : IEnumerable<JsonToken>
    {
        readonly StringBuilder sb = new StringBuilder();
        readonly TextReader input;

        public JsonReader(TextReader input)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public IEnumerator<JsonToken> GetEnumerator()
        {
            for(;;)
            {
                int n = input.Read();
                if (n == -1)
                    yield break;
                var curr = (char)n;
                if (char.IsWhiteSpace(curr))
                    continue;
                switch ((JsonType)curr)
                {
                    case JsonType.StartArray:
                        yield return new JsonToken(JsonType.StartArray, "["); // use literal singleton string, as Char.ToString() causes allocation
                        break;
                    case JsonType.EndArray:
                        yield return new JsonToken(JsonType.EndArray, "]");
                        break;
                    case JsonType.StartObject:
                        yield return new JsonToken(JsonType.StartObject, "{");
                        break;
                    case JsonType.EndObject:
                        yield return new JsonToken(JsonType.EndObject, "}");
                        break;
                    case JsonType.Comma:
                        yield return new JsonToken(JsonType.Comma, ",");
                        break;
                    case JsonType.Colon:
                        yield return new JsonToken(JsonType.Colon, ":");
                        break;
                    case JsonType.String:
                        yield return ReadString();
                        break;
                    case JsonType.True:
                        yield return ReadTrue();
                        break;
                    case JsonType.False:
                        yield return ReadFalse();
                        break;
                    case JsonType.Null:
                        yield return ReadNull();
                        break;
                    default:
                        if (char.IsNumber(curr) || curr == '-')
                            yield return ReadNumber(curr);
                        else
                            throw new Exception($"Unexpected character '{curr}'");
                        break;
                }
            }
        }

        private JsonToken ReadTrue()
        {
            Expect("true");
            return new JsonToken(JsonType.True, "true");
        }

        private JsonToken ReadFalse()
        {
            Expect("false");
            return new JsonToken(JsonType.False, "false");
        }

        private JsonToken ReadNull()
        {
            Expect("null");
            return new JsonToken(JsonType.Null, "null");
        }

        private void Expect(string expected)
        {
            foreach (var c in expected.Skip(1)) /// skip current char, allows correct reporting of expected in exception
            {
                int n = input.Read();
                if (n == -1)
                    throw new Exception("Expected closing quote of string but did not find one");

                var next = (char)n;
                if (next != c)
                    throw new Exception($"Expected '{c}' but got '{next}' when expected to read '{expected}'");
            }
        }

        private JsonToken ReadString()
        {
            sb.Clear(); // reuse existing buffer
            for (;;)
            {
                int n = input.Read();
                if (n == -1)
                    throw new Exception("Expected closing quote of string but did not find one");

                var next = (char)n;
                if (next == '"')
                {
                    if (input.Peek() != '"')
                        return new JsonToken(JsonType.String, sb.ToString());
                    input.Read(); // turn "" into one " in sb buffer
                }
                else if (next == '\\')
                {
                    switch (input.Peek())
                    {
                        case '/': next = '/'; break;
                        case '\\': next = '\\'; break;
                        case '"': next = '"'; break;
                        case 'b': next = '\b'; break;
                        case 'f': next = '\f'; break;
                        case 'n': next = '\n'; break;
                        case 'r': next = '\r'; break;
                        case 't': next = '\t'; break;
                        default:
                            throw new Exception("Expected escape sequence in string");

                    }
                    input.Read(); // consume after peeking
                }
                else if (next == '\r' || next == '\n')
                    throw new Exception("Expected closing quote of string found end of line");

                sb.Append(next);
            }
        }

        private JsonToken ReadNumber(char first)
        {
            sb.Clear(); // reuse buffer
            sb.Append(first);
            for(;;)
            {
                int n = input.Peek();
                if (n == -1)
                    return new JsonToken(JsonType.Number, sb.ToString());
                var next = (char)n;
                if (next == '.')
                    break;
                if (!char.IsNumber(next))
                    return new JsonToken(JsonType.Number, sb.ToString());
                input.Read(); // consume from stream
                sb.Append(next);
            }

            // peeked a '.' if we got here
            if (sb[sb.Length - 1] == '-')
                throw new Exception("Expected whole number part before the decimal point");
            input.Read(); // consume from stream
            sb.Append('.');

            for (;;)
            {
                int n = input.Peek();
                if (n == -1)
                    break;
                var next = (char)n;
                if (next == '.')
                    throw new Exception("Expected fractional number part but got another decimal point");
                if (!char.IsNumber(next))
                    break;
                input.Read(); // consume from stream
                sb.Append(next);
            }

            if (sb[sb.Length - 1] == '.')
                throw new Exception("Expected fractional number part after then decimal point");
            return new JsonToken(JsonType.Number, sb.ToString());
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public struct JsonToken
    {
        public JsonType Type { get; }
        public string Value { get; }

        public JsonToken(JsonType type, string value)
        {
            Type = type;
            Value = value;
        }
        public override string ToString() => $"{Value} ({Type})";
    }

    public enum JsonType
    {
        StartArray = '[',
        EndArray = ']',
        StartObject = '{',
        EndObject = '}',
        Comma = ',',
        Colon = ':',
        String = '"',
        True = 't',
        False = 'f',
        Null = 'n',
        Number,
    }
}
