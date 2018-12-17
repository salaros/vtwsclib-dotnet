using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Salaros.vTiger.WebService
{
    public class QueryOperation : OperationBase
    {
        protected IDictionary<IEnumerable<QueryCondition>, string> operationConditions;

        protected IList<string> select;

        protected IList<string> orderBy;

        protected bool isDescending;

        protected long limit, offset;

        internal QueryOperation(Client client, string moduleName)
            : base(client, "query")
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Must be a non-empty string.", nameof(moduleName));

            operationData.Add("elementType", moduleName);
            operationConditions = new Dictionary<IEnumerable<QueryCondition>, string>();
            orderBy = new List<string>();
            select = null;
            isDescending = false;
            limit = 0;
            offset = 0;
        }

        public QueryOperation Select(params string[] select)
        {
            this.select = select ?? new string[0];
            return this;
        }

        public QueryOperation WhereIn(string column, IEnumerable<string> values)
        {
            return Where(new QueryCondition(column, values));
        }

        public QueryOperation Where(string column, string value, ExpressionType expressionType)
        {
            return Where(new QueryCondition(column, value, expressionType));
        }

        public QueryOperation Where(params QueryCondition[] conditions)
        {
            if (conditions == null || !conditions.Any())
                throw new ArgumentException(nameof(conditions));

            operationConditions.Add(conditions, string.Empty);
            return this;
        }

        public QueryOperation AndWhereIn(string column, IEnumerable<string> values)
        {
            return AndWhere(new QueryCondition(column, values));
        }

        public QueryOperation AndWhere(string column, string value, ExpressionType expressionType)
        {
            return AndWhere(new QueryCondition(column, value, expressionType));
        }

        public QueryOperation AndWhere(params QueryCondition[] conditions)
        {
            if (conditions == null || !conditions.Any())
                throw new ArgumentException(nameof(conditions));

            if (operationConditions.Any(c => string.IsNullOrWhiteSpace(c.Value)))
                operationConditions.Add(conditions, "AND");
            else
                operationConditions.Add(conditions, string.Empty);

            return this;
        }

        public QueryOperation OrWhereIn(string column, IEnumerable<string> values)
        {
            return OrWhere(new QueryCondition(column, values));
        }

        public QueryOperation OrWhere(string column, string value, ExpressionType expressionType)
        {
            return OrWhere(new QueryCondition(column, value, expressionType));
        }

        public QueryOperation OrWhere(params QueryCondition[] conditions)
        {
            if (conditions == null || !conditions.Any())
                throw new ArgumentException(nameof(conditions));

            if (operationConditions.Any(c => string.IsNullOrWhiteSpace(c.Value)))
                operationConditions.Add(conditions, "OR");
            else
                operationConditions.Add(conditions, string.Empty);

            return this;
        }

        public QueryOperation OrderBy(params string[] orderBy)
        {
            if (orderBy == null || !orderBy.Any())
                throw new ArgumentException(nameof(orderBy));

            this.orderBy = orderBy ?? new string[0];
            isDescending = false;
            return this;
        }

        public QueryOperation OrderByDesc(params string[] orderBy)
        {
            if (orderBy == null || !orderBy.Any())
                throw new ArgumentException(nameof(orderBy));

            this.orderBy = orderBy ?? new string[0];
            isDescending = true;
            return this;
        }

        public QueryOperation Take(long limit)
        {
            this.limit = limit;
            return this;
        }

        public QueryOperation Skip(long offset)
        {
            this.offset = offset;
            return this;
        }

        public TResult[] GetMany<TResult>(JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            operationData["query"] = CompileQuery();
            return client?.SendRequest<TResult[]>(operationData, HttpMethod.Get, jsonSettings);
        }

        public TResult GetOne<TResult>(JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            return Take(1)
                .GetMany<TResult>(jsonSettings)
                ?.FirstOrDefault();
        }

        public JToken GetMany(JsonSerializerSettings jsonSettings = null)
        {
            operationData["query"] = CompileQuery();
            return client?.SendRequest<JToken>(operationData, HttpMethod.Get, jsonSettings);
        }

        public JToken GetOne(JsonSerializerSettings jsonSettings = null)
        {
            return Take(1).GetMany(jsonSettings);
        }

        internal string CompileQuery()
        {
            var query = (select != null && select.Any())
                ? $"SELECT {string.Join(", ", select)}"
                : "SELECT *";

            query = $"{query} FROM {operationData["elementType"]}";

            if (operationConditions.Any())
            {
                query = $"{query} WHERE";
                foreach (var conditionSet in operationConditions)
                {
                    var conditions = conditionSet.Key?.Select(c => CompileCondition(c)).ToArray();
                    var conditionsString = conditions.Length > 1
                        ? $"({string.Join(" AND ", conditions)})"
                        : string.Join(" AND ", conditions);

                    query = (string.IsNullOrWhiteSpace(conditionSet.Value))
                        ? $"{query} {conditionsString}"
                        : $"{query} {conditionSet.Value} {conditionsString}";
                }
            }

            if (orderBy.Any())
            {
                query = $"{query} ORDER BY {string.Join(",", orderBy)} {(isDescending ? "DESC" : "ASC")}";
            }

            if (limit > 0)
            {
                query = (offset > 0)
                    ? $"{query} LIMIT {offset}, {limit}"
                    : $"{query} LIMIT {limit}";
            }

            return $"{query};";
        }

        private string CompileCondition(QueryCondition condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            var description = condition.ExpressionType.GetDescription();
            if (string.IsNullOrWhiteSpace(description))
                throw new NotImplementedException();

            switch(condition.ExpressionType)
            {
                case var like when new[] { ExpressionType.Contains, ExpressionType.StartsWith, ExpressionType.EndsWith }.Contains(like):
                    if (condition.Value == null)
                        throw new InvalidOperationException(
                            $"You are trying to use {nameof(ExpressionType.Contains)}, {nameof(ExpressionType.StartsWith)}, {nameof(ExpressionType.EndsWith)} with a null value"
                        );

                    return string.Format(description, condition.Column, condition.Value);

                case ExpressionType.In:
                    if (string.IsNullOrWhiteSpace(condition.Value))
                        throw new InvalidOperationException(
                            $"You have to add at least one element to a collection to use with {ExpressionType.In} expression"
                        );

                    return string.Format(description, condition.Column, condition.Value);

                default:
                    var value = (condition.Value == null)
                        ? "null"
                        : $"'{condition.Value}'";
                    return string.Format(description, condition.Column, value);
            }     
        }
    }
}
