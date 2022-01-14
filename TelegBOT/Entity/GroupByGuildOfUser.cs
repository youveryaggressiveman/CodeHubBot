using System;
using System.Collections.Generic;

#nullable disable

namespace TelegBOT.Entity
{
    public partial class GroupByGuildOfUser
    {
        public int UserId { get; set; }
        public int GroupByGuildId { get; set; }

        public virtual GroupByGuild GroupByGuild { get; set; }
        public virtual User User { get; set; }
    }
}
