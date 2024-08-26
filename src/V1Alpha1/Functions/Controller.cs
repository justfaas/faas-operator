using Faactory.k8s.Models;
using Faactory.k8s.Models.Builders;
using k8s;
using k8s.Models;

public sealed class V1Alpha1FunctionController : KubeController<V1Alpha1Function>
{
    private readonly ILogger logger;
    private readonly IKubernetes client;
    private readonly IReconciliationService<V1Alpha1Function> reconciler;

    private IEnumerable<V1CustomResourceDefinition> crds = Enumerable.Empty<V1CustomResourceDefinition>();

    public V1Alpha1FunctionController( ILoggerFactory loggerFactory
        , IKubernetes kubernetesClient
        , ReconciliationService<V1Alpha1Function> reconciliationService )
        : base( loggerFactory, kubernetesClient )
    {
        logger = loggerFactory.CreateLogger<V1Alpha1FunctionController>();
        client = kubernetesClient;
        reconciler = reconciliationService;
    }

    protected override async Task InitializeAsync( CancellationToken cancellationToken )
    {
        var installed = await client.ApiextensionsV1.ListCustomResourceDefinitionAsync( cancellationToken: cancellationToken );

        crds = installed.Items.ToArray();

        if ( !crds.Any( x => x.Name().Equals( "functions.justfaas.com" ) ) )
        {
            logger.LogError( "CRDs for 'functions.justfaas.com' are not installed." );

            await StopAsync( CancellationToken.None );
        }
    }

    protected override async Task DeletedAsync( V1Alpha1Function func )
    {
        await reconciler.DeleteAsync<V1Ingress>( func );
        await reconciler.DeleteAsync<V1CronJob>( func );
        await reconciler.DeleteAsync<V2HorizontalPodAutoscaler>( func );
        await reconciler.DeleteAsync<V1Service>( func );
        await reconciler.DeleteAsync<V1Deployment>( func );
    }

    protected override async Task ReconcileAsync( V1Alpha1Function func )
    {
        await ReconcileDeploymentAsync( func );
        await ReconcileServiceAsync( func );
        await ReconcileScalerAsync( func );
        await ReconcileCronJobAsync( func );
        await ReconcileIngressAsync( func );
    }

    private async Task ReconcileDeploymentAsync( V1Alpha1Function func )
    {
        var deployment = new V1Alpha1DeploymentBuilder()
            .Build( func, func.Namespace() )
            .SetManagedByLabel();

        await reconciler.ReconcileAsync( func, deployment );
    }

    private async Task ReconcileServiceAsync( V1Alpha1Function func )
    {
        var svc = new V1Alpha1ServiceBuilder()
            .Build( func, func.Namespace() )
            .SetManagedByLabel();

        await reconciler.ReconcileAsync( func, svc );
    }

    private async Task ReconcileScalerAsync( V1Alpha1Function func )
    {
        var hpa = new V1Alpha1HorizontalPodAutoscalerBuilder()
            .Build( func, func.Namespace() )
            .SetManagedByLabel();

        await reconciler.ReconcileAsync( func, hpa );
    }

    private async Task ReconcileCronJobAsync( V1Alpha1Function func )
    {
        var cronExpression = func.GetAnnotationValue( OperatorAnnotations.CronExpression, string.Empty );

        if ( string.IsNullOrEmpty( cronExpression ) )
        {
            await reconciler.DeleteAsync<V1CronJob>( func );

            return;
        }

        // if the function is inside a workspace we should invoke the gateway
        // from the same namespace, since we might not be able to access outside that namespace
        // if the function is not inside a workspace, we invoke the root gateway
        V1Deployment? gatewayDeployment = await client.GetFaaSGatewayDeploymentAsync( func.Namespace() );

        if ( gatewayDeployment != null )
        {
            func.SetAnnotation( OperatorAnnotations.GatewayNamespace.Key, gatewayDeployment.Namespace() );
        }

        var job = new V1Alpha1CronJobBuilder()
            .Build( func, func.Namespace() )
            .SetManagedByLabel();

        await reconciler.ReconcileAsync<V1CronJob>( func, job );
    }

    private async Task ReconcileIngressAsync( V1Alpha1Function func )
    {
        if ( func.Spec.Ingress == null )
        {
            await reconciler.DeleteAsync<V1Ingress>( func );

            return;
        }

        // The ingress needs to be deployed in the same namespace as the gateway
        // at this point we look for a gateway in the target namespace
        // if there isn't one, we look for a gateway in the faas namespace
        // if there isn't one, we can't create the ingress
        V1Deployment? gatewayDeployment = await client.GetFaaSGatewayDeploymentAsync( func.Namespace() );

        if ( gatewayDeployment == null )
        {
            // if we can't find a suitable gateway deployment
            // we can't create the ingress
            logger.LogWarning(
                "ingress.networking.k8s.io/{Namespace}.{Name} not created. No suitable gateway was found.",
                func.Namespace(),
                func.Name()
            );

            return;
        }

        var ingress = new V1Alpha1IngressBuilder()
            .Build( func, gatewayDeployment.Namespace() )
            .SetManagedByLabel();

        await reconciler.ReconcileAsync( func, ingress );
    }
}
