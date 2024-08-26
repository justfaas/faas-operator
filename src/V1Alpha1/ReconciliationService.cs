using k8s;
using k8s.Models;

public sealed class ReconciliationService<TObject> : IReconciliationService<TObject> where TObject : IKubernetesObject<V1ObjectMeta>
{
    private readonly ILogger logger;
    private readonly IKubernetes client;

    public ReconciliationService( ILoggerFactory loggerFactory, IKubernetes kubernetes )
    {
        var attr = typeof( TObject )
            .GetKubernetesEntityAttribute();

        logger = loggerFactory.CreateLogger( $"reconciler.{attr.GetPluralName()}" );
        client = kubernetes;
    }

    public async Task DeleteAsync<T>( TObject source ) where T : IKubernetesObject<V1ObjectMeta>
    {
        // locate resource
        var obj = await GetObjectAsync<T>( source );

        if ( obj == null )
        {
            // object was not found
            return;
        }

        if ( !obj.HasLabel( OperatorLabels.ManagedBy ) )
        {
            // object exists but it is not managed by the operator
            var attr = typeof( T )
                .GetKubernetesEntityAttribute();

            logger.LogWarning( "{Kind}/{Name} exists but it is not managed by the operator.", attr.GetKindDescription(), obj.Name() );
            return;
        }

        // delete resource
        await client.DeleteNamespacedAsync<T>( obj.Namespace(), obj.Name() );

        logger.LogObjectDeleted<T>( obj.Name() );
    }

    public async Task ReconcileAsync<T>(TObject source, T obj) where T : IKubernetesObject<V1ObjectMeta>
    {
        // locate resource
        var existing = await GetObjectAsync<T>( source );

        if ( ( existing != null ) && !existing.HasLabel( OperatorLabels.ManagedBy ) )
        {
            // object exists but it is not managed by the operator
            var attr = typeof( T )
                .GetKubernetesEntityAttribute();

            logger.LogWarning( "{Kind}/{Name} exists but it is not managed by the operator.", attr.GetKindDescription(), existing.Name() );
            return;
        }

        if ( existing == null )
        {
            await CreateAsync( obj );
        }
        else
        {
            await ReplaceAsync( obj, existing.ResourceVersion() );
        }
    }

    public async Task<T?> GetObjectAsync<T>( TObject source ) where T : IKubernetesObject<V1ObjectMeta>
    {
        var labelSelector = source.GetLabelSelector();

        try
        {
            var items = await client.ListAsync<T>( labelSelector );

            if ( items?.Count() != 1 )
            {
                return default( T );
            }

            return items.Single();
        }
        catch ( k8s.Autorest.HttpOperationException ex )
        {
            if ( ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound )
            {
                return default( T );
            }

            // something else...
            throw;
        }
    }

    private async Task CreateAsync<T>( T obj ) where T : IKubernetesObject<V1ObjectMeta>
    {
        await client.CreateNamespacedWithHttpMessagesAsync( obj.Namespace(), obj );

        logger.LogObjectCreated<T>( obj.Name() );
    }

    private async Task ReplaceAsync<T>( T obj, string previousVersion ) where T : IKubernetesObject<V1ObjectMeta>
    {
        obj.Metadata.ResourceVersion = previousVersion;

        try
        {
            var response = await client.ReplaceNamespacedAsync(
                obj.Namespace(),
                obj.Name(),
                obj
            );

            if ( !response.ResourceVersion().Equals( previousVersion ) )
            {
                logger.LogObjectModified<T>( obj.Name() );
            }
            else
            {
                logger.LogObjectUnchanged<T>( obj.Name() );
            }
        }
        catch ( k8s.Autorest.HttpOperationException ex )
        {
            logger.LogError( "{Error}\n{Content}", ex.Message, ex.Response.Content );
        }
    }
}
