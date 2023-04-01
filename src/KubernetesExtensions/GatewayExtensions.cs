using k8s;
using k8s.Models;

internal static class KubernetesFaaSGatewayExtensions
{
    public static async Task<V1Deployment?> GetFaaSGatewayDeploymentAsync( this IKubernetes client, string targetNamespace )
    {
        const string labelSelector = "app.kubernetes.io/name=faas-gateway";

        var items = await client.ListAsync<V1Deployment>( labelSelector );

        if ( !( items?.Any() == true ) )
        {
            // no deployments found
            return ( null );
        }

        V1Deployment? deployment;

        // look for a deployment in the target namespace
        deployment = items.SingleOrDefault( x => x.Namespace()?.Equals( targetNamespace ) == true );

        if ( deployment != null )
        {
            return ( deployment );
        }

        // look for a deployment in the faas namespace
        deployment = items.SingleOrDefault( x => x.Namespace()?.Equals( "faas" ) == true );

        return ( deployment );
    }

    public static async Task<V1Deployment?> GetFaaSGatewayDeploymentAsync( this IKubernetes client )
    {
        const string labelSelector = "app.kubernetes.io/name=faas-gateway";

        var items = await client.ListNamespacedAsync<V1Deployment>( "faas", labelSelector );

        return items?.SingleOrDefault();
    }
}
