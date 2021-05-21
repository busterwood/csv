using System.Collections.Generic;

namespace BusterWood.Data
{
    public class DerivedRelation : Relation
    {
        readonly IEnumerable<Row> rows;

        public DerivedRelation(Schema schema, IEnumerable<Row> rows) : base(schema)
        {
            this.rows = rows;
        }

        protected override IEnumerable<Row> GetSequence() => rows;
    }

}
