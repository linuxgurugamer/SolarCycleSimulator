/*
 * Whitecat Industries Solar Cycle Simulator for Kerbal Space Program. 
 * 
 * Written by Whitecat106 (Marcus Hehir).
 * 
 * Kerbal Space Program is Copyright (C) 2016 Squad. See http://kerbalspaceprogram.com/. This
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
 * Note that Whitecat Industries is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using UnityEngine;
using KSP.UI.Screens;
using ToolbarControl_NS;
using ClickThroughFix;

namespace WhitecatIndustries
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    class SCSWindow : MonoBehaviour
    {
        static ToolbarControl toolbarControl;

        // public static ApplicationLauncherButton button = null;
        public static Rect windowPosition = new Rect(300, 60, 220, 300);
        public static Rect detailsWindowPosition = new Rect(300, 400, 200, 230);
        private static GUIStyle windowStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle labelStyleSmall;
        private static GUIStyle buttonStyle;
        private static GUIStyle scrollStyle;
        private static Vector2 scrollPos = Vector2.zero;
        private static Texture texture = null;
        private static bool showGui = false;
        static bool hideUI = false;
        static bool paused = false;
        private static bool showDetails = false;

        public static ConfigNode CurrentUICycle;

        void Awake()
        {
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            labelStyle = new GUIStyle(HighLogic.Skin.label);
            labelStyleSmall = new GUIStyle(HighLogic.Skin.label) { fontSize = 20 };
            buttonStyle = new GUIStyle(HighLogic.Skin.button);
            scrollStyle = new GUIStyle(HighLogic.Skin.scrollView);

#if false
            GameEvents.onGUIApplicationLauncherReady.Remove(ReadyEvent);
            GameEvents.onGUIApplicationLauncherReady.Add(ReadyEvent);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(DestroyEvent);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(DestroyEvent);
#endif
            InitializeToobar();

        }

        internal const string MODID = "SolarCycleSimulator_NS";
        internal const string MODNAME = "Solar Cycle Simulator";

#if false
        public void ReadyEvent()
        {
            if (ApplicationLauncher.Ready && button == null)
            {
                var Scene = ApplicationLauncher.AppScenes.TRACKSTATION;
                texture = GameDatabase.Instance.GetTexture("WhitecatIndustries/SolarCycleSimulator/Icons/Icon", false);
                button = ApplicationLauncher.Instance.AddModApplication(GuiOn, GuiOff, null, null, null, null, Scene, texture);
            }
        }
#endif
        void InitializeToobar()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(GuiOn, GuiOff,
                    ApplicationLauncher.AppScenes.TRACKSTATION,
                    MODID,
                    "SolarCyclesimButton",
                  "WhitecatIndustries/SolarCycleSimulator/Icons/Icon",
                  "WhitecatIndustries/SolarCycleSimulator/Icons/Icon",
                    MODNAME
                );
            }

        }
        public void DestroyEvent()
        {
#if false
            if (button == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(button);
            button = null;
#endif
#if false
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
                toolbarControl = null;
            }
#endif
            showGui = false;
        }

        private void GuiOn()
        {
            showGui = true;
        }

        private void GuiOff()
        {
            showGui = false;
        }

        public void OnGUI()
        {
            if (showGui && !hideUI && !paused)
            {
                windowPosition = ClickThruBlocker.GUILayoutWindow(777, windowPosition, OnWindow, "Solar Cycle Manager", windowStyle);
            }

            if (showDetails && !hideUI && !paused)
            {
                detailsWindowPosition = ClickThruBlocker.GUILayoutWindow(778, detailsWindowPosition, DetailsWindow, "Current Cycle Information", windowStyle);
            }
        }

        public void OnWindow(int WindowId)
        {
            if (GUI.Button(new Rect(windowPosition.width - 22, 3, 19, 19), "x"))
            {
                showGui = false;
            }

            GUILayout.BeginVertical();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label("Information", GUILayout.Width(200));
            GUILayout.Space(5);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(200), GUILayout.Height(230));

            foreach (ConfigNode node in SCSManager.SolarCycles.GetNodes("CYCLE"))
            {
                ConfigNode Cycle = node;
                GUI.skin.label.fontStyle = FontStyle.Normal;
                GUI.skin.label.fontSize = 12;

                if (Cycle.GetValue("Persistence") == HighLogic.SaveFolder.ToString() && Cycle.GetValue("CurrentCycle") == true.ToString())
                {
                    CurrentUICycle = Cycle;

                    GUILayout.BeginVertical();
                    GUILayout.Space(1);
                    GUILayout.Label("Current Solar Cycle: " + Cycle.GetValue("Name"));
                    GUILayout.Space(1);
                    GUILayout.Label("Duration: " + (KSPUtil.dateTimeFormatter.PrintTime(double.Parse(Cycle.GetValue("Duration")), 1, false)));
                    GUILayout.Space(1);
                    GUILayout.Label("Start Date: " + (KSPUtil.dateTimeFormatter.PrintDate(double.Parse(Cycle.GetValue("StartTime")), true, false)));
                    GUILayout.Space(1);
                    GUILayout.Label("End Date: " + (KSPUtil.dateTimeFormatter.PrintDate(double.Parse(Cycle.GetValue("EndTime")), true, false)));
                    GUILayout.Space(1);
                    GUILayout.Label("Minimum Date: " + (KSPUtil.dateTimeFormatter.PrintDate((double.Parse(Cycle.GetValue("MaximumTime"))), true, false)));
                    GUILayout.Space(1);
                    GUILayout.Label("Percent Through: " + double.Parse((Cycle.GetValue("PercentThrough"))).ToString("F1") + "%");
                    GUILayout.Space(1);
                    if (GUILayout.Button("Show Details"))
                    {
                        showDetails = !showDetails;
                    }
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
            windowPosition.x = Mathf.Clamp(windowPosition.x, 0f, Screen.width - windowPosition.width);
            windowPosition.y = Mathf.Clamp(windowPosition.y, 0f, Screen.height - windowPosition.height);
        }

        public void DetailsWindow(int id)
        {
            if (GUI.Button(new Rect(detailsWindowPosition.width - 22, 3, 19, 19), "x"))
            {
                showDetails = false;
            }

            ConfigNode Cycle = CurrentUICycle;
            GUILayout.BeginVertical();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label("Real Time Data", GUILayout.Width(300));
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.fontSize = 12;
            GUILayout.Space(3);
            GUILayout.Label("_________________________________________");
            GUILayout.Space(1);
            GUILayout.Label("F10.7cm Radio Flux (Current): " + double.Parse(Cycle.GetValue("CurrentF107")).ToString("F3") + " s.f.u");
            GUILayout.Space(1);
            GUILayout.Label("Geomagnetic Index (Current): " + double.Parse(Cycle.GetValue("CurrentAp")).ToString("F3"));
            GUILayout.Space(1);
            GUILayout.Label("Solar Irradiance (Current): " + double.Parse(Cycle.GetValue("CurrentIrradiance")).ToString("F3") + " W/m^2");
            GUILayout.Space(1);
            GUILayout.Label("Sunspot Count (Current): " + double.Parse(Cycle.GetValue("CurrentSunSpots")).ToString("F0"));
            GUILayout.Space(1);
            GUILayout.Label("_________________________________________");

            GUILayout.EndVertical();
            GUI.DragWindow();
            windowPosition.x = Mathf.Clamp(windowPosition.x, 0f, Screen.width - windowPosition.width);
            windowPosition.y = Mathf.Clamp(windowPosition.y, 0f, Screen.height - windowPosition.height);

        }
    }
}
