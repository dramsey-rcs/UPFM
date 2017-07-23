﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class EngineFailureModule : BaseFailureModule
    {
        ModuleEngines engine;
        ModuleEnginesFX engineFX;
        double timeBetweenFailureEvents = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        float staticThrust;
        ModuleGimbal gimbal;

        protected override void Overrides()
        {
            maxTimeToFailure = 120;
            Fields["displayChance"].guiName = "Chance of Engine Failure";
            postMessage = false;
            engine = part.FindModuleImplementing<ModuleEngines>();
            if(engine.maxThrust < 100.0f)
            {
                expectedLifetime = 25;
                baseChanceOfFailure = 0.05f;
            }
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().EngineFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            if (part.Resources.Contains("SolidFuel")) return;
            engine = part.FindModuleImplementing<ModuleEngines>();
            if (engine == null) engineFX = part.FindModuleImplementing<ModuleEnginesFX>();
            gimbal = part.FindModuleImplementing<ModuleGimbal>();
            if(engine != null)
            {
                if (engine.currentThrottle == 0) return;
            }
            else
            {
                if (engineFX.currentThrottle == 0) return;
            }
            if(UPFM.highlight) UPFM.SetFailedHighlight();
            if (failureType == "none")
            {
                int i = Randomiser.instance.RandomInteger(1, 5);
                switch (i)
                {
                    case 1:
                        failureType = "Fuel Flow Failure";
                        Debug.Log("[UPFM]: attempted to perform Fuel Flow Failure on " + SYP.ID);
                        break;
                    case 2:
                        failureType = "Fuel Line Leak";
                        Debug.Log("[UPFM]: attempted to perform Fuel Line Leak on " + SYP.ID);
                        break;
                    case 3:
                        failureType = "Underthrust";
                        Debug.Log("[UPFM]: attempted to perform Underthrust on " + SYP.ID);
                        break;
                    case 4:
                        if (gimbal == null) return;
                        failureType = "Gimbal Failure";
                        Debug.Log("[UPFM]: attempted to lock gimbal on" + SYP.ID);
                        break;
                    default:
                        failureType = "none";
                        Debug.Log("[UPFM]: "+SYP.ID+" decided not to fail after all");
                        break;
                }
                ScreenMessages.PostScreenMessage(failureType + " detected on " + part.name);
                postMessage = true;
                return;
            }
            switch (failureType)
            {
                case "Fuel Flow Failure":
                    engine.Shutdown();
                    break;
                case "Fuel Line Leak":
                    part.explode();
                    break;
                case "Underthrust":
                    if (timeBetweenFailureEvents <= Planetarium.GetUniversalTime())
                    {
                        if (engine != null) engine.thrustPercentage = engine.thrustPercentage * 0.9f;
                        else engineFX.thrustPercentage = engine.thrustPercentage * 0.9f;
                        timeBetweenFailureEvents = Planetarium.GetUniversalTime() + Randomiser.instance.RandomInteger(10,30);
                        if (engine != null) staticThrust = engine.thrustPercentage;
                        else staticThrust = engineFX.thrustPercentage;
                    }
                    if (engine != null) engine.thrustPercentage = staticThrust;
                    else engineFX.thrustPercentage = staticThrust;
                    break;
                case "Gimbal Failure":
                    gimbal.gimbalLock = true;
                    break;
                default:
                    return;
            }
        }

        public override void RepairPart()
        {
            engine = part.FindModuleImplementing<ModuleEngines>();
            if (engine == null) engineFX = part.FindModuleImplementing<ModuleEnginesFX>();
            switch (failureType)
            {
                case "Fuel Flow Failure":
                    if (engine != null) engine.Activate();
                    else engineFX.Activate();
                    Debug.Log("[UPFM]: Re-activated " + SYP.ID);
                    break;
                case "Underthrust":
                    if (engine != null) engine.thrustPercentage = 100;
                    else engineFX.thrustPercentage = 100;
                    Debug.Log("[UPFM]: Reset Thrust on " + SYP.ID);
                    break;
                case "Gimbal Failure":
                    gimbal.gimbalLock = false;
                    break;

                default:
                    return;
            }
        }
    }
}
