using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1RoleBuilder
{
    public V1Role Build( V1Alpha1Workspace ws )
    {
        var role = new V1Role
        {
            ApiVersion = $"{V1Role.KubeGroup}/{V1Role.KubeApiVersion}",
            Kind = V1Role.KubeKind,
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
            Rules = new List<V1PolicyRule>
            {
                new V1PolicyRule
                {
                    ApiGroups = new string[]
                    {
                        "justfaas.com"
                    },
                    Resources = new string[]
                    {
                        "functions"
                    },
                    Verbs = new string[]
                    {
                        "*"
                    }
                },
                new V1PolicyRule
                {
                    ApiGroups = new string[]
                    {
                        "apiextensions.k8s.io"
                    },
                    Resources = new string[]
                    {
                        "customresourcedefinitions"
                    },
                    Verbs = new string[]
                    {
                        "list"
                    }
                },
                new V1PolicyRule
                {
                    ApiGroups = new string[]
                    {
                        ""
                    },
                    Resources = new string[]
                    {
                        "events"
                    },
                    Verbs = new string[]
                    {
                        "create",
                        "get",
                        "list",
                        "update"
                    }
                },
            }
        };

        return ( role );
    }
}
