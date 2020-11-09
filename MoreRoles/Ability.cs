using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Grenades;
using HarmonyLib;
using Mirror;
using SixModLoader.Api.Events.Player;
using SixModLoader.Api.Events.Player.Weapon;
using SixModLoader.Api.Extensions;
using SixModLoader.Events;
using UnityEngine;
using Logger = SixModLoader.Logger;
using Object = UnityEngine.Object;

namespace MoreRoles
{
    public interface IAbility
    {
    }

    public class ExplodeOnDeathAbility : IAbility
    {
        private static readonly Lazy<GrenadeSettings> _fragGrenade = new Lazy<GrenadeSettings>(() =>
            PlayerManager.localPlayer.GetComponent<GrenadeManager>().availableGrenades.Single(x => x.inventoryID == ItemType.GrenadeFrag)
        );

        [EventHandler]
        private static void OnPlayerDeath(PlayerDeathEvent ev)
        {
            if (
                MoreRolesMod.Instance.RoleManager.Players.TryGetValue(ev.Player, out var role)
                && role != null
                && role.Abilities.OfType<ExplodeOnDeathAbility>().Any()
            )
            {
                Logger.Debug($"{ev.Player.Format()} exploded");

                var grenadeManager = ev.Player.GetComponent<GrenadeManager>();
                var grenade = Object.Instantiate(_fragGrenade.Value.grenadeInstance).GetComponent<Grenade>();
                grenade.fuseDuration = 0f;
                grenade.InitData(grenadeManager, Vector3.zero, Vector3.zero);
                NetworkServer.Spawn(grenade.gameObject);
            }
        }
    }

    public class BypassKeycardReadersAbility : IAbility
    {
        [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdOpenDoor))]
        public static class Patch
        {
            public static bool Invoke(PlayerInteract playerInteract)
            {
                try
                {
                    if (
                        MoreRolesMod.Instance.RoleManager.Players.TryGetValue(playerInteract._hub, out var role)
                        && role != null
                        && role.Abilities.OfType<BypassKeycardReadersAbility>().Any()
                    )
                    {
                        Logger.Debug($"{playerInteract._hub.Format()} bypassed keycard reader");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                return playerInteract._sr.BypassMode;
            }

            private static readonly MethodInfo m_Invoke = AccessTools.Method(typeof(Patch), nameof(Invoke));
            private static readonly FieldInfo f_BypassMode = AccessTools.Field(typeof(ServerRoles), nameof(ServerRoles.BypassMode));

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeInstructions = instructions.ToList();

                var i = codeInstructions.FindIndex(x => x.LoadsField(f_BypassMode)) - 1;
                codeInstructions.RemoveRange(i, 2);
                codeInstructions.Insert(i, new CodeInstruction(OpCodes.Call, m_Invoke));

                return codeInstructions;
            }
        }
    }

    public class LifeStealAbility : IAbility
    {
        public float Multiplier { get; set; } = 0.5f;

        [EventHandler]
        private static void OnPlayerShotByPlayer(PlayerShotByPlayerEvent ev)
        {
            if (MoreRolesMod.Instance.RoleManager.Players.TryGetValue(ev.Shooter, out var role) && role != null)
            {
                var ability = role.Abilities.OfType<LifeStealAbility>().SingleOrDefault();
                if (ability != null)
                {
                    ev.Shooter.playerStats.HealHPAmount(ev.Damage * ability.Multiplier);
                }
            }
        }
    }
}