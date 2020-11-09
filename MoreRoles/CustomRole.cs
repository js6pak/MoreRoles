using System.Collections.Generic;
using MoreItems;

namespace MoreRoles
{
    public class CustomRole
    {
        public string Name { get; set; }
        public int? Health { get; set; }
        public bool ClearStartItems { get; set; } = true;
        public BaseItemType[] StartItems { get; set; }
        public uint[] StartAmmo { get; set; }
        public List<IAbility> Abilities { get; set; } = new List<IAbility>();

        public override string ToString()
        {
            return base.ToString() + $"{{{Name}}}";
        }
    }
}