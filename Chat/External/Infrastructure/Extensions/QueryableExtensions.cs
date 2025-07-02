using System.Linq.Expressions;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static Task<PagedList<T>> ToPagedList<T>(this IQueryable<T> query,
        int skip, int take, CancellationToken cancellationToken = default)
        where T : class
    {
        return query.ToPagedList((SortExpression[])null!, skip, take, cancellationToken);
    }

    public static Task<PagedList<T>> ToPagedList<T>(this IQueryable<T> query,
        SortExpression sortExpression, int skip, int take, CancellationToken cancellationToken = default)
        where T : class
    {
        var expressions = sortExpression == null
            ? null
            : new[] { sortExpression };

        return query.ToPagedList(expressions, skip, take, cancellationToken);
    }

    public static async Task<PagedList<T>> ToPagedList<T>(this IQueryable<T> query,
        IList<SortExpression>? sortExpressions, int skip, int take, CancellationToken cancellationToken = default)
        where T : class
    {
        return await query.ToPagedListCore(
            skip,
            take,
            true,
            sortExpressions,
            cancellationToken);
    }

    public static Task<PagedList<T>> ToPagedList<T>(
        this IQueryable<T> query,
        PageSettings pagedRequest,
        CancellationToken cancellationToken
    )
        where T : class
    {
        return query.ToPagedListCore(
            pagedRequest.Skip,
            pagedRequest.Take,
            true,
            null,
            cancellationToken);
    }

    private static async Task<PagedList<T>> ToPagedListCore<T>(
        this IQueryable<T> query,
        int skip,
        int take,
        bool includeTotalCount,
        IList<SortExpression>? sortExpressions,
        CancellationToken cancellationToken
    )
        where T : class
    {
        var result = new PagedList<T>();

        if (includeTotalCount)
        {
            result.TotalCount = await query.CountAsync(cancellationToken);
        }

        if (sortExpressions is null || sortExpressions.Count == 0)
        {
            var entityInterfaces = typeof(T).GetInterfaces();

            if (entityInterfaces.Any(i =>
                    i == typeof(IEntity)))
            {
                sortExpressions =
                [
                    new SortExpression
                    {
                        Direction = SortDirection.Asc,
                        PropertyName = nameof(IEntity.Id)
                    }
                ];
            }
        }

        if (take > 0)
        {
            var orderedQuery = sortExpressions != null 
                ? query.OrderByExpressions(sortExpressions) 
                : query;
                
            result.Items = await orderedQuery
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        return result;
    }

    private static IQueryable<T> OrderByExpressions<T>(this IQueryable<T> query, IList<SortExpression> sortExpressions)
        where T : class
    {
        if (sortExpressions == null || !sortExpressions.Any())
            return query;

        var firstExpression = sortExpressions[0];
        var orderedQuery = firstExpression.Direction == SortDirection.Asc
            ? query.OrderBy(firstExpression.PropertyName)
            : query.OrderByDescending(firstExpression.PropertyName);

        return sortExpressions.Skip(1)
            .Aggregate(orderedQuery,
                (current, sortExpression) =>
                    sortExpression.Direction == SortDirection.Asc
                        ? current.ThenBy(sortExpression.PropertyName)
                        : current.ThenByDescending(sortExpression.PropertyName));
    }

    private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
    {
        return source.OrderByCore(propertyName, false);
    }

    private static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
    {
        return source.OrderByCore(propertyName, true);
    }

    private static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
    {
        return source.ThenByCore(propertyName, false);
    }

    private static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string propertyName)
    {
        return source.ThenByCore(propertyName, true);
    }

    private static IOrderedQueryable<T> OrderByCore<T>(this IQueryable<T> source, string propertyName, bool descending)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var methodCall = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            source.Expression,
            Expression.Quote(lambda));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(methodCall);
    }

    private static IOrderedQueryable<T> ThenByCore<T>(this IOrderedQueryable<T> source, string propertyName, bool descending)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "ThenByDescending" : "ThenBy";
        var methodCall = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            source.Expression,
            Expression.Quote(lambda));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(methodCall);
    }
} 