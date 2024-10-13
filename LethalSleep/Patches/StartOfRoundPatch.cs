using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace LethalSleep.Patches {
    
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch {

        private static GameObject TryGetBed() {
            AutoParentToShip[] beds = Object.FindObjectsOfType<AutoParentToShip>().Where(obj => obj.name == "Bunkbeds").ToArray();
            if (beds.Length == 1) return beds[0].gameObject;
            
            PluginLogger.Error("Can't find bed object or found multiples : plugin will not enable");
            Plugin.DisabledByError = true;
            return null;

        }
        
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        internal static void StartPostfix() {
            if (Plugin.DisabledByError) return;
            
            GameObject bed = TryGetBed();
            if (bed == null) return;
            BedWorker.ConfigureBed(bed);
        }

        [HarmonyPatch(nameof(StartOfRound.OnPlayerDC))]
        [HarmonyPostfix]
        internal static void OnPlayerDCPatch(int playerObjectNumber, ulong clientId) {
            if (BedWorker.SleepingPlayers.ContainsKey(playerObjectNumber)) {
                BedWorker.SleepingPlayers.Remove(playerObjectNumber);
            }
        }
        
    }
}