using System;
using System.Collections.Generic;
using UnityEngine;

namespace SymbiosisDefense.Core
{
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator instance;
        private readonly Dictionary<Type, object> services = new();

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void Register<T>(T service) where T : class
        {
            if (instance == null)
            {
                Debug.LogError("ServiceLocator not initialized!");
                return;
            }

            instance.services[typeof(T)] = service;
            Debug.Log($"Registered service: {typeof(T).Name}");
        }

        public static T Get<T>() where T : class
        {
            if (instance == null)
            {
                Debug.LogError("ServiceLocator not initialized!");
                return null;
            }

            if (instance.services.TryGetValue(typeof(T), out var service))
                return service as T;

            Debug.LogError($"Service not found: {typeof(T).Name}");
            return null;
        }

        public static bool IsRegistered<T>() where T : class
        {
            return instance != null && instance.services.ContainsKey(typeof(T));
        }
    }
}