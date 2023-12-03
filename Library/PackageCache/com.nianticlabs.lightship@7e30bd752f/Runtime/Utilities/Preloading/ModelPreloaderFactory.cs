// Copyright 2022-2023 Niantic.

using System;
using Niantic.Lightship.AR.Core;
using UnityEngine;

namespace Niantic.Lightship.AR.Utilities.Preloading
{
    [PublicAPI]
    public class ModelPreloaderFactory
    {
        public static IModelPreloader Create()
        {
            return Create(false);
        }

        internal static IModelPreloader Create(bool useMock = false)
        {
            var unityContextHandle = LightshipUnityContext.UnityContextHandle;

            if (unityContextHandle == IntPtr.Zero)
            {
                Debug.Assert(false, "Lightship must be initialized before the Model Preloader can be " +
                    "called. An application can use RegisterModel to complete the download before starting Lightship.");
                return null;
            }

            if (!useMock)
            {
                return new NativeModelPreloader(LightshipUnityContext.UnityContextHandle);
            }
            else
            {
                Debug.Assert(false, "ModelPreloader mock is not implemented");
                return null;
            }
        }
    }
}
