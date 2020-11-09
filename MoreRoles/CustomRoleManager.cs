using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using MoreItems;
using SixModLoader;
using SixModLoader.Api.Events.Player.Class;
using SixModLoader.Api.Extensions;
using SixModLoader.Events;
using HarmonyLib;
using SixModLoader.Api.Events.Player;

namespace MoreRoles
{
    public class CustomRoleManager
    {
        public CustomRoleManager(MoreRolesMod mod)
        {
            Mod = mod;
        }

        public MoreRolesMod Mod { get; }
        private Dictionary<ReferenceHub, CoroutineHandle> CoroutineHandles { get; } = new Dictionary<ReferenceHub, CoroutineHandle>();
        public Dictionary<ReferenceHub, CustomRole> Players { get; set; } = new Dictionary<ReferenceHub, CustomRole>();

        [EventHandler]
        private void OnPlayerRoleChange(PlayerRoleChangeEvent ev)
        {
            var role = ev.Player.characterClassManager.Classes.SafeGet(ev.RoleType);
            CustomRole customRole = null;

            foreach (var pool in Mod.Configuration.Pools)
            {
                bool Test(ICondition condition) => !condition.Test(ev.Player, ev.RoleType);
                switch (pool.ConditionsType)
                {
                    case ConditionsType.All when pool.Conditions.Any(Test):
                    case ConditionsType.Any when pool.Conditions.All(Test):
                        continue;
                }

                var randomList = new WeightedRandomList<CustomRole>();

                foreach (var weightedRole in pool.Roles)
                {
                    randomList.Add(weightedRole.Role == null ? null : Mod.Configuration.Roles[weightedRole.Role], weightedRole.Weight);
                }

                customRole = randomList.GetRandom();
            }

            Players[ev.Player] = customRole;

            if (customRole != null)
            {
                Logger.Info(ev.Player.characterClassManager.UserId + " got custom role " + customRole.Name);

                if (customRole.Name != null)
                {
                    ev.Player.ClearBroadcasts();
                    ev.Player.Broadcast(Mod.Translations.YouHaveGotRole
                        .Replace("{role}", customRole.Name.Color(role.classColor)), 5);
                }

                if (customRole.ClearStartItems)
                {
                    ev.Items.Clear();
                }

                var vanillaItems = customRole.StartItems.OfType<VanillaItemType>().ToArray();
                ev.Items.AddRange(vanillaItems.Select(x => x.Type));

                if (CoroutineHandles.TryGetValue(ev.Player, out var coroutineHandle))
                {
                    Timing.KillCoroutines(coroutineHandle);
                }

                CoroutineHandles[ev.Player] = Timing.CallDelayed(0.2f, () =>
                {
                    if (customRole.Health != null)
                    {
                        ev.Player.playerStats.Health = ev.Player.playerStats.maxHP = customRole.Health.Value;
                    }

                    if (customRole.StartAmmo != null)
                    {
                        var type = 0;
                        foreach (var ammo in customRole.StartAmmo)
                        {
                            ev.Player.ammoBox[type] += ammo;
                            type++;
                        }
                    }

                    foreach (var customItem in customRole.StartItems.OfType<CustomItemType>())
                    {
                        ((CustomItem) Activator.CreateInstance(customItem.Type)).Give(ev.Player);
                    }
                });
            }
        }

        [EventHandler]
        private void OnPlayerLeft(PlayerLeftEvent ev)
        {
            Players.Remove(ev.Player);
            CoroutineHandles.Remove(ev.Player);
        }
    }
}