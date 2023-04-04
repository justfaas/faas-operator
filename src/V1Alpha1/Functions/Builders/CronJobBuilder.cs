using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1CronJobBuilder
{
    public V1CronJob Build( V1Alpha1Function func, string targetNamespace )
    {
        var gatewayNamespace = func.GetAnnotationValue( OperatorAnnotations.GatewayNamespace );
        var cronExpression = func.GetAnnotationValue( OperatorAnnotations.CronExpression, string.Empty );
        var functionUrl = $"http://gateway.{gatewayNamespace}.svc.cluster.local:8080/proxy/{func.Namespace()}/{func.Name()}";

        var job = new V1CronJob
        {
            ApiVersion = $"{V1CronJob.KubeGroup}/{V1CronJob.KubeApiVersion}",
            Kind = V1CronJob.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = func.Name(),
                NamespaceProperty = targetNamespace,
                Labels = new Dictionary<string, string>
                {
                    { AppLabels.Name, func.NamespacedName() }
                }
            },
            Spec = new V1CronJobSpec
            {
                Schedule = cronExpression,
                ConcurrencyPolicy = "Forbid",
                SuccessfulJobsHistoryLimit = 1,
                FailedJobsHistoryLimit = 1,
                JobTemplate = new V1JobTemplateSpec
                {
                    Spec = new V1JobSpec
                    {
                        Template = new V1PodTemplateSpec
                        {
                            Spec = new V1PodSpec
                            {
                                Containers = new List<V1Container>
                                {
                                    new V1Container
                                    {
                                        Name = "curl",
                                        Image = "curlimages/curl:latest",
                                        ImagePullPolicy = "IfNotPresent",
                                        Args = new string[]
                                        {
                                            "-s",
                                            "-X",
                                            "POST",
                                            functionUrl
                                        }
                                    }
                                },
                                RestartPolicy = "OnFailure"
                            }
                        }
                    }
                }
            }
        };

        return ( job );
    }
}
