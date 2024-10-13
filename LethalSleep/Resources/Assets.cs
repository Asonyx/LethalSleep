using System.IO;
using System.Reflection;
using UnityEngine;

namespace LethalSleep.Resources {
    internal static class Assets {
        
        public static AssetBundle Bundle;

        public static Sprite SleepIcon;

        public static GameObject NetworkPrefab;

        public static void Load() {
            Stream bundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LethalSleep.Resources.sleepbundle");
            if (bundleStream == null) {
                PluginLogger.Error("Cannot load the asset bundle");
                Plugin.DisabledByError = true;
                return;
            }
            Bundle = AssetBundle.LoadFromStream(bundleStream);
            if (Bundle == null) {
                PluginLogger.Error("Cannot load the asset bundle");
                Plugin.DisabledByError = true;
                return;
            }
            
            SleepIcon = Bundle.LoadAsset<Sprite>("Assets/LCSleep/noun-sleep-2216308.png");
            NetworkPrefab = Bundle.LoadAsset<GameObject>("Assets/LCSleep/NetPrefab.prefab");
        }

    }
}