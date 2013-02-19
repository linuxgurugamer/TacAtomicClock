using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TacAtomicClock : PartModule
{
    private Boolean showingWindow = true;
    private Rect windowPos;
    private int windowId = "TAC Atomic Clock".GetHashCode();

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        Debug.Log("TAC Atomic Clock [" + Time.time + "]: OnStart " + state);
        Debug.Log("TAC Atomic Clock hash code = " + windowId);

        if (state != StartState.Editor)
        {
            vessel.OnJustAboutToBeDestroyed += CleanUp;
            part.OnJustAboutToBeDestroyed += CleanUp;

            windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);

            RenderingManager.AddToPostDrawQueue(3, new Callback(DrawWindow));
        }
    }

    public void CleanUp()
    {
        if (showingWindow)
        {
            doHideWindow();
        }
    }

    [KSPEvent(guiActive = true, guiName = "Show/Hide TAC Atomic Clock")]
    public void ToggleClockEvent()
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: toggled by right click");
        toggleWindow();
    }

    [KSPAction("Toggle TAC Atomic Clock")]
    public void ToggleClockAction(KSPActionParam param)
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: toggled by action");
        toggleWindow();
    }

    private void DrawWindow()
    {
        try
        {
            if ((this.part.State != PartStates.DEAD) && (this.vessel.isActiveVessel))
            {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(windowId, windowPos, DoDraw, "TAC Clock", GUILayout.MinWidth(100.0f));
            }
            else
            {
                doHideWindow();
            }
        }
        catch
        {
            doHideWindow();
        }
    }

    private void DoDraw(int windowID)
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        GUIStyle buttonStyle = GUI.skin.label;
        //buttonStyle.padding = new RectOffset(2, 2, 2, 2);  TODO

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("C", buttonStyle, GUILayout.ExpandWidth(false)))
        {
        }
        if (GUILayout.Button("?", buttonStyle))
        {
        }
        if (GUILayout.Button("X", buttonStyle))
        {
            doHideWindow();
        }
        GUILayout.EndHorizontal();

        GUIStyle labelStyle = GUI.skin.label;
        labelStyle.wordWrap = false;

        double ut = Planetarium.GetUniversalTime();

        GUILayout.Label("UT: " + ((int)ut).ToString("#,#"), labelStyle, GUILayout.ExpandWidth(true));
        GUILayout.Label("ET: " + getEarthTime(ut), labelStyle, GUILayout.ExpandWidth(true));
        GUILayout.Label("KT: " + getKerbinTime(ut), labelStyle, GUILayout.ExpandWidth(true));
        GUILayout.Label("RT: " + DateTime.Now.ToShortTimeString(), labelStyle, GUILayout.ExpandWidth(true));

        GUILayout.EndVertical();

        GUI.DragWindow();
    }

    private void toggleWindow()
    {
        if (showingWindow)
        {
            doHideWindow();
        }
        else
        {
            doShowWindow();
        }
    }

    private void doShowWindow()
    {
        RenderingManager.AddToPostDrawQueue(3, new Callback(DrawWindow));
        showingWindow = true;
    }

    private void doHideWindow()
    {
        RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawWindow));
        showingWindow = false;
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

    protected String getKerbinTime(double ut)
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
