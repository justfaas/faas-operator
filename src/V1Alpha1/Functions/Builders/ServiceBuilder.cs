using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1ServiceBuilder
{
    public V1Service Build( V1Alpha1Function func, string targetNamespace )
    {
        var svc = new V1Service
        {
            ApiVersion = V1Service.KubeApiVersion,
            Kind = V1Service.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = func.Name(),
                NamespaceProperty = targetNamespace,
                Labels = new Dictionary<string, string>
                {
                    { AppLabels.Name, func.NamespacedName() }
                }
            },
            Spec = new V1ServiceSpec
            {
                Ports = new List<V1ServicePort>
                {
                    new V1ServicePort
                    {
                        Port = 8080,
                        Protocol = "TCP",
                        TargetPort = func.Spec.Port
                    }
                },
                Selector = new Dictionary<string, string>
                {
                    { AppLabels.Name, func.NamespacedName() }
                }
            }
        };

        return ( svc );
    }
}