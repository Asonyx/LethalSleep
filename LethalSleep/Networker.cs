using GameNetcodeStuff;
using HarmonyLib;
using LethalSleep.Resources;
using Unity.Netcode;
using UnityEngine;

namespace LethalSleep {
    
    [HarmonyPatch]
    public class Networker : NetworkBehaviour {
        
        public static Networker Instance;
        
        [ServerRpc(RequireOwnership = false)]
        public void PlayerSleepServerRpc(int playerId) {
            PlayerSleepClientRpc(playerId);
        }
        
        [ClientRpc]
        public void PlayerSleepClientRpc(int playerId) {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            if (player.isPlayerDead || !player.isPlayerControlled) return;
            BedWorker.Sleep(player);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void PlayerWakeUpServerRpc(int playerId) {
            PlayerWakeUpClientRpc(playerId);
        }
        
        [ClientRpc]
        public void PlayerWakeUpClientRpc(int playerId) {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            if (player.isPlayerDead || !player.isPlayerControlled) return;
            BedWorker.WakeUp(player);
        }
        
        public override void OnNetworkSpawn() {
            
            
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;
            
            base.OnNetworkSpawn();
        }
        
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        private static void SpawnNetHandler() {
            if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;
            
            GameObject networkHandlerHost = Instantiate(Assets.NetworkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
        
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPostfix]
        private static void Init() {
            Assets.NetworkPrefab.AddComponent<Networker>();
            NetworkManager.Singleton.AddNetworkPrefab(Assets.NetworkPrefab);
        }
    }
}