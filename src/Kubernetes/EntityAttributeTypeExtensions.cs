using k8s.Models;

public static class KubernetesEntityAttributeTypeExtensions
{
    public static KubernetesEntityAttribute GetKubernetesEntityAttribute( this Type type )
    {
        var attr = TryGetKubernetesEntityAttribute( type );

        if ( attr == null )
        {
            throw new ArgumentException( $"Type '{type.Name}' is missing a 'KubernetesEntity' attribute." );
        }

        return ( attr );
    }

    public static KubernetesEntityAttribute? TryGetKubernetesEntityAttribute( this Type type )
        => type.GetCustomAttributes( typeof( KubernetesEntityAttribute ), false )
            .Cast<KubernetesEntityAttribute>()
            .SingleOrDefault();
}
