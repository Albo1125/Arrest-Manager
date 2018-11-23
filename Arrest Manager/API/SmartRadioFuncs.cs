using Albo1125.Common.CommonLibrary;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arrest_Manager.API
{
    internal static class SmartRadioFuncs
    {
        /// <summary>
        /// Adds an Action and an availability check to the specified button. Only buttons contained in a folder matching your plugin's name can be manipulated.
        /// </summary>
        /// <param name="action">The action to execute if the button is selected.</param>
        /// <param name="buttonName">The texture file name of the button, excluding any directories or file extensions.</param>
        /// <returns></returns>
        public static bool AddActionToButton(Action action, string buttonName)
        {
            return PoliceSmartRadio.API.Functions.AddActionToButton(action, buttonName);
        }

        /// <summary>
        /// Adds an Action and an availability check to the specified button. Only buttons contained in a folder matching your plugin's name can be manipulated.
        /// </summary>
        /// <param name="action">The action to execute if the button is selected.</param>
        /// <param name="isAvailable">Function returning a bool indicating whether the button is currently available (if false, button is hidden). This is often called, so try making this light-weight (e.g. simply return the value of a boolean property). Make sure to do proper checking in your Action too, as the user can forcefully display all buttons via a setting in their config file.</param>
        /// <param name="buttonName">The texture file name of the button, excluding any directories or file extensions.</param>
        /// <returns></returns>
        public static bool AddActionToButton(Action action, Func<bool> isAvailable, string buttonName)
        {
            return PoliceSmartRadio.API.Functions.AddActionToButton(action, isAvailable, buttonName);
        }

        /// <summary>
        /// Raised whenever the player selects a button on the SmartRadio.
        /// </summary>
        /// <param name="handler"></param>
        public static void AddButtonSelectedHandler(Action handler)
        {
            PoliceSmartRadio.API.Functions.ButtonSelected += handler;
        }


        public static void RequestTransport()
        {
            if (EntryPoint.canChoose && EntryPoint.suspectAPI != null && EntryPoint.suspectsArrestedByPlayer.Contains(EntryPoint.suspectAPI) && !EntryPoint.suspectsPendingTransport.Contains(EntryPoint.suspectAPI))
            {
                if (EntryPoint.twoSuspectsApi.Count == 2 && !ExtensionMethods.IsPointOnWater(Game.LocalPlayer.Character.Position))
                {

                    if (EntryPoint.twoSuspectsApi[0].Exists() && EntryPoint.twoSuspectsApi[1].Exists() && EntryPoint.suspectsArrestedByPlayer.Contains(EntryPoint.twoSuspectsApi[0]) &&
                        EntryPoint.suspectsArrestedByPlayer.Contains(EntryPoint.twoSuspectsApi[1]) && !EntryPoint.suspectsPendingTransport.Contains(EntryPoint.twoSuspectsApi[0]) &&
                            !EntryPoint.suspectsPendingTransport.Contains(EntryPoint.twoSuspectsApi[1]))
                    {

                        if (Vector3.Distance(EntryPoint.twoSuspectsApi[0].Position, EntryPoint.twoSuspectsApi[1].Position) < 25f)
                        {
                            Game.LogTrivial("API detected multi transport - calling");
                            EntryPoint.TransportMenu.Visible = true;
                            EntryPoint.TransportMenu.CurrentSelection = 2;
                            return;

                        }
                    }
                }

                EntryPoint.canChoose = false;
                Game.LogTrivial("API detected single transport - calling");
                Game.RemoveNotification(EntryPoint.multiVanOnStandbyMsg);
                Game.RemoveNotification(EntryPoint.vanOnStandbyMsg);
                Ped officer;
                Vehicle van;
                if (EntryPoint.RecruitNearbyOfficer(out officer, out van))
                {
                    Game.LogTrivial("Recruited Officer");
                    EntryPoint.SuspectTransporterRecruitedOfficer(officer, van, false);
                }
                else if (ExtensionMethods.IsPointOnWater(Game.LocalPlayer.Character.Position))
                {
                    Game.LogTrivial("API detected boat transport.");
                    EntryPoint.BoatTransport(anims:false);
                }
                else
                {
                    EntryPoint.SuspectTransporterNewOfficer(anims:false);
                }
            }


        }
    }
}
