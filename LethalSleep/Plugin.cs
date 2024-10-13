using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalSleep.Resources;
using UnityEngine;

namespace LethalSleep {
    
    [BepInPlugin(PluginConstants.GUID, PluginConstants.NAME, PluginConstants.VERSION)]
    public class Plugin : BaseUnityPlugin {
        
        internal static Plugin Instance { get; private set; }

        internal static bool DisabledByError = false;

        public ManualLogSource logger;

        private Harmony _harmony;
        
        private void Awake() {
            if (Instance != null) return;
            
            Instance = this;
            logger = BepInEx.Logging.Logger.CreateLogSource(PluginConstants.GUID);

            _harmony = new Harmony(PluginConstants.GUID);
            
            Assets.Load();
            
            _harmony.PatchAll();
            NetcodePatcher();
            
            logger.LogInfo("LethalSleep loaded successfully");
        }
        
        private static void NetcodePatcher() {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (MethodInfo method in methods)
                {
                    object[] attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
    
    public static class PluginConstants {
        public const string GUID = "LethalSleep";
        public const string NAME = "Lethal Sleep";
        public const string VERSION = "0.1.0";
    }

    internal static class PluginLogger {
        internal static void Info(object o) {
            Plugin.Instance.logger.LogInfo(o);
        }
        
        internal static void Warn(object o) {
            Plugin.Instance.logger.LogWarning(o);
        }
        
        internal static void Error(object o) {
            Plugin.Instance.logger.LogError(o);
        }
        
        internal static void Debug(object o) {
            Plugin.Instance.logger.LogDebug(o);
        }
    }
}