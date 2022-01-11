using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TelegBOT.Models;

namespace TelegBOT.Core
{
    public class RegHelper
    {
        private BotContext db;

        public RegHelper()
        {
            db = EntitySinglton.GetContext().Value;
        }

        public User User { get; set; }
        public GroupByCollege GroupByCollege { get; set; }

        public bool Registration(string response, long chatID)
        {
            var check = Regex.IsMatch(response, "(^[A-Z]{1}[a-z]{1,14} [A-Z]{1}[a-z]{1,14} [A-Z]{1}[a-z]{1,14}$)|(^[А-Я]{1}[а-я]{1,14} [А-Я]{1}[а-я]{1,14} [А-Я]{1}[а-я]{1,14}$)");

            if (check == false)
            {
                return false;
            }

            var result = response.Split(" ");

            User = new User
            {
                SecondName = result[0],
                FirstName = result[1],
                LastName = result[2],
                TelegramId = chatID.ToString(),
                UserStatusId = 1,
                RoleId = 1,
            };

            return true;
        }

        public bool SetCource(string response)
        {

            switch (response)
            {
                case "Мобильная разработка":
                    var course1 = new List<GroupByGuildOfUser>()
                    {
                        new GroupByGuildOfUser
                        {
                            GroupByGuildId = 1,
                        }
                    };
                    User.GroupByGuildOfUsers = course1;
                    return true;
                case "Разработка desktop-приложений":
                    var course2 = new List<GroupByGuildOfUser>()
                    {
                        new GroupByGuildOfUser
                        {
                            GroupByGuildId = 2,
                        }
                    };
                    User.GroupByGuildOfUsers = course2;
                    return true;
                case "Разработка комплексных ИС":
                    var course3 = new List<GroupByGuildOfUser>()
                    {
                        new GroupByGuildOfUser
                        {
                            GroupByGuildId = 3,
                        }
                    };
                    User.GroupByGuildOfUsers = course3;
                    return true;

                default:
                    return false;
            }

        }

        public async Task<bool> SetGroup(string response)
        {
            var result = Regex.IsMatch(response.ToUpper(), "^(?!.*0$)([А-Я]{2}[-А-Я]{1}[-0-9]{1}[0-9]{1,2})$");

            if (result == false)
            {
                return false;
            }

            var group = db.GroupByColleges.FirstOrDefault(group => group.Name == response.ToUpper());

            if (group != null)
            {
                User.GroupByCollege = group;
                return true;
            }

            var groupByCollege = new GroupByCollege
            {
                Name = response.ToUpper(),

            };

            var createdGroup = db.GroupByColleges.Add(groupByCollege);

            try
            {
                await db.SaveChangesAsync();
                User.GroupByCollegeId = db.GroupByColleges.FirstOrDefault(group => group.Name == groupByCollege.Name).Id;
                User.GroupByCollege = db.GroupByColleges.FirstOrDefault(group => group.Name == groupByCollege.Name);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public async Task SaveUser()
        {
            db.Users.Add(User);

            await db.SaveChangesAsync();
        }
    }
}
