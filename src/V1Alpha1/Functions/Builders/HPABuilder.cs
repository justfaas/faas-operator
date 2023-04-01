using k8s.Models;

namespace Faactory.k8s.Models.Builders;

public sealed class V1Alpha1HorizontalPodAutoscalerBuilder
{
    public V2HorizontalPodAutoscaler Build( V1Alpha1Function func, string targetNamespace )
    {
        var scaleMin = func.GetAnnotationValue( AppAnnotations.ScaleMin );
        var scaleMax = func.GetAnnotationValue( AppAnnotations.ScaleMax );
        var scaleMode = func.GetAnnotationValue( AppAnnotations.ScaleMode );
        var scaleTarget = func.GetAnnotationValue( AppAnnotations.ScaleThreshold );
        
        var metricName = GetMetricName( scaleMode );

        var hpa = new V2HorizontalPodAutoscaler
        {
            ApiVersion = $"{V2HorizontalPodAutoscaler.KubeGroup}/{V2HorizontalPodAutoscaler.KubeApiVersion}",
            Kind = V2HorizontalPodAutoscaler.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = func.Name(),
                NamespaceProperty = targetNamespace,
                Labels = new Dictionary<string, string>
                {
                    { AppLabels.Name, func.NamespacedName() }
                }
            },
            Spec = new V2HorizontalPodAutoscalerSpec
            {
                ScaleTargetRef = new V2CrossVersionObjectReference
                {
                    ApiVersion = "apps/v1",
                    Kind = "Deployment",
                    Name = func.Name()
                },
                MinReplicas = scaleMin,
                MaxReplicas = scaleMax,
                Behavior = new V2HorizontalPodAutoscalerBehavior
                {
                    ScaleDown = new V2HPAScalingRules
                    {
                        Policies = new List<V2HPAScalingPolicy>
                        {
                            new V2HPAScalingPolicy
                            {
                                Type = "Percent",
                                PeriodSeconds = 30,
                                Value = 100
                            }
                        },
                        StabilizationWindowSeconds = 30
                    }
                },
                Metrics = new List<V2MetricSpec>
                {
                    new V2MetricSpec
                    {
                        Type = "Object",
                        ObjectProperty = new V2ObjectMetricSource
                        {
                            Metric = new V2MetricIdentifier
                            {
                                Name = metricName
                            },
                            DescribedObject = new V2CrossVersionObjectReference
                            {
                                ApiVersion = "justfaas.com/v1alpha1",
                                Kind = "Function",
                                Name = func.Name()
                            },
                            Target = new V2MetricTarget
                            {
                                Type = "Value",
                                AverageValue = new ResourceQuantity( scaleTarget.ToString() )
                            }
                        }
                    }
                }
            }
        };

        // if scale to zero annotation exists
        if ( func.GetAnnotation( AppAnnotations.ScaleToZero.Key ) == "true" )
        {
            // the annotation becomes a label in the HPA so that the idler can select it
            hpa.Metadata.Labels.Add( AppAnnotations.ScaleToZero.Key, "true" );

            // annotated cooldown period
            if ( func.Annotations().ContainsKey( AppAnnotations.ScaleToZeroCooldown.Key ) )
            {
                hpa.Metadata.EnsureAnnotations()
                    .Add( 
                        AppAnnotations.ScaleToZeroCooldown.Key,
                        func.GetAnnotation( AppAnnotations.ScaleToZeroCooldown.Key )
                    );
            }
        }

        return ( hpa );
    }

    private string GetMetricName( string mode )
    {
        if ( !mode.Equals( "rps" ) )
        {
            throw new ArgumentOutOfRangeException( $"Scale mode '{mode}' is not supported." );
        }

        return "faas_proxy_requests_per_second";
    }
}
