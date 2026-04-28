using System;
using System.Linq;
using System.Linq.Expressions;

namespace MyMvcApp.Areas.MvcDashboardTasks.Controllers
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Performs ordering given a string expression.
        /// </summary>
        /// <param name="query">The query to order.</param>
        /// <param name="orderByExpression">The property (path) to order by. Append " ASC" or " DESC" to be explicit about the ordering direction.
        /// Can contain multiple terms separated by comma, i.e. "Name, Town ASC, Country DESC".
        /// </param>
        /// <see href="https://stackoverflow.com/a/64085775"/>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string orderByExpression)
        {
            string[] array = orderByExpression.Split(',', 2);
            string[] array2 = array[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string propertyName = array2[0];
            string methodName = ((array2.Length != 1) ? (array2[1].Equals("DESC", StringComparison.OrdinalIgnoreCase) ? "OrderByDescending" : "OrderBy") : "OrderBy");
            ParameterExpression parameterExpression = Expression.Parameter(query.ElementType);
            string[] propertyPath = propertyName.Split('.');
            MemberExpression memberExpression = Expression.Property(parameterExpression, propertyPath[0]);
            for (int i = 1; i < propertyPath.Length; i++)
                memberExpression = Expression.Property(memberExpression, propertyPath[i]);
            MethodCallExpression expression = Expression.Call(typeof(Queryable), methodName, new Type[2] { query.ElementType, memberExpression.Type }, query.Expression, Expression.Quote(Expression.Lambda(memberExpression, parameterExpression)));
            if (array.Length == 1)
            {
                return (IOrderedQueryable<T>)query.Provider.CreateQuery(expression);
            }

            return ((IOrderedQueryable<T>)query.Provider.CreateQuery(expression)).ThenBy(array[1]);
        }

        /// <summary>
        /// Performs further ordering given a string expression.
        /// </summary>
        /// <param name="query">The query to order.</param>
        /// <param name="orderByExpression">The property (path) to order by. Append " ASC" or " DESC" to be explicit about the ordering direction.
        /// Can contain multiple terms separated by comma, i.e. "Name, Town ASC, Country DESC".
        /// </param>
        /// <see href="https://stackoverflow.com/a/64085775"/>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string orderByExpression)
        {
            string propertyName, orderByMethod;
            string[] terms = orderByExpression.Split(',', 2);
            string[] strs = terms[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            propertyName = strs[0];

            if (strs.Length == 1)
                orderByMethod = "ThenBy";
            else
                orderByMethod = strs[1].Equals("DESC", StringComparison.OrdinalIgnoreCase) ? "ThenByDescending" : "ThenBy";

            ParameterExpression pe = Expression.Parameter(query.ElementType);
            string[] propertyPath = propertyName.Split('.');
            MemberExpression me = Expression.Property(pe, propertyPath[0]);
            for (int i = 1; i < propertyPath.Length; i++)
                me = Expression.Property(me, propertyPath[i]);

            MethodCallExpression orderByCall = Expression.Call(typeof(Queryable), orderByMethod, new Type[] { query.ElementType, me.Type }, query.Expression
                , Expression.Quote(Expression.Lambda(me, pe)));

            if (terms.Length == 1)
            {
                return (IOrderedQueryable<T>)query.Provider.CreateQuery(orderByCall);
            }
            else
            {
                return ((IOrderedQueryable<T>)query.Provider.CreateQuery(orderByCall)).ThenBy(terms[1]);
            }
        }
    }
}
