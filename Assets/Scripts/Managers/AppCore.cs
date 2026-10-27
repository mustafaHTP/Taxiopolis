using System;
using Framework.Services;
using Framework.Singleton;
using Game.Utils;
using UnityEngine;

namespace Game.Managers
{
    /// <summary>
    /// The persistent application root that initializes core systems
    /// before scene-specific and gameplay objects.
    /// This must be run before everything else
    /// </summary>
    public class AppCore : PersistentSingleton<AppCore>
    {
        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
            {
                LogX.LogWarning("Another instance of AppCore already exists. Destroying this instance.");

                return;
            }

            RegisterServices();
            InitServices();
            AfterInitServices();

            LogX.LogDebug("AppCore initialized. Global services registered and initialized.");
        }


        private void OnDestroy()
        {
            if (Instance != this) return;

            LogX.LogDebug("AppCore is being destroyed. Clearing global services.");
            DisposeServices();
            // instead of registering one by one, i clear it
            ClearServices();
        }

        private void RegisterServices()
        {
            IGlobalService[] globalServices = GetComponentsInChildren<IGlobalService>();
            if (globalServices == null)
            {
                LogX.LogError("There is no global service found!...");

                return;
            }

            for (int i = 0; i < globalServices.Length; i++)
            {
                IGlobalService globalService = globalServices[i];
                Type typeOfGlobalService = globalService.GetType();
                ServiceLocator.Global.RegisterService(typeOfGlobalService, globalService);

                LogX.LogDebug($"{(globalService as MonoBehaviour).name} is registered");
            }
        }

        private void InitServices()
        {
            foreach (var service in ServiceLocator.Global.Services)
            {
                ((IGlobalService)service).Init();
            }
        }

        private void AfterInitServices()
        {
            foreach (var service in ServiceLocator.Global.Services)
            {
                ((IGlobalService)service).AfterInit();
            }
        }

        private void DisposeServices()
        {
            foreach (var service in ServiceLocator.Global.Services)
            {
                ((IGlobalService)service).Dispose();
            }
        }

        private void ClearServices()
        {
            ServiceLocator.Global.ClearServices();
        }
    }
}

