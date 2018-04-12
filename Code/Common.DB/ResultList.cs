using System;
using System.Collections.Generic;

namespace Common.DB
{
    [Serializable]
    public class ResultList : List<NVCollection>
    {
        public ResultList()
            : base(10)
        {

        }
        public ResultList(int capacity)
            : base(capacity)
        {

        }

        public int Page { get; set; }
        public int RecordCount { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
    }
}