using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegBOT.Models
{
    public static class EntitySinglton
    {
        static Lazy<BotContext> Context { get; set; }

        public static Lazy<BotContext> GetContext()
        {
            if (Context == null)
            {
                Context = new Lazy<BotContext>();
            }

            return Context;
        }
    }
}
