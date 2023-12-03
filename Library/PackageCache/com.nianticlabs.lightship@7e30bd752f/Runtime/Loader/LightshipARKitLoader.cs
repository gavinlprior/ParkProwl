// Copyright 2022-2023 Niantic.
// Restrict inclusion to iOS builds to avoid symbol resolution error

#if UNITY_IOS || UNITY_EDITOR

#if NIANTIC_LIGHTSHIP_AR_LOADER_ENABLED && UNITY_IOS && !UNITY_EDITOR
#define NIANTIC_LIGHTSHIP_ARKIT_LOADER_ENABLED
#endif

using Niantic.Lightship.AR.Subsystems;
using Niantic.Lightship.AR.Subsystems.Playback;
using Niantic.Lightship.AR.Utilities.Log;
using Niantic.Lightship.AR.XRSubsystems;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARSubsystems;

namespace Niantic.Lightship.AR.Loader
{
    /// <summary>
    /// Manages the lifecycle of Lightship and ARKit subsystems.
    /// </summary>
    public class LightshipARKitLoader : ARKitLoader, ILightshipLoader
    {
        private PlaybackLoaderHelper _playbackHelper;
        private NativeLoaderHelper _nativeHelper;

        PlaybackDatasetReader ILightshipLoader.PlaybackDatasetReader => _playbackHelper?.DatasetReader;

        /// <summary>
        /// Optional override settings for manual XR Loader initialization
        /// </summary>
        public LightshipSettings InitializationSettings { get; set; }

        /// <summary>
        /// The `XROcclusionSubsystem` whose lifecycle is managed by this loader.
        /// </summary>
        public XROcclusionSubsystem LightshipOcclusionSubsystem => GetLoadedSubsystem<XROcclusionSubsystem>();

        /// <summary>
        /// The `XRPersistentAnchorSubsystem` whose lifecycle is managed by this loader.
        /// </summary>
        public XRPersistentAnchorSubsystem LightshipPersistentAnchorSubsystem =>
            GetLoadedSubsystem<XRPersistentAnchorSubsystem>();

        /// <summary>
        /// Initializes the loader.
        /// </summary>
        /// <returns>`True` if the session subsystems were successfully created, otherwise `false`.</returns>
        public override bool Initialize()
        {
            if (InitializationSettings == null)
            {
                InitializationSettings = LightshipSettings.Instance;
            }

            return ((ILightshipLoader)this).InitializeWithSettings(InitializationSettings);
        }

        bool ILightshipLoader.InitializeWithSettings(LightshipSettings settings, bool isTest)
        {
#if NIANTIC_LIGHTSHIP_ARKIT_LOADER_ENABLED
            bool initializationSuccess;

            if (settings.OverrideLoggingLevel)
            {
                Log.LogLevel = settings.LogLevel;
            }

            if (settings.UsePlayback)
            {
                // Initialize Playback subsystems instead of initializing ARKit subsystems
                // (for those features that aren't added/supplanted by Lightship),
                _playbackHelper = new PlaybackLoaderHelper();
                initializationSuccess = _playbackHelper.Initialize(this, settings);
            }
            else
            {
                // Initialize ARKit subsystems
                initializationSuccess = base.Initialize();
            }

            // Don't initialize lightship subsystems if ARKit's initialization has already failed.
            if (!initializationSuccess)
            {
                return false;
            }

            // Must initialize Lightship subsystems after ARKit's, because when there's overlap, the native helper will
            // (1) destroy ARKit's subsystems and then
            // (2) create Lightship's version of the subsystems
            _nativeHelper = new NativeLoaderHelper();

            // Determine if device supports LiDAR only during the window where AFTER arf loader initializes but BEFORE
            // lightship loader initializes as non-playback relies on checking the existence of ARF meshing subsystem
            var isLidarSupported = settings.UsePlayback
                ? _playbackHelper.DatasetReader.GetIsLidarAvailable()
                : _nativeHelper.DetermineIfDeviceSupportsLidar();

            return _nativeHelper.Initialize(this, settings, isLidarSupported, isTest);
#else
            return false;
#endif
        }

        /// <summary>
        /// This method does nothing. Subsystems must be started individually.
        /// </summary>
        /// <returns>Returns `true` on iOS. Returns `false` otherwise.</returns>
        public override bool Start()
        {
#if NIANTIC_LIGHTSHIP_ARKIT_LOADER_ENABLED
            return base.Start();
#else
            return false;
#endif
        }

        /// <summary>
        /// This method does nothing. Subsystems must be stopped individually.
        /// </summary>
        /// <returns>Returns `true` on iOS. Returns `false` otherwise.</returns>
        public override bool Stop()
        {
#if NIANTIC_LIGHTSHIP_ARKIT_LOADER_ENABLED
            return base.Stop();
#else
            return false;
#endif
        }

        /// <summary>
        /// Destroys each subsystem.
        /// </summary>
        /// <returns>Always returns `true`.</returns>
        public override bool Deinitialize()
        {
#if NIANTIC_LIGHTSHIP_ARKIT_LOADER_ENABLED
            _nativeHelper?.Deinitialize(this);
            _playbackHelper?.Deinitialize(this);

            return base.Deinitialize();
#else
            return true;
#endif
        }
    }
}

#endif
