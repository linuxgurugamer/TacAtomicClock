/**
 * TacAtomicClock.cs
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
using System.Threading.Tasks;
using UnityEngine;

public class TacAtomicClock : PartModule
{
    private string filename;
    private MainWindow mainWindow;
    private ConfigWindow configWindow;
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

    private GUIStyle buttonStyle;
    private RectOffset buttonPadding;
    private RectOffset buttonMargin;

    public override void OnAwake()
    {
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnAwake");
        mainWindow = new MainWindow(this);
        configWindow = new ConfigWindow(this);
        helpWindow = new HelpWindow(this);

        showingUniversalTime = true;
        showingEarthTime = true;
        showingKerbinTime = true;
        showingRealTime = true;

        initialOffsetInEarthSeconds = 0.0;

        // TODO Got this from the wiki, is it correct?
        earthSecondsPerKerbinDay = 6 * 3600.0 + 50.0;

        kerbinSecondsPerMinute = 36.0;
        kerbinMinutesPerHour = 36.0;
        kerbinHoursPerDay = 12.0;
        kerbinDaysPerMonth = 6.418476;
        kerbinMonthsPerYear = 66.23057;

        buttonPadding = new RectOffset(5, 5, 3, 0);
        buttonMargin = new RectOffset(1, 1, 1, 1);

        debug = false;

        filename = IOUtils.GetFilePathFor(this.GetType(), "TacAtomicClock.cfg");
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: filename = " + filename);
    }

    public override void OnStart(PartModule.StartState state)
    {
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnStart " + state);
        base.OnStart(state);

        if (state != StartState.Editor)
        {
            vessel.OnJustAboutToBeDestroyed += CleanUp;
            part.OnJustAboutToBeDestroyed += CleanUp;

            //mainWindow.SetVisible(true);
        }
    }

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);

        try
        {
            ConfigNode config;
            if (File.Exists<TacAtomicClock>(filename))
            {
                config = ConfigNode.Load(filename);
                Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: loaded from file: " + node);
            }
            else
            {
                Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: failed to load file: file does not exist");
                return;
            }

            getValue(config, "debug", ref debug);

            getValue(config, "showingUniversalTime", ref showingUniversalTime);
            getValue(config, "showingEarthTime", ref showingEarthTime);
            getValue(config, "showingKerbinTime", ref showingKerbinTime);
            getValue(config, "showingRealTime", ref showingRealTime);

            getValue(config, "initialOffsetInEarthSeconds", ref initialOffsetInEarthSeconds);
            getValue(config, "earthSecondsPerKerbinDay", ref earthSecondsPerKerbinDay);
            getValue(config, "kerbinSecondsPerMinute", ref kerbinSecondsPerMinute);
            getValue(config, "kerbinMinutesPerHour", ref kerbinMinutesPerHour);
            getValue(config, "kerbinHoursPerDay", ref kerbinHoursPerDay);
            getValue(config, "kerbinDaysPerMonth", ref kerbinDaysPerMonth);
            getValue(config, "kerbinMonthsPerYear", ref kerbinMonthsPerYear);

            mainWindow.Load(config, "mainWindow");
            configWindow.Load(config, "configWindow");
            helpWindow.Load(config, "helpWindow");

            int newValue;
            if (config.HasValue("buttonStyle.padding.left") && int.TryParse(config.GetValue("buttonStyle.padding.left"), out newValue))
            {
                buttonPadding.left = newValue;
            }
            if (config.HasValue("buttonStyle.padding.right") && int.TryParse(config.GetValue("buttonStyle.padding.right"), out newValue))
            {
                buttonPadding.right = newValue;
            }
            if (config.HasValue("buttonStyle.padding.top") && int.TryParse(config.GetValue("buttonStyle.padding.top"), out newValue))
            {
                buttonPadding.top = newValue;
            }
            if (config.HasValue("buttonStyle.padding.bottom") && int.TryParse(config.GetValue("buttonStyle.padding.bottom"), out newValue))
            {
                buttonPadding.bottom = newValue;
            }

            if (config.HasValue("buttonStyle.margin.left") && int.TryParse(config.GetValue("buttonStyle.margin.left"), out newValue))
            {
                buttonMargin.left = newValue;
            }
            if (config.HasValue("buttonStyle.margin.right") && int.TryParse(config.GetValue("buttonStyle.margin.right"), out newValue))
            {
                buttonMargin.right = newValue;
            }
            if (config.HasValue("buttonStyle.margin.top") && int.TryParse(config.GetValue("buttonStyle.margin.top"), out newValue))
            {
                buttonMargin.top = newValue;
            }
            if (config.HasValue("buttonStyle.margin.bottom") && int.TryParse(config.GetValue("buttonStyle.margin.bottom"), out newValue))
            {
                buttonMargin.bottom = newValue;
            }
        }
        catch
        {
            Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: failed to load file: an exception was thrown.");
        }
    }

    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);
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

            mainWindow.Save(config, "mainWindow");
            configWindow.Save(config, "configWindow");
            helpWindow.Save(config, "helpWindow");

            config.AddValue("buttonStyle.padding.left", buttonStyle.padding.left);
            config.AddValue("buttonStyle.padding.right", buttonStyle.padding.right);
            config.AddValue("buttonStyle.padding.top", buttonStyle.padding.top);
            config.AddValue("buttonStyle.padding.bottom", buttonStyle.padding.bottom);

            config.AddValue("buttonStyle.margin.left", buttonStyle.margin.left);
            config.AddValue("buttonStyle.margin.right", buttonStyle.margin.right);
            config.AddValue("buttonStyle.margin.top", buttonStyle.margin.top);
            config.AddValue("buttonStyle.margin.bottom", buttonStyle.margin.bottom);

            config.Save(filename);
            Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: saved to file: " + config);
        }
        catch
        {
            Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: failed to save config file");
        }
    }

    private static void getValue(ConfigNode config, string name, ref bool value)
    {
        bool newValue;
        if (config.HasValue(name) && bool.TryParse(config.GetValue(name), out newValue))
        {
            value = newValue;
        }
    }

    private static void getValue(ConfigNode config, string name, ref int value)
    {
        int newValue;
        if (config.HasValue(name) && int.TryParse(config.GetValue(name), out newValue))
        {
            value = newValue;
        }
    }

    private static void getValue(ConfigNode config, string name, ref double value)
    {
        double newValue;
        if (config.HasValue(name) && double.TryParse(config.GetValue(name), out newValue))
        {
            value = newValue;
        }
    }

    public void CleanUp()
    {
        mainWindow.SetVisible(false);
        configWindow.SetVisible(false);
        helpWindow.SetVisible(false);
    }

    [KSPEvent(guiActive = true, guiName = "Show TAC Atomic Clock", active = false)]
    public void ShowClockEvent()
    {
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: shown by right click");
        mainWindow.SetVisible(true);
    }

    [KSPEvent(guiActive = true, guiName = "Hide TAC Atomic Clock")]
    public void HideClockEvent()
    {
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: hidden by right click");
        mainWindow.SetVisible(false);
    }

    [KSPAction("Toggle TAC Atomic Clock")]
    public void ToggleClockAction(KSPActionParam param)
    {
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: toggled by action");
        mainWindow.SetVisible(!mainWindow.IsVisible());
    }

    private class MainWindow : Window
    {
        private TacAtomicClock parent;

        public MainWindow(TacAtomicClock parent)
            : base("TAC Atomic Clock", parent)
        {
            this.parent = parent;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (newValue)
            {
                parent.Events["ShowClockEvent"].active = false;
                parent.Events["HideClockEvent"].active = true;
            }
            else
            {
                parent.Events["ShowClockEvent"].active = true;
                parent.Events["HideClockEvent"].active = false;
                parent.configWindow.SetVisible(false);
                parent.helpWindow.SetVisible(false);
            }
        }

        protected override void CreateWindow()
        {
            if (parent.buttonStyle == null)
            {
                parent.buttonStyle = new GUIStyle(GUI.skin.button);
                parent.buttonStyle.padding = parent.buttonPadding;
                parent.buttonStyle.margin = parent.buttonMargin;
            }

            base.CreateWindow();
        }

        protected override void Draw(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("C", parent.buttonStyle))
            {
                parent.configWindow.SetVisible(!parent.configWindow.IsVisible());
            }
            if (GUILayout.Button("?", parent.buttonStyle))
            {
                parent.helpWindow.SetVisible(!parent.helpWindow.IsVisible());
            }
            if (GUILayout.Button("X", parent.buttonStyle))
            {
                SetVisible(false);
            }
            GUILayout.EndHorizontal();

            double ut = Planetarium.GetUniversalTime();
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.wordWrap = false;

            if (parent.showingUniversalTime)
            {
                GUILayout.Label("UT: " + ((long)ut).ToString("#,#"), labelStyle, GUILayout.ExpandWidth(true));
            }
            if (parent.showingEarthTime)
            {
                GUILayout.Label("ET: " + GetEarthTime(ut), labelStyle, GUILayout.ExpandWidth(true));
            }
            if (parent.showingKerbinTime)
            {
                GUILayout.Label("KT: " + GetKerbinTime(ut), labelStyle, GUILayout.ExpandWidth(true));
                if (parent.debug)
                {
                    GUILayout.Label("KT: " + GetKerbinTimeSideReel(ut), labelStyle, GUILayout.ExpandWidth(true));
                }
            }
            if (parent.showingRealTime)
            {
                GUILayout.Label("RT: " + DateTime.Now.ToLongTimeString(), labelStyle, GUILayout.ExpandWidth(true));
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
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
                + seconds.ToString("00") + "";
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
            double kerbinSecondsPerEarthSecond = (parent.kerbinSecondsPerMinute * parent.kerbinMinutesPerHour * parent.kerbinHoursPerDay) / parent.earthSecondsPerKerbinDay;

            long seconds = (long)((ut + parent.initialOffsetInEarthSeconds) * kerbinSecondsPerEarthSecond);

            long minutes = (long)(seconds / parent.kerbinSecondsPerMinute);
            seconds -= (long)(minutes * parent.kerbinSecondsPerMinute);

            long hours = (long)(minutes / parent.kerbinMinutesPerHour);
            minutes -= (long)(hours * parent.kerbinMinutesPerHour);

            long days = (long)(hours / parent.kerbinHoursPerDay);
            hours -= (long)(days * parent.kerbinHoursPerDay);

            long months = (long)(days / parent.kerbinDaysPerMonth);
            days -= (long)(months * parent.kerbinDaysPerMonth);

            long years = (long)(months / parent.kerbinMonthsPerYear);
            months -= (long)(years * parent.kerbinMonthsPerYear);

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

    private class ConfigWindow : Window
    {
        private bool showAdvanced = false;
        private TacAtomicClock parent;

        public ConfigWindow(TacAtomicClock parent)
            : base("TAC Clock Config", parent)
        {
            this.parent = parent;
        }

        protected override void Draw(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", parent.buttonStyle))
            {
                SetVisible(false);
            }
            GUILayout.EndHorizontal();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.wordWrap = false;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show Universal Time", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            parent.showingUniversalTime = GUILayout.Toggle(parent.showingUniversalTime, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show Earth Time", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            parent.showingEarthTime = GUILayout.Toggle(parent.showingEarthTime, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show Kerbin Time", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            parent.showingKerbinTime = GUILayout.Toggle(parent.showingKerbinTime, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show Real Time", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            parent.showingRealTime = GUILayout.Toggle(parent.showingRealTime, "");
            GUILayout.EndHorizontal();

            GUILayout.Space(20.0f);

            if (!showAdvanced)
            {
                if (GUILayout.Button("Advanced", parent.buttonStyle, GUILayout.ExpandWidth(true)))
                {
                    showAdvanced = true;
                }
            }
            else
            {
                if (GUILayout.Button("Simple", parent.buttonStyle, GUILayout.ExpandWidth(true)))
                {
                    showAdvanced = false;
                }

                double temp2;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Initial offset in Earth seconds", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.initialOffsetInEarthSeconds.ToString(), 10), out temp2))
                {
                    parent.initialOffsetInEarthSeconds = temp2;
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Earth seconds per Kerbin day", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.earthSecondsPerKerbinDay.ToString(), 10), out temp2))
                {
                    parent.earthSecondsPerKerbinDay = temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin seconds per minute", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.kerbinSecondsPerMinute.ToString(), 10), out temp2))
                {
                    parent.kerbinSecondsPerMinute = temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin minutes per hour", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.kerbinMinutesPerHour.ToString(), 10), out temp2))
                {
                    parent.kerbinMinutesPerHour = temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin hours per day", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.kerbinHoursPerDay.ToString(), 10), out temp2))
                {
                    parent.kerbinHoursPerDay = temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin days per month", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.kerbinDaysPerMonth.ToString(), 10), out temp2))
                {
                    parent.kerbinDaysPerMonth = temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin months per year", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.kerbinMonthsPerYear.ToString(), 10), out temp2))
                {
                    parent.kerbinMonthsPerYear = temp2;
                }
                GUILayout.EndHorizontal();
            }

            if (parent.debug)
            {
                int temp;
                GUILayout.Space(20.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - left margin", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.margin.left.ToString(), 10), out temp))
                {
                    parent.buttonStyle.margin.left = temp;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - top margin", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.margin.top.ToString(), 5), out temp))
                {
                    parent.buttonStyle.margin.top = temp;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - right margin", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.margin.right.ToString(), 10), out temp))
                {
                    parent.buttonStyle.margin.right = temp;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - bottom margin", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.margin.bottom.ToString(), 10), out temp))
                {
                    parent.buttonStyle.margin.bottom = temp;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - left padding", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.padding.left.ToString(), 10), out temp))
                {
                    parent.buttonStyle.padding.left = temp;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - top padding", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.padding.top.ToString(), 10), out temp))
                {
                    parent.buttonStyle.padding.top = temp;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - right padding", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.padding.right.ToString(), 10), out temp))
                {
                    parent.buttonStyle.padding.right = temp;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Button Style - bottom padding", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (int.TryParse(GUILayout.TextField(parent.buttonStyle.padding.bottom.ToString(), 10), out temp))
                {
                    parent.buttonStyle.padding.bottom = temp;
                }
                GUILayout.EndHorizontal();
            }

            parent.debug = GUILayout.Toggle(parent.debug, "Debug");

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                // Reset the main window size because the user toggled the display of something
                parent.mainWindow.SetSize(10, 10);
                SetSize(10, 10);
            }

            GUI.DragWindow();
        }
    }

    private class HelpWindow : Window
    {
        private TacAtomicClock parent;

        public HelpWindow(TacAtomicClock parent)
            : base("TAC Clock Help", parent)
        {
            this.parent = parent;
        }

        protected override void Draw(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", parent.buttonStyle))
            {
                SetVisible(false);
            }
            GUILayout.EndHorizontal();

            GUIStyle textAreaStyle = new GUIStyle(GUI.skin.textArea);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Atomic Clock by Thunder Aerospace Corporation (TaranisElsu).");
            sb.AppendLine();
            sb.AppendLine("Definitions:");
            sb.AppendLine("Universal Time -- time in Earth seconds since the game began.");
            sb.AppendLine("Earth Time -- elapsed time in Earth years/months/days/hours/minutes/seconds since the game began.");
            sb.AppendLine("Kerbin Time -- elapsed time in Kerbin years/months/days/hours/minutes/seconds since the game began.");
            sb.AppendLine("Real Time -- current clock time from your computer.");

            GUILayout.TextArea(sb.ToString(), textAreaStyle);

            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
