using k8s;
using k8s.Models;

public abstract class KubeController<T> : BackgroundService where T : IKubernetesObject, IMetadata<V1ObjectMeta>
{
    private readonly ILogger logger;
    private readonly IKubernetes client;

    public KubeController( ILoggerFactory loggerFactory, IKubernetes kubernetesClient )
    {
        logger = loggerFactory.CreateLogger( GetType() );
        client = kubernetesClient;
    }

    protected virtual Task InitializeAsync( CancellationToken stoppingToken )
    {
        return Task.CompletedTask;
    }

    protected abstract Task DeletedAsync( T obj );
    protected abstract Task ReconcileAsync( T obj );

    public override Task StartAsync( CancellationToken cancellationToken = default( CancellationToken ) )
    {
        logger.LogInformation( "Started." );

        return base.StartAsync( cancellationToken );
    }

    public override Task StopAsync( CancellationToken cancellationToken = default( CancellationToken ) )
    {
        logger.LogInformation( "Stopped." );

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        try
        {
            await InitializeAsync( stoppingToken );
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, ex.Message );

            await StopAsync();
            return;
        }

        var attr = typeof( T ).TryGetKubernetesEntityAttribute();

        if ( attr == null )
        {
            // T is not a valid kubernetes entity; missing the KubernetesEntity attribute
            // cancel execution
            logger.LogError( $"Type '{typeof( T ).Name}' is missing a 'KubernetesEntity' attribute." );

            await StopAsync();
            return;
        }

        while ( !stoppingToken.IsCancellationRequested )
        {
            try
            {
                logger.LogInformation( $"listening to '{attr.GetPluralName()}.{attr.GetApiVersion()}' objects." );

                // var result = await client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(
                //     group: attr.Group,
                //     version: attr.ApiVersion,
                //     plural: attr.GetPluralName(),
                //     watch: true,
                //     cancellationToken: stoppingToken
                // )
                // .ConfigureAwait( false );
                var result = await ListObjectAsync( attr, stoppingToken )
                    .ConfigureAwait( false );

                var watcher = await CreateWatcherAsync( result, attr.GetKindDescription(), stoppingToken );

                while ( watcher.Watching && !stoppingToken.IsCancellationRequested )
                {
                    await Task.Delay( 1000, stoppingToken );
                }
            }
            catch ( k8s.Autorest.HttpOperationException ex )
            {
                logger.LogError( ex.Message + "\n" + ex.Response.Content );

                await Task.Delay( 3000, stoppingToken );
            }
        }
    }

    protected virtual Task<k8s.Autorest.HttpOperationResponse<object>> ListObjectAsync( KubernetesEntityAttribute attr, CancellationToken cancellationToken )
        => client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(
            group: attr.Group,
            version: attr.ApiVersion,
            plural: attr.GetPluralName(),
            watch: true,
            cancellationToken: cancellationToken
        );

    private async Task<Watcher<T>> CreateWatcherAsync( k8s.Autorest.HttpOperationResponse<object> result, string kind, CancellationToken cancellationToken )
    {
        var watcher = result.Watch<T, object>( 
            onEvent: async ( type, item ) =>
            {
                logger.LogInformation( $"{kind}/{item.Name()} {type.ToString().ToLower()}." );

                switch ( type )
                {
                    case WatchEventType.Added:
                    case WatchEventType.Modified:
                    await ReconcileAsync( item );
                    break;

                    case WatchEventType.Deleted:
                    await DeletedAsync( item );
                    break;

                    case WatchEventType.Error:
                    // TODO: the watch should be restarted in these scenarios
                    break;

                    default:
                    break;
                }
            }
            , onError: ex =>
            {
                logger.LogError( ex, ex.Message );
            }
        );

        // wait for watcher to initialize
        while ( !watcher.Watching && !cancellationToken.IsCancellationRequested )
        {
            await Task.Delay( 1000, cancellationToken );
        }

        return ( watcher );
    }
}
