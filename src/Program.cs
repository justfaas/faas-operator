using k8s;

var builder = Host.CreateApplicationBuilder();

builder.Logging.ClearProviders()
    .AddSimpleConsole( options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    } );

builder.Services.AddSingleton<IKubernetes>( provider =>
{
    var config = KubernetesClientConfiguration.IsInCluster()
        ? KubernetesClientConfiguration.InClusterConfig()
        : KubernetesClientConfiguration.BuildConfigFromConfigFile();

    return new Kubernetes( config );
} );

builder.Services.AddHostedService<V1Alpha1FunctionController>()
    .AddTransient<ReconciliationService<Faactory.k8s.Models.V1Alpha1Function>>();

builder.Services.AddHostedService<V1Alpha1WorkspaceController>()
    .AddTransient<ReconciliationService<Faactory.k8s.Models.V1Alpha1Workspace>>();

var app = builder.Build();

app.Run();
