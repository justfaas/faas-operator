using k8s.Models;

internal static class KubernetesEntityAttributeExtensions
{
    public static string GetApiVersion( this KubernetesEntityAttribute attr )
    {
        if ( string.IsNullOrEmpty( attr.Group ) )
        {
            return ( attr.ApiVersion );
        }

        return string.Concat( attr.Group, "/", attr.ApiVersion );
    }

    public static string GetPluralName( this KubernetesEntityAttribute attr )
    {
        if ( string.IsNullOrEmpty( attr.PluralName ) )
        {
            return string.Concat( attr.Kind, "s" ).ToLower();
        }

        return attr.PluralName.ToLower();
    }

    public static string GetKindDescription( this KubernetesEntityAttribute attr )
    {
        if ( string.IsNullOrEmpty( attr.Group ) )
        {
            return attr.Kind.ToLower();
        }

        return $"{attr.Kind.ToLower()}.{attr.Group}";
    }
}
