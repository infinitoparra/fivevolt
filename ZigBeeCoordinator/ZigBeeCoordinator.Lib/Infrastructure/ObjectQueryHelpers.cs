using System;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Reflection;

namespace ZigBeeCoordinator.Lib.Infrastructure
{
    public static class ObjectQueryHelpers
    {
        public static ObjectQuery<T> Include<T>(this ObjectQuery<T> query, Expression<Func<T, object>> exp)
        {
            MemberExpression memberExpression = (MemberExpression)exp.Body;
            string path = GetIncludePath(memberExpression);
            return query.Include(path);
        }

        private static string GetIncludePath(MemberExpression memberExpression)
        {
            string path = "";
            if (memberExpression.Expression is MemberExpression)
            {
                path = GetIncludePath((MemberExpression)memberExpression.Expression) + ".";
            }
            PropertyInfo propertyInfo = (PropertyInfo)memberExpression.Member;
            return path + propertyInfo.Name;
        }
    }
}
