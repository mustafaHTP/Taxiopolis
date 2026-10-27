using UnityEngine;

namespace Framework.Services
{
    /// <summary>
    /// It is for services that is used for global scope.  
    /// </summary>
    public interface IGlobalService
    {
        void Init();
        void AfterInit();
        void Dispose();
    }
}
