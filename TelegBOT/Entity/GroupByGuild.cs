using System;
using System.Collections.Generic;

#nullable disable

namespace TelegBOT.Entity
{
    public partial class GroupByGuild
    {
        public GroupByGuild()
        {
            GroupByGuildOfUsers = new HashSet<GroupByGuildOfUser>();
            HeadOfGroups = new HashSet<HeadOfGroup>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<GroupByGuildOfUser> GroupByGuildOfUsers { get; set; }
        public virtual ICollection<HeadOfGroup> HeadOfGroups { get; set; }
    }
}
