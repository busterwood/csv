using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Data
{
    public class Class1 : IHasSchema
    {
        public string Text { get; }
        public int Id { get; }
        public DateTime When { get; }

        public Class1(string Text, int Id, DateTime When)
        {
            this.Text = Text;
            this.Id = Id;
            this.When = When;
        }

        public IEnumerator<ColumnValue> GetEnumerator()
        {
            yield return new ColumnValue(new Column(nameof(Id), typeof(int)), Id);
            yield return new ColumnValue(new Column(nameof(Text), typeof(string)), Text);
            yield return new ColumnValue(new Column(nameof(When), typeof(DateTime)), When);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(IHasSchema other)
        {
            if (other == null) return false;
            if (other.SchemaHashCode() != SchemaHashCode()) return false;
            var e = other.GetEnumerator();

            if (!e.MoveNext()) return false;
            if (!Equals(Id, e.Current.Value)) return false;

            if (!e.MoveNext()) return false;
            if (!Equals(Text, e.Current.Value)) return false;

            if (!e.MoveNext()) return false;
            if (!Equals(When, e.Current.Value)) return false;

            return true;
        }

        public int SchemaHashCode()
        {
            return 1234;
        }
    }

    public class RestrictClass1
    {
        readonly Class1 inner;
        public string Text => inner.Text;
        public int Id => inner.Id;

        public RestrictClass1(Class1 inner)
        {
            this.inner = inner;
        }
    }

    public class ProjectClass1
    {
        readonly Class1 inner;
        public string Text => inner.Text;
        public int Id => inner.Id;
        public DateTime When => inner.When;
        public int Extra { get; }

        public ProjectClass1(Class1 inner, int extra)
        {
            this.inner = inner;
            this.Extra = extra;
        }
    }
}
