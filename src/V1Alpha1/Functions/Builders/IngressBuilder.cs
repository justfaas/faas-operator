using Faactory.k8s.Models.Builders;
using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1IngressBuilder
{
    private readonly string[] certIssuerKinds = new string[]
    {
        "ClusterIssuer",
        "Issuer"
    };

    public V1Ingress Build( V1Alpha1Function func, string targetNamespace )
    {
        if ( func.Spec.Ingress == null )
        {
            throw new ArgumentNullException( nameof( func.Spec.Ingress ) );
        }

        // using gateway from faas namespace
        var useGlobalWorkspace = !targetNamespace.Equals( func.Namespace() );

        // if the ingress is going to be created on a different namespace
        // we prefix the function's namespace in the ingress name
        var ingressName = useGlobalWorkspace
            ? $"{func.Namespace()}.{func.Name()}"
            : func.Name();

        var ingress = new V1Ingress
        {
            ApiVersion = $"{V1Ingress.KubeGroup}/{V1Ingress.KubeApiVersion}",
            Kind = V1Ingress.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = ingressName,
                NamespaceProperty = targetNamespace,
                Labels = new Dictionary<string, string>
                {
                    { AppLabels.Name, func.NamespacedName() },
                },
                Annotations = new Dictionary<string, string>
                {
                    { "kubernetes.io/ingress.class", "nginx" },
                    { "nginx.ingress.kubernetes.io/enable-cors", "true" },
                    { 
                        "nginx.ingress.kubernetes.io/rewrite-target", $"/proxy/{func.Name()}/$1"
                    },
                }
            },
            Spec = new V1IngressSpec()
        };

        ingress.Spec.Rules = new List<V1IngressRule>
        {
            new V1IngressRule
            {
                Host = func.Spec.Ingress.Host,
                Http = new V1HTTPIngressRuleValue
                {
                    Paths = new List<V1HTTPIngressPath>
                    {
                        new V1HTTPIngressPath
                        {
                            PathType = "ImplementationSpecific",
                            Path = func.Spec.Ingress.Path,
                            Backend = new V1IngressBackend
                            {
                                Service = new V1IngressServiceBackend
                                {
                                    Name = "gateway",
                                    Port = new V1ServiceBackendPort
                                    {
                                        Number = 8080
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        if ( func.Spec.Ingress.IsValid() && ( func.Spec.Ingress.Tls?.IsValid() == true ) && certIssuerKinds.Contains( func.Spec.Ingress.Tls.IssuerRef!.Kind, StringComparer.OrdinalIgnoreCase ) )
        {
            var issuerKind = func.Spec.Ingress.Tls.IssuerRef.Kind!.Equals( "ClusterIssuer", StringComparison.OrdinalIgnoreCase )
                ? "cluster-issuer"
                : "issuer";

            ingress.SetAnnotation( $"cert-manager.io/{issuerKind}", func.Spec.Ingress.Tls.IssuerRef.Name );

            // TODO: support wildcard certificates
            ingress.Spec.Tls = new List<V1IngressTLS>
            {
                new V1IngressTLS
                {
                    Hosts = new string[]
                    {
                        func.Spec.Ingress.Host!
                    },
                    SecretName = $"{func.Spec.Ingress.Host}-cert"
                }
            };
        }

        return ( ingress );
    }
}
