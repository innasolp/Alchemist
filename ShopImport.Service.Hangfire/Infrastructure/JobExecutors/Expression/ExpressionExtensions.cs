using Hangfire.Common;
using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors.Expression;

internal static class ExpressionExtensions
{
    public static Expression<Func<T1, TResult>> BindSecondParameter<T1, T2, TResult>(
        this Expression<Func<T1, T2, TResult>> source, T2 value)
    {
        var parameterT2 = source.Parameters[1];
        var visitor = new ParameterBinder(parameterT2, value);

        var newBody = visitor.Visit(source.Body);

        return System.Linq.Expressions.Expression.Lambda<Func<T1, TResult>>(newBody, source.Parameters[0]);
    }

    public static object?[]? GetArguments<T>(this Expression<T> lambda)
        where T : Delegate
    {
        var methodCall = (MethodCallExpression)lambda.Body;

        return [.. methodCall.Arguments
            .Select(arg => {
                if (arg is ConstantExpression constant)
                    return constant.Value;

                return System.Linq.Expressions.Expression.Lambda(arg).Compile().DynamicInvoke();
            })];
    }
    public static Expression<Func<TArg, TResult>> ToExpression<TArg, TResult>(this Job job)
    {
        ArgumentNullException.ThrowIfNull(job);

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TArg), "t");

        var arguments = job.Method.GetParameters().Select((p, i) =>
        {
            var value = job.Args[i];
            return System.Linq.Expressions.Expression.Constant(value, p.ParameterType);
        });

        var methodCall = System.Linq.Expressions.Expression.Call(parameter, job.Method, arguments);

        return System.Linq.Expressions.Expression.Lambda<Func<TArg, TResult>>(methodCall, parameter);
    }
}