using Faactory.k8s.Models.Builders;
using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1DeploymentBuilder
{
    public V1Deployment Build( V1Alpha1Function func, string targetNamespace )
    {
        var volumes = new List<V1Volume>();
        var volumeMounts = new List<V1VolumeMount>();
        var secretMountPath = func.GetAnnotationValue( OperatorAnnotations.SecretsMountPath );

        if ( func.Spec.Secrets?.Any() == true )
        {
            foreach ( var secretName in func.Spec.Secrets )
            {
                var volumeName = string.Concat( secretName, "-secret-vol" );

                var volumeMount = new V1VolumeMount
                {
                    Name = volumeName,
                    MountPath = secretMountPath,
                    ReadOnlyProperty = true
                };

                var volume = new V1Volume
                {
                    Name = volumeName,
                    Secret = new V1SecretVolumeSource
                    {
                        SecretName = secretName,
                        Optional = true
                    }
                };

                volumes.Add( volume );
                volumeMounts.Add( volumeMount );
            }
        }

        if ( func.Spec.ConfigMaps?.Any() == true )
        {
            foreach ( var configName in func.Spec.ConfigMaps )
            {
                var volumeName = string.Concat( configName, "-config-vol" );

                var volumeMount = new V1VolumeMount
                {
                    Name = volumeName,
                    MountPath = "/var/faas/config",
                    ReadOnlyProperty = true
                };

                var volume = new V1Volume
                {
                    Name = volumeName,
                    ConfigMap = new V1ConfigMapVolumeSource
                    {
                        Name = configName,
                        Optional = true
                    }
                };

                volumes.Add( volume );
                volumeMounts.Add( volumeMount );
            }
        }

        var deployment = new V1Deployment
        {
            ApiVersion = $"{V1Deployment.KubeGroup}/{V1Deployment.KubeApiVersion}",
            Kind = V1Deployment.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = func.Name(),
                NamespaceProperty = targetNamespace,
                Labels = new Dictionary<string, string>
                {
                    { AppLabels.Name, func.NamespacedName() }
                }
            },
            Spec = new V1DeploymentSpec
            {
                Selector = new V1LabelSelector
                {
                    MatchLabels = new Dictionary<string, string>
                    {
                        { AppLabels.Name, func.NamespacedName() }
                    }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>
                        {
                            { AppLabels.Name, func.NamespacedName() }
                        }
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Image = func.Spec.Image,
                                Name = func.Name(),
                                Env = func.Spec.Env,
                                VolumeMounts = volumeMounts
                            }
                        },
                        ServiceAccountName = func.GetAnnotationValue(
                            AppAnnotations.ServiceAccountName
                        ),
                        Volumes = volumes
                    }
                }
            }
        };

        var kubernetesLabels = func.Labels()
            ?.Where( x => x.Key.StartsWith( "app.kubernetes.io/" ) )
            .ToArray();

        // include kubernetes labels in deployment
        if ( kubernetesLabels?.Any() == true )
        {
            foreach ( var label in kubernetesLabels )
            {
                deployment.Labels().Add( label.Key, label.Value );
            }
        }

        return ( deployment );
    }
}
