/**
 * Atomic Clock from Thunder Aerospace Corporation.
 * by TaranisElsu.
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
    private static string filename = "PluginData\\tacatomicclock\\TacAtomicClock.cfg";
    private MainWindow mainWindow;
    private ConfigWindow configWindow;
    private HelpWindow helpWindow;

    private bool showingUniversalTime;
    private bool showingEarthTime;
    private bool showingKerbinTime;
    private bool showingRealTime;

    private double initialOffsetInEarthSeconds;
    private double kerbinSecondsPerEarthSecond;
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
        kerbinSecondsPerMinute = 36.0;
        kerbinMinutesPerHour = 36.0;
        kerbinHoursPerDay = 12.0;
        kerbinDaysPerMonth = 6.418476;
        kerbinMonthsPerYear = 66.23057;

        buttonPadding = new RectOffset(5, 5, 3, 0);
        buttonMargin = new RectOffset(1, 1, 1, 1);

        // TODO Got this from the wiki, is it correct?
        const double earthHoursPerKerbinDay = 6 + 50.0 / 3600.0; // 6 hours and 50 seconds, or 21650 seconds
        kerbinSecondsPerEarthSecond = (kerbinHoursPerDay * kerbinMinutesPerHour * kerbinSecondsPerMinute) / (earthHoursPerKerbinDay * 3600.0);

        debug = false;
    }

    public override void OnStart(PartModule.StartState state)
    {
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnStart " + state);
        base.OnStart(state);

        if (state != StartState.Editor)
        {
            vessel.OnJustAboutToBeDestroyed += CleanUp;
            part.OnJustAboutToBeDestroyed += CleanUp;

            mainWindow.SetVisible(true);
        }
    }

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);

        ConfigNode config = ConfigNode.Load(filename);
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: loaded from file: " + node);

        getValue(config, "debug", ref debug);

        getValue(config, "showingUniversalTime", ref showingUniversalTime);
        getValue(config, "showingEarthTime", ref showingEarthTime);
        getValue(config, "showingKerbinTime", ref showingKerbinTime);
        getValue(config, "showingRealTime", ref showingRealTime);

        getValue(config, "initialOffsetInEarthSeconds", ref initialOffsetInEarthSeconds);
        getValue(config, "kerbinSecondsPerEarthSecond", ref kerbinSecondsPerEarthSecond);
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

    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);

        ConfigNode config = new ConfigNode();
        config.AddValue("debug", debug);

        config.AddValue("showingUniversalTime", showingUniversalTime);
        config.AddValue("showingEarthTime", showingEarthTime);
        config.AddValue("showingKerbinTime", showingKerbinTime);
        config.AddValue("showingRealTime", showingRealTime);

        config.AddValue("initialOffsetInEarthSeconds", initialOffsetInEarthSeconds);
        config.AddValue("kerbinSecondsPerEarthSecond", kerbinSecondsPerEarthSecond);
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
        Debug.Log("TAC Atomic Clock [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: saved to file: " + node);
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

    private class MainWindow
    {
        private bool visible = false;
        private Rect windowPos = new Rect(60, 60, 60, 60);
        private String windowTitle = "TAC Atomic Clock";
        private TacAtomicClock parent;

        public MainWindow(TacAtomicClock parent)
        {
            this.parent = parent;
        }

        public bool IsVisible()
        {
            return visible;
        }

        public void SetVisible(bool newValue)
        {
            if (newValue)
            {
                if (!visible)
                {
                    RenderingManager.AddToPostDrawQueue(3, new Callback(CreateWindow));
                    parent.Events["ShowClockEvent"].active = false;
                    parent.Events["HideClockEvent"].active = true;
                }
            }
            else
            {
                if (visible)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(CreateWindow));
                    parent.Events["ShowClockEvent"].active = true;
                    parent.Events["HideClockEvent"].active = false;
                    parent.configWindow.SetVisible(false);
                    parent.helpWindow.SetVisible(false);
                }
            }

            this.visible = newValue;
        }

        public void SetSize(int width, int height)
        {
            windowPos.width = width;
            windowPos.height = height;
        }

        public void Load(ConfigNode config, string subnode)
        {
            Debug.Log("TAC Atomic Clock [" + Time.time + "]: Load " + subnode);
            if (config.HasNode(subnode))
            {
                ConfigNode windowConfig = config.GetNode(subnode);

                float newValue;
                if (windowConfig.HasValue("xPos") && float.TryParse(windowConfig.GetValue("xPos"), out newValue))
                {
                    windowPos.xMin = newValue;
                }

                if (windowConfig.HasValue("yPos") && float.TryParse(windowConfig.GetValue("yPos"), out newValue))
                {
                    windowPos.yMin = newValue;
                }
            }
        }

        public void Save(ConfigNode config, string subnode)
        {
            Debug.Log("TAC Atomic Clock [" + Time.time + "]: Save " + subnode);

            ConfigNode windowConfig;
            if (config.HasNode(subnode))
            {
                windowConfig = config.GetNode(subnode);
            }
            else
            {
                windowConfig = new ConfigNode(subnode);
                config.AddNode(windowConfig);
            }

            windowConfig.AddValue("xPos", windowPos.xMin);
            windowConfig.AddValue("yPos", windowPos.yMin);
        }

        private void CreateWindow()
        {
            try
            {
                if (parent.part.State != PartStates.DEAD && parent.vessel.isActiveVessel)
                {
                    GUI.skin = HighLogic.Skin;

                    if (parent.buttonStyle == null)
                    {
                        parent.buttonStyle = new GUIStyle(GUI.skin.button);
                        parent.buttonStyle.padding = parent.buttonPadding;
                        parent.buttonStyle.margin = parent.buttonMargin;
                    }

                    windowPos = GUILayout.Window(windowTitle.GetHashCode(), windowPos, Draw, windowTitle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
                else
                {
                    SetVisible(false);
                }
            }
            catch
            {
                SetVisible(false);
            }
        }

        private void Draw(int windowID)
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
                GUILayout.Label("UT: " + ((int)ut).ToString("#,#"), labelStyle, GUILayout.ExpandWidth(true));
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
                    GUILayout.Label("KT: " + GetKerbinTime2(ut), labelStyle, GUILayout.ExpandWidth(true));
                }
            }
            if (parent.showingRealTime)
            {
                GUILayout.Label("RT: " + DateTime.Now.ToLongTimeString(), labelStyle, GUILayout.ExpandWidth(true));
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        protected String GetEarthTime(double ut)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double SECONDS_PER_HOUR = SECONDS_PER_MINUTE * 60.0; // 3,600
            const double SECONDS_PER_DAY = SECONDS_PER_HOUR * 24.0; // 86,400
            const double SECONDS_PER_YEAR = SECONDS_PER_DAY * 365.0; // 31,536,000
            const double SECONDS_PER_MONTH = SECONDS_PER_DAY * 30;// SECONDS_PER_YEAR / 12.0; // 2,628,000 seconds, 30.4 days

            double temp = ut;

            int years = (int)(temp / SECONDS_PER_YEAR);
            temp -= years * SECONDS_PER_YEAR;

            int months = (int)(temp / SECONDS_PER_MONTH);
            temp -= months * SECONDS_PER_MONTH;

            int days = (int)(temp / SECONDS_PER_DAY);
            temp -= days * SECONDS_PER_DAY;

            int hours = (int)(temp / SECONDS_PER_HOUR);
            temp -= hours * SECONDS_PER_HOUR;

            int minutes = (int)(temp / SECONDS_PER_MINUTE);
            temp -= minutes * SECONDS_PER_MINUTE;

            int seconds = (int)(temp);

            // The game starts on Year 1, Day 1
            years += 1;
            months += 1;
            days += 1;

            // TODO adjust for leap years?

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
            const double SECONDS_PER_HOUR = SECONDS_PER_MINUTE * 60.0; // 3,600 seconds
            const double SECONDS_PER_DAY = SECONDS_PER_HOUR * 6.0; // 6 hours = 21,600 seconds
            const double SECONDS_PER_MONTH = SECONDS_PER_HOUR * 38.6; // 38.6 hours = 138,960 seconds
            const double SECONDS_PER_YEAR = SECONDS_PER_HOUR * 2556.50; // 2556.50 hours = 9,203,400 seconds

            double temp = ut;

            int years = (int)(temp / SECONDS_PER_YEAR);
            temp -= years * SECONDS_PER_YEAR;

            int months = (int)(temp / SECONDS_PER_MONTH);
            temp -= months * SECONDS_PER_MONTH;

            int days = (int)(temp / SECONDS_PER_DAY);
            temp -= days * SECONDS_PER_DAY;

            int hours = (int)(temp / SECONDS_PER_HOUR);
            temp -= hours * SECONDS_PER_HOUR;

            int minutes = (int)(temp / SECONDS_PER_MINUTE);
            temp -= minutes * SECONDS_PER_MINUTE;

            int seconds = (int)(temp);

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
            // ****************************************************************************
            double initialOffsetInEarthSeconds;
            double kerbinSecondsPerEarthSecond;
            double kerbinSecondsPerMinute;
            double kerbinSecondsPerHour;
            double kerbinSecondsPerDay;
            double kerbinSecondsPerMonth;
            double kerbinSecondsPerYear;

            // ****************************************************************************

            // from the wiki, TODO are they accurate?
            const double earthHoursPerKerbinDay = 6 + 50.0 / 3600.0; // 6 hours and 50 seconds, or 21650 seconds
            const double earthHoursPerKerbinMonth = 38.6;
            const double earthHoursPerKerbinYear = 2556.5;

            // Given that Kerbals have 4 fingers (including thumbs) and 2 joints per finger,
            // they can count to 6 on the three fingers on one hand, and multiply by the four fingers on the other hand for 24
            kerbinSecondsPerMinute = 24.0;
            kerbinSecondsPerHour = kerbinSecondsPerMinute * 24.0; // 576 seconds
            kerbinSecondsPerDay = kerbinSecondsPerHour * 12.0; // 6912 seconds

            double earthHoursPerKerbinHour = earthHoursPerKerbinDay / 12.0;
            kerbinSecondsPerMonth = kerbinSecondsPerHour * earthHoursPerKerbinMonth / earthHoursPerKerbinHour;
            kerbinSecondsPerYear = kerbinSecondsPerHour * earthHoursPerKerbinYear / earthHoursPerKerbinHour;

            kerbinSecondsPerEarthSecond = kerbinSecondsPerDay / (earthHoursPerKerbinDay * 3600.0);
            initialOffsetInEarthSeconds = 0.0;

            // ****************************************************************************

            double temp = (ut + initialOffsetInEarthSeconds) * kerbinSecondsPerEarthSecond;

            int years = (int)(temp / kerbinSecondsPerYear);
            temp -= years * kerbinSecondsPerYear;

            int months = (int)(temp / kerbinSecondsPerMonth);
            temp -= months * kerbinSecondsPerMonth;

            int days = (int)(temp / kerbinSecondsPerDay);
            temp -= days * kerbinSecondsPerDay;

            int hours = (int)(temp / kerbinSecondsPerHour);
            temp -= hours * kerbinSecondsPerHour;

            int minutes = (int)(temp / kerbinSecondsPerMinute);
            temp -= minutes * kerbinSecondsPerMinute;

            int seconds = (int)(temp);

            // The game starts on Year 1, Day 1
            years += 1;
            months += 1;
            days += 1;

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00") + " (solar days)";
        }

        private String GetKerbinTime2(double ut)
        {
            int seconds = (int)((ut + parent.initialOffsetInEarthSeconds) * parent.kerbinSecondsPerEarthSecond);

            int minutes = (int)(seconds / parent.kerbinSecondsPerMinute);
            seconds -= (int)(minutes * parent.kerbinSecondsPerMinute);

            int hours = (int)(minutes / parent.kerbinMinutesPerHour);
            minutes -= (int)(hours * parent.kerbinMinutesPerHour);

            int days = (int)(hours / parent.kerbinHoursPerDay);
            hours -= (int)(days * parent.kerbinHoursPerDay);

            int months = (int)(days / parent.kerbinDaysPerMonth);
            days -= (int)(months * parent.kerbinDaysPerMonth);

            int years = (int)(months / parent.kerbinMonthsPerYear);
            months -= (int)(years * parent.kerbinMonthsPerYear);

            // The game starts on Year 1, Day 1
            years += 1;
            months += 1;
            days += 1;

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00") + " (solar days v2)";
        }
    }

    private class ConfigWindow
    {
        private bool visible = false;
        private bool showAdvanced = false;
        private Rect windowPos = new Rect(60, 60, 60, 60);
        private String windowTitle = "TAC Clock Config";
        private TacAtomicClock parent;

        public ConfigWindow(TacAtomicClock parent)
        {
            this.parent = parent;
        }

        public bool IsVisible()
        {
            return visible;
        }

        public void SetVisible(bool newValue)
        {
            if (newValue)
            {
                if (!visible)
                {
                    RenderingManager.AddToPostDrawQueue(3, new Callback(CreateWindow));
                }
            }
            else
            {
                if (visible)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(CreateWindow));
                }
            }

            this.visible = newValue;
        }

        public void SetSize(int width, int height)
        {
            windowPos.width = width;
            windowPos.height = height;
        }

        public void Load(ConfigNode config, string subnode)
        {
            Debug.Log("TAC Atomic Clock [" + Time.time + "]: Load " + subnode);
            if (config.HasNode(subnode))
            {
                ConfigNode windowConfig = config.GetNode(subnode);

                float newValue;
                if (windowConfig.HasValue("xPos") && float.TryParse(windowConfig.GetValue("xPos"), out newValue))
                {
                    windowPos.xMin = newValue;
                }

                if (windowConfig.HasValue("yPos") && float.TryParse(windowConfig.GetValue("yPos"), out newValue))
                {
                    windowPos.yMin = newValue;
                }
            }
        }

        public void Save(ConfigNode config, string subnode)
        {
            Debug.Log("TAC Atomic Clock [" + Time.time + "]: Save " + subnode);

            ConfigNode windowConfig;
            if (config.HasNode(subnode))
            {
                windowConfig = config.GetNode(subnode);
            }
            else
            {
                windowConfig = new ConfigNode(subnode);
                config.AddNode(windowConfig);
            }

            windowConfig.AddValue("xPos", windowPos.xMin);
            windowConfig.AddValue("yPos", windowPos.yMin);
        }

        private void CreateWindow()
        {
            try
            {
                if (parent.part.State != PartStates.DEAD && parent.vessel.isActiveVessel)
                {
                    GUI.skin = HighLogic.Skin;
                    windowPos = GUILayout.Window(windowTitle.GetHashCode(), windowPos, Draw, windowTitle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
                else
                {
                    SetVisible(false);
                }
            }
            catch
            {
                SetVisible(false);
            }
        }

        private void Draw(int windowID)
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
                GUILayout.Label("Kerbin seconds per Earth second", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField(parent.kerbinSecondsPerEarthSecond.ToString(), 10), out temp2))
                {
                    parent.kerbinSecondsPerEarthSecond = temp2;
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

    private class HelpWindow
    {
        private bool visible = false;
        private Rect windowPos = new Rect(60, 60, 60, 60);
        private String windowTitle = "TAC Clock Help";
        private TacAtomicClock parent;

        public HelpWindow(TacAtomicClock parent)
        {
            this.parent = parent;
        }

        public bool IsVisible()
        {
            return visible;
        }

        public void SetVisible(bool newValue)
        {
            if (newValue)
            {
                if (!visible)
                {
                    RenderingManager.AddToPostDrawQueue(3, new Callback(CreateWindow));
                }
            }
            else
            {
                if (visible)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(CreateWindow));
                }
            }

            this.visible = newValue;
        }

        public void Load(ConfigNode config, string subnode)
        {
            Debug.Log("TAC Atomic Clock [" + Time.time + "]: Load " + subnode);
            if (config.HasNode(subnode))
            {
                ConfigNode windowConfig = config.GetNode(subnode);

                float newValue;
                if (windowConfig.HasValue("xPos") && float.TryParse(windowConfig.GetValue("xPos"), out newValue))
                {
                    windowPos.xMin = newValue;
                }

                if (windowConfig.HasValue("yPos") && float.TryParse(windowConfig.GetValue("yPos"), out newValue))
                {
                    windowPos.yMin = newValue;
                }
            }
        }

        public void Save(ConfigNode config, string subnode)
        {
            Debug.Log("TAC Atomic Clock [" + Time.time + "]: Save " + subnode);

            ConfigNode windowConfig;
            if (config.HasNode(subnode))
            {
                windowConfig = config.GetNode(subnode);
            }
            else
            {
                windowConfig = new ConfigNode(subnode);
                config.AddNode(windowConfig);
            }

            windowConfig.AddValue("xPos", windowPos.xMin);
            windowConfig.AddValue("yPos", windowPos.yMin);
        }

        private void CreateWindow()
        {
            try
            {
                if (parent.part.State != PartStates.DEAD && parent.vessel.isActiveVessel)
                {
                    GUI.skin = HighLogic.Skin;
                    windowPos = GUILayout.Window(windowTitle.GetHashCode(), windowPos, Draw, windowTitle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(300.0f));
                }
                else
                {
                    SetVisible(false);
                }
            }
            catch
            {
                SetVisible(false);
            }
        }

        private void Draw(int windowID)
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
