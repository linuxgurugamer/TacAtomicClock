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
    private Boolean showingUniversalTime = true;
    private Boolean showingEarthTime = true;
    private Boolean showingKerbinTime = true;
    private Boolean showingRealTime = true;
    private MainWindow mainWindow;
    private ConfigWindow configWindow;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        Debug.Log("TAC Atomic Clock [" + Time.time + "]: OnStart " + state);

        if (state != StartState.Editor)
        {
            vessel.OnJustAboutToBeDestroyed += CleanUp;
            part.OnJustAboutToBeDestroyed += CleanUp;

            mainWindow = new MainWindow(this);
            mainWindow.setVisible(true);

            configWindow = new ConfigWindow(this);
        }
    }

    public void CleanUp()
    {
        if (mainWindow != null)
        {
            mainWindow.setVisible(false);
        }
        if (configWindow != null)
        {
            configWindow.setVisible(false);
        }
    }

    [KSPEvent(guiActive = true, guiName = "Show/Hide TAC Atomic Clock")]
    public void ToggleClockEvent()
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: toggled by right click");
        mainWindow.setVisible(!mainWindow.isVisible());
    }

    [KSPAction("Toggle TAC Atomic Clock")]
    public void ToggleClockAction(KSPActionParam param)
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: toggled by action");
        mainWindow.setVisible(!mainWindow.isVisible());
    }

    private class MainWindow
    {
        private Boolean visible = false;
        private Rect windowPos = new Rect(60, 60, 60, 60);
        private String windowTitle = "TAC Atomic Clock";
        private TacAtomicClock parent;

        public MainWindow(TacAtomicClock parent)
        {
            this.parent = parent;
        }

        public Boolean isVisible()
        {
            return visible;
        }

        public void setVisible(Boolean newValue)
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
                    parent.configWindow.setVisible(false);
                }
            }

            this.visible = newValue;
        }

        public void setSize(int width, int height)
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
                    //GUI.skin = HighLogic.Skin;
                    GUI.skin = null;
                    windowPos = GUILayout.Window(windowTitle.GetHashCode(), windowPos, Draw, windowTitle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
                else
                {
                    setVisible(false);
                }
            }
            catch
            {
                setVisible(false);
            }
        }

        private void Draw(int windowID)
        {
            GUILayout.BeginVertical();

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("C", buttonStyle))
            {
                parent.configWindow.setVisible(!parent.configWindow.isVisible());
            }
            if (GUILayout.Button("?", buttonStyle))
            {
            }
            if (GUILayout.Button("X", buttonStyle))
            {
                setVisible(false);
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
                GUILayout.Label("ET: " + getEarthTime(ut), labelStyle, GUILayout.ExpandWidth(true));
            }
            if (parent.showingKerbinTime)
            {
                GUILayout.Label("KT: " + getKerbinTime(ut), labelStyle, GUILayout.ExpandWidth(true));
            }
            if (parent.showingRealTime)
            {
                GUILayout.Label("RT: " + DateTime.Now.ToLongTimeString(), labelStyle, GUILayout.ExpandWidth(true));
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        protected String getEarthTime(double ut)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double SECONDS_PER_HOUR = SECONDS_PER_MINUTE * 60.0; // 3,600
            const double SECONDS_PER_DAY = SECONDS_PER_HOUR * 24.0; // 86,400
            const double SECONDS_PER_YEAR = SECONDS_PER_DAY * 365.0; // 31,536,000
            const double SECONDS_PER_MONTH = SECONDS_PER_YEAR / 12.0; // 2,628,000

            int seconds = (int)(ut % SECONDS_PER_MINUTE);
            int minutes = (int)(ut % SECONDS_PER_HOUR / SECONDS_PER_MINUTE);
            int hours = (int)(ut % SECONDS_PER_DAY / SECONDS_PER_HOUR);
            int days = (int)(ut % SECONDS_PER_MONTH / SECONDS_PER_DAY);
            int months = (int)(ut % SECONDS_PER_YEAR / SECONDS_PER_MONTH);
            int years = (int)(ut / SECONDS_PER_YEAR);

            // days -= ((years - 1970) / 4); // adjust for leap years

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00") + "";
        }

        private String getKerbinTime(double ut)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double SECONDS_PER_HOUR = SECONDS_PER_MINUTE * 60.0; // 3,600
            const double SECONDS_PER_DAY = SECONDS_PER_HOUR * 6.0 + 50.0; // 6 hours and 50 seconds = 21,650 seconds
            const double SECONDS_PER_MONTH = SECONDS_PER_HOUR * 38.6; // 38.6 hours = 138,960 seconds
            const double SECONDS_PER_YEAR = SECONDS_PER_HOUR * 2556.50; // 2556.50 hours = 9,203,400 seconds
            //const double SECONDS_PER_MINUTE = 60.0;
            //const double SECONDS_PER_HOUR = SECONDS_PER_MINUTE * 60.0; // 2,165 seconds
            //const double SECONDS_PER_DAY = SECONDS_PER_HOUR * 6.0 + 50.0; // 6 hours and 50 seconds = 21,650 seconds
            //const double SECONDS_PER_MONTH = SECONDS_PER_HOUR * 38.6; // 38.6 hours = 138,960 seconds
            //const double SECONDS_PER_YEAR = SECONDS_PER_HOUR * 2556.50; // 2556.50 hours = 9,203,400 seconds

            int seconds = (int)(ut % SECONDS_PER_MINUTE);
            int minutes = (int)(ut % SECONDS_PER_HOUR / SECONDS_PER_MINUTE);
            int hours = (int)(ut % SECONDS_PER_DAY / SECONDS_PER_HOUR);
            int days = (int)(ut % SECONDS_PER_MONTH / SECONDS_PER_DAY);
            int months = (int)(ut % SECONDS_PER_YEAR / SECONDS_PER_MONTH);
            int years = (int)(ut / SECONDS_PER_YEAR);

            return years.ToString("00") + ":"
                + months.ToString("00") + ":"
                + days.ToString("00") + " "
                + hours.ToString("00") + ":"
                + minutes.ToString("00") + ":"
                + seconds.ToString("00") + "";
        }
    }

    private class ConfigWindow
    {
        private Boolean visible = false;
        private Rect windowPos = new Rect(60, 60, 60, 60);
        private String windowTitle = "TAC Clock Config";
        private TacAtomicClock parent;

        public ConfigWindow(TacAtomicClock parent)
        {
            this.parent = parent;
        }

        public Boolean isVisible()
        {
            return visible;
        }

        public void setVisible(Boolean newValue)
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
                    windowPos = GUILayout.Window(windowTitle.GetHashCode(), windowPos, Draw, windowTitle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
                else
                {
                    setVisible(false);
                }
            }
            catch
            {
                setVisible(false);
            }
        }

        private void Draw(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
            {
                setVisible(false);
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

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                // Reset the main window size because the user toggled the display of something
                parent.mainWindow.setSize(10, 10);
            }

            GUI.DragWindow();
        }
    }
}
