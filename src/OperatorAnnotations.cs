internal static class OperatorAnnotations
{
    /// <summary>
    /// CRON expression for a function's CronJob
    /// </summary>
    public const string CronExpression = "justfaas.com/cron-expression";

    /// <summary>
    /// Timezone for CronJob. **This is not yet implemented.**
    /// </summary>
    public const string CronTimezone = "justfaas.com/cron-timezone";

    /// <summary>
    /// Indicates the namespace of the gateway that owns the function. This is an internal runtime-only annotation.
    /// </summary>
    public static readonly ( string Key, string Default ) GatewayNamespace = ( "justfaas.com/gateway-namespace", "faas" );
}