using System;
using System.Collections.Generic;
using System.Linq;

namespace Salaros.vTiger.WebService
{
    public class QueryCondition
    {
        public QueryCondition(string column, string value, ExpressionType expressionType = ExpressionType.Equals)
        {
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException("Must be a non-empty string.", nameof(column));

            Column = column;
            Value = value;
            ExpressionType = expressionType;
        }

        public QueryCondition(string column, IEnumerable<string> values)
            : this(
                  column, 
                  string.Join(", ", values?.Select(v => (v != null) ? $"'{v}'" : "null")),
                  ExpressionType.In
             )
        {
        }

        public string Column { get; }

        public string Value { get; }

        public ExpressionType ExpressionType { get; }
    }
}