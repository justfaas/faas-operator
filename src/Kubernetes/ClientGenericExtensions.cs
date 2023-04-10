using k8s;
using k8s.Autorest;
using k8s.Models;

public static class KubernetesClientGenericExtensions
{
    public static Task<T> CreateNamespacedAsync<T>( this IKubernetes client
        , string ns
        , T body )
    where T : IKubernetesObject
        => client.WithType<T>()
            .CreateNamespacedAsync<T>( body, ns );

    public static async Task<HttpOperationResponse<T>> CreateNamespacedWithHttpMessagesAsync<T>( this IKubernetes client
        , string ns
        , T body )
    where T : IKubernetesObject
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        body.Kind = attr.Kind;
        body.ApiVersion = attr.GetApiVersion();

        var response = await client.CustomObjects.CreateNamespacedCustomObjectWithHttpMessagesAsync(
            body: body,
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: ns,
            plural: attr.GetPluralName()
        );

        var obj = k8s.KubernetesJson.Deserialize<T>( response.Body.ToString() );

        return new HttpOperationResponse<T>
        {
            Body = obj,
            Request = response.Request,
            Response = response.Response
        };
    }

    public static async Task DeleteNamespacedAsync<T>( this IKubernetes client
        , string ns
        , string name )
    where T : IKubernetesObject
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        await client.CustomObjects.DeleteNamespacedCustomObjectAsync(
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: ns,
            plural: attr.GetPluralName(),
            name: name
        );
    }

    public static async Task<HttpOperationResponse> DeleteNamespacedWithHttpMessagesAsync<T>( this IKubernetes client
        , string ns
        , string name )
    where T : IKubernetesObject
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        var response = await client.CustomObjects.DeleteNamespacedCustomObjectWithHttpMessagesAsync(
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: ns,
            plural: attr.GetPluralName(),
            name: name
        );

        return new HttpOperationResponse
        {
            Request = response.Request,
            Response = response.Response
        };
    }

    public static async Task<T> PatchNamespacedAsync<T>( this IKubernetes client
        , string ns
        , string name
        , T body )
    where T : IKubernetesObject
    {
        var response = await PatchNamespacedWithHttpMessagesAsync<T>( client, ns, name, body );

        return response.Body;
    }
        // => client.WithType<T>()
        //     .PatchNamespacedAsync<T>( new V1Patch( body, V1Patch.PatchType.MergePatch ), ns, name );

    public static async Task<HttpOperationResponse<T>> PatchNamespacedWithHttpMessagesAsync<T>( this IKubernetes client
        , string ns
        , string name
        , T body )
    where T : IKubernetesObject
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        var response = await client.CustomObjects.PatchNamespacedCustomObjectWithHttpMessagesAsync(
            body: new V1Patch( body, V1Patch.PatchType.MergePatch ),
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: ns,
            plural: attr.GetPluralName(),
            name: name
        );

        var obj = KubernetesJson.Deserialize<T>( response.Body.ToString() );

        return new HttpOperationResponse<T>
        {
            Body = obj,
            Request = response.Request,
            Response = response.Response
        };
    }

    public static Task<T> ReadNamespacedAsync<T>( this IKubernetes client
        , string ns
        , string name )
    where T : IKubernetesObject
        => client.WithType<T>()
            .ReadNamespacedAsync<T>( ns, name );

    public static async Task<IList<T>> ListAsync<T>( this IKubernetes client
        , string labelSelector )
    where T : IKubernetesObject<V1ObjectMeta>
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        var result = await client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(
            group: attr.Group,
            version: attr.ApiVersion,
            plural: attr.GetPluralName(),
            labelSelector: labelSelector
        );

        var list = KubernetesJson.Deserialize<ResourceList<T>>( result.Body.ToString() );

        return ( list.Items );
    }
    public static async Task<IList<T>> ListNamespacedAsync<T>( this IKubernetes client
        , string ns
        , string? labelSelector = null )
    where T : IKubernetesObject<V1ObjectMeta>
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        var result = await client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: ns,
            plural: attr.GetPluralName(),
            labelSelector: labelSelector
        );

        var list = KubernetesJson.Deserialize<ResourceList<T>>( result.Body.ToString() );

        return ( list.Items );
    }

    public static async Task<HttpOperationResponse<T>> ReadNamespacedWithHttpMessagesAsync<T>( this IKubernetes client
        , string ns
        , string name )
    where T : IKubernetesObject
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        var response = await client.CustomObjects.GetNamespacedCustomObjectWithHttpMessagesAsync(
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: ns,
            plural: attr.GetPluralName(),
            name: name
        );

        var obj = KubernetesJson.Deserialize<T>( response.Body.ToString() );

        return new HttpOperationResponse<T>
        {
            Body = obj,
            Request = response.Request,
            Response = response.Response
        };
    }

    public static Task<T> ReplaceNamespacedAsync<T>( this IKubernetes client
        , string ns
        , string name
        , T body )
    where T : IKubernetesObject
        => client.WithType<T>()
            .ReplaceNamespacedAsync<T>( body, ns, name );

    public static async Task<HttpOperationResponse<T>> ReplaceNamespacedWithHttpMessagesAsync<T>( this IKubernetes client
        , string ns
        , string name
        , T body )
    where T : IKubernetesObject
    {
        var attr = typeof( T ).GetKubernetesEntityAttribute();

        var response = await client.CustomObjects.ReplaceNamespacedCustomObjectWithHttpMessagesAsync(
            body: body,
            group: attr.Group,
            version: attr.ApiVersion,
            namespaceParameter: ns,
            plural: attr.GetPluralName(),
            name: name
        );

        var obj = KubernetesJson.Deserialize<T>( response.Body.ToString() );

        return new HttpOperationResponse<T>
        {
            Body = body,
            Request = response.Request,
            Response = response.Response
        };
    }
}
