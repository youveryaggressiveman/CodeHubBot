using System;
using System.Collections.Generic;

#nullable disable

namespace TelegBOT.Models
{
    public partial class HeadOfGroup
    {
        public int HeadId { get; set; }
        public int GroupId { get; set; }

        public virtual GroupByGuild Group { get; set; }
        public virtual User Head { get; set; }
    }
}
