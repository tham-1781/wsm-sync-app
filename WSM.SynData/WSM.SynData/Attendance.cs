using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSM.SynData
{
    public class Attendance
    {
        #region Properties
        public string EnrollNumber;
        public DateTime date;
        public bool pushed;
        #endregion
        #region Methods
        public Attendance(string strEnrollNumber, DateTime dtDate, bool isPushed)
        {
            EnrollNumber = strEnrollNumber;
            date = dtDate;
            pushed = isPushed;
        }
        #endregion
    }
}
