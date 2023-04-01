using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1GatewayFunctionBuilder
{
    public V1Alpha1Function Build( V1Alpha1Workspace ws, string image )
    {
        var func = new V1Alpha1Function
        {
            ApiVersion = "justfaas.com/v1alpha1",
            Kind = "Function",
            Metadata = new V1ObjectMeta
            {
                Name = "gateway",
                NamespaceProperty = ws.Name(),
                Labels = new Dictionary<string, string>
                {
                    { "app.kubernetes.io/name", "faas-gateway" },
                    { "app.kubernetes.io/component", "gateway" },
                    { "app.kubernetes.io/part-of", "faas" },
                    { AppLabels.WorkspaceName, ws.Name() },
                },
                Annotations = new Dictionary<string, string>
                {
                    { AppAnnotations.ServiceAccountName.Key, "gateway" }
                },
            },
            Spec = new V1Alpha1Function.FunctionSpec
            {
                Image = image,
                Port = 8080,
                Env = new List<V1EnvVar>
                {
                    new V1EnvVar
                    {
                        Name = "FAAS_WORKSPACE",
                        Value = ws.Name()
                    }
                }
            }
        };

        return ( func );
    }
}
