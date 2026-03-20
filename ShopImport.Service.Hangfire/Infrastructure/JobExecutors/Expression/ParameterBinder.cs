using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors.Expression;

internal class ParameterBinder(ParameterExpression parameter, object value) : ExpressionVisitor
{
    private readonly ParameterExpression _parameter = parameter;

    private readonly object _value = value;

    protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
    {
        return node == _parameter ? System.Linq.Expressions.Expression.Constant(_value, _parameter.Type) : base.VisitParameter(node);
    }
}