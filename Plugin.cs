using System;
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
            if (ev.Door.IsLocked) return;
            if (ev.IsAllowed) return;
            ev.IsAllowed = CheckPermissions(ev.Player, ev.Door.KeycardPermissions);
        }

        private void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (ev.IsAllowed) return;
            ev.IsAllowed = CheckPermissions(ev.Player, ev.InteractingChamber.RequiredPermissions);
        }

        private void OnOpeningGenerator(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed) return;
            ev.IsAllowed = CheckPermissions(ev.Player, ev.Generator.KeycardPermissions);
        }

        private bool CheckPermissions(Player player, KeycardPermissions requiredPermissions)
        {
            if (player.IsScp || !player.IsAlive || requiredPermissions == 0)
                return false;

            if (HasRequiredPermissions(player.CurrentItem, requiredPermissions))
                return true;

            foreach (var item in player.Items)
            {
                if (!HasRequiredPermissions(item, requiredPermissions)) continue;
                if (item.Type == ItemType.SurfaceAccessPass)
                    player.RemoveItem(item);
                if (Config.Debug)
                {
                    Log.Debug(
                        $"Player {player.Nickname} used remote keycard access for permission level {requiredPermissions}");
                }

                return true;
            }

            return !Config.RequireKeycard;
        }

        private static bool HasRequiredPermissions(Item item, KeycardPermissions requiredPermissions)
        {
            if (item == null || !item.IsKeycard) return false;
            var validItem = (Keycard)item;
            return (validItem.Permissions & requiredPermissions) != 0;
        }
    }
}