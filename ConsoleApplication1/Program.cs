using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BusterWood.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchMark1>();
            //var summary = BenchmarkRunner.Run<ArrayOrBinSearch>();
        }

    }

    public class ArrayOrBinSearch
    {
        Schema schema;
        public ArrayOrBinSearch()
        {
            var cols = SetupColumns();
            schema = new Schema("", cols);
        }

        private static Column[] SetupColumns()
        {
            var cols = new Column[10];
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i] = new Column("col" + i, typeof(string));
            }
            return cols;
        }

        [Benchmark]
        public Column[] ForEach()
        {
            var results = new Column[schema.Count];
            for (int i = 0; i < schema.Count; i++)
            {
                results[i] = schema["col" + 1];
            }
            return results;
        }


        //[Benchmark]
        //public Column[] BinarySearch()
        //{
        //    var results = new Column[schema.Count];
        //    for (int i = 0; i < schema.Count; i++)
        //    {
        //        results[i] = schema.BinarySearch("col" + 1);
        //    }
        //    return results;
        //}
    }
    public class BenchMark1
    {
        private const int Max = 1000;

        //[Benchmark]
        //public void RunObject()
        //{
        //    var seq = Enumerable.Range(1, Max).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i) });
        //    foreach (var item in seq)
        //    {
        //        GC.KeepAlive(item.Text);
        //        GC.KeepAlive(item.Id);
        //        GC.KeepAlive(item.OptId);
        //        GC.KeepAlive(item.When);
        //    }
        //}

        [Benchmark]
        public void RunDynamicObj()
        {
            var seq = Enumerable.Range(1, Max).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i), Apple = "apple", Pair = "pair", Orange = "orange", Bear = "bear" });
            foreach (dynamic item in seq)
            {
                GC.KeepAlive(item.Text);
                GC.KeepAlive(item.Id);
                GC.KeepAlive(item.OptId);
                GC.KeepAlive(item.When);
            }
        }

        [Benchmark]
        public void RunExpressionRow()
        {
            var seq = Enumerable.Range(1, Max).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i), Apple = "apple", Pair = "pair", Orange = "orange", Bear = "bear" }).ToRelation();
            foreach (var item in seq)
            {
                GC.KeepAlive(item.Get("Text"));
                GC.KeepAlive(item.Get("Id"));
                GC.KeepAlive(item.Get("OptId"));
                GC.KeepAlive(item.Get("When"));
            }
        }

        //[Benchmark]
        //public void RunExpressionMap()
        //{
        //    var seq = Enumerable.Range(1, Max).Select(i => new { Text = "hello", Id = i, OptId = (int?)i, When = new DateTime(i), Apple="apple", Pair="pair", Orange="orange", Bear="bear" }).ToRelation("test");
        //    foreach (var item in seq)
        //    {
        //        GC.KeepAlive(item.Get("Text"));
        //        GC.KeepAlive(item.Get("Id"));
        //        GC.KeepAlive(item.Get("OptId"));
        //        GC.KeepAlive(item.Get("When"));
        //    }
        //}
    }
}
