using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1RoleBindingBuilder
{
    public V1RoleBinding Build( V1Alpha1Workspace ws )
    {
        var binding = new V1RoleBinding
        {
            ApiVersion = $"{V1RoleBinding.KubeGroup}/{V1RoleBinding.KubeApiVersion}",
            Kind = V1RoleBinding.KubeKind,
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
            },
            RoleRef = new V1RoleRef
            {
                ApiGroup = V1Role.KubeGroup,
                Kind = "Role",
                Name = "gateway"
            },
            Subjects =
            [
                new Rbacv1Subject
                {
                    Kind = "ServiceAccount",
                    Name = "gateway",
                    NamespaceProperty = ws.Name()
                }
            ]
        };

        return binding;
    }
}
