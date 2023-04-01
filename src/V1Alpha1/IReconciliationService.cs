using k8s;
using k8s.Models;

public interface IReconciliationService<TObject> where TObject : IKubernetesObject<V1ObjectMeta>
{
    Task DeleteAsync<T>( TObject source ) where T : IKubernetesObject<V1ObjectMeta>;
    Task ReconcileAsync<T>( TObject source, T obj ) where T : IKubernetesObject<V1ObjectMeta>;

    Task<T?> GetObjectAsync<T>( TObject source ) where T : IKubernetesObject<V1ObjectMeta>;
}

/*
TODO: optimize reconciliation services

There is probably duplicate code in both reconciliation services. It's probably
a good idea to breate a base service class with the duplicate code and override
custom behaviour where needed.
*/
