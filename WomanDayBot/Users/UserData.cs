using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WomanDayBot.Orders;

namespace WomanDayBot.Users
{
    /// <summary>
    /// Class for storing persistent user data.
    /// </summary>
    public class UserData
    {
        public string Name { get; set; }
        public string Room { get; set; }
        public OrderCategory OrderCategory { get; set; }
    }
}
