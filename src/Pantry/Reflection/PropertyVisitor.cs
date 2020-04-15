using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pantry.Reflection
{
    /// <summary>
    /// <see cref="ExpressionVisitor"/> that can extract property paths.
    /// </summary>\
    public class PropertyVisitor : ExpressionVisitor
    {
        private IList<MemberInfo> _path = new List<MemberInfo>();

        /// <summary>
        /// Gets the property path as a string.
        /// </summary>
        public string PropertyPath => string.Join(".", _path.Reverse().Select(p => p.Name));

        /// <summary>
        /// Gets the property path as a string.
        /// </summary>
        /// <typeparam name="TSource">The source.</typeparam>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The property path as a string.</returns>
        public static string GetPropertyPath<TSource, TResult>(Expression<Func<TSource, TResult>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var visitor = new PropertyVisitor();
            visitor.Visit(expression.Body);

            return visitor.PropertyPath;
        }

        /// <inheritdoc/>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            _path.Add(node.Member);
            return base.VisitMember(node);
        }
    }
}
