using QueryGenerator.Core;
using QueryGenerator.Entities;
using System.Linq.Expressions;

namespace QueryGenerator.Extensions
{
    public static class QueryableExtensions
    {
        private static readonly List<int> AllowedPageSizes = [5, 10, 20, 50, 100];
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> source, Type entityType, string? sortBy, bool? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return source;

            var parameter = Expression.Parameter(entityType, "x");
            var property = IgnoreCaseHelper.BuildSafePropertyAccessExpression(parameter, sortBy, out var propertyType);
            if (property == null)
                return source;

            var lambda = Expression.Lambda(property, parameter);
            var methodName = sortDirection ?? true ? "OrderBy" : "OrderByDescending";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                [entityType, propertyType],
                source.Expression,
                Expression.Quote(lambda)
            );

            return (IQueryable<T>)source.Provider.CreateQuery(resultExpression);
        }

        public static PaginationResult ApplyPagination(this IQueryable query, PaginationRequest? request)
        {
            request ??= new PaginationRequest();

            var take = AllowedPageSizes.Contains(request.Take) && request.Take > 0
                                                                 ? request.Take : AllowedPageSizes.First();
            var page = request.Page < 1 ? 1 : request.Page;

            var totalCount = query.Count();

            if (request.IsInfiniteScroll)
            {
                take = AllowedPageSizes.Contains(take) ? take : 5;
            }

            var items = query
                .Skip((page - 1) * take)
                .Take(take)
                .ToDynamicList();

            return new PaginationResult
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                Take = take
            };
        }
    }
}