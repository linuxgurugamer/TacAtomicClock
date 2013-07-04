using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
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
