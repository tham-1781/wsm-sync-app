using System;

namespace WSM.SynData.Models
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
