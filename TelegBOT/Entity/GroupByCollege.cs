﻿using System;
using System.Collections.Generic;

#nullable disable

namespace TelegBOT.Entity
{
    public partial class GroupByCollege
    {
        public GroupByCollege()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
