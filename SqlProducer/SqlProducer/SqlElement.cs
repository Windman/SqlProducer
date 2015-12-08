using System.Collections;
using System.Collections.Generic;

namespace SqlProducer
{
    //TODO SqlElement. Алиас присваивать отдельно а не в параметре
    //TODO SqlElement. Если добавляется уже существующая таблица сообщать об ошибке или создвать новый алиас
    public class SqlElement
    {
        internal List<string> TablesInUse { get; set; }
        internal string TableName { get; set; }
        internal Hashtable TablesHash { get; private set; }
        internal string JoinType { get; private set; }
        internal string DistinctClause { get; private set; }
        internal string KeyCondition { get; private set; }

        internal Dictionary<string, string> ColumnNames { get; private set; }
        internal Dictionary<string, string> Clause { get; private set; }
        internal Dictionary<string, object> Parameters { get; private set; }
        internal Dictionary<string, string> OrderByColumns { get; private set; }

        internal SqlElement Child { get; set; }
        internal SqlElement Parent { get; set; }

        public SqlElement(string tablename)
            : this()
        {
            TableName = tablename;
        }

        public SqlElement()
        {
            ColumnNames = new Dictionary<string, string>();
            Clause = new Dictionary<string, string>();
            Parameters = new Dictionary<string, object>();
            OrderByColumns = new Dictionary<string, string>();
            TablesHash = new Hashtable();
        }

        #region PUBLIC ELEMENTS
        public SqlElement Distinct()
        {
            DistinctClause = " DISTINCT ";
            return this;
        }

        public SqlElement AddParameter(string name, object value)
        {
            if (!Parameters.ContainsKey(name))
                Parameters.Add(name, value);
            return this;
        }

        public SqlElement AddLikeParameter(string name, object value)
        {
            var likeEncoded = value.ToString().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]").ToLower();
            Parameters.Add(name, string.Format("{0}{1}{2}", "%", likeEncoded, "%"));
            return this;
        }

        public SqlElement AddOneSideLikeParameter(string name, object value)
        {
            var likeEncoded = value.ToString().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            Parameters.Add(name, string.Format("{0}{1}", likeEncoded, "%"));
            return this;
        }

        public SqlElement AddColumn(string name, string alias)
        {
            if (ColumnNames.ContainsKey(name))
                return this;

            ColumnNames.Add(name, alias);
            return this;
        }

        public SqlElement AddRootTable(string tablename)
        {
            if (TablesHash.ContainsKey(tablename))
                return this;

            TablesHash.Add(tablename, null);

            AddTableName(tablename);
            return this;
        }

        public SqlElement AddTable(string tablename)
        {
            if (TablesHash.ContainsKey(tablename))
                return FindElement(tablename, this);

            TablesHash.Add(tablename, null);

            SqlElement elem = new SqlElement(tablename) { Parent = this };

            if (Child == null)
                Child = elem;
            else
            {
                var child = FindLastChild(Child);
                child.Child = elem;
            }
            return elem;
        }

        public SqlElement AddTableName(string tablename)
        {
            TableName = tablename;
            return this;
        }

        public SqlElement InnerJoin()
        {
            JoinType = "INNER JOIN";
            return this;
        }

        public SqlElement LeftOuterJoin()
        {
            JoinType = "LEFT OUTER JOIN";
            return this;
        }

        public SqlElement AddKeyCondition(string p1, string p2)
        {
            KeyCondition = string.Format("{0} = {1}", p1, p2);
            return this;
        }

        public SqlElement AddOrClause(string column)
        {
            Clause.Add(column, "or");
            return this;
        }

        public SqlElement AddAndClause(string column)
        {
            if (!Clause.ContainsKey(column))
                Clause.Add(column, "and");

            return this;
        }

        public SqlElement OrderBy(string column, bool OrderASC)
        {
            string direction = OrderASC ? "ASC" : "DESC";

            if (!OrderByColumns.ContainsKey(column))
                OrderByColumns.Add(column, direction);

            return this;
        }
        #endregion

        internal SqlElement FindLastChild(SqlElement element)
        {
            if (element.Child != null)
            {
                return FindLastChild(element.Child);
            }
            return element;
        }

        internal SqlElement FindElement(string name, SqlElement element)
        {
            if (!element.TableName.Equals(name))
            {
                return FindElement(name, element.Child);
            }
            return element;
        }
    }
}
