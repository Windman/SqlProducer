using System;

namespace SqlProducer
{
    public class QueryObject
    {
        public QueryObject(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql text");

            SqlString = sql;
        }

        public QueryObject(string sql, object queryParams)
            : this(sql)
        {
            Parameters = queryParams;
        }

        public string SqlString { get; set; }

        public object Parameters { get; private set; }

        public void SetNewParameters(object p)
        {
            Parameters = null;
            Parameters = p;
        }

    }
}
