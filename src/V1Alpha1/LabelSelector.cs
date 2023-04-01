using Faactory.k8s.Models;
using k8s;
using k8s.Models;

internal static class LabelSelector
{
    private static readonly IReadOnlyDictionary<Type, Func<IMetadata<V1ObjectMeta>, string>> selectors = new Dictionary<Type, Func<IMetadata<V1ObjectMeta>, string>>
    {
        {
            typeof( V1Alpha1Function ),
            func => $"{AppLabels.Name}={func.NamespacedName()}"
        },
        {
            typeof( V1Alpha1Workspace ),
            ws => $"{AppLabels.WorkspaceName}={ws.Name()}"
        }
    };

    public static string GetLabelSelector( this IMetadata<V1ObjectMeta> obj )
        => selectors.GetValueOrDefault( obj.GetType() )
            ?.Invoke( obj ) ?? string.Empty;
}
