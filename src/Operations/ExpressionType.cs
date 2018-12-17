using System.ComponentModel;

namespace Salaros.vTiger.WebService
{
    public enum ExpressionType
    {
        [Description("=")]
        Equals,

        [Description("!=")]
        NotEquals,

        [Description("LIKE")]
        Like,

        [Description(">")]
        GreaterThan,

        [Description("<")]
        LessThan,

        [Description(">=")]
        GreaterOrEquals,

        [Description("=<")]
        LessOrEquals,

        [Description("IN")]
        In,
    }
}