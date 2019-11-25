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

        /// <summary>Initializes a new instance of the <see cref="QueryOperation"/> class.</summary>
        /// <param name="client">The client.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <exception cref="ArgumentException">Must be a non-empty string. - moduleName</exception>
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

        /// <summary>Adds WHERE <field> IN (item1, item2) type condition.</summary>
        /// <param name="column">The column.</param>
        /// <param name="values">The collection of values.</param>
        /// <returns>Query object</returns>
        public QueryOperation WhereIn(string column, IEnumerable<string> values)
        {
            return Where(new QueryCondition(column, values));
        }

        /// <summary>Adds a WHERE condition with custom expression type.</summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="expressionType">Type of the expression.</param>
        /// <returns>Query object</returns>
        public QueryOperation Where(string column, string value, ExpressionType expressionType = ExpressionType.Equals)
        {
            return Where(new QueryCondition(column, value, expressionType));
        }

        /// <summary>Adds WHERE conditions.</summary>
        /// <param name="conditions">The conditions.</param>
        /// <returns>Query object</returns>
        /// <exception cref="ArgumentException">conditions</exception>
        public QueryOperation Where(params QueryCondition[] conditions)
        {
            if (conditions == null || !conditions.Any())
                throw new ArgumentException(nameof(conditions));

            operationConditions.Add(conditions, string.Empty);
            return this;
        }

        /// <summary>Adds a WHERE IN condition ANDed to previous ones.</summary>
        /// <param name="column">The column.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public QueryOperation AndWhereIn(string column, IEnumerable<string> values)
        {
            return AndWhere(new QueryCondition(column, values));
        }

        /// <summary>Adds a WHERE condition ANDed to previous ones.</summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="expressionType">Type of the expression.</param>
        /// <returns></returns>
        public QueryOperation AndWhere(string column, string value, ExpressionType expressionType = ExpressionType.Equals)
        {
            return AndWhere(new QueryCondition(column, value, expressionType));
        }

        /// <summary>Adds WHERE conditions ANDed to previous ones.</summary>
        /// <param name="conditions">The conditions.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">conditions</exception>
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

        /// <summary>Adds a WHERE IN condition ORed to previous ones.</summary>
        /// <param name="column">The column.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public QueryOperation OrWhereIn(string column, IEnumerable<string> values)
        {
            return OrWhere(new QueryCondition(column, values));
        }

        /// <summary>Adds a WHERE condition ORed to previous ones.</summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="expressionType">Type of the expression.</param>
        /// <returns>Query object</returns>
        public QueryOperation OrWhere(string column, string value, ExpressionType expressionType = ExpressionType.Equals)
        {
            return OrWhere(new QueryCondition(column, value, expressionType));
        }

        /// <summary>Adds WHERE conditions ORed to previous ones.</summary>
        /// <param name="conditions">The conditions.</param>
        /// <returns>Query object</returns>
        /// <exception cref="ArgumentException">conditions</exception>
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

        /// <summary>Orders (ascending) the items by properties.</summary>
        /// <param name="orderBy">The list of properties used for ordering.</param>
        /// <returns>Query object</returns>
        /// <exception cref="ArgumentException">orderBy</exception>
        public QueryOperation OrderBy(params string[] orderBy)
        {
            if (orderBy == null || !orderBy.Any())
                throw new ArgumentException(nameof(orderBy));

            this.orderBy = orderBy ?? new string[0];
            isDescending = false;
            return this;
        }

        /// <summary>Orders (descending) the items by properties.</summary>
        /// <param name="orderBy">The list of properties used for ordering.</param>
        /// <returns>Query object</returns>
        /// <exception cref="ArgumentException">orderBy</exception>
        public QueryOperation OrderByDesc(params string[] orderBy)
        {
            if (orderBy == null || !orderBy.Any())
                throw new ArgumentException(nameof(orderBy));

            this.orderBy = orderBy ?? new string[0];
            isDescending = true;
            return this;
        }

        /// <summary>Takes the specified amount of items.</summary>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public QueryOperation Take(long limit)
        {
            this.limit = limit;
            return this;
        }

        /// <summary>Skips the specified number of items.</summary>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public QueryOperation Skip(long offset)
        {
            this.offset = offset;
            return this;
        }

        /// <summary>Generates select the query.</summary>
        /// <param name="select">Object properties to select.</param>
        /// <returns>The select query object</returns>
        public QueryOperation SelectQuery(params string[] select)
        {
            this.select = select ?? new string[0];
            return this;
        }

        /// <summary>Selects the specified select.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="select">Object properties to select.</param>
        /// <returns></returns>
        public TResult[] Select<TResult>(params string[] select)
            where TResult : class
        {
            return Select<TResult>(null, select);
        }

        /// <summary>Selects the specified JSON settings.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="jsonSettings">The JSON settings.</param>
        /// <param name="select">Object properties to select.</param>
        /// <returns></returns>
        public TResult[] Select<TResult>(JsonSerializerSettings jsonSettings, params string[] select)
            where TResult : class
        {
            this.select = select ?? new string[0];
            operationData["query"] = CompileQuery();
            return client?.SendRequest<TResult[]>(operationData, HttpMethod.Get, jsonSettings);
        }

        /// <summary>Selects the first.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="jsonSettings">The JSON settings.</param>
        /// <returns></returns>
        public TResult SelectFirst<TResult>(JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            return Take(1)
                .Select<TResult>(jsonSettings)
                ?.FirstOrDefault();
        }

        /// <summary>Selects the specified select.</summary>
        /// <param name="select">Object properties to select.</param>
        /// <returns></returns>
        public ICollection<JObject> Select(params string[] select)
        {
            return Select(null, select);
        }

        /// <summary>Selects the specified JSON settings.</summary>
        /// <param name="jsonSettings">The JSON settings.</param>
        /// <param name="select">Object properties to select.</param>
        /// <returns></returns>
        public ICollection<JObject> Select(JsonSerializerSettings jsonSettings = null, params string[] select)
        {
            this.select = select ?? new string[0];
            return GetManyRaw(jsonSettings)
                .Children()
                .Select(jt => JObject.Parse(jt.ToString()))
                .ToArray();
        }

        /// <summary>Selects the single item.</summary>
        /// <param name="jsonSettings">The JSON settings.</param>
        /// <returns></returns>
        public JToken SelectSingle(JsonSerializerSettings jsonSettings = null)
        {
            return JObject.Parse(SelectSingleRaw(jsonSettings)?.ToString());
        }

        /// <summary>Gets the many raw.</summary>
        /// <param name="jsonSettings">The JSON settings.</param>
        /// <returns></returns>
        public JToken GetManyRaw(JsonSerializerSettings jsonSettings = null)
        {
            operationData["query"] = CompileQuery();
            return client?.SendRequest<JToken>(operationData, HttpMethod.Get, jsonSettings);
        }

        /// <summary>Selects the single raw.</summary>
        /// <param name="jsonSettings">The JSON settings.</param>
        /// <returns></returns>
        public JToken SelectSingleRaw(JsonSerializerSettings jsonSettings = null)
        {
            return Take(1).GetManyRaw(jsonSettings);
        }

        /// <summary>Compiles the query.</summary>
        /// <returns>Compiled query as string</returns>
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

        /// <summary>Compiles the condition.</summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">condition</exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="InvalidOperationException">
        /// You are trying to use ExpressionType.Contains, ExpressionType.StartsWith, ExpressionType.EndsWith with a null value
        /// or
        /// You have to add at least one element to a collection to use with {ExpressionType.In} expression
        /// </exception>
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
