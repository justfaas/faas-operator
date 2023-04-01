using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1NamespaceBuilder
{
    public V1Namespace Build( V1Alpha1Workspace ws )
    {
        var ns = new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = ws.Name(),
                Labels = new Dictionary<string, string>
                {
                    { AppLabels.WorkspaceName, ws.Name() },
                }
            },
            Spec = new V1NamespaceSpec()
        };

        return ( ns );
    }
}