using System.Collections.Generic;
using MoreItems;

namespace MoreRoles
{
    public class Configuration
    {
        public Dictionary<string, CustomRole> Roles { get; set; } = new Dictionary<string, CustomRole>
        {
            ["test"] = new CustomRole
            {
                Name = "Test",
                StartItems = new BaseItemType[]
                {
                    (VanillaItemType) ItemType.Coin
                },
                StartAmmo = new uint[] { 1, 2, 3 },
                Abilities =
                {
                    new ExplodeOnDeathAbility(),
                    new BypassKeycardReadersAbility(),
                    new LifeStealAbility()
                }
            }
        };

        public List<Pool> Pools { get; set; } = new List<Pool>()
        {
            new Pool
            {
                Conditions =
                {
                    new RoleCondition { Role = RoleType.ClassD },
                    new RoleCondition { Role = RoleType.NtfCadet },
                    new RoleCondition { Role = RoleType.NtfLieutenant },
                    new RoleCondition { Role = RoleType.NtfCommander }
                },

                Roles = new List<WeightedRole>
                {
                    new WeightedRole { Role = "test", Weight = 50 },
                    new WeightedRole { Role = null, Weight = 50 }
                }
            }
        };
    }

    public class Pool
    {
        public List<WeightedRole> Roles { get; set; } = new List<WeightedRole>();
        public List<ICondition> Conditions { get; set; } = new List<ICondition>();
        public ConditionsType ConditionsType { get; set; } = ConditionsType.Any;
    }

    public class WeightedRole
    {
        public string Role { get; set; }
        public double Weight { get; set; }
    }

    public enum ConditionsType
    {
        All,
        Any
    }

    public interface ICondition
    {
        public bool Test(ReferenceHub player, RoleType newRoleType);
    }

    public class RoleCondition : ICondition
    {
        public RoleType Role { get; set; }

        public bool Test(ReferenceHub player, RoleType newRoleType)
        {
            return newRoleType == Role;
        }
    }
}