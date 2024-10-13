using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalSleep.Resources;
using UnityEngine;
using UnityEngine.Events;

namespace LethalSleep {
    public static class BedWorker {
        
        public const int INTERACT_LAYER = 9;

        public static readonly Vector3[] SleepingPositions = { 
            new Vector3(-2.069098f, -0.7690924f, -0.5f), 
            new Vector3(-2.069098f, -0.7690924f, 0.5f),
            new Vector3(-2.069098f, -0.7690924f, 1.5f),
            new Vector3(-2.069098f, -0.7690924f, 2.5f)
        };
        
        public static readonly Vector3 WakeUpPosition = new Vector3(-2.569098f, 0.9809076f, -1.25f);

        private static InteractTrigger _sleepTrigger;
        private static PlaceableShipObject _bed;
        
        public static readonly Dictionary<int, int> SleepingPlayers = new Dictionary<int, int>();
        public static readonly List<Transform> SleepingSlots = new List<Transform>();
        
        private static int _currentSlot;
        
        public static void ConfigureBed(GameObject bed) {
            PluginLogger.Debug("Configuring bed");
            
            GameObject bedCollider = bed.transform.GetChild(0).gameObject;
            
            bedCollider.tag = "InteractTrigger";
            bedCollider.layer = INTERACT_LAYER;
            
            bedCollider.GetComponent<MeshRenderer>().enabled = false;

            _bed = bed.GetComponentInChildren<PlaceableShipObject>();

            int i = 0;
            foreach (Vector3 position in SleepingPositions) {
                GameObject slot = new GameObject("SleepingSlot " + i);
                slot.transform.SetParent(bed.transform);
                slot.transform.localPosition = position;
                slot.transform.localRotation = Quaternion.Euler(0, 180, 90);
                SleepingSlots.Add(slot.transform);
                i++;
            }
            
            InteractTrigger sleepTrigger = bedCollider.AddComponent<InteractTrigger>();
            SetupTrigger(sleepTrigger);
        }
        
        private static void SetupTrigger(InteractTrigger trigger) {
            if (trigger == null) {
                PluginLogger.Error("InteractTrigger is null");
                return;
            }
            trigger.holdingInteractEvent = new InteractEventFloat();
            trigger.onInteract = new InteractEvent();
            trigger.onInteractEarly = new InteractEvent();
            trigger.onInteractEarlyOtherClients = new InteractEvent();
            trigger.onStopInteract = new InteractEvent();
            trigger.onCancelAnimation = new InteractEvent();
            trigger.hoverTip = "Sleep";
            trigger.hoverIcon = Assets.SleepIcon;
            trigger.disabledHoverTip = "Not enough space to sleep";
            trigger.interactable = true;
            trigger.oneHandedItemAllowed = true;
            trigger.twoHandedItemAllowed = false;
            trigger.holdInteraction = true;
            trigger.timeToHold = 0.5f;
            trigger.timeToHoldSpeedMultiplier = 1f;
            trigger.hidePlayerItem = true;
            
            trigger.onInteract.AddListener(OnPlayerInteract);
            
            _sleepTrigger = trigger;
        }

        public static void Update() {
            _bed.inUse =  SleepingPlayers.Count > 0;
            _sleepTrigger.interactable = SleepingPlayers.Count < SleepingPositions.Length;
            if (SleepingPlayers.Count >= SleepingPositions.Length) return;
            
            for (int i = 0; i < SleepingSlots.Count; i++) {
                if (SleepingPlayers.ContainsValue(i)) continue;
                _sleepTrigger.playerPositionNode = SleepingSlots[i];
                _currentSlot = i;
                break;
            }
        }
        
        public static bool EnoughSpaceToSleep() {
            return SleepingPlayers.Count < SleepingPositions.Length;
        }
        
        private static void OnPlayerInteract(PlayerControllerB player) {
            if (!EnoughSpaceToSleep()) {
                Update();
                return;
            }
            
            Networker.Instance.PlayerSleepServerRpc((int)player.playerClientId);
        }

        public static void Sleep(PlayerControllerB player) {
            SleepingPlayers.Add((int)player.playerClientId, _currentSlot);
            player.inSpecialInteractAnimation = true;
            player.disableSyncInAnimation = true;
            Update();
        }
        
        public static void WakeUp(PlayerControllerB player) {
            SleepingPlayers.Remove((int)player.playerClientId);
            player.inSpecialInteractAnimation = false;
            player.disableSyncInAnimation = false;
            player.transform.SetParent(player.playersManager.playersContainer);
            player.transform.localPosition = Vector3.zero;
            player.transform.position = WakeUpPosition + _sleepTrigger.gameObject.transform.position;
            Update();
        }

    }
}