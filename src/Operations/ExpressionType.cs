using System.ComponentModel;

namespace Salaros.vTiger.WebService
{
    public enum ExpressionType
    {
        [Description("{0} = {1}")]
        Equals,

        [Description("{0} != {1}")]
        NotEquals,

        [Description("{0} LIKE '{1}%'")]
        StartsWith,

        [Description("{0} LIKE '%{1}'")]
        EndsWith,

        [Description("{0} LIKE '%{1}%'")]
        Contains,

        [Description("{0} > {1}")]
        GreaterThan,

        [Description("{0} < {1}")]
        LessThan,

        [Description("{0} >= {1}")]
        GreaterOrEquals,

        [Description("{0} =< {1}")]
        LessOrEquals,

        [Description("{0} IN ({1})")]
        In,
    }

    public static class ExpressionTypeExtensions
    {
        public static string GetDescription(this ExpressionType source)
        {
            var fi = typeof(ExpressionType).GetField(source.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }
    }
}