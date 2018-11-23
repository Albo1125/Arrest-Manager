using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterEMS.API;

namespace Arrest_Manager.API
{
    internal static class BetterEMSFuncs
    {
        public static void RequestTransportToHospitalForNearestValidPed(Ped ped)
        {
            if (ped.Exists())
            {
                if (!EntryPoint.suspectsPendingTransport.Contains(ped))
                {
                    EntryPoint.suspectsPendingTransport.Add(ped);

                    EMSFunctions.PickUpPatient(ped);                    
                       
                }
                else
                {
                    Game.LogTrivial("Already pending police transport.");
                }

            }
        }

        public static uint GetOriginalDeathWeaponAssetHash(Ped p)
        {
            if (p && p.IsDead)
            {
                return EMSFunctions.GetOriginalDeathWeaponAsset(p).Hash;
            }
            else
            {
                return 0;
            }
            
        }

        public static bool HasBeenTreated(Ped p)
        {
            return EMSFunctions.DidEMSRevivePed(p) != null;
        }
    }
}
