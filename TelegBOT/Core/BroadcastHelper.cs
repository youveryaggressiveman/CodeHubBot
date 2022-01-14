using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegBOT.Entity;
using TelegBOT.Models;

namespace TelegBOT.Core
{
    public class BroadcastHelper
    {
        private BotContext db = EntitySinglton.GetContext().Value;

        public IQueryable<User> GetUserByDir(int dirID)
        {
            var userList = from groupByGuild in db.GroupByGuilds
                           join list in db.GroupByGuildOfUsers on groupByGuild.Id equals list.GroupByGuildId
                           join user in db.Users on list.UserId equals user.Id
                           where groupByGuild.Id == dirID
                           select user;

            return userList;
        } 
    }
}
