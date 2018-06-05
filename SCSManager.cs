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

namespace WhitecatIndustries
{
    [KSPAddon(KSPAddon.Startup.EveryScene,false)]
    public class SCSManager : MonoBehaviour
    {
        public static ConfigNode SolarCycles = new ConfigNode();
        public static int CycleCount;
        public static bool KerbinTime = GameSettings.KERBIN_TIME;
        public static double SecondsInYear;

        public void Start() 
        {
            CycleCount = 0;
            SolarCycles = new ConfigNode();

            if ((HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT))
            {
                SecondsInYear = KerbinTime ? 9203545 : 31557600;

                SCSData.LoadInfo();
                print("WhitecatIndustries SCS - Solar Cycle Data Loaded");


                // print("Cycle Count " + CycleCount);

                if (CycleCount < 1)
                {
                    StartNewCycle();
                }
            }
            }

        public void FixedUpdate()
        {
            if (!(Time.timeSinceLevelLoad > 1.5)) return;
            if ((HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT))
            {
                SystemManage();
            }
        }

        public void OnDestroy()
        {
            //if (SpaceCenter.Instance.isActiveAndEnabled) // Fix on load RSS deletions!
            if (HighLogic.LoadedScene != GameScenes.FLIGHT && 
                Planetarium.GetUniversalTime() != HighLogic.CurrentGame.UniversalTime) return;
            if ((HighLogic.LoadedScene != GameScenes.SPACECENTER &&
                 HighLogic.LoadedScene != GameScenes.TRACKSTATION &&
                 HighLogic.LoadedScene != GameScenes.FLIGHT)) return;
            SCSData.SaveInfo();
            print("WhitecatIndustries SCS - Saving to .cfg");
        }

        public void SystemManage()
        {
            if (SolarCycles.nodes.Count == 0) return;
            ConfigNode Cycle = new ConfigNode();
            ConfigNode ListNode = new ConfigNode();

            Cycle = FetchCurrentCycle();
            ListNode = Cycle;

            {
                double Duration = double.Parse(Cycle.GetValue("Duration"));
                double CurrentCycleStartTime = double.Parse(Cycle.GetValue("StartTime"));
                double CurrentCycleEndTime = double.Parse(Cycle.GetValue("EndTime"));
                double PercentThrough = (((HighLogic.CurrentGame.UniversalTime + 1.0) - CurrentCycleStartTime) / Duration) * 100; // Maybe Needs work 

                if (CurrentCycleEndTime == null) return;
                if (HighLogic.CurrentGame.UniversalTime >= CurrentCycleEndTime)
                {
                    print("WhitecatIndustries SCS - Solar Cycle Complete");
                    EndCurrentCycle(Cycle);
                    StartNewCycle();
                }
                else
                {
                    float BasicCycleEquation = Mathf.Cos(((Mathf.PI * 2) / (float)(Duration) * (float)((HighLogic.CurrentGame.UniversalTime + 1.0) - CurrentCycleStartTime)));
                    double MaxF107 = double.Parse(Cycle.GetValue("MaxF107"));
                    double MinF107 = double.Parse(Cycle.GetValue("MinF107"));
                    double MinAp = double.Parse(Cycle.GetValue("MinAp"));
                    double MaxAp = double.Parse(Cycle.GetValue("MaxAp"));
                    double MinIPMF = double.Parse(Cycle.GetValue("MinIPMF"));
                    double MaxIPMF = double.Parse(Cycle.GetValue("MaxIPMF"));
                    double MinIrradiance = double.Parse(Cycle.GetValue("MinIrradiance"));
                    double MaxIrradiance = double.Parse(Cycle.GetValue("MaxIrradiance"));
                    double MinSunSpots = double.Parse(Cycle.GetValue("MinSunSpots"));
                    double MaxSunSpots = double.Parse(Cycle.GetValue("MaxSunSpots"));

                    double CurrentF107 = (((MaxF107 - MinF107) / 2) + MinF107) + (((MaxF107 - MinF107) / 2) * BasicCycleEquation);
                    double CurrentAp = (((MaxAp - MinAp) / 2) + MinAp) + (((MaxAp - MinAp) / 2) * BasicCycleEquation);
                    double CurrentIPMF = (((MaxIPMF - MinIPMF) / 2) + MinIPMF) + (((MaxIPMF - MinIPMF) / 2) * BasicCycleEquation);
                    double CurrentIrradiance = (((MaxIrradiance - MinIrradiance) / 2) + MinIrradiance) + (((MaxIrradiance - MinIrradiance) / 2) * BasicCycleEquation);
                    double CurrentSunSpots = (((MaxSunSpots - MinSunSpots) / 2) + MinSunSpots) + (((MaxSunSpots - MinSunSpots) / 2) * BasicCycleEquation);

                    Cycle.SetValue("BasicCycleEquation", BasicCycleEquation.ToString());
                    Cycle.SetValue("PercentThrough", PercentThrough.ToString());
                    Cycle.SetValue("CurrentF107", CurrentF107.ToString());
                    Cycle.SetValue("CurrentAp", CurrentAp.ToString());
                    Cycle.SetValue("CurrentIPMF", CurrentIPMF.ToString());
                    Cycle.SetValue("CurrentIrradiance", CurrentIrradiance.ToString());
                    Cycle.SetValue("CurrentSunSpots", CurrentSunSpots.ToString());

                    SolarCycles.AddNode(Cycle);
                    SolarCycles.RemoveNode(ListNode);
                }
            }
        }

        public static bool CheckPersistence(ConfigNode node)
        {
            return node.GetValue("Persistence") == HighLogic.SaveFolder;
        }

        public void EndCurrentCycle(ConfigNode Cycle)
        {
            foreach (ConfigNode node in SolarCycles.GetNodes("CYCLE"))
            {
                if (node.GetValue("Id") != Cycle.GetValue("Id") || CheckPersistence(node) != true) continue;
                node.SetValue("CurrentCycle", false.ToString());
                node.SetValue("PercentThrough", "100.0");
            }
            print("WhitecatIndustries SCS - Ending solar cycle");
        }

        public static ConfigNode FetchCurrentCycle()
        {
            ConfigNode Cycle = new ConfigNode();
            bool found = false;

            foreach (ConfigNode node in SolarCycles.GetNodes("CYCLE"))
            {
                if (!bool.Parse(node.GetValue("CurrentCycle")) || !CheckPersistence(node)) continue;
                Cycle = node;
                found = true;
                break;
            }

            // If the Cycle does not exist
            if (!found)
            {
                StartNewCycle();

                foreach (ConfigNode node in SolarCycles.GetNodes("CYCLE"))
                {
                    if (bool.Parse(node.GetValue("CurrentCycle")) && CheckPersistence(node))
                    {
                        Cycle = node;
                    }
                }
            }

            return Cycle;
        }

        public static void StartNewCycle()
        {

            string Name = "Solar Cycle- " + (CycleCount + 1); 
            double Id = CycleCount + 1 ;
            bool CurrentCycle = true;
            double Duration = Random.Range((float)(9.2 * SecondsInYear), (float)(12.9 * SecondsInYear));
            double CurrentCycleStartTime = HighLogic.CurrentGame.UniversalTime;
            double MaximumTime = (CurrentCycleStartTime + (Duration / 2));
            double EndTime = CurrentCycleStartTime + Duration;
            float BasicCycleEquation = Mathf.Cos(((Mathf.PI * 2) / (float)(Duration) * (float)((HighLogic.CurrentGame.UniversalTime + 1.0) - CurrentCycleStartTime)));
            double MaxF107 = Random.Range(180f, 240f);
            double MinF107 = Random.Range(40f, 80f);
            double MaxAp = Random.Range(20f, 30f);
            double MinAp = Random.Range(3f, 7f);
            double MaxIPMF = Random.Range(9f, 11f);
            double MinIPMF = Random.Range(5f, 7f);
            double MaxIrradiance = Random.Range(1366.25f, 1366.5f);
            double MinIrradiance = Random.Range(1365.5f, 1365.75f);
            double MaxSunSpots = Random.Range(300f, 150f);
            double MinSunSpots = Random.Range(1f, 25f);

            double PercentThrough = (((HighLogic.CurrentGame.UniversalTime + 1.0) - CurrentCycleStartTime) / Duration) * 100; // Maybe Needs work 

            double CurrentF107 = (((MaxF107 - MinF107) / 2) + MinF107) + (((MaxF107 - MinF107) / 2) * BasicCycleEquation);
            double CurrentAp = (((MaxAp - MinAp) / 2) + MinAp) + (((MaxAp - MinAp) / 2) * BasicCycleEquation);
            double CurrentIPMF = (((MaxIPMF - MinIPMF) / 2) + MinIPMF) + (((MaxIPMF - MinIPMF) / 2) * BasicCycleEquation);
            double CurrentIrradiance = (((MaxIrradiance - MinIrradiance) / 2) + MinIrradiance) + (((MaxIrradiance - MinIrradiance) / 2) * BasicCycleEquation);
            double CurrentSunSpots = (((MaxSunSpots - MinSunSpots) / 2) + MinSunSpots) + (((MaxSunSpots - MinSunSpots) / 2) * BasicCycleEquation);

            ConfigNode Cycle = new ConfigNode("CYCLE");
            Cycle.AddValue("Persistence", HighLogic.SaveFolder.ToString());
            Cycle.AddValue("Name", Name);
            Cycle.AddValue("Id", Id.ToString());
            Cycle.AddValue("CurrentCycle", CurrentCycle);
            Cycle.AddValue("Duration", Duration);
            Cycle.AddValue("StartTime", CurrentCycleStartTime);
            Cycle.AddValue("MaximumTime", MaximumTime);
            Cycle.AddValue("EndTime", EndTime);
            Cycle.AddValue("BasicCycleEquation", BasicCycleEquation);
            Cycle.AddValue("MaxF107", MaxF107);
            Cycle.AddValue("MinF107", MinF107);
            Cycle.AddValue("MinAp", MinAp);
            Cycle.AddValue("MaxAp", MaxAp);
            Cycle.AddValue("MinIPMF", MinIPMF);
            Cycle.AddValue("MaxIPMF", MaxIPMF);
            Cycle.AddValue("MinIrradiance", MinIrradiance);
            Cycle.AddValue("MaxIrradiance", MaxIrradiance);
            Cycle.AddValue("MinSunSpots", MinSunSpots);
            Cycle.AddValue("MaxSunSpots", MaxSunSpots);
            Cycle.AddValue("PercentThrough", PercentThrough);
            Cycle.AddValue("CurrentF107", CurrentF107);
            Cycle.AddValue("CurrentAp", CurrentAp);
            Cycle.AddValue("CurrentIPMF", CurrentIPMF);
            Cycle.AddValue("CurrentIrradiance", CurrentIrradiance);
            Cycle.AddValue("CurrentSunSpots", CurrentSunSpots);

            print("WhitecatIndustries SCS - Starting next solar cycle");

            CycleCount = CycleCount + 1;

            SolarCycles.AddNode(Cycle);
        }

        public static double FetchCurrentF107()
        {
                string Temp = FetchCurrentCycle().GetValue("CurrentF107");
                double F107 = double.Parse(Temp);
                return F107;
        }

        public static double FetchCurrentAp()
        {
                double Ap = double.Parse(FetchCurrentCycle().GetValue("CurrentAp"));
                return Ap;
        }

        public static double FetchAverageF107()
        {
            double F107 = 0.0;
            double F1071 = double.Parse(FetchCurrentCycle().GetValue("MaxF107"));
            double F1072 = double.Parse(FetchCurrentCycle().GetValue("MinF107"));
            F107 = (F1071 + F1072) / 2;
            return F107;
        }

        public static double FetchAverageAp()
        {
            double Ap = 0.0;
            double Ap1 = double.Parse(FetchCurrentCycle().GetValue("MaxAp"));
            double Ap2 = double.Parse(FetchCurrentCycle().GetValue("MinAp"));
            Ap = (Ap1 + Ap2) / 2;
            return Ap;
        }

        public static double FetchCurrentIPMF()
        {
            double IPMF = double.Parse(FetchCurrentCycle().GetValue("CurrentIPMF"));
            return IPMF;
        }

        public static double FetchCurrentIrradiance()
        {
            double Irradiance = double.Parse(FetchCurrentCycle().GetValue("CurrentIrradiance"));
            return Irradiance;
        }

        public static double FetchCurrentSunSpots()
        {
            double SunSpots = double.Parse(FetchCurrentCycle().GetValue("CurrentSunSpots"));
            return SunSpots;
        }
    }
}
