using System;
using System.Linq;

namespace TaskManager.Models.Enums
{
    public static class EnumHelper
    {
        public static Array GetStatusTypeValues()
        {
            return Enum.GetValues(typeof(StatusType));
        }

        public static Array GetPriorityTypeValues()
        {
            return Enum.GetValues(typeof(PriorityType));
        }
    }
} 