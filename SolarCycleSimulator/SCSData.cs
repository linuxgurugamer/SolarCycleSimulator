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
    public class SCSData : MonoBehaviour
    {
        public static string FilePath;

        public static void SaveInfo()
        {
            FilePath = KSPUtil.ApplicationRootPath + "GameData/WhitecatIndustries/SolarCycleSimulator/PluginData/Data.cfg";
            ConfigNode NFile = new ConfigNode();
            ConfigNode Original = ConfigNode.Load(FilePath);

            if (SCSManager.SolarCycles.nodes.Count != 0)
            {
                foreach (ConfigNode node in SCSManager.SolarCycles.GetNodes("CYCLE"))
                {
                    NFile.AddNode(node);
                }
            }

            if (Original.nodes.Count != 0)
            {
                foreach (ConfigNode node in Original.GetNodes("CYCLE"))
                {
                    if (SCSManager.CheckPersistence(node) == false)
                    {
                        NFile.AddNode(node);
                    }
                }
            }

            Original.ClearNodes();
            NFile.Save(FilePath);
            SCSManager.SolarCycles.ClearData();
        }

        public static void LoadInfo()
        {
            FilePath = KSPUtil.ApplicationRootPath + "GameData/WhitecatIndustries/SolarCycleSimulator/PluginData/Data.cfg";
            ConfigNode Original = ConfigNode.Load(FilePath);

                if (Original.CountNodes > 0)
                {
                    foreach (ConfigNode node in Original.nodes)
                    {
                        if (node.GetValue("Persistence") == HighLogic.SaveFolder.ToString())
                        {
                            SCSManager.SolarCycles.AddNode(node);
                      
                              if (node.GetValue("Id") != "0")
                             {
                                SCSManager.CycleCount = SCSManager.CycleCount + 1;
                             }
                        }
                    }
                }
            }
        }
    }
