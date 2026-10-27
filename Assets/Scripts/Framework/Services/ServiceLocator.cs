using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Services
{
    public static class ServiceLocator
    {
        public static ServiceContainer Global { get; private set; } = new();
        public static ServiceContainer Scene { get; private set; } = new();
    }
}
