using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;

namespace RemoteKeycard
{
    public class Config : IConfig
    {
        public bool RequireKeycard { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
    }

    public class RemoteKeycard : Plugin<Config>
    {
        public override string Name => "RemoteKeycard";
        public override string Author => "NotKeira";
        public override Version Version => new Version(1, 0, 0);

        public override void OnEnabled()
        {
            RegisterEvents();
            Log.Info("Enabled Remote Keycards");
        }

        public override void OnDisabled()
        {
            UnregisterEvents();
            Log.Info("Disabled Remote Keycards and unloaded events");
        }

        private void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingLocker += OnInteractingLocker;
            Exiled.Events.Handlers.Player.UnlockingGenerator += OnOpeningGenerator;
        }

        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingLocker -= OnInteractingLocker;
            Exiled.Events.Handlers.Player.UnlockingGenerator -= OnOpeningGenerator;
        }

        private void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            Log.Debug($"Interacting door: {ev.Door} {ev.IsAllowed}");
            if (ev.IsAllowed) return;
            var shouldAllow = CheckPermissions(ev.Player, ev.Door.KeycardPermissions);
            if (!shouldAllow) return;
            ev.IsAllowed = true;
            Log.Debug($"Door interaction allowed for {ev.Player.Nickname}");
        }

        private void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (ev.IsAllowed) return;
            var shouldAllow = CheckPermissions(ev.Player, ev.InteractingChamber.RequiredPermissions);
            if (!shouldAllow) return;
            ev.IsAllowed = true;
            Log.Debug($"Locker interaction allowed for {ev.Player.Nickname}");
        }

        private void OnOpeningGenerator(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed) return;
            var shouldAllow = CheckPermissions(ev.Player, (KeycardPermissions)2); // 2 = Scientist keycard level
            if (!shouldAllow) return;
            ev.IsAllowed = true;
            Log.Debug($"Generator interaction allowed for {ev.Player.Nickname}");
        }

        private bool CheckPermissions(Player player, KeycardPermissions requiredPermissions)
        {
            Log.Debug($"Checking permissions for {player.Nickname} using {requiredPermissions}");

            if (!player.IsAlive)
            {
                Log.Debug("Player is either null or not alive.");
                return false;
            }

            if (player.IsScp)
            {
                Log.Debug($"Player role {player.Role.Name} is disabled.");
                return false;
            }

            if (requiredPermissions == 0)
            {
                Log.Debug("No permissions required.");
                return false;
            }


            if (HasRequiredPermissions(player.CurrentItem, requiredPermissions))
                return false;
            if (Config.RequireKeycard && !HasKeycardInInventory(player, requiredPermissions))
                return false;


            if (Config.Debug)
            {
                Log.Debug(
                    $"Player {player.Nickname} used remote keycard access for permission level {requiredPermissions}");
            }

            return true;
        }

        private static bool HasRequiredPermissions(Item item, KeycardPermissions requiredPermissions)
        {
            if (item == null || !item.IsKeycard) return false;
            Log.Debug($"Checking item {item}");
            var validItem = (Keycard)item;
            Log.Debug($"Item {item} exports {(validItem.Permissions & requiredPermissions) != 0}");
            return (validItem.Permissions & requiredPermissions) != 0;
        }

        private bool HasKeycardInInventory(Player player, KeycardPermissions requiredPermissions)
        {
            return player.Items.Any(item => HasRequiredPermissions(item, requiredPermissions));
        }
    }
}