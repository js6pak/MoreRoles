using System.Linq;
using HarmonyLib;
using SixModLoader;
using SixModLoader.Api.Configuration;
using SixModLoader.Api.Extensions;
using SixModLoader.Events;
using SixModLoader.Mods;

namespace MoreRoles
{
    [Mod("pl.js6pak.MoreRoles")]
    public class MoreRolesMod
    {
        public static MoreRolesMod Instance { get; private set; }

        public MoreRolesMod(EventManager eventManager)
        {
            Instance = this;

            GetType().Assembly.GetLoadableTypes()
                .Where(x => typeof(ICondition).IsAssignableFrom(x) || typeof(IAbility).IsAssignableFrom(x))
                .Do(ConfigurationManager.RegisterTagMapping);

            RoleManager = new CustomRoleManager(this);
            eventManager.Register(RoleManager);
            eventManager.RegisterStatic(typeof(ExplodeOnDeathAbility));
            eventManager.RegisterStatic(typeof(LifeStealAbility));
        }

        public CustomRoleManager RoleManager { get; }

        [AutoConfiguration(ConfigurationType.Configuration)]
        public Configuration Configuration { get; set; }

        [AutoConfiguration(ConfigurationType.Translations)]
        public Translations Translations { get; set; }

        [AutoHarmony]
        public Harmony Harmony { get; set; }
    }
}