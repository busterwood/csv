using System.Linq;
using System.Collections.Generic;

namespace BusterWood.Data
{
    /// <summary>Materialized sequence that can be efficiently enumerated multiple times</summary>
    public class MaterializedRelation : Relation, IReadOnlyCollection<Row>
    {
        readonly IReadOnlyList<Row> rows;

        public MaterializedRelation(Schema schema, IEnumerable<Row> rows) : base(schema)
        {
            this.rows = rows.ToList(); // this *could* be lazerly created
        }

        protected override IEnumerable<Row> GetSequence() => rows;

        public int Count => rows.Count;
    }
}
