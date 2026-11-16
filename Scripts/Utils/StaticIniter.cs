using System;
using System.Collections.Generic;
using System.Text;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace LX.Common.Core
{
    /// <summary>
    /// A way to explicitly init static members in the modern CoreCLR world
    /// 
    /// Recommended usage example:
    /// 
    /// public class SomeClassWithStatics
    /// {
    ///     private static StaticIniter staticIniter(() => {
    ///         staticA = null,
    ///         staticB = false, // etc
    ///     });
    /// }
    /// </summary>
    [NoAutoStaticsCleanup]
    public struct StaticIniter
    {
        private static List<Action> initializers;

        public StaticIniter(Action initializer)
        {
            (initializers ??= new()).Add(initializer);
            initializer();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnInitializeOnLoad()
        {
            if (initializers != null)
            {
                foreach (var initializer in initializers)
                    initializer();
            }
        }
    }
}
