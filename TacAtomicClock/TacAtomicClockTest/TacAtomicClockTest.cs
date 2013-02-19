using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class TacAtomicClockTest
{
    private DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    private void testFormats()
    {
        Console.WriteLine("Hello!");

        String formatStr = "#,0.0##########################";

        DateTime now = System.DateTime.UtcNow;
        Console.WriteLine("Now = " + now.ToString("yyyy:MM:dd HH:mm:ss.F"));
        Console.WriteLine("UNIX Epoch = " + epoch.ToString("yyyy:MM:dd HH:mm:ss.F"));

        TimeSpan temp = now - epoch;
        double time = temp.TotalSeconds;
        Console.WriteLine("Seconds since epoch: " + time.ToString(formatStr));
        Console.WriteLine("Seconds since epoch: " + getEarthTime(time));

        TimeSpan temp2 = epoch - new DateTime();
        double epochSeconds = temp2.TotalSeconds;
        Console.WriteLine(getEarthTime(epochSeconds));
        Console.WriteLine();

        double time2 = 123456789.89;
        Console.WriteLine(time2.ToString(formatStr));
        Console.WriteLine(getEarthTime(time2));
        Console.WriteLine();

        double time3 = 0.5;
        Console.WriteLine(time3.ToString(formatStr));
        Console.WriteLine(getEarthTime(time3));
        Console.WriteLine();
    }

    private String getEarthTime(double ut)
    {
        const double SECONDS_PER_MINUTE = 60.0;
        const double SECONDS_PER_HOUR = SECONDS_PER_MINUTE * 60.0; // 3,600
        const double SECONDS_PER_DAY = SECONDS_PER_HOUR * 24.0; // 86,400
        const double SECONDS_PER_YEAR = SECONDS_PER_DAY * 365.0; // 31,536,000
        const double SECONDS_PER_MONTH = SECONDS_PER_YEAR / 12.0; // 2,628,000

        /*
        double seconds = ut;
        int minutes = (int)(seconds / 60.0);
        int hours = (int)(minutes / 60.0);
        int days = (int)(hours / 24.0);
        int months = (int)(days / 365.0 * 12.0);
        int years = (int)(days / 365.0);

        seconds = seconds - (minutes * 60);
        minutes = minutes - (hours * 60);
        hours = hours - (days * 24);
        days = days - (years * 365) - (years / 4);
        months = months - (years * 12);
        years = years + 1970;
         */

        
        double seconds = ut % SECONDS_PER_MINUTE;
        int minutes = (int)(ut % SECONDS_PER_HOUR / SECONDS_PER_MINUTE);
        int hours = (int)(ut % SECONDS_PER_DAY / SECONDS_PER_HOUR);
        int days = (int)(ut % SECONDS_PER_MONTH / SECONDS_PER_DAY) + 1;
        int months = (int)(ut % SECONDS_PER_YEAR / SECONDS_PER_MONTH) + 1;
        int years = (int)(ut / SECONDS_PER_YEAR) + 1970;

        // days -= ((years - 1970) / 4); // adjust for leap years
         

        return years.ToString("#######00") + "y "
            + months.ToString("#######00") + "m "
            + days.ToString("#######00") + "d "
            + hours.ToString("#######00") + "h "
            + minutes.ToString("#######00") + "m "
            + seconds.ToString("#######00.0#######") + "s";
    }

    private void test2()
    {
        for (int i = 0; i < 120; ++i)
        {
            DateTime now = System.DateTime.UtcNow;
            double time = (now - epoch).TotalSeconds;

            Console.WriteLine("Time is now: " + getEarthTime(time));

            Thread.Sleep(1000);
        }
    }

    static void Main()
    {
        TacAtomicClockTest test = new TacAtomicClockTest();
        test.testFormats();
        test.test2();

        // Wait to exit
        Console.ReadKey();
    }
}
