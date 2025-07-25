#if !(SILVERLIGHT)
using JetBrains.Annotations;
using QueryGenerator.Util;
using QueryGenerator.Validation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
#endif

namespace QueryGenerator.Core
{
    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for querying data structures that implement <see cref="IQueryable"/>.
    /// It allows dynamic string based querying. Very handy when, at compile time, you don't know the type of queries that will be generated,
    /// or when downstream components only return column names to sort and filter by.
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static class DynamicQueryableExtensions
    {
#if !(SILVERLIGHT)
        private static readonly TraceSource TraceSource = new(nameof(DynamicQueryableExtensions));
#endif

        private static Expression OptimizeExpression(Expression expression)
        {
            if (ExtensibilityPoint.QueryOptimizer != null)
            {
                var optimized = ExtensibilityPoint.QueryOptimizer(expression);

#if !(SILVERLIGHT)
                if (optimized != expression)
                {
                    TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Expression before : {0}", expression);
                    TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Expression after  : {0}", optimized);
                }
#endif
                return optimized;
            }

            return expression;
        }
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="config">The <see cref="ParsingConfig"/>.</param>
        /// <param name="selector">A projection string expression to apply to each element.</param>
        /// <param name="args">An object array that contains zero or more objects to insert into the predicate as parameters. Similar to the way String.Format formats strings.</param>
        /// <returns>An <see cref="IQueryable"/> whose elements are the result of invoking a projection string on each element of source.</returns>
        /// <example>
        /// <code>
        /// var singleField = queryable.Select("StringProperty");
        /// var dynamicObject = queryable.Select("new (StringProperty1, StringProperty2 as OtherStringPropertyName)");
        /// </code>
        /// </example>
        /// 
         #region Select
        public static IQueryable Select(this IQueryable source, ParsingConfig config, string selector, params object?[] args)
        {
            Check.NotNull(source);
            Check.NotNull(config);
            Check.NotEmpty(selector);

            bool createParameterCtor = config.EvaluateGroupByAtDatabase || SupportsLinqToObjects(config, source);
            LambdaExpression lambda = DynamicExpressionParser.ParseLambda(config, createParameterCtor, source.ElementType, null, selector, args);

            var optimized = OptimizeExpression(Expression.Call(
                typeof(Queryable), nameof(Queryable.Select),
                [source.ElementType, lambda.Body.Type],
                source.Expression, Expression.Quote(lambda))
            );

            return source.Provider.CreateQuery(optimized);
        }

        /// <inheritdoc cref="Select(IQueryable, ParsingConfig, string, object[])"/>
        public static IQueryable Select(this IQueryable source, string selector, params object?[] args)
        {
            return source.Select(ParsingConfig.Default, selector, args);
        }

        /// <summary>
        /// Projects each element of a sequence into a new class of type TResult.
        /// Details see <see href="http://solutionizing.net/category/linq/"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="config">The <see cref="ParsingConfig"/>.</param>
        /// <param name="selector">A projection string expression to apply to each element.</param>
        /// <param name="args">An object array that contains zero or more objects to insert into the predicate as parameters.</param>
        /// <returns>An <see cref="IQueryable{TResult}"/> whose elements are the result of invoking a projection string on each element of source.</returns>
        /// <example>
        /// <code language="cs">
        /// <![CDATA[
        /// var users = queryable.Select<User>("new (Username, Pwd as Password)");
        /// ]]>
        /// </code>
        /// </example>
        public static IQueryable<TResult> Select<TResult>(this IQueryable source, ParsingConfig config, string selector, params object?[] args)
        {
            Check.NotNull(source);
            Check.NotNull(config);
            Check.NotEmpty(selector);

            bool createParameterCtor = config.EvaluateGroupByAtDatabase || SupportsLinqToObjects(config, source);
            LambdaExpression lambda = DynamicExpressionParser.ParseLambda(config, createParameterCtor, source.ElementType, typeof(TResult), selector, args);

            var methodCallExpression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                [source.ElementType, typeof(TResult)],
                source.Expression,
                Expression.Quote(lambda)
            );

            var optimized = OptimizeExpression(methodCallExpression);

            return source.Provider.CreateQuery<TResult>(optimized);
        }

        /// <inheritdoc cref="Select{TResult}(IQueryable, ParsingConfig, string, object[])"/>
        public static IQueryable<TResult> Select<TResult>(this IQueryable source, string selector, params object?[] args)
        {
            return source.Select<TResult>(ParsingConfig.Default, selector, args);
        }

        /// <summary>
        /// Projects each element of a sequence into a new class of type TResult.
        /// Details see http://solutionizing.net/category/linq/ 
        /// </summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="config">The <see cref="ParsingConfig"/>.</param>
        /// <param name="resultType">The result type.</param>
        /// <param name="selector">A projection string expression to apply to each element.</param>
        /// <param name="args">An object array that contains zero or more objects to insert into the predicate as parameters.</param>
        /// <returns>An <see cref="IQueryable"/> whose elements are the result of invoking a projection string on each element of source.</returns>
        /// <example>
        /// <code>
        /// var users = queryable.Select(typeof(User), "new (Username, Pwd as Password)");
        /// </code>
        /// </example>
        public static IQueryable Select(this IQueryable source, ParsingConfig config, Type resultType, string selector, params object?[] args)
        {
            Check.NotNull(source);
            Check.NotNull(config);
            Check.NotNull(resultType, nameof(resultType));
            Check.NotEmpty(selector);

            bool createParameterCtor = config.EvaluateGroupByAtDatabase || SupportsLinqToObjects(config, source);
            LambdaExpression lambda = DynamicExpressionParser.ParseLambda(config, createParameterCtor, source.ElementType, resultType, selector, args);

            var optimized = OptimizeExpression(Expression.Call(
                typeof(Queryable), nameof(Queryable.Select),
                [source.ElementType, resultType],
                source.Expression, Expression.Quote(lambda)));

            return source.Provider.CreateQuery(optimized);
        }

        /// <inheritdoc cref="Select(IQueryable, ParsingConfig, Type, string, object[])"/>
        public static IQueryable Select(this IQueryable source, Type resultType, string selector, params object?[] args)
        {
            return source.Select(ParsingConfig.Default, resultType, selector, args);
        }
        #endregion Select

        #region Count
        private static readonly MethodInfo _count = QueryableMethodFinder.GetMethod(nameof(Queryable.Count));

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <param name="source">The <see cref="IQueryable"/> that contains the elements to be counted.</param>
        /// <example>
        /// <code language="cs">
        /// IQueryable queryable = employees.AsQueryable();
        /// var result = queryable.Count();
        /// </code>
        /// </example>
        /// <returns>The number of elements in the input sequence.</returns>
        public static int Count(this IQueryable source)
        {
            Check.NotNull(source);

            return Execute<int>(_count, source);
        }

        private static readonly MethodInfo _countPredicate = QueryableMethodFinder.GetMethod(nameof(Queryable.Count), 1);

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <param name="source">The <see cref="IQueryable"/> that contains the elements to be counted.</param>
        /// <param name="config">The <see cref="ParsingConfig"/>.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="args">An object array that contains zero or more objects to insert into the predicate as parameters. Similar to the way String.Format formats strings.</param>
        /// <example>
        /// <code language="cs">
        /// IQueryable queryable = employees.AsQueryable();
        /// var result1 = queryable.Count("Income > 50");
        /// var result2 = queryable.Count("Income > @0", 50);
        /// var result3 = queryable.Select("Roles.Count()");
        /// </code>
        /// </example>
        /// <returns>The number of elements in the specified sequence that satisfies a condition.</returns>
        [PublicAPI]
        public static int Count(this IQueryable source, ParsingConfig config, string predicate, params object?[] args)
        {
            Check.NotNull(source);
            Check.NotNull(config);
            Check.NotEmpty(predicate);

            bool createParameterCtor = SupportsLinqToObjects(config, source);
            LambdaExpression lambda = DynamicExpressionParser.ParseLambda(config, createParameterCtor, source.ElementType, null, predicate, args);

            return Execute<int>(_countPredicate, source, lambda);
        }

        /// <inheritdoc cref="Count(IQueryable, ParsingConfig, string, object[])"/>
        public static int Count(this IQueryable source, string predicate, params object?[] args)
        {
            return source.Count(ParsingConfig.Default, predicate, args);
        }

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <param name="source">The <see cref="IQueryable"/> that contains the elements to be counted.</param>
        /// <param name="lambda">A cached Lambda Expression.</param>
        /// <returns>The number of elements in the specified sequence that satisfies a condition.</returns>
        public static int Count(this IQueryable source, LambdaExpression lambda)
        {
            Check.NotNull(source);
            Check.NotNull(lambda);

            return Execute<int>(_countPredicate, source, lambda);
        }
        #endregion Count

        #region Private Helpers
        private static bool SupportsLinqToObjects(ParsingConfig config, IQueryable query)
        {
            return config.QueryableAnalyzer.SupportsLinqToObjects(query);
        }

        private static IQueryable CreateQuery(MethodInfo operatorMethodInfo, IQueryable source)
        {
            if (operatorMethodInfo.IsGenericMethod)
            {
                operatorMethodInfo = operatorMethodInfo.MakeGenericMethod(source.ElementType);
            }

            var optimized = OptimizeExpression(Expression.Call(null, operatorMethodInfo, source.Expression));
            return source.Provider.CreateQuery(optimized);
        }

        private static IQueryable CreateQuery(MethodInfo operatorMethodInfo, IQueryable source, LambdaExpression expression)
            => CreateQuery(operatorMethodInfo, source, Expression.Quote(expression));

        private static IQueryable CreateQuery(MethodInfo operatorMethodInfo, IQueryable source, Expression expression)
        {
            operatorMethodInfo = operatorMethodInfo.GetGenericArguments().Length == 2
                    ? operatorMethodInfo.MakeGenericMethod(source.ElementType, typeof(object))
                    : operatorMethodInfo.MakeGenericMethod(source.ElementType);

            return source.Provider.CreateQuery(Expression.Call(null, operatorMethodInfo, source.Expression, expression));
        }

        private static TResult Execute<TResult>(MethodInfo operatorMethodInfo, IQueryable source)
        {
            if (operatorMethodInfo.IsGenericMethod)
            {
                operatorMethodInfo = operatorMethodInfo.MakeGenericMethod(source.ElementType);
            }

            var optimized = OptimizeExpression(Expression.Call(null, operatorMethodInfo, source.Expression));
            var result = source.Provider.Execute(optimized)!;

            return ConvertResultIfNeeded<TResult>(result);
        }

        private static TResult Execute<TResult>(MethodInfo operatorMethodInfo, IQueryable source, Expression expression)
        {
            operatorMethodInfo = operatorMethodInfo.GetGenericArguments().Length == 2
                    ? operatorMethodInfo.MakeGenericMethod(source.ElementType, typeof(TResult))
                    : operatorMethodInfo.MakeGenericMethod(source.ElementType);

            var optimized = OptimizeExpression(Expression.Call(null, operatorMethodInfo, source.Expression, expression));
            var result = source.Provider.Execute(optimized)!;

            return ConvertResultIfNeeded<TResult>(result);
        }

        private static TResult ConvertResultIfNeeded<TResult>(object result)
        {
            if (result.GetType() == typeof(TResult))
            {
                return (TResult)result;
            }

            return (TResult?)Convert.ChangeType(result, typeof(TResult))!;
        }
        #endregion Private Helpers

        #region Where
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IQueryable{TSource}"/> to filter.</param>
        /// <param name="config">The <see cref="ParsingConfig"/>.</param>
        /// <param name="predicate">An expression string to test each element for a condition.</param>
        /// <param name="args">An object array that contains zero or more objects to insert into the predicate as parameters. Similar to the way String.Format formats strings.</param>
        /// <returns>A <see cref="IQueryable{TSource}"/> that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <example>
        /// <code language="cs">
        /// var result1 = queryable.Where("NumberProperty = 1");
        /// var result2 = queryable.Where("NumberProperty = @0", 1);
        /// var result3 = queryable.Where("StringProperty = null");
        /// var result4 = queryable.Where("StringProperty = \"abc\"");
        /// var result5 = queryable.Where("StringProperty = @0", "abc");
        /// </code>
        /// </example>
        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, ParsingConfig config, string predicate, params object?[] args)
        {
            return (IQueryable<TSource>)((IQueryable)source).Where(config, predicate, args);
        }

        /// <inheritdoc cref="Where{TSource}(IQueryable{TSource}, ParsingConfig, string, object[])"/>
        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, string predicate, params object?[] args)
        {
            return source.Where(ParsingConfig.Default, predicate, args);
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <param name="source">A <see cref="IQueryable"/> to filter.</param>
        /// <param name="config">The <see cref="ParsingConfig"/>.</param>
        /// <param name="predicate">An expression string to test each element for a condition.</param>
        /// <param name="args">An object array that contains zero or more objects to insert into the predicate as parameters. Similar to the way String.Format formats strings.</param>
        /// <returns>A <see cref="IQueryable"/> that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <example>
        /// <code>
        /// var result1 = queryable.Where("NumberProperty = 1");
        /// var result2 = queryable.Where("NumberProperty = @0", 1);
        /// var result3 = queryable.Where("StringProperty = null");
        /// var result4 = queryable.Where("StringProperty = \"abc\"");
        /// var result5 = queryable.Where("StringProperty = @0", "abc");
        /// </code>
        /// </example>
        public static IQueryable Where(this IQueryable source, ParsingConfig config, string predicate, params object?[] args)
        {
            Check.NotNull(source);
            Check.NotNull(config);
            Check.NotEmpty(predicate);

            bool createParameterCtor = SupportsLinqToObjects(config, source);
            LambdaExpression lambda = DynamicExpressionParser.ParseLambda(config, createParameterCtor, source.ElementType, null, predicate, args);

            var optimized = OptimizeExpression(Expression.Call(typeof(Queryable), nameof(Queryable.Where), [source.ElementType], source.Expression, Expression.Quote(lambda)));
            return source.Provider.CreateQuery(optimized);
        }

        /// <inheritdoc cref="Where(IQueryable, ParsingConfig, string, object[])"/>
        public static IQueryable Where(this IQueryable source, string predicate, params object?[] args)
        {
            return source.Where(ParsingConfig.Default, predicate, args);
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <param name="source">A <see cref="IQueryable"/> to filter.</param>
        /// <param name="lambda">A cached Lambda Expression.</param>
        /// <returns>A <see cref="IQueryable"/> that contains elements from the input sequence that satisfy the condition specified by LambdaExpression.</returns>
        public static IQueryable Where(this IQueryable source, LambdaExpression lambda)
        {
            Check.NotNull(source);
            Check.NotNull(lambda);

            var optimized = OptimizeExpression(Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { source.ElementType }, source.Expression, Expression.Quote(lambda)));
            return source.Provider.CreateQuery(optimized);
        }

        /// <inheritdoc cref="Where(IQueryable, LambdaExpression)"/>
        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, LambdaExpression lambda)
        {
            return (IQueryable<TSource>)((IQueryable)source).Where(lambda);
        }
        #endregion

        #region Skip
        private static readonly MethodInfo _skip = QueryableMethodFinder.GetMethod(nameof(Queryable.Skip), 1);

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        /// <param name="source">A <see cref="IQueryable"/> to return elements from.</param>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>A <see cref="IQueryable"/> that contains elements that occur after the specified index in the input sequence.</returns>
        public static IQueryable Skip(this IQueryable source, int count)
        {
            Check.NotNull(source);
            Check.Condition(count, x => x >= 0, nameof(count));

            //no need to skip if count is zero
            if (count == 0)
                return source;

            return CreateQuery(_skip, source, Expression.Constant(count));
        }
        #endregion Skip

        #region Take
        private static readonly MethodInfo _take = QueryableMethodFinder.GetMethodWithIntParameter(nameof(Queryable.Take));
        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>A <see cref="IQueryable"/> that contains the specified number of elements from the start of source.</returns>
        public static IQueryable Take(this IQueryable source, int count)
        {
            Check.NotNull(source);
            Check.Condition(count, x => x >= 0, nameof(count));

            return CreateQuery(_take, source, Expression.Constant(count));
        }
        #endregion Take
    }
}