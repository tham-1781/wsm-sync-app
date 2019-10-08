using System;

namespace WSM.SynData.Models
{
    public class Attendance
    {
        #region Properties
        public string EnrollNumber { get; set; }
        public DateTime date { get; set; }
        public bool pushed { get; set; }
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
