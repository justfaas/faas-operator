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
    /// The mount path for the function's secrets.
    /// </summary>
    public static readonly ( string Key, string Default ) SecretsMountPath = ( "justfaas.com/secrets-mount-path", "/var/faas/secrets" );

    /// <summary>
    /// Indicates the namespace of the gateway that operates the function. This is an internal runtime-only annotation.
    /// </summary>
    internal static readonly ( string Key, string Default ) GatewayNamespace = ( "justfaas.com/gateway-namespace", "faas" );
}
