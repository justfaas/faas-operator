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

        Assert.Single( deployment.Spec.Template.Spec.Volumes );
        Assert.Equal( 2, deployment.Spec.Template.Spec.Volumes.Single().Projected.Sources.Count );
        Assert.Equal( "secret1", deployment.Spec.Template.Spec.Volumes[0].Projected.Sources[0].Secret.Name );
        Assert.Equal( "secret2", deployment.Spec.Template.Spec.Volumes[0].Projected.Sources[1].Secret.Name );

        Assert.Single( deployment.Spec.Template.Spec.Containers );
        Assert.Single( deployment.Spec.Template.Spec.Containers.Single().VolumeMounts );
        Assert.Equal( "/var/faas/secrets", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[0].MountPath );


        // annotate custom secrets mount path
        function.Metadata.Annotations = new Dictionary<string, string>
        {
            { OperatorAnnotations.SecretsMountPath.Key, "/app/secrets" }
        };

        // rebuild deployment
        deployment = new V1Alpha1DeploymentBuilder().Build( function, "test" );

        Assert.Single( deployment.Spec.Template.Spec.Volumes );
        Assert.Equal( 2, deployment.Spec.Template.Spec.Volumes.Single().Projected.Sources.Count );
        Assert.Equal( "secret1", deployment.Spec.Template.Spec.Volumes[0].Projected.Sources[0].Secret.Name );
        Assert.Equal( "secret2", deployment.Spec.Template.Spec.Volumes[0].Projected.Sources[1].Secret.Name );

        Assert.Single( deployment.Spec.Template.Spec.Containers );
        Assert.Single( deployment.Spec.Template.Spec.Containers.Single().VolumeMounts );
        Assert.Equal( "/app/secrets", deployment.Spec.Template.Spec.Containers[0].VolumeMounts[0].MountPath );
    }
}
