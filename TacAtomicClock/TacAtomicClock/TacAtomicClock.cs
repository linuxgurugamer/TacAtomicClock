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
    private MainWindow mainWindow;
    private ConfigWindow configWindow;
    private HelpWindow helpWindow;

    private bool showingUniversalTime = true;
    private bool showingEarthTime = true;
    private bool showingKerbinTime = true;
    private bool showingRealTime = true;

    double initialOffsetInEarthSeconds;
    double kerbinSecondsPerEarthSecond;

    double kerbinSecondsPerMinute;
    double kerbinSecondsPerHour;
    double kerbinSecondsPerDay;
    double kerbinSecondsPerMonth;
    double kerbinSecondsPerYear;

    private bool debug = true;

    private GUIStyle buttonStyle = null;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        Debug.Log("TAC Atomic Clock [" + Time.time + "]: OnStart " + state);

        mainWindow = new MainWindow(this);
        configWindow = new ConfigWindow(this);
        helpWindow = new HelpWindow(this);

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

        if (state != StartState.Editor)
        {
            vessel.OnJustAboutToBeDestroyed += CleanUp;
            part.OnJustAboutToBeDestroyed += CleanUp;

            mainWindow.SetVisible(true);
        }
    }

    public void CleanUp()
    {
        mainWindow.SetVisible(false);
        configWindow.SetVisible(false);
        helpWindow.SetVisible(false);
    }

    [KSPEvent(guiActive = true, guiName = "Show/Hide TAC Atomic Clock")]
    public void ToggleClockEvent()
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: toggled by right click");
        mainWindow.SetVisible(!mainWindow.IsVisible());
    }

    [KSPAction("Toggle TAC Atomic Clock")]
    public void ToggleClockAction(KSPActionParam param)
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: toggled by action");
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
                    parent.Events["ToggleClockEvent"].guiName = "Hide TAC Atomic Clock";
                }
            }
            else
            {
                if (visible)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(CreateWindow));
                    parent.Events["ToggleClockEvent"].guiName = "Show TAC Atomic Clock";
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
            const double SECONDS_PER_MONTH = SECONDS_PER_YEAR / 12.0; // 2,628,000 seconds, 30.4 days

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

            // The game starts on Year 1, Month 1
            years += 1;
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

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00") + " (sidereel days)";
        }

        private String GetKerbinTime(double ut)
        {
            double temp = (ut + parent.initialOffsetInEarthSeconds) * parent.kerbinSecondsPerEarthSecond;

            int years = (int)(temp / parent.kerbinSecondsPerYear);
            temp -= years * parent.kerbinSecondsPerYear;

            int months = (int)(temp / parent.kerbinSecondsPerMonth);
            temp -= months * parent.kerbinSecondsPerMonth;

            int days = (int)(temp / parent.kerbinSecondsPerDay);
            temp -= days * parent.kerbinSecondsPerDay;

            int hours = (int)(temp / parent.kerbinSecondsPerHour);
            temp -= hours * parent.kerbinSecondsPerHour;

            int minutes = (int)(temp / parent.kerbinSecondsPerMinute);
            temp -= minutes * parent.kerbinSecondsPerMinute;

            int seconds = (int)(temp);

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00") + " (solar days)";
        }
    }

    private class ConfigWindow
    {
        private bool visible = false;
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
                if (double.TryParse(GUILayout.TextField((parent.kerbinSecondsPerHour / parent.kerbinSecondsPerMinute).ToString(), 10), out temp2))
                {
                    parent.kerbinSecondsPerHour = parent.kerbinSecondsPerMinute * temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin hours per day", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField((parent.kerbinSecondsPerDay / parent.kerbinSecondsPerHour).ToString(), 10), out temp2))
                {
                    parent.kerbinSecondsPerDay = parent.kerbinSecondsPerHour * temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin days per month", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField((parent.kerbinSecondsPerMonth / parent.kerbinSecondsPerDay).ToString(), 10), out temp2))
                {
                    parent.kerbinSecondsPerMonth = parent.kerbinSecondsPerDay * temp2;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Kerbin days per year", labelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (double.TryParse(GUILayout.TextField((parent.kerbinSecondsPerYear / parent.kerbinSecondsPerDay).ToString(), 10), out temp2))
                {
                    parent.kerbinSecondsPerYear = parent.kerbinSecondsPerDay * temp2;
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
