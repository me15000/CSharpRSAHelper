using System.Text;
using System.Text.RegularExpressions;

namespace Common.DB.SQLServer
{
    public class PagerQueryString : IPagerQueryString
    {
        private int absolutePage = 1;
        private string fields = " * ";
        private int pageSize = 10;
        private string sort = " 1 desc ";
        private string table = string.Empty;
        private string where = string.Empty;

        public string GetQueryString()
        {
            int p = this.absolutePage;

            if (this.absolutePage < 1)
            {
                this.absolutePage = 1;
            }

            StringBuilder builder = new StringBuilder();
            if (p > 1)
            {
                builder.Append("select * from (");

                builder.Append("select ");
                builder.Append(this.fields);
                builder.Append(",ROW_NUMBER () OVER (ORDER BY " + this.sort + ") AS RowNumber ");

                builder.Append(" from " + this.table + " with(nolock) ");

                if (!string.IsNullOrWhiteSpace(this.where))
                {
                    builder.Append(" where ");
                    builder.Append(this.where);
                }

                builder.Append(") AS temptb");
                builder.Append(" WHERE temptb.RowNumber BETWEEN " + ((p - 1) * pageSize + 1) + "  AND " + (p * pageSize));
            }
            else
            {
                builder.Append(" select top ");
                builder.Append(this.pageSize);
                builder.Append(" ");
                builder.Append(this.fields);
                builder.Append(" from ");
                builder.Append(this.table);
                builder.Append(" with(nolock)  ");

                if (!string.IsNullOrWhiteSpace(this.where))
                {
                    builder.Append(" where ");
                    builder.Append(this.where);
                }

                builder.Append(" order by ");
                builder.Append(this.sort);
            }
            return builder.ToString();
        }

        public string GetCountQueryString()
        {


            return "select count(1) from " + this.table + " with(nolock)   " + (string.IsNullOrWhiteSpace(this.where) ? string.Empty : " where " + this.where);
        }

        public int AbsolutePage
        {
            get
            {
                return this.absolutePage;
            }

            set
            {
                this.absolutePage = value;
            }
        }

        private Regex reg_zero = new Regex(@"^0,");
        public string Fields
        {
            get
            {
                return this.fields;
            }
            set
            {
                this.fields = reg_zero.Replace(value, string.Empty);
            }
        }

        public int PageSize
        {
            get
            {
                return this.pageSize;
            }
            set
            {
                this.pageSize = value;
            }
        }

        public string Sort
        {
            get
            {
                return this.sort;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.sort = value;
                }
            }
        }

        public string Table
        {
            get
            {
                return this.table;
            }
            set
            {
                this.table = value;
            }
        }

        private Regex reg_where = new Regex(@"^\s*and", RegexOptions.IgnoreCase);
        public string Where
        {
            get
            {
                return this.where;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.where = reg_where.Replace(value, string.Empty);
                }
            }
        }
    }
}