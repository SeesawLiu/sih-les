using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Utility
{
    public static class DatePeriodRepititionHelper
    {
        #region 比较两个时间区间是否有交集，true表示重复，false表示不重复
        public static bool CheckDatePeriodRepitition(DateTime? dateTimeFrom1,DateTime? dateTimeTo1,DateTime? dateTimeFrom2,DateTime? dateTimeTo2)
        {
            if(!dateTimeFrom1.HasValue && !dateTimeTo1.HasValue)
               return true;
            else if (!dateTimeFrom1.HasValue && dateTimeTo1.HasValue)
            {
                if (!dateTimeFrom2.HasValue)
                    return true;
                else
                {
                    if (dateTimeFrom2 < dateTimeTo1)
                        return true;
                    else
                        return false;
                }
            }
            else if (dateTimeFrom1.HasValue && !dateTimeTo1.HasValue)
            {
                if (!dateTimeTo2.HasValue)
                    return true;
                else
                {
                    if (dateTimeTo2 < dateTimeFrom1)
                        return false;
                    else
                        return true;
                }
            }
            else
            {
                if (!dateTimeFrom2.HasValue && !dateTimeTo2.HasValue)
                {
                    return true;
                }
                else
                {
                    if (dateTimeFrom2 > dateTimeTo1 || dateTimeTo2 < dateTimeFrom1)
                        return false;
                    else
                        return true;
                }

            }
        }
        #endregion
    }
}
