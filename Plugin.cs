using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;

namespace RemoteKeycard
{
    public class Config : IConfig
    {
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

        private static void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.IsAllowed) return;
            if (ev.Door.IsLocked) return;
            ev.IsAllowed = HasValidKeycard(ev.Player, ev.Door.KeycardPermissions);
        }

        private static void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (ev.IsAllowed) return;
            ev.IsAllowed = HasValidKeycard(ev.Player, ev.InteractingChamber.RequiredPermissions);
        }

        private static void OnOpeningGenerator(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed) return;
            ev.IsAllowed = HasValidKeycard(ev.Player, ev.Generator.KeycardPermissions);
        }

        private static bool HasValidKeycard(Player player, KeycardPermissions permissions)
        {
            return player.Items.Any(item => item is Keycard kc && kc.Permissions.HasFlagFast(permissions));
        }
    }
}