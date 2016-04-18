using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm_Tec_Service.Models
{
    public class Alarm
    {
        /// <summary>
        /// Item's Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The User Id of the Item's owner; Who does this message belong to
        /// </summary>
        public int UserId { get; set; }

        public string Name { get; set; }

        public string CreateDate { get; set; }

        public string WakeUpTime { get; set; }

        public bool WantsNews { get; set; }

        public bool WantsWeather { get; set; }

        public bool IsEnabled { get; set; }
    }
}
