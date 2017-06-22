using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Data
{
    public class Class1
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
