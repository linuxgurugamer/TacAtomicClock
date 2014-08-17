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
        private GUIStyle valueStyle;

        public MainWindow(Settings settings, SettingsWindow settingsWindow, HelpWindow helpWindow)
            : base("TAC Atomic Clock", 140, 150)
        {
            this.settings = settings;
            this.settingsWindow = settingsWindow;
            this.helpWindow = helpWindow;

            base.HideCloseButton = true;
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
                labelStyle.margin.top = 0;
                labelStyle.margin.bottom = 0;
                labelStyle.padding.top = 0;
                labelStyle.padding.bottom = 1;
                labelStyle.wordWrap = false;

                valueStyle = new GUIStyle(labelStyle);
                valueStyle.alignment = TextAnchor.MiddleRight;
                valueStyle.stretchWidth = true;
            }
        }

        protected override void DrawWindowContents(int windowID)
        {
            double ut = Planetarium.GetUniversalTime();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            if (settings.showingUniversalTime)
            {
                GUILayout.Label("UT", labelStyle);
            }
            if (settings.showingEarthTime)
            {
                GUILayout.Label("ET", labelStyle);
            }
            if (settings.showingKerbinTime)
            {
                GUILayout.Label("KT", labelStyle);
                if (settings.debug)
                {
                    GUILayout.Label("KT(ref)", labelStyle);
                }
            }
            if (settings.showingKerbinMissionTime)
            {
                GUILayout.Label("KMET", labelStyle);
                if (settings.debug)
                {
                    GUILayout.Label("KMET(ref)", labelStyle);
                }
            }
            if (settings.showingRealTime)
            {
                GUILayout.Label("RT", labelStyle);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (settings.showingUniversalTime)
            {
                GUILayout.Label(((long)ut).ToString("#,#"), valueStyle);
            }
            if (settings.showingEarthTime)
            {
                GUILayout.Label(GetEarthTime(ut), valueStyle);
            }
            if (settings.showingKerbinTime)
            {
                GUILayout.Label(GetKerbinTime(ut), valueStyle);
                if (settings.debug)
                {
                    GUILayout.Label(GetKerbinTimeReference(ut), valueStyle);
                }
            }
            if (settings.showingKerbinMissionTime)
            {
                if (FlightGlobals.ready && FlightGlobals.ActiveVessel != null)
                {
                    GUILayout.Label(GetKerbinElapsedTime(FlightGlobals.ActiveVessel.missionTime), valueStyle);
                    if (settings.debug)
                    {
                        GUILayout.Label(GetKerbinElapsedTimeReference(FlightGlobals.ActiveVessel.missionTime), valueStyle);
                    }
                }
                else
                {
                    GUILayout.Label("-", valueStyle);
                    if (settings.debug)
                    {
                        GUILayout.Label("-", valueStyle);
                    }
                }
            }
            if (settings.showingRealTime)
            {
                GUILayout.Label(DateTime.Now.ToLongTimeString(), valueStyle);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), "S", closeButtonStyle))
            {
                settingsWindow.SetVisible(true);
            }
            if (GUI.Button(new Rect(windowPos.width - 24, 4, 20, 20), "?", closeButtonStyle))
            {
                helpWindow.SetVisible(true);
            }
        }

        private static string GetTime(double time, double secondsPerMinute, double minutesPerHour,
            double hoursPerDay, double monthsPerYear, double daysPerYear, double daysPerMonth,
            bool showMonth)
        {
            long seconds = (long)(time);

            long minutes = (long)(seconds / secondsPerMinute);
            seconds -= (long)(minutes * secondsPerMinute);

            long hours = (long)(minutes / minutesPerHour);
            minutes -= (long)(hours * minutesPerHour);

            long days = (long)(hours / hoursPerDay);
            hours -= (long)(days * hoursPerDay);

            long months = 0;
            long years = 0;
            if (showMonth)
            {
                months = (long)(days / daysPerMonth);
                days -= (long)(months * daysPerMonth);

                years = (long)(months / monthsPerYear);
                months -= (long)(years * monthsPerYear);
            }
            else
            {
                years = (long)(days / daysPerYear);
                days -= (long)(years * daysPerYear);
            }

            // The game starts on Year 1, Day 1
            years += 1;
            months += 1;
            days += 1;

            if (showMonth)
            {
                return "Y" + years.ToString("00")
                    + " M" + months.ToString("00")
                    + " D" + days.ToString("00") + " "
                    + hours.ToString("00") + ":"
                    + minutes.ToString("00") + ":"
                    + seconds.ToString("00");
            }
            else
            {
                return "Y" + years.ToString("00")
                    + " D" + days.ToString("00") + " "
                    + hours.ToString("00") + ":"
                    + minutes.ToString("00") + ":"
                    + seconds.ToString("00");
            }
        }

        private static string GetElapsedTime(double time, double secondsPerMinute, double minutesPerHour,
            double hoursPerDay, double monthsPerYear, double daysPerYear, double daysPerMonth,
            bool showMonth)
        {
            long seconds = (long)(time);

            long minutes = (long)(seconds / secondsPerMinute);
            seconds -= (long)(minutes * secondsPerMinute);

            long hours = (long)(minutes / minutesPerHour);
            minutes -= (long)(hours * minutesPerHour);

            long days = (long)(hours / hoursPerDay);
            hours -= (long)(days * hoursPerDay);

            long months = 0;
            long years = 0;
            if (showMonth)
            {
                months = (long)(days / daysPerMonth);
                days -= (long)(months * daysPerMonth);

                years = (long)(months / monthsPerYear);
                months -= (long)(years * monthsPerYear);
            }
            else
            {
                years = (long)(days / daysPerYear);
                days -= (long)(years * daysPerYear);
            }

            StringBuilder result = new StringBuilder();
            if (years > 0)
            {
                result.Append(years.ToString("#0")).Append("y ");
            }

            if (showMonth && (months > 0 || result.Length > 0))
            {
                result.Append(months.ToString("00")).Append("m ");
            }

            if (days > 0 || result.Length > 0)
            {
                result.Append(days.ToString("00")).Append("d ");
            }

            result.Append(hours.ToString("00")).Append(":");
            result.Append(minutes.ToString("00")).Append(":");
            result.Append(seconds.ToString("00"));

            return result.ToString();
        }

        private string GetEarthTime(double ut)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double MINUTES_PER_HOUR = 60.0;
            const double HOURS_PER_DAY = 24.0;
            const double MONTHS_PER_YEAR = 12.0;
            const double DAYS_PER_YEAR = 365.25;
            const double DAYS_PER_MONTH = DAYS_PER_YEAR / MONTHS_PER_YEAR; // 30.4375 days

            return GetTime(ut, SECONDS_PER_MINUTE, MINUTES_PER_HOUR, HOURS_PER_DAY, MONTHS_PER_YEAR,
                DAYS_PER_YEAR, DAYS_PER_MONTH, settings.showMonth);
        }

        private string GetKerbinTimeReference(double ut)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double MINUTES_PER_HOUR = 60.0;
            const double HOURS_PER_DAY = 6.0;
            const double SECONDS_PER_DAY = SECONDS_PER_MINUTE * MINUTES_PER_HOUR * HOURS_PER_DAY;
            double DAYS_PER_YEAR = FlightGlobals.Bodies[1].orbit.period / SECONDS_PER_DAY; // ~426.090

            return GetTime(ut, SECONDS_PER_MINUTE, MINUTES_PER_HOUR, HOURS_PER_DAY, 0, DAYS_PER_YEAR, 0, false);
        }

        private string GetKerbinTime(double ut)
        {
            double kerbinSecondsPerEarthSecond = (settings.kerbinSecondsPerMinute * settings.kerbinMinutesPerHour * settings.kerbinHoursPerDay) / settings.earthSecondsPerKerbinDay;
            double scaledUt = (ut + settings.initialOffsetInEarthSeconds) * kerbinSecondsPerEarthSecond;

            return GetTime(scaledUt, settings.kerbinSecondsPerMinute, settings.kerbinMinutesPerHour,
                settings.kerbinHoursPerDay, settings.kerbinMonthsPerYear, settings.kerbinDaysPerYear,
                settings.kerbinDaysPerMonth, settings.showMonth);
        }

        private string GetKerbinElapsedTime(double time)
        {
            double kerbinSecondsPerEarthSecond = (settings.kerbinSecondsPerMinute * settings.kerbinMinutesPerHour * settings.kerbinHoursPerDay) / settings.earthSecondsPerKerbinDay;
            double scaledTime = time * kerbinSecondsPerEarthSecond;

            return GetElapsedTime(scaledTime, settings.kerbinSecondsPerMinute, settings.kerbinMinutesPerHour,
                settings.kerbinHoursPerDay, settings.kerbinMonthsPerYear, settings.kerbinDaysPerYear,
                settings.kerbinDaysPerMonth, settings.showMonth);
        }

        private string GetKerbinElapsedTimeReference(double value)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double MINUTES_PER_HOUR = 60.0;
            const double HOURS_PER_DAY = 6.0;
            const double SECONDS_PER_DAY = SECONDS_PER_MINUTE * MINUTES_PER_HOUR * HOURS_PER_DAY;
            double DAYS_PER_YEAR = FlightGlobals.Bodies[1].orbit.period / SECONDS_PER_DAY; // ~426.090

            return GetElapsedTime(value, SECONDS_PER_MINUTE, MINUTES_PER_HOUR, HOURS_PER_DAY, 0, DAYS_PER_YEAR, 0, false);
        }
    }
}
