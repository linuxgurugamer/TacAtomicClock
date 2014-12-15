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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    public class TacAtomicClock : PartModule
    {
        private TacAtomicClockMain clock;

        public override void OnAwake()
        {
            this.Log("OnAwake");
            base.OnAwake();
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.Log("OnStart: " + state);
            base.OnStart(state);
            if (state != StartState.Editor && state != StartState.None)
            {
                clock = TacAtomicClockMain.Instance;

                clock.Observers += UpdateEvents;
                UpdateEvents(clock.IsVisible());

                part.OnJustAboutToBeDestroyed += CleanUp;
                vessel.OnJustAboutToBeDestroyed += CleanUp;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            this.Log("OnLoad");
            base.OnLoad(node);
            if (TacAtomicClockMain.Instance != null)
            {
                TacAtomicClockMain.Instance.Load();
            }
        }

        public override void OnSave(ConfigNode node)
        {
            this.Log("OnSave");
            base.OnSave(node);
            if (TacAtomicClockMain.Instance != null)
            {
                TacAtomicClockMain.Instance.Save();
            }
        }

        [KSPEvent(guiActive = true, guiName = "Show TAC Atomic Clock", active = true)]
        public void ShowClockEvent()
        {
            clock.SetVisible(true);
        }

        [KSPEvent(guiActive = true, guiName = "Hide TAC Atomic Clock", active = false)]
        public void HideClockEvent()
        {
            clock.SetVisible(false);
        }

        [KSPAction("Toggle TAC Atomic Clock")]
        public void ToggleClockAction(KSPActionParam param)
        {
            clock.SetVisible(!clock.IsVisible());
        }

        private void UpdateEvents(bool visible)
        {
            Events["ShowClockEvent"].active = !visible;
            Events["HideClockEvent"].active = visible;
        }

        private void CleanUp()
        {
            this.Log("CleanUp");
            clock.Observers -= UpdateEvents;
        }
    }
}
