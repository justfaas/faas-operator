using Faactory.k8s.Models;

internal static class OperatorLabels
{
    public static readonly ( string Key, string Default ) ManagedBy = ( AppLabels.ManagedBy, "faas-operator" );
}
