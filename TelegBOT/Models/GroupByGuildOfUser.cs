using System;
using System.Collections.Generic;

#nullable disable

namespace TelegBOT.Models
{
    public partial class GroupByGuildOfUser
    {
        public int UserId { get; set; }
        public int GroupByGuildId { get; set; }

        public virtual GroupByGuild GroupByGuild { get; set; }
        public virtual User User { get; set; }
    }
}
