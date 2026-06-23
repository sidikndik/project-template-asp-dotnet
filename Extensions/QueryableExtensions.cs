using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyApi.DTOs;

namespace MyApi.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            var stringProperties = typeof(T).GetProperties()
                .Where(property => property.PropertyType == typeof(string))
                .ToList();

            if (stringProperties.Count == 0) return query;

            var parameter = Expression.Parameter(typeof(T), "item");
            var searchValue = Expression.Constant(search.ToLower());
            Expression? predicate = null;

            foreach (var property in stringProperties)
            {
                var propertyAccess = Expression.Property(parameter, property);
                var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                var toLower = Expression.Call(propertyAccess, nameof(string.ToLower), Type.EmptyTypes);
                var contains = Expression.Call(toLower, nameof(string.Contains), Type.EmptyTypes, searchValue);
                var condition = Expression.AndAlso(notNull, contains);

                predicate = predicate == null ? condition : Expression.OrElse(predicate, condition);
            }

            if (predicate == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0) return query;

            foreach (var filter in filters)
            {
                var property = typeof(T).GetProperties()
                    .FirstOrDefault(item => string.Equals(item.Name, filter.Key, StringComparison.OrdinalIgnoreCase));

                if (property == null || string.IsNullOrWhiteSpace(filter.Value)) continue;

                query = ApplyFilter(query, property.Name, filter.Value, property.PropertyType);
            }

            return query;
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return query;

            var property = typeof(T).GetProperties()
                .FirstOrDefault(item => string.Equals(item.Name, sortBy, StringComparison.OrdinalIgnoreCase));

            if (property == null) return query;

            var parameter = Expression.Parameter(typeof(T), "item");
            var propertyAccess = Expression.Property(parameter, property);
            var keySelector = Expression.Lambda(propertyAccess, parameter);
            var methodName = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? nameof(Queryable.OrderByDescending)
                : nameof(Queryable.OrderBy);

            var result = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(keySelector));

            return query.Provider.CreateQuery<T>(result);
        }

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, QueryParameters parameters)
        {
            var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
            var pageSize = parameters.PageSize < 1 ? 10 : parameters.PageSize;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        private static IQueryable<T> ApplyFilter<T>(
            IQueryable<T> query,
            string propertyName,
            string filterValue,
            Type propertyType)
        {
            var parameter = Expression.Parameter(typeof(T), "item");
            var propertyAccess = Expression.Property(parameter, propertyName);

            if (propertyType == typeof(string))
            {
                var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                var toLower = Expression.Call(propertyAccess, nameof(string.ToLower), Type.EmptyTypes);
                var contains = Expression.Call(
                    toLower,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    Expression.Constant(filterValue.ToLower()));

                var stringLambda = Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(notNull, contains),
                    parameter);

                return query.Where(stringLambda);
            }

            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (!TryConvertValue(filterValue, targetType, out var convertedValue)) return query;

            Expression left = targetType == propertyType ? propertyAccess : Expression.Convert(propertyAccess, targetType);
            var constant = Expression.Constant(convertedValue, targetType);
            var equal = Expression.Equal(left, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

            return query.Where(lambda);
        }

        private static bool TryConvertValue(string value, Type targetType, out object? convertedValue)
        {
            convertedValue = null;

            if (targetType == typeof(Guid))
            {
                if (!Guid.TryParse(value, out var guidValue)) return false;

                convertedValue = guidValue;
                return true;
            }

            if (targetType.IsEnum)
            {
                try
                {
                    convertedValue = Enum.Parse(targetType, value, ignoreCase: true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                convertedValue = Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
