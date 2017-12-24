using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace myProvider
{
    public class DbQueryProvider : QueryProvider
    {
        DbConnection connection;

        public DbQueryProvider(DbConnection connection)
        {
            this.connection = connection;
        }

        public override object Execute(Expression expression)
        {
            DbCommand cmd = this.connection.CreateCommand();
            cmd.CommandText = this.Translate(expression);

            DbDataReader reader = cmd.ExecuteReader();

            Type elementType = TypeSystem.GetElementType(expression.Type);

            return Activator.CreateInstance(
                typeof(ObjectReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { reader },
                null);
        }

        public override string GetQueryText(Expression expression)
        {
            return this.Translate(expression);
        }

        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator().Translate(expression);
        }
    }
}
