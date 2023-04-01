using Faactory.k8s.Models;
using Faactory.k8s.Models.Builders;
using k8s;
using k8s.Autorest;
using k8s.Models;

/*
TODO: listen only to resources on `faas` namespace
The current implementation of KubeController watches for resources
of type T in all namespaces. Since we are only interested in workspaces
created on `faas` namespace, it would be best to listen only on that namespace.

Maybe an override...
*/

public sealed class V1Alpha1WorkspaceController : KubeController<V1Alpha1Workspace>
{
    private readonly ILogger logger;
    private readonly IKubernetes client;
    private readonly IReconciliationService<V1Alpha1Workspace> reconciler;

    private IEnumerable<V1CustomResourceDefinition> crds = Enumerable.Empty<V1CustomResourceDefinition>();

    public V1Alpha1WorkspaceController( ILoggerFactory loggerFactory
        , IKubernetes kubernetesClient
        , ReconciliationService<V1Alpha1Workspace> reconciliationService )
        : base( loggerFactory, kubernetesClient )
    {
        logger = loggerFactory.CreateLogger<V1Alpha1WorkspaceController>();
        client = kubernetesClient;
        reconciler = reconciliationService;
    }

    protected override async Task InitializeAsync( CancellationToken stoppingToken )
    {
        var installed = await client.ApiextensionsV1.ListCustomResourceDefinitionAsync();

        crds = installed.Items.ToArray();

        if ( !crds.Any( x => x.Name().Equals( "workspaces.justfaas.com" ) ) )
        {
            logger.LogError( "CRDs for 'workspaces.justfaas.com' are not installed." );

            await StopAsync();
        }
    }

    protected override Task<HttpOperationResponse<object>> ListObjectAsync( KubernetesEntityAttribute attr, CancellationToken cancellationToken )
        => client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: "faas",
            plural: attr.GetPluralName(),
            watch: true,
            cancellationToken: cancellationToken
        );

    protected override async Task DeletedAsync( V1Alpha1Workspace ws )
    {
        if ( !ws.Namespace().Equals( "faas" ) )
        {
            // workspaces created outside 'faas' namespace are ignored
            var attr = ws.GetType()
                .GetKubernetesEntityAttribute();

            logger.LogWarning( $"{attr.GetKindDescription()}/{ws.Name()} is not managed by the operator." );

            return;
        }

        await reconciler.DeleteAsync<V1Alpha1Function>( ws );
        await reconciler.DeleteAsync<V1RoleBinding>( ws );
        await reconciler.DeleteAsync<V1Role>( ws );
        await reconciler.DeleteAsync<V1ServiceAccount>( ws );

        // can't use reconciler on this one because it's not a namespaced object
        await DeleteNamespaceAsync( ws );
    }

    private async Task DeleteNamespaceAsync( V1Alpha1Workspace ws )
    {
        var existing = await reconciler.GetObjectAsync<V1Namespace>( ws );

        if ( existing == null )
        {
            // object was not found
            return;
        }

        if ( !existing.HasLabel( OperatorLabels.ManagedBy ) )
        {
            // object exists but it is not managed by the operator
            var attr = typeof( V1Namespace )
                .GetKubernetesEntityAttribute();

            logger.LogWarning( $"{attr.GetKindDescription()}/{existing.Name()} exists but it is not managed by the operator." );
            return;
        }

        // delete resource
        await client.CoreV1.DeleteNamespaceAsync( existing.Name() );

        logger.LogObjectDeleted<V1Namespace>( existing.Name() );
    }

    protected override async Task ReconcileAsync( V1Alpha1Workspace ws )
    {
        if ( !ws.Namespace().Equals( "faas" ) )
        {
            // workspaces created outside 'faas' namespace are ignored
            var attr = ws.GetType()
                .GetKubernetesEntityAttribute();

            logger.LogWarning( $"{attr.GetKindDescription()}/{ws.Name()} is not managed by the operator." );

            return;
        }

        await ReconcileNamespaceAsync( ws );
        await ReconcileServiceAccountAsync( ws );
        await ReconcileRoleAsync( ws );
        await ReconcileRoleBindingAsync( ws );
        await ReconcileGatewayFunctionAsync( ws );
    }

    private async Task ReconcileNamespaceAsync( V1Alpha1Workspace ws )
    {
        var ns = new V1Alpha1NamespaceBuilder()
            .Build( ws )
            .SetManagedByLabel();

        // can't use reconciler because it's not a namespaced object
        var existing = await reconciler.GetObjectAsync<V1Namespace>( ws );

        if ( ( existing != null ) && !existing.HasLabel( OperatorLabels.ManagedBy ) )
        {
            // object exists but it is not managed by the operator
            var attr = typeof( V1Namespace )
                .GetKubernetesEntityAttribute();

            logger.LogWarning( $"{attr.GetKindDescription()}/{existing.Name()} exists but it is not managed by the operator." );
            return;
        }

        if ( existing == null )
        {
            await client.CoreV1.CreateNamespaceAsync( ns );

            logger.LogObjectCreated<V1Namespace>( ns.Name() );
        }
        else
        {
            var previousVersion = existing.ResourceVersion();

            try
            {
                var response = await client.CoreV1.PatchNamespaceAsync(
                    new V1Patch( ns, V1Patch.PatchType.MergePatch ),
                    ns.Name()
                );

                if ( !response.ResourceVersion().Equals( previousVersion ) )
                {
                    logger.LogObjectModified<V1Namespace>( ns.Name() );
                }
                else
                {
                    logger.LogObjectUnchanged<V1Namespace>( ns.Name() );
                }
            }
            catch ( k8s.Autorest.HttpOperationException ex )
            {
                logger.LogError( $"{ex.Message}\n{ex.Response.Content}" );
            }
        }
    }

    private Task ReconcileServiceAccountAsync( V1Alpha1Workspace ws )
    {
        var serviceAccount = new V1Alpha1ServiceAccountBuilder()
            .Build( ws )
            .SetManagedByLabel();

        return reconciler.ReconcileAsync<V1ServiceAccount>( ws, serviceAccount );
    }

    private Task ReconcileRoleAsync( V1Alpha1Workspace ws )
    {
        var role = new V1Alpha1RoleBuilder()
            .Build( ws )
            .SetManagedByLabel();

        return reconciler.ReconcileAsync<V1Role>( ws, role );
    }

    private Task ReconcileRoleBindingAsync( V1Alpha1Workspace ws )
    {
        var binding = new V1Alpha1RoleBindingBuilder()
            .Build( ws )
            .SetManagedByLabel();

        return reconciler.ReconcileAsync<V1RoleBinding>( ws, binding );
    }

    private async Task ReconcileGatewayFunctionAsync( V1Alpha1Workspace ws )
    {
        // get faas gateway deployment
        var deployment = await client.GetFaaSGatewayDeploymentAsync();

        if ( deployment == null )
        {
            // can't deploy a workspaced gateway if there isn't a faas gateway
            return;
        }

        // we want to use the same image and tag from the faas gateway
        // if it was recently updated, then the workspaced gateways will get updated too
        var image = deployment.Spec.Template.Spec.Containers.Single( x => x.Name.Equals( "gateway" ) )
            .Image;

        var func = new V1Alpha1GatewayFunctionBuilder()
            .Build( ws, image )
            .SetManagedByLabel();

        await reconciler.ReconcileAsync<V1Alpha1Function>( ws, func );
    }
}
