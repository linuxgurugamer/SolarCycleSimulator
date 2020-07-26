﻿using UnityEngine;
using ToolbarControl_NS;

namespace WhitecatIndustries
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(SCSWindow.MODID, SCSWindow.MODNAME);
        }
    }
}