using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace myProvider
{
    internal class QueryTranslator : ExpressionVisitor
    {
        StringBuilder sb;

        internal QueryTranslator()
        {

        }

        internal string Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            return this.sb.ToString();
        }

        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Where")
            {
                sb.Append("SELECT * FROM (");
                this.Visit(node.Arguments[0]);
                sb.Append(") AS T0 WHERE ");
                LambdaExpression lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                this.Visit(lambda.Body);

                return node;
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", node.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", node.NodeType));
            }

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            sb.Append("(");
            this.Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", node.NodeType));
            }

            this.Visit(node.Right);

            sb.Append(")");

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            IQueryable queryable = node.Value as IQueryable;
            if (queryable != null)
            {
                sb.Append("SELECT * FROM ");
                sb.Append(queryable.ElementType.Name);
            }
            else if (node.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(node.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append((bool)node.Value ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(node.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", node.Value));
                    default:
                        sb.Append(node.Value);
                        break;
                }
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                sb.Append(node.Member.Name);
                return node;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", node.Member.Name));
        }
    }
}
