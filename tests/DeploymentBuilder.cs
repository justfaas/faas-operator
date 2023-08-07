using Faactory.k8s.Models;
using Faactory.k8s.Models.Builders;
using k8s.Models;

namespace tests;

public class DeploymentBuilderTests
{
    [Fact]
    public void TestSecretsMountPath()
    {
        var function = new V1Alpha1Function
        {
            Metadata = new V1ObjectMeta
            {
                Name = "hello",
                NamespaceProperty = "test"
            },
            Spec = new V1Alpha1Function.FunctionSpec
            {
                Image = "gcr.io/google-samples/hello-app:1.0",
                Secrets = new string[]
                {
                    "secret1",
                    "secret2"
                }
            }
        };

        var deployment = new V1Alpha1DeploymentBuilder().Build( function, "test" );

        Assert.Single( deployment.Spec.Template.Spec.Containers );
        Assert.Equal( 2, deployment.Spec.Template.Spec.Containers.Single().VolumeMounts.Count );
        Assert.Equal( "secret1-secret-vol", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[0].Name );
        Assert.Equal( "secret2-secret-vol", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[1].Name );
        Assert.Equal( "/var/faas/secrets", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[0].MountPath );
        Assert.Equal( "/var/faas/secrets", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[1].MountPath );

        // annotate custom secrets mount path
        function.Metadata.Annotations = new Dictionary<string, string>
        {
            { OperatorAnnotations.SecretsMountPath.Key, "/app/secrets" }
        };

        // rebuild deployment
        deployment = new V1Alpha1DeploymentBuilder().Build( function, "test" );

        Assert.Single( deployment.Spec.Template.Spec.Containers );
        Assert.Equal( 2, deployment.Spec.Template.Spec.Containers.Single().VolumeMounts.Count );
        Assert.Equal( "secret1-secret-vol", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[0].Name );
        Assert.Equal( "secret2-secret-vol", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[1].Name );
        Assert.Equal( "/app/secrets", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[0].MountPath );
        Assert.Equal( "/app/secrets", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[1].MountPath );
    }
}
