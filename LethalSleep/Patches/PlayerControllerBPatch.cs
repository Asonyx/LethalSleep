using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalSleep.Patches {
    
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerControllerBPatch {

        [HarmonyPatch("Interact_performed")]
        [HarmonyPostfix]
        internal static void InteractPerformedPatch(PlayerControllerB __instance) {
            if (!__instance.IsOwner || !__instance.isPlayerControlled || !BedWorker.SleepingPlayers.ContainsKey((int)__instance.playerClientId)) return;
            Networker.Instance.PlayerWakeUpServerRpc((int)__instance.playerClientId);
        }

        [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
        [HarmonyPrefix]
        internal static bool SetTipPatch(PlayerControllerB __instance) {
            if (!BedWorker.SleepingPlayers.ContainsKey((int)__instance.playerClientId)) return true;
            
            __instance.cursorIcon.enabled = false;
            __instance.cursorTip.text = "Wake up : [E]";
            return false;
        }
        
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        internal static void UpdatePatch(PlayerControllerB __instance, ref float ___targetYRot) {
            if (!BedWorker.SleepingPlayers.ContainsKey((int)__instance.playerClientId)) return;
            
            if (!__instance.isPlayerControlled) {
                BedWorker.SleepingPlayers.Remove((int)__instance.playerClientId);
                return;
            }
            Transform snapPoint = BedWorker.SleepingSlots[BedWorker.SleepingPlayers[(int)__instance.playerClientId]];
            
            __instance.transform.SetParent(snapPoint);
            __instance.transform.localPosition = Vector3.Lerp(__instance.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
            
            if (__instance.IsOwner) return;
            
            __instance.transform.localEulerAngles = new Vector3(
                snapPoint.transform.localEulerAngles.x,
                Mathf.LerpAngle(__instance.transform.localEulerAngles.y, ___targetYRot, Time.deltaTime * 10f), 
                snapPoint.transform.localEulerAngles.z - 90);
        }
        
    }
}