using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegBOT.Models;

namespace TelegBOT.Core
{
    public class UserHelper
    {
        private BotContext db = EntitySinglton.GetContext().Value;

        public async Task<User> DBUSer(long chatID)
        {
            var user = await db.Users.Where(user => user.TelegramId == chatID.ToString()).FirstOrDefaultAsync();

            return user;
        }

        public async Task<User> RefreshUser(long chatID, string path)
        {
            var user = await db.Users.Where(user => user.TelegramId == chatID.ToString()).FirstOrDefaultAsync();

            var result = path.Split(" ");

            user.FirstName = result[1];
            user.SecondName = result[0];
            user.LastName = result[2];

            db.Update(user);

            db.SaveChanges();

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var userList = await db.Users.ToListAsync();

            return userList;
        } 
    }
}
