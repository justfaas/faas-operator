using Faactory.k8s.Models;
using Faactory.k8s.Models.Builders;
using k8s.Models;

namespace tests;

public class IngressBuilderTests
{
    [Fact]
    public void Test()
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
                Ingress = new V1Alpha1Function.FunctionSpec.IngressSpec
                {
                    Host = "hello.example.com",
                }
            }
        };

        // build ingress on the same namespace as the function
        var ingress = new V1Alpha1IngressBuilder().Build( function, "test" );

        Assert.NotNull( ingress );
        Assert.Equal( "hello", ingress.Metadata.Name );
        Assert.Equal( "test", ingress.Metadata.NamespaceProperty );
        Assert.Equal( "hello.example.com", ingress.Spec.Rules[0].Host );
        Assert.Equal( "/proxy/test/hello/$1", ingress.Metadata.Annotations["nginx.ingress.kubernetes.io/rewrite-target"] );

        // build ingress on a different namespace
        ingress = new V1Alpha1IngressBuilder().Build( function, "faas" );

        Assert.NotNull( ingress );
        Assert.Equal( "test.hello", ingress.Metadata.Name );
        Assert.Equal( "faas", ingress.Metadata.NamespaceProperty );
        Assert.Equal( "hello.example.com", ingress.Spec.Rules[0].Host );
        Assert.Equal( "/proxy/test/hello/$1", ingress.Metadata.Annotations["nginx.ingress.kubernetes.io/rewrite-target"] );
    }
}
