﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class TankFailureModule : BaseFailureModule
    {
        PartResource leaking;
        [KSPField(isPersistant = true, guiActive = false)]
        public string leakingName = "None";
        private string savedFile;
        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Resource Tank Failure";
        }
        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().TankFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            savedFile = KSPUtil.ApplicationRootPath + "/GameData/UntitledFailures/MM Patches/DontLeak.cfg";
            if (leaking == null)
            {
                if (leakingName != "None")
                {
                    leaking = part.Resources[leakingName];
                    failureType = leaking.resourceName +" leak";
                    return;
                }
                List<PartResource> potentialLeakCache = part.Resources.ToList();
                List<PartResource> potentialLeaks = part.Resources.ToList();
                if (potentialLeaks.Count == 0) return;
                ConfigNode cn = ConfigNode.Load(savedFile);
                if (cn != null)
                {
                    ConfigNode[] blackListNode = cn.GetNodes("BLACKLISTED");
                    if (blackListNode.Count() > 0)
                    {
                        for (int i = 0; i < blackListNode.Count(); i++)
                        {
                            ConfigNode node = blackListNode.ElementAt(i);
                            for (int p = 0; p < potentialLeakCache.Count(); p++)
                            {
                                PartResource pr = potentialLeakCache.ElementAt(p);
                                if (pr.resourceName == node.GetValue("name")) potentialLeaks.Remove(pr);
                            }
                        }
                        if (potentialLeaks.Count == 0)
                        {
                            leaking = null;
                            leakingName = "None";
                            hasFailed = false;
                            willFail = false;
                            postMessage = false;
                            Debug.Log("[UPFM]: "+SYP.ID + "has no resources that could fail. Failure aborted");
                            return;
                        }
                    }
                }
                leaking = potentialLeaks.ElementAt(Randomiser.instance.RandomInteger(0, potentialLeaks.Count()));
                leakingName = leaking.resourceName;
                Debug.Log("[UPFM]: " + leaking.resourceName + " started leaking from " + SYP.ID);
                ScreenMessages.PostScreenMessage("A tank of " + leaking.resourceName + " started to leak!");
                failureType = leaking.resourceName+" leak";
            }
            leaking.amount = leaking.amount * 0.999f;
            if(UPFM.highlight)UPFM.SetFailedHighlight();
        }
    }
}
    
 

