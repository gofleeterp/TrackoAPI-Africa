using System;

namespace CronExpressionDescriptor
{
    public class CronViewModel
    {
        /// <summary>
        /// day of week (0 - 6) (Sunday=0)
        /// </summary>
        public string Minute { get; set; } = "0";
        /// <summary>
        /// hour (0 - 23)
        /// </summary>
        public string Hour { get; set; } = "0";
        /// <summary>
        /// day of month (1 - 31)
        /// </summary>
        public string DayInMonth { get; set; } = "*";
        /// <summary>
        /// month (1 - 12)
        /// </summary>
        public string Month { get; set; } = "*";
        /// <summary>
        /// day of week (0 - 6) (Sunday=0)
        /// </summary>
        public string WeekDay { get; set; } = "*";
        public override string ToString()
        {
            return $"{Minute} {Hour} {DayInMonth} {Month} {WeekDay}";
        }
        public CronViewModel()
        {

        }
        public CronViewModel(string cronText)
        {
            if (string.IsNullOrWhiteSpace(cronText)|| cronText == "*") Build("*", "*", "*", "*", "*");
            else
            {
                var split = cronText.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);//5 Items
                if (split.Length < 5) throw new InvalidOperationException("Cron Expression should have atleast 5 parts");
                for (int i = 0; i < split.Length; i++)
                {
                    var expr = split[i];
                    switch (i)
                    {
                        case 0://Minute
                            Minute = expr;
                            break;
                        case 1://Hour
                            Hour = expr;
                            break;
                        case 2://Day in Month
                            DayInMonth = expr;
                            break;
                        case 3://Month
                            Month = expr;
                            break;
                        case 4://Weekday
                            WeekDay = expr;
                            break;
                    }
                }                
            }
        }
        public CronViewModel(string minute,string hour,string dayInMonth,string month,string weekDay)
        {
            Build(minute, hour, dayInMonth, month, weekDay);
        }
        private void Build(string minute, string hour, string dayInMonth, string month, string weekDay)
        {
            this.Minute = minute;
            this.Hour = hour;
            this.DayInMonth = dayInMonth;
            this.Month = month;
            this.WeekDay = weekDay;
        }
    }
}
