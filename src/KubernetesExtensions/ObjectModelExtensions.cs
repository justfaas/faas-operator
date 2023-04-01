using k8s;
using k8s.Models;

internal static class KubernetesObjectModelExtensions
{
    public static string NamespacedName<T>( this T obj ) where T : IMetadata<V1ObjectMeta>
        => string.Concat( obj.Namespace(), ".", obj.Name() );
}
