using k8s;
using k8s.Models;

public interface IReconciliationService<TObject> where TObject : IKubernetesObject<V1ObjectMeta>
{
    Task DeleteAsync<T>( TObject source ) where T : IKubernetesObject<V1ObjectMeta>;
    Task ReconcileAsync<T>( TObject source, T obj ) where T : IKubernetesObject<V1ObjectMeta>;

    Task<T?> GetObjectAsync<T>( TObject source ) where T : IKubernetesObject<V1ObjectMeta>;
}
