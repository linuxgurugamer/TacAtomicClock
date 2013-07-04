using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class MainWindow : Window<MainWindow>
    {
        private readonly Settings settings;
        private readonly SettingsWindow settingsWindow;
        private readonly HelpWindow helpWindow;

        private GUIStyle labelStyle;

        public MainWindow(Settings settings, SettingsWindow settingsWindow, HelpWindow helpWindow)
            : base("TAC Atomic Clock", 140, 150)
        {
            this.settings = settings;
            this.settingsWindow = settingsWindow;
            this.helpWindow = helpWindow;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (!newValue)
            {
                settingsWindow.SetVisible(false);
                helpWindow.SetVisible(false);
            }
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.wordWrap = false;
            }
        }

        protected override void DrawWindowContents(int windowID)
        {
            GUILayout.BeginVertical();

            double ut = Planetarium.GetUniversalTime();

            if (settings.showingUniversalTime)
            {
                GUILayout.Label("UT: " + ((long)ut).ToString("#,#"), labelStyle);
            }
            if (settings.showingEarthTime)
            {
                GUILayout.Label("ET: " + GetEarthTime(ut), labelStyle);
            }
            if (settings.showingKerbinTime)
            {
                GUILayout.Label("KT: " + GetKerbinTime(ut), labelStyle);
                if (settings.debug)
                {
                    GUILayout.Label("KT: " + GetKerbinTimeSideReel(ut), labelStyle);
                }
            }
            if (settings.showingRealTime)
            {
                GUILayout.Label("RT: " + DateTime.Now.ToLongTimeString(), labelStyle);
            }

            GUILayout.EndVertical();

            if (GUI.Button(new Rect(windowPos.width - 68, 4, 20, 20), "S", closeButtonStyle))
            {
                settingsWindow.SetVisible(true);
            }
            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), "?", closeButtonStyle))
            {
                helpWindow.SetVisible(true);
            }
        }

        private String GetEarthTime(double ut)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double MINUTES_PER_HOUR = 60.0;
            const double HOURS_PER_DAY = 24.0;
            const double DAYS_PER_MONTH = 365.25 / 12.0; // 30.4375 days
            const double MONTHS_PER_YEAR = 12.0;

            long seconds = (long)(ut);

            long minutes = (long)(seconds / SECONDS_PER_MINUTE);
            seconds -= (long)(minutes * SECONDS_PER_MINUTE);

            long hours = (long)(minutes / MINUTES_PER_HOUR);
            minutes -= (long)(hours * MINUTES_PER_HOUR);

            long days = (long)(hours / HOURS_PER_DAY);
            hours -= (long)(days * HOURS_PER_DAY);

            long months = (long)(days / DAYS_PER_MONTH);
            days -= (long)(months * DAYS_PER_MONTH);

            long years = (long)(months / MONTHS_PER_YEAR);
            months -= (long)(years * MONTHS_PER_YEAR);

            // The game starts on Year 1, Day 1
            years += 1;
            months += 1;
            days += 1;

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00");
        }

        private String GetKerbinTimeSideReel(double ut)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double MINUTES_PER_HOUR = 60.0;
            const double HOURS_PER_DAY = 6.0;
            const double DAYS_PER_MONTH = 38.6 / HOURS_PER_DAY; // 38.6 hours, 6.43 days
            const double MONTHS_PER_YEAR = 2556.50 / HOURS_PER_DAY / DAYS_PER_MONTH; // 2556.50 hours, 66.26 months

            long seconds = (long)(ut);

            long minutes = (long)(seconds / SECONDS_PER_MINUTE);
            seconds -= (long)(minutes * SECONDS_PER_MINUTE);

            long hours = (long)(minutes / MINUTES_PER_HOUR);
            minutes -= (long)(hours * MINUTES_PER_HOUR);

            long days = (long)(hours / HOURS_PER_DAY);
            hours -= (long)(days * HOURS_PER_DAY);

            long months = (long)(days / DAYS_PER_MONTH);
            days -= (long)(months * DAYS_PER_MONTH);

            long years = (long)(months / MONTHS_PER_YEAR);
            months -= (long)(years * MONTHS_PER_YEAR);

            // The game starts on Year 1, Day 1
            years += 1;
            months += 1;
            days += 1;

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00") + " (sidereel days)";
        }

        private String GetKerbinTime(double ut)
        {
            double kerbinSecondsPerEarthSecond = (settings.kerbinSecondsPerMinute * settings.kerbinMinutesPerHour * settings.kerbinHoursPerDay) / settings.earthSecondsPerKerbinDay;

            long seconds = (long)((ut + settings.initialOffsetInEarthSeconds) * kerbinSecondsPerEarthSecond);

            long minutes = (long)(seconds / settings.kerbinSecondsPerMinute);
            seconds -= (long)(minutes * settings.kerbinSecondsPerMinute);

            long hours = (long)(minutes / settings.kerbinMinutesPerHour);
            minutes -= (long)(hours * settings.kerbinMinutesPerHour);

            long days = (long)(hours / settings.kerbinHoursPerDay);
            hours -= (long)(days * settings.kerbinHoursPerDay);

            long months = (long)(days / settings.kerbinDaysPerMonth);
            days -= (long)(months * settings.kerbinDaysPerMonth);

            long years = (long)(months / settings.kerbinMonthsPerYear);
            months -= (long)(years * settings.kerbinMonthsPerYear);

            // The game starts on Year 1, Day 1
            years += 1;
            months += 1;
            days += 1;

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00");
        }
    }
}
