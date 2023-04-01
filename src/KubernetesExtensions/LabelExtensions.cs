using Faactory.k8s.Models;
using k8s;
using k8s.Models;

internal static class KubernetesObjectLabelExtensions
{
    public static T SetManagedByLabel<T>( this T obj ) where T : IKubernetesObject, IMetadata<V1ObjectMeta>
    {
        obj.SetLabel( AppLabels.ManagedBy, "faas-operator" );

        return ( obj );
    }

    public static bool HasLabel( this IMetadata<V1ObjectMeta> obj, string key, string? value = null )
    {
        var labelValue = obj.GetLabel( key );

        if ( value == null )
        {
            return ( labelValue != null );
        }

        return value.Equals( labelValue );
    }

    public static bool HasLabel( this IMetadata<V1ObjectMeta> obj, (string key, string value) label )
        => HasLabel( obj, label.key, label.value );

    public static string SelectLabels( this IMetadata<V1ObjectMeta> obj, Func<IMetadata<V1ObjectMeta>,string> predicate )
        => predicate.Invoke( obj );
}
