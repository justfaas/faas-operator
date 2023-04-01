using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1ServiceAccountBuilder
{
    public V1ServiceAccount Build( V1Alpha1Workspace ws )
    {
        var serviceAccount = new V1ServiceAccount
        {
            ApiVersion = V1ServiceAccount.KubeApiVersion,
            Kind = V1ServiceAccount.KubeKind,
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
                }
            }
        };

        return ( serviceAccount );
    }
}
