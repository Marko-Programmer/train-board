using System;

public class Train
{
    public string ScheduleId { get; set; }
    public string OrderId { get; set; }
    public string Number { get; set; }
    public string Type { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string Platform { get; set; }
    public string Track { get; set; }

    public string DepartureDisplay => DepartureTime?.ToString("HH:mm") ?? "--:--";
    public string ArrivalDisplay => ArrivalTime?.ToString("HH:mm") ?? "--:--";
    public string DurationDisplay
    {
        get
        {
            if (DepartureTime.HasValue && ArrivalTime.HasValue)
            {
                DateTime dep = DepartureTime.Value;
                DateTime arr = ArrivalTime.Value;

                TimeSpan diff = arr - dep;

                if (diff.TotalSeconds < 0)
                {
                    diff = diff.Add(TimeSpan.FromDays(1));
                }

                if (diff.TotalHours > 20)
                {
                    diff = TimeSpan.FromDays(1) - diff;
                    if (diff.TotalSeconds < 0) diff = diff.Duration();
                }

                return $"{(int)diff.TotalHours}h {diff.Minutes}m";
            }
            return "--";
        }
    }
}