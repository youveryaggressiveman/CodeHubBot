using System;
using System.Collections.Generic;

#nullable disable

namespace TelegBOT.Models
{
    public partial class User
    {
        public User()
        {
            GroupByGuildOfUsers = new HashSet<GroupByGuildOfUser>();
            HeadOfGroups = new HashSet<HeadOfGroup>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public string TelegramId { get; set; }
        public int RoleId { get; set; }
        public int GroupByCollegeId { get; set; }

        public virtual GroupByCollege GroupByCollege { get; set; }
        public virtual Role Role { get; set; }
        public virtual ICollection<GroupByGuildOfUser> GroupByGuildOfUsers { get; set; }
        public virtual ICollection<HeadOfGroup> HeadOfGroups { get; set; }
    }
}
