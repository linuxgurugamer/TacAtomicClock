/**
 * TacAtomicClockMain.cs
 * 
 * Thunder Aerospace Corporation's Atomic Clock for the Kerbal Space Program, by Taranis Elsu
 * 
 * (C) Copyright 2013, Taranis Elsu
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TacAtomicClockMain : MonoBehaviour
    {
        public static TacAtomicClockMain Instance { get; private set; }

        private string filename;
        private MainWindow mainWindow;
        private SettingsWindow settingsWindow;
        private HelpWindow helpWindow;

        private bool showingUniversalTime;
        private bool showingEarthTime;
        private bool showingKerbinTime;
        private bool showingRealTime;

        private double initialOffsetInEarthSeconds;
        private double earthSecondsPerKerbinDay;
        private double kerbinSecondsPerMinute;
        private double kerbinMinutesPerHour;
        private double kerbinHoursPerDay;
        private double kerbinDaysPerMonth;
        private double kerbinMonthsPerYear;

        private bool debug;

        public delegate void Update(bool visible);
        public event Update Observers;

        void Awake()
        {
            Debug.Log("TAC Atomic Clock (obj) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
            Instance = this;

            settingsWindow = new SettingsWindow(this);
            helpWindow = new HelpWindow();
            mainWindow = new MainWindow(this, settingsWindow, helpWindow);
            mainWindow.SetVisible(true);

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

            filename = IOUtils.GetFilePathFor(this.GetType(), "TacAtomicClock.cfg");
        }

        void FixedUpdate()
        {
            if (FlightGlobals.ready && mainWindow.IsVisible())
            {
                Vessel vessel = FlightGlobals.ActiveVessel;
                if (vessel != null)
                {
                    int numClocks = vessel.parts.Count(p => p.Modules.Contains("TacAtomicClock"));
                    if (numClocks < 1)
                    {
                        mainWindow.SetVisible(false);
                    }
                }
            }
        }

        public void Load()
        {
            try
            {
                ConfigNode config;
                if (File.Exists<TacAtomicClock>(filename))
                {
                    config = ConfigNode.Load(filename);
                    Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: loaded from file: " + config);

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

                    mainWindow.Load(config);
                    settingsWindow.Load(config);
                    helpWindow.Load(config);

                    Observers(mainWindow.IsVisible());
                }
                else
                {
                    Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: failed to load file: file does not exist");
                }
            }
            catch
            {
                Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: failed to load file: an exception was thrown.");
            }
        }

        public void Save()
        {
            try
            {
                ConfigNode config = new ConfigNode();
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

                mainWindow.Save(config);
                settingsWindow.Save(config);
                helpWindow.Save(config);

                config.Save(filename);
                Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: saved to file: " + config);
            }
            catch
            {
                Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: failed to save config file");
            }
        }

        public void SetVisible(bool newValue)
        {
            mainWindow.SetVisible(newValue);
            Observers(mainWindow.IsVisible());
        }

        public bool IsVisible()
        {
            return mainWindow.IsVisible();
        }

        private class MainWindow : Window<MainWindow>
        {
            private readonly TacAtomicClockMain settings;
            private readonly SettingsWindow settingsWindow;
            private readonly HelpWindow helpWindow;

            private GUIStyle labelStyle;

            public MainWindow(TacAtomicClockMain settings, SettingsWindow settingsWindow, HelpWindow helpWindow)
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

        private class SettingsWindow : Window<SettingsWindow>
        {
            private readonly TacAtomicClockMain settings;

            private bool showAdvanced = false;

            private GUIStyle labelStyle;
            private GUIStyle editStyle;
            private GUIStyle buttonStyle;

            public SettingsWindow(TacAtomicClockMain settings)
                : base("TAC Clock Settings", 180, 230)
            {
                this.settings = settings;
            }

            protected override void ConfigureStyles()
            {
                base.ConfigureStyles();

                if (labelStyle == null)
                {
                    labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.wordWrap = false;
                    labelStyle.fontStyle = FontStyle.Normal;
                    labelStyle.normal.textColor = Color.white;

                    editStyle = new GUIStyle(GUI.skin.textField);

                    buttonStyle = new GUIStyle(GUI.skin.button);
                }
            }

            protected override void DrawWindowContents(int windowID)
            {
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show Universal Time", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                settings.showingUniversalTime = GUILayout.Toggle(settings.showingUniversalTime, "");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show Earth Time", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                settings.showingEarthTime = GUILayout.Toggle(settings.showingEarthTime, "");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show Kerbin Time", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                settings.showingKerbinTime = GUILayout.Toggle(settings.showingKerbinTime, "");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show Real Time", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                settings.showingRealTime = GUILayout.Toggle(settings.showingRealTime, "");
                GUILayout.EndHorizontal();

                GUILayout.Space(20);

                if (!showAdvanced)
                {
                    if (GUILayout.Button("Advanced", buttonStyle, GUILayout.ExpandWidth(true)))
                    {
                        showAdvanced = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Simple", buttonStyle, GUILayout.ExpandWidth(true)))
                    {
                        showAdvanced = false;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Initial offset in Earth seconds", labelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    settings.initialOffsetInEarthSeconds = Utilities.ShowTextField(settings.initialOffsetInEarthSeconds, 10, editStyle, GUILayout.MinWidth(50));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Earth seconds per Kerbin day", labelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    settings.earthSecondsPerKerbinDay = Utilities.ShowTextField(settings.earthSecondsPerKerbinDay, 10, editStyle, GUILayout.MinWidth(50));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Kerbin seconds per minute", labelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    settings.kerbinSecondsPerMinute = Utilities.ShowTextField(settings.kerbinSecondsPerMinute, 10, editStyle, GUILayout.MinWidth(50));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Kerbin minutes per hour", labelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    settings.kerbinMinutesPerHour = Utilities.ShowTextField(settings.kerbinMinutesPerHour, 10, editStyle, GUILayout.MinWidth(50));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Kerbin hours per day", labelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    settings.kerbinHoursPerDay = Utilities.ShowTextField(settings.kerbinHoursPerDay, 10, editStyle, GUILayout.MinWidth(50));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Kerbin days per month", labelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    settings.kerbinDaysPerMonth = Utilities.ShowTextField(settings.kerbinDaysPerMonth, 10, editStyle, GUILayout.MinWidth(50));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Kerbin months per year", labelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    settings.kerbinMonthsPerYear = Utilities.ShowTextField(settings.kerbinMonthsPerYear, 10, editStyle, GUILayout.MinWidth(50));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);
                    settings.debug = GUILayout.Toggle(settings.debug, "Debug");
                }

                GUILayout.EndVertical();

                GUILayout.Space(8);
            }
        }

        class HelpWindow : Window<HelpWindow>
        {
            private GUIStyle labelStyle;
            private GUIStyle sectionStyle;
            private Vector2 scrollPosition;

            public HelpWindow()
                : base("TAC Clock Help", 420, 360)
            {
                scrollPosition = Vector2.zero;
            }

            protected override void ConfigureStyles()
            {
                base.ConfigureStyles();

                if (labelStyle == null)
                {
                    labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.wordWrap = true;
                    labelStyle.fontStyle = FontStyle.Normal;
                    labelStyle.normal.textColor = Color.white;
                    labelStyle.stretchWidth = true;
                    labelStyle.stretchHeight = false;
                    labelStyle.margin.bottom -= 2;
                    labelStyle.padding.bottom -= 2;

                    sectionStyle = new GUIStyle(labelStyle);
                    sectionStyle.fontStyle = FontStyle.Bold;
                }
            }

            protected override void DrawWindowContents(int windowID)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.BeginVertical();

                GUILayout.Label("Atomic Clock by Taranis Elsu of Thunder Aerospace Corporation.", labelStyle);
                GUILayout.Label("Copyright (c) Thunder Aerospace Corporation. Patents pending.", labelStyle);
                GUILayout.Space(20);
                GUILayout.Label("Definitions", sectionStyle);
                GUILayout.Label("* Universal Time -- time in Earth seconds since the game began.", labelStyle);
                GUILayout.Label("* Earth Time -- elapsed time in Earth years:months:days hours:minutes:seconds since the game began.", labelStyle);
                GUILayout.Label("* Kerbin Time -- elapsed time in Kerbin years:months:days hours:minutes:seconds since the game began.", labelStyle);
                GUILayout.Label("* Real Time -- current clock time from your computer.", labelStyle);

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                GUILayout.Space(8);
            }
        }
    }
}
