using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProducer;
using System.Linq;

namespace SqlProducerTests
{
    [TestClass]
    public class MultipleQueryGenerator
    {
        class SelectSomething : Select
        {
            public SelectSomething ByIdInRange(int[] ids)
            {
                Tree.AddRootTable("stub l")
                    .AddColumn("l.id", "Id")
                    .AddColumn("l.stub_name", "Name")
                    .AddColumn("l.stub_reg_date", "RegDate")
                    .AddColumn("l.stub_status_id", "stubStatusId")
                    .AddAndClause("l.id in :Id")
                    .AddParameter(":Id", ids);

                return this;
            }
        }

        private const int BATCH_SIZE = 10;

        [TestMethod]
        public void CREATE_QUERY_MULTIPLE_TIMES()
        {
            int[] id = Enumerable.Range(1, 100).ToArray();

            var query = new SelectSomething();
            QueryObject objectQuery = null;
            for (int i = 0; i < id.Length; i = i + BATCH_SIZE)
            {
                var ids = id.Skip(i).Take(BATCH_SIZE).ToArray();
                objectQuery = query.ByIdInRange(ids).CreateQuery();
            }
            char[] delimiterChars = { ' ', ',', '.', ':', '\r', '\n' };
            string[] words = objectQuery.SqlString.Split(delimiterChars);

            var selectOccur = words.Where(x => string.Equals(x, "SELECT")).Count();

            Assert.IsTrue(selectOccur == 1);
        }
    }
}
