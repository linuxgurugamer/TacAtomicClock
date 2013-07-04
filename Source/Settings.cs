using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tac
{
    class Settings
    {
        public bool showingUniversalTime { get; set; }
        public bool showingEarthTime { get; set; }
        public bool showingKerbinTime { get; set; }
        public bool showingRealTime { get; set; }

        public double initialOffsetInEarthSeconds { get; set; }
        public double earthSecondsPerKerbinDay { get; set; }
        public double kerbinSecondsPerMinute { get; set; }
        public double kerbinMinutesPerHour { get; set; }
        public double kerbinHoursPerDay { get; set; }
        public double kerbinDaysPerMonth { get; set; }
        public double kerbinMonthsPerYear { get; set; }

        public bool debug { get; set; }

        public Settings()
        {
            showingUniversalTime = true;
            showingEarthTime = true;
            showingKerbinTime = true;
            showingRealTime = true;

            initialOffsetInEarthSeconds = 0.0;

            // TODO Got this from the wiki, is it correct?
            earthSecondsPerKerbinDay = 6 * 3600.0 + 50.0;

            kerbinSecondsPerMinute = 24.0;
            kerbinMinutesPerHour = 24.0;
            kerbinHoursPerDay = 12.0;
            kerbinDaysPerMonth = 6.418476;
            kerbinMonthsPerYear = 66.23057;

            debug = false;
        }

        public void Load(ConfigNode config)
        {
            debug = Utilities.GetValue(config, "debug", debug);

            showingUniversalTime = Utilities.GetValue(config, "showingUniversalTime", showingUniversalTime);
            showingEarthTime = Utilities.GetValue(config, "showingEarthTime", showingEarthTime);
            showingKerbinTime = Utilities.GetValue(config, "showingKerbinTime", showingKerbinTime);
            showingRealTime = Utilities.GetValue(config, "showingRealTime", showingRealTime);

            initialOffsetInEarthSeconds = Utilities.GetValue(config, "initialOffsetInEarthSeconds", initialOffsetInEarthSeconds);
            earthSecondsPerKerbinDay = Utilities.GetValue(config, "earthSecondsPerKerbinDay", earthSecondsPerKerbinDay);
            kerbinSecondsPerMinute = Utilities.GetValue(config, "kerbinSecondsPerMinute", kerbinSecondsPerMinute);
            kerbinMinutesPerHour = Utilities.GetValue(config, "kerbinMinutesPerHour", kerbinMinutesPerHour);
            kerbinHoursPerDay = Utilities.GetValue(config, "kerbinHoursPerDay", kerbinHoursPerDay);
            kerbinDaysPerMonth = Utilities.GetValue(config, "kerbinDaysPerMonth", kerbinDaysPerMonth);
            kerbinMonthsPerYear = Utilities.GetValue(config, "kerbinMonthsPerYear", kerbinMonthsPerYear);
        }

        public void Save(ConfigNode config)
        {
            config.AddValue("debug", debug);

            config.AddValue("showingUniversalTime", showingUniversalTime);
            config.AddValue("showingEarthTime", showingEarthTime);
            config.AddValue("showingKerbinTime", showingKerbinTime);
            config.AddValue("showingRealTime", showingRealTime);

            config.AddValue("initialOffsetInEarthSeconds", initialOffsetInEarthSeconds);
            config.AddValue("earthSecondsPerKerbinDay", earthSecondsPerKerbinDay);
            config.AddValue("kerbinSecondsPerMinute", kerbinSecondsPerMinute);
            config.AddValue("kerbinMinutesPerHour", kerbinMinutesPerHour);
            config.AddValue("kerbinHoursPerDay", kerbinHoursPerDay);
            config.AddValue("kerbinDaysPerMonth", kerbinDaysPerMonth);
            config.AddValue("kerbinMonthsPerYear", kerbinMonthsPerYear);
        }
    }
}
