using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BusterWood.Json
{
    public static class Extensions
    {
        public static void WriteJson(this IEnumerable<Row> rows, TextWriter output)
        {
            var jw = new JsonWriter(output);
            WriteMultipleRows(jw, rows);

        }
        public static void WriteMultipleRows(JsonWriter output, IEnumerable<Row> rows)
        {
            output.BeginArray().WriteLine();
            bool firstRow = true;
            foreach (var row in rows)
            {
                if (firstRow)
                    firstRow = false;
                else
                    output.Comma().WriteLine();

                WriteRow(output, row);
            }
            output.WriteLine();
            output.EndArray();
        }

        private static void WriteRow(JsonWriter output, Row row)
        {
            output.BeginObject();
            bool firstVal = true;
            foreach (var cv in row)
            {
                if (firstVal)
                    firstVal = false;
                else
                    output.Comma();

                output.String(cv.Name).Colon();
                if (cv.Column.Type is Schema)
                    WriteMultipleRows(output, (IEnumerable<Row>)cv.Value);
                else
                    output.Write(cv.Value);
            }
            output.EndObject();
        }
    }

    public class JsonWriter
    {
        readonly TextWriter inner;
        readonly StringBuilder padding = new StringBuilder(4);
        Token last;
        private int _indent;

        public JsonWriter(TextWriter inner)
        {
            this.inner = inner;
        }

        public int Indent
        {
            get { return _indent; }
            set
            {
                _indent = value;
                padding.Clear();
                for (int i = 0; i < _indent; i++)
                {
                    padding.Append(' ');
                }
            }
        }

        public JsonWriter BeginArray()
        {
            WriteNextIndent();
            inner.Write("[ ");
            last = Token.Begin;
            return this;
        }

        public JsonWriter EndArray()
        {
            if (last == Token.NewLine)
            {
                Indent--;
                WriteNextIndent();
                inner.Write("]");
            }
            else
                inner.Write(" ]");
            last = Token.End;
            return this;
        }

        public JsonWriter BeginObject()
        {
            WriteNextIndent();
            inner.Write("{ ");
            last = Token.Begin;
            return this;
        }

        public JsonWriter EndObject()
        {
            if (last == Token.NewLine)
            {
                Indent--;
                WriteNextIndent();
                inner.Write("}");
            }
            else
                inner.Write(" }");
            last = Token.End;
            return this;
        }

        public JsonWriter Write(object val)
        {
            if (val == null)
                return Null();
            if (val is string)
                return String((string)val);
            if (val is decimal)
                return Number((decimal)val);
            if (val is int)
                return Number((int)val);
            if (val is DateTime)
                return String((DateTime)val);
            if (val is DateTimeOffset)
                return String((DateTimeOffset)val);
            if (val is bool)
                return Bool((bool)val);
            throw new NotSupportedException(val.GetType().Name);
        }

        public JsonWriter Null()
        {
            WriteNextIndent();
            inner.Write("null");
            last = 0;
            return this;
        }

        public JsonWriter Bool(bool val)
        {
            WriteNextIndent();
            inner.Write(val ? "true" : "false");
            last = 0;
            return this;
        }

        public JsonWriter String(string content)
        {
            WriteNextIndent();
            if (content == null)
                inner.Write("null");
            else
                inner.Write($"\"{content}\""); //TODO: string escaping
            last = 0;
            return this;
        }

        public JsonWriter String(DateTime content)
        {
            WriteNextIndent();
            inner.Write($"\"{content:u}\"");
            last = 0;
            return this;
        }

        public JsonWriter String(DateTimeOffset content)
        {
            WriteNextIndent();
            inner.Write($"\"{content:u}\"");
            last = 0;
            return this;
        }

        public JsonWriter Number(int num)
        {
            WriteNextIndent();
            inner.Write(num);
            last = 0;
            return this;
        }

        public JsonWriter Number(decimal num)
        {
            WriteNextIndent();
            inner.Write(num);
            last = 0;
            return this;
        }

        public JsonWriter Colon()
        {
            WriteNextIndent();
            inner.Write($" : ");
            last = 0;
            return this;
        }

        public JsonWriter Comma()
        {
            WriteNextIndent();
            inner.Write($", ");
            last = 0;
            return this;
        }

        private void WriteNextIndent()
        {
            if (last == Token.NewLine)
            {
                inner.Write(padding);
            }
        }

        public JsonWriter WriteLine()
        {
            inner.WriteLine();
            switch (last)
            {
                case Token.Begin:
                    Indent++;
                    break;
                case Token.End:
                    Indent--;
                    break;
                default:
                    break;
            }
            last = Token.NewLine;
            return this;

        }

        enum Token
        {
            Begin = 1,
            End,
            NewLine,
        }
    }

}
