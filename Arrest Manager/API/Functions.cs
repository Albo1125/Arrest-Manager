using Albo1125.Common.CommonLibrary;
using Rage;

namespace Arrest_Manager.API
{
    public delegate void PedEvent(Ped ped);
    public static class Functions
    {

        /// <summary>
        /// Request transport for the specified suspect. Returns a bool indicating whether requesting transport was successful.
        /// </summary>
        /// <param name="suspect">The ped to be transported. Does not necessarily have to be arrested.</param>
        /// <returns>Returns a bool indicating whether requesting transport was successful.</returns>
        public static bool RequestTransport(Ped suspect)
        {
            Game.LogTrivial("Requesting transport for specific suspect - API");
            if (!EntryPoint.suspectsPendingTransport.Contains(suspect) && suspect)
            {
                EntryPoint.canChoose = false;
                EntryPoint.SuspectTransporterNewOfficer(suspect);
                return true;
            }
            Game.LogTrivial("Suspect already pending transport or does not exist. Aborting. API");
            return false;
        }

        /// <summary>
        /// Request transport for the nearest suspect that has transport on standby. If multiple suspects are available, requests multi transport automatically. Returns a bool indicating whether requesting transport was successful.
        /// </summary>
        /// <returns>Returns a bool indicating whether requesting transport was successful.</returns>
        public static bool RequestTransport()
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
                            return true;

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
                    EntryPoint.SuspectTransporterRecruitedOfficer(officer, van);
                }
                else if (ExtensionMethods.IsPointOnWater(Game.LocalPlayer.Character.Position))
                {
                    Game.LogTrivial("API detected boat transport.");
                    EntryPoint.BoatTransport();
                }
                else
                {
                    EntryPoint.SuspectTransporterNewOfficer();
                }
                return true;
            }
            else
            {
                return false;
            }

            
        }

        /// <summary>
        /// Requests transport for the nearest ped that has transport on standby. Returns a bool indicating whether requesting transport was successful.
        /// </summary>
        /// <param name="Cop">Cop to drive the pickup vehicle.</param>
        /// <param name="PoliceTransportVehicle">Pickup vehicle to be driven by the cop.</param>
        /// <returns>Returns a bool indicating whether requesting transport was successful.</returns>
        public static bool RequestTransport(Ped Cop, Vehicle PoliceTransportVehicle)
        {
            if (Cop.Exists() && PoliceTransportVehicle.Exists() && EntryPoint.canChoose && EntryPoint.suspectAPI != null && EntryPoint.suspectsArrestedByPlayer.Contains(EntryPoint.suspectAPI) && !EntryPoint.suspectsPendingTransport.Contains(EntryPoint.suspectAPI))
            {
                if (PoliceTransportVehicle.IsPoliceVehicle && PoliceTransportVehicle.FreePassengerSeatsCount > 1 && PoliceTransportVehicle.GetDoors().Length > 4)
                {
                    EntryPoint.canChoose = false;
                    Game.RemoveNotification(EntryPoint.multiVanOnStandbyMsg);
                    Game.RemoveNotification(EntryPoint.vanOnStandbyMsg);
                    Game.LogTrivial("Arrest Manager API requesting custom officer/vehicle.");
                    EntryPoint.SuspectTransporterRecruitedOfficer(Cop, PoliceTransportVehicle);
                    return true;
                }
                else
                {

                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Dispatches a tow truck for the target vehicle.
        /// </summary>
        /// <param name="VehicleToTow">Must not have occupants and be a valid model that can be towed (no planes etc.).</param>
        /// <param name="PlayAnims">Determines whether the player performs the radio animation or not.</param>
        public static void RequestTowTruck(Vehicle VehicleToTow, bool PlayAnims = true)
        {
            new VehicleManager().towVehicle(VehicleToTow, PlayAnims);
        }

        /// <summary>
        /// Dispatches a tow truck for the nearest valid vehicle.
        /// </summary>
        /// <param name="PlayAnims">Determines whether the player performs the radio animation or not.</param>
        public static void RequestTowTruck(bool PlayAnims = true)
        {
            new VehicleManager().towVehicle(PlayAnims);
        }


        /// <summary>
        /// Requests insurance company pickup for the nearest valid vehicle.
        /// </summary>
        public static void RequestInsurancePickupForNearbyVehicle()
        {
            new VehicleManager().insurancePickUp();
        }

        

        /// <summary>
        /// Raised whenever the player arrests a ped. Only raised once per arrested ped.
        /// </summary>
        public static event PedEvent PlayerArrestedPed;

        internal static void OnPlayerArrestedPed(Ped ped)
        {
           
            if (PlayerArrestedPed != null)
            {
                PlayerArrestedPed(ped);
            }
        }

        /// <summary>
        /// Raised whenever the player grabs a ped.
        /// </summary>
        public static event PedEvent PlayerGrabbedPed;

        internal static void OnPlayerGrabbedPed(Ped ped)
        {
            if (PlayerGrabbedPed != null)
            {
                PlayerGrabbedPed(ped);
            }
        }

        /// <summary>
        /// Returns a boolean indicating if the specified ped is grabbed or not.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        public static bool IsPedGrabbed(Ped ped)
        {
            if (PedManager.EnableGrab)
            {
                return ped.Equals(PedManager.pedfollowing);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a boolean indicating if any ped is grabbed or not.
        /// </summary>
        /// <returns></returns>
        public static bool IsPedGrabbed()
        {
            return PedManager.EnableGrab;
        }

        /// <summary>
        /// If a ped is currently grabbed, releases it.
        /// </summary>
        public static void ReleaseGrabbedPed()
        {
            PedManager.EnableGrab = false;
        }

        /// <summary>
        /// Arrests the ped as would happen using the Ped Management menu. Must use Grab feature to move the ped around and place in vehicle.
        /// </summary>
        /// <param name="suspect">The ped to be arrested.</param>
        public static void ArrestPed(Ped suspect)
        {
            if (suspect)
            {
                Game.LogTrivial("Arrest Manager API arresting suspect.");
                PedManager.ArrestPed(suspect);
            }
        }

        /// <summary>
        /// Calls a coroner to the player's location if there are dead bodies in the vicinity.
        /// </summary>
        /// <param name="radioAnimation">Determines whether to play a radio animation for the player.</param>
        public static void CallCoroner(bool radioAnimation)
        {
            if (radioAnimation)
            {
                Coroner.Main();
            }
            else
            {
                Coroner.smartRadioMain();
            }
        }

        /// <summary>
        /// Calls a coroner to the specified location even if there are no dead bodies in the vicinity (yet).
        /// </summary>
        /// <param name="destination">The destination for the coroners.</param>
        /// <param name="radioAnimation">Determines whether to play a radio animation for the player.</param>
        public static void CallCoroner(Vector3 destination, bool radioAnimation)
        {
            new Coroner(destination, radioAnimation).handleCoroner();
        }

    }
}
