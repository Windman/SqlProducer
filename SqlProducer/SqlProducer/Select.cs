using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlProducer
{
    //TODO Select. Just oracle select. How to extend to other dialects?
    public class Select
    {
        public SqlElement Tree { get; set; }
        public StringBuilder Query { get; set; }
        public string Sql { get { return Query.ToString(); } }

        private const string COMMA = ",";

        public Select()
        {
            Query = new StringBuilder();
            Tree = new SqlElement();
        }

        public QueryObject CountTotal()
        {
            Query.Clear();
            var query = CreateQuery();
            query.SqlString = string.Format("select count(1) from ({0})", query.SqlString);
            return query;
        }

        public QueryObject CreateQuery(int skip, int take)
        {
            Query.Clear();
            var query = CreateQuery();
            query.SqlString = string.Format(@"select * from (select rownum rn, j.* from ({0}) j) where rn > {1} and rn <= {2}", query.SqlString, skip, take + skip);
            return query;
        }

        public QueryObject CreateQuery()
        {
            Query.Clear();

            Dictionary<string, object> dbArgs = new Dictionary<string, object>();

            StringBuilder select = new StringBuilder();
            StringBuilder from = new StringBuilder();
            StringBuilder where = new StringBuilder();
            StringBuilder orderby = new StringBuilder();

            select.AppendLine("SELECT");
            select.Append(Tree.DistinctClause);

            from.AppendLine("FROM ");
            where.AppendLine("WHERE 1=1 ");

            SqlElement temp = Tree;
            while (true)
            {
                var x = temp;
                GenerateQuery(select, from, where, orderby, x, dbArgs);
                temp = x.Child;

                if (temp == null)
                    break;
            }

            var lastCommaIndex = select.ToString().TrimEnd().LastIndexOf(COMMA);

            Query.AppendLine(string.Format("{0} ", select.ToString().Remove(lastCommaIndex)));
            Query.AppendLine(from.ToString());
            Query.AppendLine(where.ToString());
            Query.AppendLine(orderby.ToString());

            return new QueryObject(Query.ToString());
        }

        private void GenerateQuery(StringBuilder select, StringBuilder from, StringBuilder where, StringBuilder orderby, SqlElement x, Dictionary<string, object> dbArgs)
        {
            int length = x.ColumnNames.Count;

            for (int i = 0; i < length; i++)
            {
                select.Append(string.Format("{0} {1}{2} ", x.ColumnNames.ElementAt(i).Key, x.ColumnNames.ElementAt(i).Value, COMMA));
            }

            if (x.JoinType != null)
                from.AppendLine(string.Format("{0} ", x.JoinType));

            from.AppendLine(string.Format("{0} ", x.TableName));

            if (x.KeyCondition != null)
                from.AppendLine(string.Format("{0} {1} ", "ON", x.KeyCondition));

            foreach (var item in x.Clause)
            {
                where.AppendLine(string.Format("{0} {1} ", item.Value, item.Key));
            }

            if (x.OrderByColumns.Count() > 0)
                orderby.AppendLine("ORDER BY");

            foreach (var item in x.OrderByColumns) orderby.AppendLine(string.Format("{0} {1}", item.Key, item.Value));

            foreach (var pair in x.Parameters) dbArgs.Add(pair.Key, pair.Value);
        }
    }
}
