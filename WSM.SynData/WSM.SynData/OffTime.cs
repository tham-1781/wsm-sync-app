using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSM.SynData
{
    public class OffTime
    {
        #region Properties
        public TimeSpan tsBegin;
        public TimeSpan tsEnd;
        #endregion
        #region Methods
        public OffTime(TimeSpan begin, TimeSpan end)
        {
            tsBegin = begin;
            tsEnd = end;
        }
        public bool CheckTime(TimeSpan now)
        {
            if (now >= tsBegin && now <= tsEnd)
                return true;
            else
                return false;
        }
        #endregion
    }
}
