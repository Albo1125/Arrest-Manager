using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using Rage.Native;
using System.IO;
using LSPD_First_Response;
using LSPD_First_Response.Mod.API;

using System.Windows.Forms;
using Arrest_Manager;
using System.Reflection;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Albo1125.Common.CommonLibrary;
using System.Diagnostics;
using System.Threading;
using System.Management;
using System.Net;

namespace Arrest_Manager
{
    internal class EntryPoint
    {

        //PROPERTIES FOR CHOICE & KEYS
        private static bool messageReceived { get; set; }
        private static bool transportMessageReceived { get; set; }
        private static bool jailRouteMessageReceived { get; set; }
        internal static bool canChoose { get; set; }
        private static bool checkForJail { get; set; }


        private static Keys transportKey { get; set; }
        private static Keys jailRouteKey { get; set; }
        //private static Keys multiTransportKey { get; set; }
        private static Keys releaseKey { get; set; }
        private static Keys transportModifierKey { get; set; }
        private static Keys jailRouteModifierKey { get; set; }
        //private static Keys multiTransportModifierKey { get; set; }
        private static Keys releaseModifierKey { get; set; }





        private static bool autoDoorEnabled { get; set; }
        public static Keys SceneManagementKey { get; set; }
        public static Keys SceneManagementModifierKey { get; set; }
        public static float SceneManagementSpawnDistance { get; set; }

        //INI

        public static InitializationFile initialiseFile()
        {
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/Arrest Manager.ini");
            ini.Create();
            return ini;
        }

        public static string getWarpKey()
        {
            InitializationFile ini = initialiseFile();
            string warpKey = ini.ReadString("Keybindings", "Warpkey", "D9");
            return warpKey;
        }

        public static string getTransportKey()
        {
            InitializationFile ini = initialiseFile();
            string transportKey = ini.ReadString("Keybindings", "Transportkey", "D8");
            return transportKey;
        }

        public static string getJailRouteKey()
        {
            InitializationFile ini = initialiseFile();
            string jailRouteKey = ini.ReadString("Keybindings", "JailRouteKey", "D0");
            return jailRouteKey;
        }

        public static string getMultiTransportKey()
        {
            InitializationFile ini = initialiseFile();
            string getMultiTransportKey = ini.ReadString("Keybindings", "MultiTransportKey", "D7");
            return getMultiTransportKey;
        }

        private static string getReleaseKey()
        {
            InitializationFile ini = initialiseFile();
            string releasekey = ini.ReadString("Keybindings", "ReleaseKey", "D6");
            return releasekey;
        }

        public static string getWarpModifierKey()
        {
            InitializationFile ini = initialiseFile();
            string getModifierKey = ini.ReadString("Keybindings", "WarpModifierKey", "None");
            return getModifierKey;
        }
        private static string getTransportModifierKey()
        {
            InitializationFile ini = initialiseFile();
            string getModifierKey = ini.ReadString("Keybindings", "TransportModifierKey", "None");
            return getModifierKey;
        }
        private static string getJailRouteModifierKey()
        {
            InitializationFile ini = initialiseFile();
            string getModifierKey = ini.ReadString("Keybindings", "JailRouteModifierKey", "None");
            return getModifierKey;
        }
        private static string getMultiTransportModifierKey()
        {
            InitializationFile ini = initialiseFile();
            string getModifierKey = ini.ReadString("Keybindings", "MultiTransportModifierKey", "None");
            return getModifierKey;
        }
        private static string getReleaseModifierKey()
        {
            InitializationFile ini = initialiseFile();
            string getModifierKey = ini.ReadString("Keybindings", "ReleaseModifierKey", "None");
            return getModifierKey;
        }

        private static float getTransportSpawnDistance()
        {
            InitializationFile ini = initialiseFile();
            transportSpawnDistance = ini.ReadSingle("Misc", "TransportSpawnDistance", 85f);
            if (transportSpawnDistance < 50f)
            {
                transportSpawnDistance = 50f;
            }
            else if (transportSpawnDistance > 250f)
            {
                transportSpawnDistance = 250f;
            }
            return transportSpawnDistance;
        }
        private static string getAutoDoorEnabled()
        {
            InitializationFile ini = initialiseFile();
            string enabled = ini.ReadString("General", "AutoDoorShutEnabled", "true");
            return enabled;
        }
        private static float getSceneManagementSpawnDistance()
        {
            InitializationFile ini = initialiseFile();
            SceneManagementSpawnDistance = ini.ReadSingle("Misc", "SceneManagementSpawnDistance", 70f);
            if (SceneManagementSpawnDistance < 50f)
            {
                SceneManagementSpawnDistance = 50f;
            }
            else if (SceneManagementSpawnDistance > 250f)
            {
                SceneManagementSpawnDistance = 250f;
            }
            return SceneManagementSpawnDistance;
        }




        //Obsolete


        public static float getX()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vector3 currentPosition = playerPed.Position;
            return currentPosition.X;
        }
        public static float getY()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vector3 currentPosition = playerPed.Position;
            return currentPosition.Y;
        }
        public static float getZ()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vector3 currentPosition = playerPed.Position;
            return currentPosition.Z;
        }
        public static float getHeading()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            return playerPed.Heading;
        }



        //Ped info


        public static List<Ped> suspectsPendingTransport { get; set; }
        private static Ped getSuspectAPI()
        {

            //Loops through the 10 nearest peds. If one of them is arrested, returns it, otherwise returns null.
            Ped playerPed = Game.LocalPlayer.Character;
            Ped suspect = null;
            if (!playerPed.Exists()) { return suspect; }

            for (int i = 0; i < playerPed.GetNearbyPeds(7).Length; i++)
            {
                try
                {
                    if (playerPed.GetNearbyPeds(7).Length == 0)
                    {
                        break;
                    }
                    if (playerPed.GetNearbyPeds(7)[i].IsValid() && playerPed.GetNearbyPeds(7)[i].Exists())
                    {
                        if (Functions.IsPedArrested(playerPed.GetNearbyPeds(7)[i]))
                        {
                            if (!suspectsPendingTransport.Contains(playerPed.GetNearbyPeds(7)[i]))
                            {
                                suspect = playerPed.GetNearbyPeds(7)[i];
                                break;
                            }

                        }
                    }

                    if (!playerPed.Exists()) { break; }

                }
                catch (ThreadAbortException) { }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Error handled by AM; did not crash because of AM.");
                }
            }

            return suspect;
        }
        private static List<Ped> getTwoSuspectAPI()
        {
            //Loops through the 10 nearest peds. If one of them is arrested, returns it, otherwise returns null.
            Ped playerPed = Game.LocalPlayer.Character;


            List<Ped> suspects = new List<Ped>();
            if (!playerPed.IsValid()) { return suspects; }
            for (int i = 0; i < playerPed.GetNearbyPeds(8).Length; i++)
            {
                try
                {
                    if (playerPed.GetNearbyPeds(8).Length == 0)
                    {
                        break;
                    }
                    if (playerPed.GetNearbyPeds(8)[i].IsValid() && playerPed.GetNearbyPeds(8)[i].Exists())
                    {
                        if (Functions.IsPedArrested(playerPed.GetNearbyPeds(8)[i]))
                        {
                            if (!suspectsPendingTransport.Contains(playerPed.GetNearbyPeds(8)[i]))
                            {
                                if (!suspects.Contains(playerPed.GetNearbyPeds(8)[i]))
                                {
                                    suspects.Add(playerPed.GetNearbyPeds(8)[i]);
                                    if (suspects.Count == 2)
                                    {
                                        break;
                                    }
                                }
                            }

                        }
                    }

                    if (!playerPed.IsValid()) { break; }
                }
                catch (Exception)
                {
                    return new List<Ped>();
                }
            }
            if (suspects.Count < 2)
            {
                suspects.Clear();
            }
            return suspects;
        }


        private static void releaseSuspectFromVehicle()
        {
            GameFiber.StartNew(delegate
            {
                bool leavewithvehicle = false;
                Vehicle suspectsOldVehicle = null;
                Ped playerPed = Game.LocalPlayer.Character;
                Vehicle car = playerPed.CurrentVehicle;
                Ped suspect = getSuspectFromVehicle();
                if (suspect == null)
                {

                    canChoose = true;
                    return;
                }
                if (suspectsPendingTransport.Contains(suspect)) { canChoose = true; return; }
                if (suspectsWithVehicles.ContainsKey(suspect))
                {

                    if (suspectsWithVehicles[suspect] != null)
                    {

                        if (suspectsWithVehicles[suspect].Exists())
                        {

                            if (Vector3.Distance(suspectsWithVehicles[suspect].Position, suspect.Position) < 25f)
                            {
                                suspectsWithVehicles[suspect].IsPersistent = true;
                                uint noti = Game.DisplayNotification("Allow the released suspect to leave in their vehicle? ~n~~h~~b~Y / N");
                                while (true)
                                {
                                    GameFiber.Yield();
                                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.Y))
                                    {
                                        Game.RemoveNotification(noti);
                                        leavewithvehicle = true;
                                        suspectsOldVehicle = suspectsWithVehicles[suspect];
                                        break;
                                    }
                                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.N))
                                    {
                                        Game.RemoveNotification(noti);
                                        suspectsWithVehicles[suspect].Dismiss();
                                        break;
                                    }

                                }
                            }

                        }
                    }
                }
                //suspectsWithVehicles = new Dictionary<Ped, Vehicle>();
                suspect = suspect.ClonePed(true);
                suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                Rage.Native.NativeFunction.Natives.SET_VEHICLE_DOORS_SHUT(car, false);
                if (OfficerAudio)
                {
                    suspect.PlayAmbientSpeech("GENERIC_THANKS");
                }
                if (leavewithvehicle)
                {
                    suspect.Tasks.FollowNavigationMeshToPosition(suspectsOldVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), suspectsOldVehicle.Heading, 2f).WaitForCompletion(9000);
                    suspect.Tasks.EnterVehicle(suspectsOldVehicle, 7000, -1).WaitForCompletion();
                    suspect.Dismiss();
                    suspectsOldVehicle.Dismiss();
                    canChoose = true;

                }
                else
                {
                    suspect.Dismiss();

                    canChoose = true;
                }
            });
        }
        

        private static Ped getSuspectFromVehicle()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Ped suspect = null;
            //Check if the player is driving a vehicle

            if (playerPed.CurrentVehicle == null || playerPed.IsPassenger)
            {

                return suspect;
            }
            Vehicle car = playerPed.CurrentVehicle;
            //Cycle through all passengers if there are any 
            if (car.HasPassengers == true)
            {
                for (int x = 0; x <= 4; x++)
                {
                    suspect = car.GetPedOnSeat(x);
                    if (suspect.Exists())
                    {
                        if (Functions.IsPedArrested(suspect))
                        {
                            if (!suspectsPendingTransport.Contains(suspect))
                            {
                                return suspect;
                            }
                        }
                    }

                }
            }
            return suspect;
        }


        //PRISONER TRANSPORT STUFF

        internal static Vector3 NearestDrivingNodePosition(Vector3 pos, int Nth, int nodeType = 1)
        {
            int node_id = GetNearestDrivingNodeID(pos, Nth, nodeType);
            //Game.LogTrivial("Node ID: " + node_id);
            if (IsNodeIDValid(node_id))
            {
                Vector3 out_position;

                NativeFunction.Natives.GET_VEHICLE_NODE_POSITION(node_id, out out_position);
                
                //Game.LogTrivial("SpawnPos from nodeID: " + out_position.ToString());
                return out_position;
            }
            else
            {
                //Game.LogTrivial("Invalid node ID: " + node_id);
                return Vector3.Zero;
            }
        }

        internal static int GetNearestDrivingNodeID(Vector3 pos, int Nth = 0, int nodeType = 1)
        {
            int node_id = NativeFunction.Natives.GET_NTH_CLOSEST_VEHICLE_NODE_ID<int>(pos.X, pos.Y, pos.Z, Nth, nodeType, 0x40400000, 100f);
            return node_id;
        }

        private static bool IsNodeIDValid(int nodeID)
        {
            return NativeFunction.Natives.IS_VEHICLE_NODE_ID_VALID<bool>(nodeID);
        }
        internal static Vector3 GetBoatSpawnPoint(Vector3 destination, float distance)
        {
            distance += (float)MathHelper.GetRandomDouble(0, 10);
            Vector3 spawnpoint = destination;
            int n = 0;
            while (spawnpoint.DistanceTo(destination) < distance - 5f)
            {
                spawnpoint = NearestDrivingNodePosition(destination, n, 3);
                n += 10;

                // if (GameFiber.CanSleepNow) GameFiber.Yield();
            }

            return spawnpoint;
        }
        internal static void BoatTransport(Ped suspect = null, bool anims = true)
        {
            GameFiber.StartNew(delegate
            {
                Guid CalloutID = Guid.NewGuid();
                bool CalloutConcluded = false;
                List<Entity> EntitiesUsedForTransport = new List<Entity>();
                try
                {


                    Ped playerPed = Game.LocalPlayer.Character;

                    //Get the suspect and check if he exists.
                    if (!suspect.Exists())
                    {
                        suspect = getSuspectAPI();
                        if (suspect == null)
                        {
                            canChoose = true;
                            return;
                        }
                    }
                    if (suspectsArrestedByPlayer.Contains(suspect))
                    {
                        suspectsArrestedByPlayer.Remove(suspect);
                    }
                    //Name to use in notifications
                    string suspectName = Functions.GetPersonaForPed(suspect).FullName;
                    suspect.BlockPermanentEvents = true;
                    suspect.IsInvincible = true;
                    suspect.IsPersistent = true;
                    suspect.Tasks.StandStill(-1);
                    EntitiesUsedForTransport.Add(suspect);
                    suspectsPendingTransport.Add(suspect);
                    //Spawn a cop and a transport van for the pickup and safeguard them
                    if (anims)
                    {
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                    }
                    GameFiber.Wait(1000);
                    if (DispatchVoice)
                    {
                        Functions.PlayScannerAudioUsingPosition("ASSISTANCE_REQUIRED FOR SUSPECT_UNDER_ARREST IN_OR_ON_POSITION OFFICER_INTRO UNIT_RESPONDING_DISPATCH_01 INTRO REPORT_RESPONSE_COPY_03", suspect.Position);
                    }
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")))
                    {
                        CalloutID = API.ComputerPlusFuncs.CreateCallout("Prisoner Transport Required", "Transport Required", suspect.Position, 0, "Requesting prisoner transport for " + suspectName + ". Please respond.", 2, new List<Ped>() { suspect }, null);
                        API.ComputerPlusFuncs.AssignCallToAIUnit(CalloutID);
                    }
                    if (EntryPoint.IsLSPDFRPlusRunning)
                    {
                        API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Prisoners picked up");
                    }
                    playerPed.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_UNARMED"), 0, true);
                    TransportRegion trnsreg = null;
                    string ZoneName = Zones.GetLowerZoneName(suspect.Position);

                    //Game.LogTrivial("WorldDistrict: " + Zones.GetWorldDistrict(suspect.Position).ToString());
                    List<TransportWorldDistrict> AvailableWorldDistricts = (from x in TransportWorldDistricts where x.WorldDistrict == Zones.GetWorldDistrict(suspect.Position) select x).ToList();
                    TransportWorldDistrict SelectedTransportWorldDistrict = AvailableWorldDistricts[EntryPoint.rnd.Next(AvailableWorldDistricts.Count)];

                    Ped cop = spawnTransportCop(true, trnsreg, SelectedTransportWorldDistrict, true);
                    Vehicle transportBoat = spawnTransportVehicle(cop, trnsreg, SelectedTransportWorldDistrict);

                    warpIntoVehicle(cop, transportBoat);

                    //Safeguards
                    if (!cop.Exists())
                    {
                        Game.LogTrivial("Spawning cop again");
                        cop = spawnTransportCop(true, trnsreg, SelectedTransportWorldDistrict, true);

                    }
                    if (!transportBoat.Exists() || !transportBoat.IsValid())
                    {
                        transportBoat = spawnTransportVehicle(cop, trnsreg, SelectedTransportWorldDistrict);
                    }
                    if (!cop.IsInVehicle(transportBoat, false))
                    {
                        cop.WarpIntoVehicle(transportBoat, -1); Game.Console.Print("Cop Warped");
                    }
                    EntitiesUsedForTransport.Add(cop);
                    EntitiesUsedForTransport.Add(transportBoat);
                    //add suspect to suspects pending transport so there isn't another van called for it

                    if (namesUsed.Contains(Functions.GetPersonaForPed(suspect).FullName))
                    {
                        namesUsed.Remove(Functions.GetPersonaForPed(suspect).FullName);
                    }
                    //playerPed.Tasks.Clear();

                    //radio in for assistance



                    //player can now radio for another suspect
                    canChoose = true;
                    transportMessageReceived = false;
                    multiTransportMessageReceived = false;
                    //create a blip
                    Blip copBlip = cop.AttachBlip();
                    copBlip.Color = System.Drawing.Color.DeepSkyBlue;
                    copBlip.Flash(1500, -1);
                    copBlip.IsFriendly = true;
                    //While the transport van isn't near the player, drive to the player
                    driveToSuspect(cop, transportBoat, suspect);
                    if (!suspect.Exists() || !suspect.IsValid())
                    {
                        Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");

                        foreach (Entity ent in EntitiesUsedForTransport)
                        {
                            if (ent.Exists()) { ent.Dismiss(); }
                        }


                        canChoose = true;
                        transportMessageReceived = false;
                        if (copBlip.Exists()) { copBlip.Delete(); }
                        return;
                    }

                    if (copBlip.Exists()) { copBlip.Delete(); }
                    //Game.DisplayNotification("Transport has arrived for ~r~" + suspectName + ".");

                    //Put suspect in the vehicle
                    if (transportBoat.GetFreePassengerSeatIndex() != null)
                    {
                        suspect.WarpIntoVehicle(transportBoat, transportBoat.GetFreePassengerSeatIndex().GetValueOrDefault());
                    }
                    else
                    {
                        suspect.Delete();
                    }

                    EntitiesUsedForTransport.Add(suspect);

                    Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");
                    if (!suspectsPendingTransport.Contains(suspect))
                    {
                        suspectsPendingTransport.Add(suspect);
                    }

                    //Dispose of the entities
                    if (!transportBoat.HasDriver)
                    {
                        cop.WarpIntoVehicle(transportBoat, -1);
                    }
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }
                    cop.Dismiss();
                    Functions.SetCopAsBusy(cop, false);
                    transportBoat.IsSirenOn = false;
                    transportBoat.Dismiss();
                    while (true)
                    {
                        GameFiber.Yield();
                        try
                        {
                            if (copBlip.Exists()) { copBlip.Delete(); }
                            if (suspect.Exists())
                            {
                                if (!transportBoat.Exists())
                                {
                                    suspect.Delete();
                                    break;
                                }
                                if (!suspect.IsDead)
                                {
                                    if (Vector3.Distance(Game.LocalPlayer.Character.Position, suspect.Position) > 120f)
                                    {
                                        suspect.Delete();
                                        break;
                                    }
                                    if (!suspect.IsInVehicle(transportBoat, false))
                                    {
                                        suspect.Delete();
                                        break;
                                    }
                                }
                                else
                                {
                                    suspect.Delete();
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }


                        }
                        catch (Exception e)
                        {
                            if (copBlip.Exists()) { copBlip.Delete(); }
                            if (suspect.Exists()) { suspect.Delete(); }
                            Game.LogTrivial(e.ToString());
                            break;
                        }
                    }


                }
                catch (ThreadAbortException)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }

                    canChoose = true;
                    transportMessageReceived = false;
                }
                catch (Exception e)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }

                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Arrest Manager handled the single new officer transport exception.");
                    canChoose = true;
                    transportMessageReceived = false;
                }
                finally
                {
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }
                }
            });
        }


        #region Prisoner Transport Stuff
        private static void GetTransportVanSpawnPoint(Vector3 StartPoint, out Vector3 SpawnPoint1, out float Heading1, bool UseSpecialID)
        {
            Vector3 tempspawn = World.GetNextPositionOnStreet(StartPoint.Around2D(transportSpawnDistance));
            Vector3 SpawnPoint = Vector3.Zero;
            float Heading = 0;

            if (!UseSpecialID || (!NativeFunction.Natives.GET_NTH_CLOSEST_VEHICLE_NODE_FAVOUR_DIRECTION<bool>(tempspawn.X, tempspawn.Y, tempspawn.Z, StartPoint.X, StartPoint.Y, StartPoint.Z, 0, out SpawnPoint, out Heading, 0, 0x40400000, 0) && ExtensionMethods.IsNodeSafe(SpawnPoint)))
            {
                SpawnPoint = World.GetNextPositionOnStreet(StartPoint.Around2D(transportSpawnDistance));
                Vector3 directionFromVehicleToPed1 = (StartPoint - SpawnPoint);
                directionFromVehicleToPed1.Normalize();

                Heading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);
            }

            
            SpawnPoint1 = SpawnPoint;
            Heading1 = Heading;
        }


        private static Ped spawnTransportCop(bool DriverCop, TransportRegion transportreg, TransportWorldDistrict SelectedWorldDistrict, bool boat = false)
        {

            Ped playerPed = Game.LocalPlayer.Character;
            //Vector3 tempspawn = World.GetNextPositionOnStreet(playerPed.Position.Around(transportSpawnDistance));
            Vector3 SpawnPoint;
            float Heading;
            bool UseSpecialID = true;
            GetTransportVanSpawnPoint(Game.LocalPlayer.Character.Position, out SpawnPoint, out Heading, UseSpecialID);

            float travelDistance;
            int waitCount = 0;

            while (DriverCop)
            {
                GameFiber.Yield();
                if (boat)
                {
                    SpawnPoint = GetBoatSpawnPoint(Game.LocalPlayer.Character.Position, transportSpawnDistance);
                    Heading = SpawnPoint.CalculateHeadingTowardsPosition(Game.LocalPlayer.Character.Position);
                    travelDistance = SpawnPoint.DistanceTo(Game.LocalPlayer.Character);
                }
                else
                {
                    GetTransportVanSpawnPoint(Game.LocalPlayer.Character.Position, out SpawnPoint, out Heading, UseSpecialID);
                    travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>(SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, playerPed.Position.X, playerPed.Position.Y, playerPed.Position.Z);

                }

                waitCount++;
                if (Vector3.Distance(playerPed.Position, SpawnPoint) > transportSpawnDistance - 15f)
                {

                    if (travelDistance < (transportSpawnDistance * 4.5f))
                    {
                        Vector3 directionFromVehicleToPed1 = (Game.LocalPlayer.Character.Position - SpawnPoint);
                        directionFromVehicleToPed1.Normalize();

                        float HeadingToPlayer = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);
                        
                        if (Math.Abs(MathHelper.NormalizeHeading(Heading) - MathHelper.NormalizeHeading(HeadingToPlayer)) < 150f)
                        {

                            break;
                        }
                    }
                }
                if (waitCount >= 400)
                {
                    UseSpecialID = false;
                }
                if (waitCount == 600)
                {
                    Game.DisplayNotification("Take your ~r~suspect ~s~to a more reachable location.");
                    Game.DisplayNotification("Alternatively, press ~b~Y ~s~to force a spawn in the ~g~wilderness.");
                }
                if ((waitCount >= 600) && Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.Y))
                {
                    SpawnPoint = Game.LocalPlayer.Character.Position.Around(15f);
                    break;
                }
            }

            float copHeading = Heading;
            Ped cop;
            if (transportreg == null)
            {
                if (DriverCop)
                {
                    cop = new Ped(SelectedWorldDistrict.DriverModels[EntryPoint.rnd.Next(SelectedWorldDistrict.DriverModels.Length)], SpawnPoint, copHeading);
                }
                else
                {
                    cop = new Ped(SelectedWorldDistrict.PassengerModels[EntryPoint.rnd.Next(SelectedWorldDistrict.PassengerModels.Length)], SpawnPoint, copHeading);
                }

            }
            else
            {
                //Game.LogTrivial("Transport region available");
                if (DriverCop)
                {
                    cop = new Ped(transportreg.DriverModels[EntryPoint.rnd.Next(transportreg.DriverModels.Length)], SpawnPoint, copHeading);
                }
                else
                {
                    cop = new Ped(transportreg.PassengerModels[EntryPoint.rnd.Next(transportreg.PassengerModels.Length)], SpawnPoint, copHeading);
                }
            }

            Functions.SetPedAsCop(cop);
            Functions.SetCopAsBusy(cop, true);
            cop.MakePersistent();
            cop.IsInvincible = true;
            cop.BlockPermanentEvents = true;
            OfficersCurrentlyTransporting.Add(cop);

            return cop;

        }

        //private static List<Model> cityTransportVehicleModel { get; set; }
        //private static List<Model> countrysideTransportVehicleModel { get; set; }

        private static Vehicle spawnTransportVehicle(Ped cop, TransportRegion transportreg, TransportWorldDistrict SelectedWorldDistrict)
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vehicle transportVan;
            VehicleSettings selectedvehsets;

            if (transportreg == null)
            {

                selectedvehsets = SelectedWorldDistrict.VehSettings[EntryPoint.rnd.Next(SelectedWorldDistrict.VehSettings.Length)];
            }
            else
            {
                //Game.LogTrivial("Transport region available");
                selectedvehsets = transportreg.VehSettings[EntryPoint.rnd.Next(transportreg.VehSettings.Length)];
            }

            transportVan = new Vehicle(selectedvehsets.VehicleModel, cop.GetOffsetPosition(Vector3.RelativeFront), cop.Heading);
            if (NativeFunction.Natives.GET_VEHICLE_LIVERY_COUNT<int>(transportVan) != -1)
            {
                if (selectedvehsets.LiveryNumber >= 0)
                {
                    if (selectedvehsets.LiveryNumber < NativeFunction.Natives.GET_VEHICLE_LIVERY_COUNT<int>(transportVan))
                    {

                        NativeFunction.Natives.SET_VEHICLE_LIVERY(transportVan, selectedvehsets.LiveryNumber);
                    }
                    else
                    {
                        NativeFunction.Natives.SET_VEHICLE_LIVERY(transportVan, -1);
                    }
                }
                else
                {
                    NativeFunction.Natives.SET_VEHICLE_LIVERY(transportVan, -1);
                }
            }
            if (selectedvehsets.ExtraNumbers.Length > 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (NativeFunction.Natives.DOES_EXTRA_EXIST<bool>(transportVan, i))
                    {
                        if (selectedvehsets.ExtraNumbers.Contains(i))
                        {
                            NativeFunction.Natives.SET_VEHICLE_EXTRA(transportVan, i, 0);
                        }
                        else
                        {
                            NativeFunction.Natives.SET_VEHICLE_EXTRA(transportVan, i, -1);
                        }
                    }
                }
            }


            transportVan.IsPersistent = true;

            transportVan.IsSirenOn = TransportSirenLightsEnabled;
            transportVan.IsSirenSilent = !TransportSirenSoundEnabled;
            transportVan.CanTiresBurst = false;
            transportVan.IsInvincible = true;

            return transportVan;

        }

        private static Ped putSuspectInVehicle(Ped cop, Vehicle transportVan, Ped suspect)
        {
            //Driver gets out to get suspect
            try
            {
                suspect.Model.LoadAndWait();
                Rage.Native.NativeFunction.Natives.SET_PED_CAN_RAGDOLL(cop, false);
                Ped playerPed = Game.LocalPlayer.Character;
                transportVan.BlipSiren(true);
                if (OfficerAudio)
                {
                    cop.PlayAmbientSpeech("GENERIC_HI", true);
                }
                if (cop.IsInAnyVehicle(false))
                {
                    cop.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                }

                suspect = suspect.ClonePed(true);
                Functions.SetPedAsArrested(suspect);
                suspect.Tasks.PlayAnimation("mp_arresting", "idle", 8f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);

                //bool suspectInVehicle;
                // If suspect in vehicle do the handcuff animation
                if (!suspect.Exists() || !suspect.IsValid())
                {
                    return null;
                }

                int travelCount = 0;
                while (true)
                {
                    GameFiber.Yield();
                    travelCount++;
                    if (travelCount > 13) { break; }
                    if (suspect.IsInAnyVehicle(true))
                    {
                        if (suspect.SeatIndex == 1)
                        {
                            cop.Tasks.FollowNavigationMeshToPosition(suspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeLeft * 1.3f), suspect.CurrentVehicle.Heading + 180f,
                                1.6f).WaitForCompletion(800);
                            if (Vector3.Distance(cop.Position, suspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeLeft * 1.3f)) < 2f)
                            {
                                break;
                            }
                        }
                        else
                        {
                            cop.Tasks.FollowNavigationMeshToPosition(suspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeRight * 1.3f), suspect.CurrentVehicle.Heading + 180f,
                                1.6f).WaitForCompletion(800);
                            if (Vector3.Distance(cop.Position, suspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeRight * 1.3f)) < 2f)
                            {
                                break;
                            }
                        }

                    }
                    else
                    {
                        cop.Tasks.FollowNavigationMeshToPosition(suspect.Position, suspect.Heading + 180f, 1.6f).WaitForCompletion(800);
                        if (Vector3.Distance(cop.Position, suspect.Position) < 2.2f)
                        {
                            break;
                        }
                    }
                }





                Rage.Native.NativeFunction.Natives.SET_PED_CAN_RAGDOLL(suspect, false);
                if (OfficerAudio)
                {
                    cop.PlayAmbientSpeech("GENERIC_INSULT_MED");
                }
                int SeatToPutInto = 1;
                if (!transportVan.IsSeatFree(SeatToPutInto))
                {
                    Game.LogTrivial("Seat 2");
                    SeatToPutInto = 2;
                }

                //Put suspect in vehicle
                if (SeatToPutInto == 1)
                {
                    cop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeLeft * 2f), transportVan.Heading - 90f, 1.47f);
                }
                else
                {
                    cop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeRight * 2f), transportVan.Heading + 90f, 1.47f);
                }

                int waitCount = 0;
                while (true)
                {
                    GameFiber.Wait(1000);
                    waitCount++;
                    if (transportVan.GetDoors().Length > SeatToPutInto + 1)
                    {
                        if (transportVan.Doors[SeatToPutInto + 1].IsOpen)
                        {
                            GameFiber.Sleep(1000);
                            break;
                        }
                    }
                    if (waitCount >= 17)
                    {
                        break;
                    }
                    if (suspect.Exists())
                    {
                        if (!suspect.IsDead)
                        {
                            if (cop.DistanceTo(transportVan.Position) > 4.2f)
                            {
                                if (SeatToPutInto == 1)
                                {
                                    cop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeLeft * 2f), transportVan.Heading - 90f, 1.47f);
                                }
                                else
                                {
                                    cop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeRight * 2f), transportVan.Heading + 90f, 1.47f);
                                }
                                suspect.Tasks.FollowNavigationMeshToPosition(cop.GetOffsetPosition(Vector3.RelativeBack * 1f), cop.Heading, 1.33f);
                            }
                            else if (transportVan.GetDoors().Length > SeatToPutInto + 1)
                            {
                                NativeFunction.Natives.TASK_OPEN_VEHICLE_DOOR(cop, transportVan, 18000f, SeatToPutInto, 1.47f);
                                suspect.Tasks.FollowNavigationMeshToPosition(cop.GetOffsetPosition(Vector3.RelativeBack * 1f), cop.Heading, 1.33f);
                            }
                            else
                            {
                                Game.LogTrivial(transportVan.Model.Name + " doesn't have enough doors. Skipping opening door logic for Arrest Manager.");
                                break;
                            }


                        }
                        else { return null; }
                    }
                    else { return null; }

                }





                cop.Tasks.FollowNavigationMeshToPosition(cop.GetOffsetPosition(Vector3.RelativeBack * 2.4f), cop.Heading, 1.47f).WaitForCompletion(2000);

                suspect.Tasks.EnterVehicle(transportVan, 6000, SeatToPutInto).WaitForCompletion();
                if (transportVan.GetDoors().Length > SeatToPutInto + 1)
                {
                    transportVan.Doors[SeatToPutInto + 1].Close(false);
                }
                GameFiber.Sleep(20);
                if (OfficerAudio)
                {
                    cop.PlayAmbientSpeech("GENERIC_THANKS");
                }
                //Driver gets in, warps player out if he got in
                cop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeLeft * 1.3f), transportVan.Heading,
                        1.4f).WaitForCompletion(3000);

                Rage.Task copEnterVehicle = cop.Tasks.EnterVehicle(transportVan, 7000, -1);

                if (playerPed.IsInVehicle(transportVan, true))
                {
                    playerPed.Position = transportVan.GetOffsetPosition(Vector3.RelativeFront * 4.5f);
                    cop.WarpIntoVehicle(transportVan, -1);
                    Game.DisplayNotification("Get out of the van!");
                }

                copEnterVehicle.WaitForCompletion();
                if (!suspect.IsInVehicle(transportVan, false))
                {
                    suspect.WarpIntoVehicle(transportVan, SeatToPutInto);
                }
                if (OfficerAudio)
                {
                    cop.PlayAmbientSpeech("GENERIC_BYE", true);
                }
                return suspect;
            }
            catch (Exception e)
            {
                Game.LogTrivial(e.ToString());
                return null;
            }

        }
        private static Rage.Task goToFirstSuspect { get; set; }
        private static Rage.Task goToSecondSuspect { get; set; }
        private static Ped[] putSuspectInVehicle(Ped driverCop, Ped passengerCop, Vehicle transportVan, Ped firstSuspect, Ped secondSuspect)
        {
            //Cops gets out to get suspects
            try
            {
                Rage.Native.NativeFunction.Natives.SET_PED_CAN_RAGDOLL(driverCop, false);
                Rage.Native.NativeFunction.Natives.SET_PED_CAN_RAGDOLL(passengerCop, false);
                firstSuspect.Model.LoadAndWait();
                secondSuspect.Model.LoadAndWait();
                Ped playerPed = Game.LocalPlayer.Character;
                transportVan.BlipSiren(true);
                if (OfficerAudio)
                {
                    driverCop.PlayAmbientSpeech("GENERIC_HI", true);
                }
                Rage.Task driverGetOut = driverCop.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                Rage.Task passengerGetOut = passengerCop.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                driverGetOut.WaitForCompletion();
                passengerGetOut.WaitForCompletion();
                firstSuspect = firstSuspect.ClonePed(true);
                Functions.SetPedAsArrested(firstSuspect);
                firstSuspect.Tasks.PlayAnimation("mp_arresting", "idle", 8f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);




                secondSuspect = secondSuspect.ClonePed(true);
                Functions.SetPedAsArrested(secondSuspect);
                secondSuspect.Tasks.PlayAnimation("mp_arresting", "idle", 8f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);



                if (!firstSuspect.Exists() || !firstSuspect.IsValid() || !secondSuspect.Exists() || !secondSuspect.IsValid())
                {
                    return new Ped[] { null, null };
                }

                // If suspects in vehicle do the handcuff animation else go to them
                if (firstSuspect.IsInAnyVehicle(true))
                {
                    if (firstSuspect.SeatIndex == 1)
                    {
                        goToFirstSuspect = driverCop.Tasks.FollowNavigationMeshToPosition(firstSuspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeLeft * 1.3f), firstSuspect.CurrentVehicle.Heading + 180f,
                            1.6f);
                    }
                    else
                    {
                        goToFirstSuspect = driverCop.Tasks.FollowNavigationMeshToPosition(firstSuspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeRight * 1.3f), firstSuspect.CurrentVehicle.Heading + 180f,
                            1.6f);
                    }

                }
                else
                {
                    goToFirstSuspect = driverCop.Tasks.GoToOffsetFromEntity(firstSuspect, 17000, 1.5f, 0f, 1.6f);

                }

                if (secondSuspect.IsInAnyVehicle(true))
                {
                    if (secondSuspect.SeatIndex == 1)
                    {
                        goToSecondSuspect = passengerCop.Tasks.FollowNavigationMeshToPosition(secondSuspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeLeft * 1.3f), secondSuspect.CurrentVehicle.Heading + 180f,
                            1.8f);
                    }
                    else
                    {
                        goToSecondSuspect = passengerCop.Tasks.FollowNavigationMeshToPosition(secondSuspect.CurrentVehicle.GetOffsetPosition(Vector3.RelativeRight * 1.3f), secondSuspect.CurrentVehicle.Heading + 180f,
                                                    1.8f);
                    }

                }
                else
                {
                    goToSecondSuspect = passengerCop.Tasks.GoToOffsetFromEntity(secondSuspect, 17000, 1.5f, 0f, 1.6f);

                }
                int travelCount = 0;
                while (true)
                {
                    GameFiber.Sleep(10);
                    travelCount++;
                    if (travelCount > 1700) { break; }
                    if ((Vector3.Distance(passengerCop.Position, secondSuspect.Position) < 2.2f) && (Vector3.Distance(driverCop.Position, firstSuspect.Position) < 2.2f))
                    {
                        break;

                    }
                }
                if (OfficerAudio)
                {
                    driverCop.PlayAmbientSpeech("GENERIC_INSULT_MED");

                    passengerCop.PlayAmbientSpeech("GENERIC_HOWS_IT_GOING");

                }





                Rage.Native.NativeFunction.Natives.SET_PED_CAN_RAGDOLL(firstSuspect, false);
                Rage.Native.NativeFunction.Natives.SET_PED_CAN_RAGDOLL(secondSuspect, false);
                //Driver puts first suspect in vehicle

                driverCop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeLeft * 2f), transportVan.Heading - 90f, 1.47f);

                passengerCop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeRight * 2f), transportVan.Heading + 90f, 1.47f);

                int waitDriveCount = 0;
                int waitPassengerCount = 0;
                bool driverDone = false;
                bool passengerDone = false;
                bool inDriverLoop = false;
                bool inPassengerLoop = false;

                bool DriverReadyForPutIn = false;
                bool PassengerReadyForPutIn = false;
                while (true)
                {
                    GameFiber.Sleep(1000);
                    waitDriveCount++;
                    waitPassengerCount++;

                    if (DriverReadyForPutIn || (waitDriveCount == 18))
                    {
                        if (!inDriverLoop)
                        {
                            inDriverLoop = true;
                            GameFiber.StartNew(delegate
                            {

                                GameFiber.Wait(1000);
                                driverCop.Tasks.FollowNavigationMeshToPosition(driverCop.GetOffsetPosition(Vector3.RelativeBack * 2.4f), driverCop.Heading, 1.47f).WaitForCompletion(2000);
                                if (transportVan.GetDoors().Length > 2)
                                {
                                    transportVan.Doors[2].Open(false);
                                }
                                firstSuspect.Tasks.EnterVehicle(transportVan, 10000, 1).WaitForCompletion();
                                if (transportVan.GetDoors().Length > 2)
                                {
                                    transportVan.Doors[2].Close(false);
                                }
                                if (OfficerAudio)
                                {
                                    driverCop.PlayAmbientSpeech("GENERIC_THANKS");
                                }
                                driverCop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeLeft * 1.3f), transportVan.Heading + 180f,
                                        1.4f);
                                GameFiber.Wait(1000);
                                driverDone = true;

                            });
                        }
                    }

                    //hello

                    if (PassengerReadyForPutIn || (waitPassengerCount == 18))
                    {
                        if (!inPassengerLoop)
                        {
                            inPassengerLoop = true;
                            GameFiber.StartNew(delegate
                            {

                                GameFiber.Sleep(1000);
                                passengerCop.Tasks.FollowNavigationMeshToPosition(passengerCop.GetOffsetPosition(Vector3.RelativeBack * 2.4f), passengerCop.Heading, 1.47f).WaitForCompletion(2000);
                                if (transportVan.GetDoors().Length > 3)
                                {
                                    transportVan.Doors[3].Open(false);
                                }
                                secondSuspect.Tasks.EnterVehicle(transportVan, 10000, 2).WaitForCompletion();
                                if (transportVan.GetDoors().Length > 3)
                                {
                                    transportVan.Doors[3].Close(false);
                                }
                                if (OfficerAudio)
                                {
                                    passengerCop.PlayAmbientSpeech("GENERIC_THANKS");
                                }
                                passengerCop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeRight * 1.3f), transportVan.Heading + 180f,
                                        1.4f);
                                GameFiber.Sleep(1000);
                                passengerDone = true;
                            });
                        }
                    }

                    if (driverDone && passengerDone)
                    {
                        break;
                    }
                    if (firstSuspect.Exists())
                    {
                        if (!firstSuspect.IsDead)
                        {
                            if (!inDriverLoop)
                            {
                                if (driverCop.DistanceTo(transportVan.Position) > 4.2f)
                                {

                                    driverCop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeLeft * 2f), transportVan.Heading - 90f, 1.47f);

                                    firstSuspect.Tasks.FollowNavigationMeshToPosition(driverCop.GetOffsetPosition(Vector3.RelativeBack * 1f), driverCop.Heading, 1.33f);
                                }
                                else if (transportVan.GetDoors().Length > 2)
                                {
                                    NativeFunction.Natives.TASK_OPEN_VEHICLE_DOOR(driverCop, transportVan, 18000f, 1, 1.47f);
                                    firstSuspect.Tasks.FollowNavigationMeshToPosition(driverCop.GetOffsetPosition(Vector3.RelativeBack * 1f), driverCop.Heading, 1.33f);
                                    if (transportVan.Doors[2].IsOpen)
                                    {
                                        DriverReadyForPutIn = true;
                                    }
                                }
                                else
                                {
                                    Game.LogTrivial(transportVan.Model.Name + " doesn't have enough doors. Skipping opening door logic for Arrest Manager.");
                                    DriverReadyForPutIn = true;
                                }

                            }
                        }
                        else { return new Ped[] { null, null }; }
                    }
                    else { return new Ped[] { null, null }; ; }

                    if (secondSuspect.Exists())
                    {
                        if (!secondSuspect.IsDead)
                        {
                            if (!inPassengerLoop)
                            {
                                if (passengerCop.DistanceTo(transportVan.Position) > 4.2f)
                                {

                                    passengerCop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeRight * 2f), transportVan.Heading + 90f, 1.47f);

                                    secondSuspect.Tasks.FollowNavigationMeshToPosition(passengerCop.GetOffsetPosition(Vector3.RelativeBack * 1f), passengerCop.Heading, 1.33f);
                                }
                                else if (transportVan.GetDoors().Length > 3)
                                {
                                    NativeFunction.Natives.TASK_OPEN_VEHICLE_DOOR( passengerCop, transportVan, 18000f, 2, 1.47f);
                                    secondSuspect.Tasks.FollowNavigationMeshToPosition(passengerCop.GetOffsetPosition(Vector3.RelativeBack * 1f), passengerCop.Heading, 1.33f);
                                    if (transportVan.Doors[3].IsOpen)
                                    {
                                        PassengerReadyForPutIn = true;
                                    }
                                }
                                else
                                {
                                    Game.LogTrivial(transportVan.Model.Name + " doesn't have enough doors. Skipping opening door logic for Arrest Manager.");
                                    PassengerReadyForPutIn = true;
                                }


                            }
                        }
                        else { return new Ped[] { null, null }; }
                    }
                    else { return new Ped[] { null, null }; ; }


                }

                //Driver & passenger get in, warps player out if he got in
                Rage.Task driverCopEnterVehicle = driverCop.Tasks.EnterVehicle(transportVan, 5000, -1);
                Rage.Task passengerCopEnterVehicle = passengerCop.Tasks.EnterVehicle(transportVan, 5000, 0);
                if (playerPed.IsInVehicle(transportVan, true))
                {
                    playerPed.Position = transportVan.GetOffsetPosition(Vector3.RelativeFront * 5f);
                    driverCop.WarpIntoVehicle(transportVan, -1);
                    passengerCop.WarpIntoVehicle(transportVan, 0);
                    Game.DisplayNotification("Get out of the van!");
                }
                driverCopEnterVehicle.WaitForCompletion();
                passengerCopEnterVehicle.WaitForCompletion();
                if (!driverCop.IsInVehicle(transportVan, false))
                {
                    driverCop.WarpIntoVehicle(transportVan, -1);
                }
                if (!passengerCop.IsInVehicle(transportVan, false))
                {
                    passengerCop.WarpIntoVehicle(transportVan, 0);
                }
                if (!firstSuspect.IsInVehicle(transportVan, false))
                {
                    firstSuspect.WarpIntoVehicle(transportVan, 1);
                }
                if (!secondSuspect.IsInVehicle(transportVan, false))
                {
                    secondSuspect.WarpIntoVehicle(transportVan, 2);
                }
                if (OfficerAudio)
                {
                    driverCop.PlayAmbientSpeech("GENERIC_BYE", true);
                }
                Ped[] suspects = new Ped[2] { firstSuspect, secondSuspect };

                return suspects;
            }
            catch (Exception e)
            {
                Game.LogTrivial(e.ToString());
                Ped[] suspects = new Ped[] { null, null };
                return suspects;
            }
        }

        private static void driveToSuspect(Ped cop, Vehicle transportVan, Ped suspect)
        {

            Ped playerPed = Game.LocalPlayer.Character;
            int drivingLoopCount = 0;
            bool transportVanTeleported = false;
            int waitCount = 0;
            bool forceCloseSpawn = false;

            GameFiber.StartNew(delegate
            {
                while (!forceCloseSpawn)
                {
                    GameFiber.Yield();
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(transportKey)) // || Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(multiTransportKey))
                    {
                        GameFiber.Sleep(500);
                        if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(transportKey))// || Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(multiTransportKey))
                        {
                            GameFiber.Sleep(500);
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(transportKey))// || Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(multiTransportKey))
                            {
                                forceCloseSpawn = true;
                            }
                            else
                            {
                                Game.DisplayNotification("Hold down the ~b~transport key ~s~to force a close spawn.");
                            }
                        }
                    }
                }
            });
            Rage.Task driveToPed = null;
            cop.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardStraight).WaitForCompletion(500);
            while (Vector3.Distance(transportVan.Position, suspect.Position) > 35f)
            {
                if (!suspect.Exists() || !suspect.IsValid())
                {
                    return;
                }

                transportVan.Repair();
                if (driveToPed == null || !driveToPed.IsActive)
                {
                    driveToPed = cop.Tasks.DriveToPosition(suspect.Position, 15f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                    
                }
                NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(cop, 786607);
                NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(cop, 0f);
                NativeFunction.Natives.SET_DRIVER_ABILITY(cop, 1f);
                GameFiber.Wait(600);

                waitCount++;
                if (waitCount == 55)
                {
                    Game.DisplayHelp("Transport taking too long? Hold down ~b~" + kc.ConvertToString(transportKey) + " ~s~to speed it up.", 5000);
                }
                //If van isn't moving
                if (!transportVan.IsBoat)
                {
                    if (transportVan.Speed < 0.2f)
                    {
                        //cop.Tasks.PerformDrivingManeuver(transportVan, VehicleManeuver.ReverseStraight, 700).WaitForCompletion();
                        //drivingLoopCount += 1;
                        //cop.Tasks.DriveToPosition(suspect.Position, 17f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                    }
                    if (transportVan.Speed < 2f)
                    {
                        drivingLoopCount++;
                    }
                    //if van is very far away
                    if ((Vector3.Distance(suspect.Position, transportVan.Position) > transportSpawnDistance + 70f))
                    {
                        drivingLoopCount++;
                    }
                    //If Van is stuck, relocate it

                    if ((drivingLoopCount >= 33 && drivingLoopCount <= 38) && AllowWarping)
                    {
                        Vector3 SpawnPoint;
                        float Heading;
                        bool UseSpecialID = true;
                        float travelDistance;
                        int WaitCount = 0;
                        while (true)
                        {
                            GetTransportVanSpawnPoint(suspect.Position, out SpawnPoint, out Heading, UseSpecialID);
                            travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>(SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, playerPed.Position.X, playerPed.Position.Y, playerPed.Position.Z);

                            if (Vector3.Distance(playerPed.Position, SpawnPoint) > transportSpawnDistance - 15f)
                            {

                                if (travelDistance < transportSpawnDistance * 4.5f)
                                {
                                    Vector3 directionFromVehicleToPed1 = (Game.LocalPlayer.Character.Position - SpawnPoint);
                                    directionFromVehicleToPed1.Normalize();

                                    float HeadingToPlayer = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);

                                    if (Math.Abs(MathHelper.NormalizeHeading(Heading) - MathHelper.NormalizeHeading(HeadingToPlayer)) < 150f)
                                    {

                                        break;
                                    }
                                }
                            }
                            WaitCount++;
                            if (WaitCount >= 400)
                            {
                                UseSpecialID = false;
                            }

                            GameFiber.Yield();
                        }
                        //float travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>( SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, playerPed.Position.X, playerPed.Position.Y, playerPed.Position.Z);

                        Game.Console.Print("Relocating because van was stuck...");
                        transportVan.Position = SpawnPoint;
                        //Vector3 directionFromVehicleToPed = (suspect.Position - SpawnPoint);
                        //directionFromVehicleToPed.Normalize();

                        //float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                        transportVan.Heading = Heading;
                        drivingLoopCount = 39;
                        


                    }
                    // if van is stuck for a 2nd time or takes too long, spawn it very near to the suspect
                    else if (((drivingLoopCount >= 70 || waitCount >= 110) && AllowWarping) || forceCloseSpawn)
                    {
                        Game.Console.Print("Relocating to a close position");

                        Vector3 SpawnPoint = World.GetNextPositionOnStreet(suspect.Position.Around2D(15f));

                        int waitCounter = 0;
                        while ((SpawnPoint.Z - suspect.Position.Z < -3f) || (SpawnPoint.Z - suspect.Position.Z > 3f) || (Vector3.Distance(SpawnPoint, suspect.Position) > 25f))
                        {
                            waitCounter++;
                            SpawnPoint = World.GetNextPositionOnStreet(suspect.Position.Around2D(15f));
                            GameFiber.Yield();
                            if (waitCounter >= 500)
                            {
                                SpawnPoint = suspect.Position.Around2D(15f);
                                break;
                            }
                        }
                        transportVan.Position = SpawnPoint;
                        Vector3 directionFromVehicleToPed = (suspect.Position - SpawnPoint);
                        directionFromVehicleToPed.Normalize();

                        float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                        transportVan.Heading = vehicleHeading;
                        transportVanTeleported = true;

                        break;
                    }
                }
                else
                {
                    NativeFunction.Natives.REQUEST_COLLISION_AT_COORD(transportVan.Position.X, transportVan.Position.Y, transportVan.Position.Z);
                    NativeFunction.Natives.REQUEST_ADDITIONAL_COLLISION_AT_COORD(transportVan.Position.X, transportVan.Position.Y, transportVan.Position.Z);

                    if (waitCount > 85)
                    {
                        break;
                    }
                    if (transportVan.Speed < 0.2f)
                    {
                        NativeFunction.Natives.SET_VEHICLE_FORWARD_SPEED(transportVan, 15f);
                        GameFiber.Wait(700);
                    }
                }

            }

            forceCloseSpawn = true;
            //park the van
            Game.HideHelp();
            while ((Vector3.Distance(suspect.Position, transportVan.Position) > 16f) && !transportVanTeleported)
            {
                if (!suspect.Exists() || !suspect.IsValid())
                {
                    return;
                }
                Rage.Task parkNearSuspect = cop.Tasks.DriveToPosition(suspect.Position, 6f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                parkNearSuspect.WaitForCompletion(800);
                transportVanTeleported = false;
                if (Vector3.Distance(suspect.Position, transportVan.Position) > 90f)
                {
                    Vector3 SpawnPoint = World.GetNextPositionOnStreet(suspect.Position.Around2D(12f));
                    transportVan.Position = SpawnPoint;
                }
                //if (transportVan.Speed < 0.2f)
                //{
                //    reverseCount++;
                //    if (reverseCount == 3)
                //    {
                //        cop.Tasks.PerformDrivingManeuver(transportVan, VehicleManeuver.ReverseStraight, 1700).WaitForCompletion();
                //        reverseCount = 0;
                //    }
                //}

                if ((transportVan.IsBoat && transportVan.DistanceTo(suspect) < 25) || ExtensionMethods.IsPointOnWater(suspect.Position))
                {
                    break;
                }

            }
            GameFiber.Wait(600);



        }
        internal static void SuspectTransporterRecruitedOfficer(Ped cop, Vehicle transportVan, bool anims = true)
        {
            GameFiber.StartNew(delegate
            {
                Guid CalloutID = Guid.NewGuid();

                bool CalloutConcluded = false;
                List<Entity> EntitiesUsedForTransport = new List<Entity>();
                try
                {
                    Ped suspect = getSuspectAPI();
                    if (suspect == null)
                    {
                        canChoose = true;
                        return;
                    }
                    suspectsArrestedByPlayer.Remove(suspect);
                    string suspectName = Functions.GetPersonaForPed(suspect).FullName;
                    suspect.BlockPermanentEvents = true;
                    suspect.IsInvincible = true;
                    suspect.IsPersistent = true;
                    EntitiesUsedForTransport.Add(suspect);
                    transportVan.IsPersistent = true;
                    cop.IsPersistent = true;
                    cop.BlockPermanentEvents = true;
                    suspectsPendingTransport.Add(suspect);
                    if (anims)
                    {
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                    }
                    OfficersCurrentlyTransporting.Add(cop);
                    EntitiesUsedForTransport.Add(cop);
                    EntitiesUsedForTransport.Add(transportVan);
                    GameFiber.Wait(1000);
                    if (DispatchVoice)
                    {
                        Functions.PlayScannerAudioUsingPosition("ASSISTANCE_REQUIRED FOR SUSPECT_UNDER_ARREST IN_OR_ON_POSITION OFFICER_INTRO UNIT_RESPONDING_DISPATCH_01 INTRO REPORT_RESPONSE_COPY_03", suspect.Position);
                        cop.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                    }
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")))
                    {
                        CalloutID = API.ComputerPlusFuncs.CreateCallout("Prisoner Transport Required", "Transport Required", suspect.Position, 0, "Requesting prisoner transport for " + suspectName + ". Please respond.", 2, new List<Ped>() { suspect }, null);
                        API.ComputerPlusFuncs.AssignCallToAIUnit(CalloutID);
                    }
                    if (EntryPoint.IsLSPDFRPlusRunning)
                    {
                        API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Prisoners picked up");
                    }
                    
                    Game.LocalPlayer.Character.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_UNARMED"), 0, true);

                    if (namesUsed.Contains(Functions.GetPersonaForPed(suspect).FullName))
                    {
                        namesUsed.Remove(Functions.GetPersonaForPed(suspect).FullName);
                    }
                    //player can now radio for another suspect
                    canChoose = true;
                    transportMessageReceived = false;
                    multiTransportMessageReceived = false;
                    //create a blip
                    Blip copBlip = cop.AttachBlip();
                    copBlip.Color = System.Drawing.Color.DeepSkyBlue;
                    copBlip.Flash(1500, -1);
                    copBlip.IsFriendly = true;

                    if (cop.IsInVehicle(transportVan, false))
                    {

                        //While the transport van isn't near the player, drive to the player
                        driveToSuspect(cop, transportVan, suspect);
                        if (!suspect.Exists() || !suspect.IsValid())
                        {
                            Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");
                            foreach (Entity ent in EntitiesUsedForTransport)
                            {
                                if (ent.Exists()) { ent.Dismiss(); }
                            }
                            canChoose = true;
                            transportMessageReceived = false;
                            if (copBlip.Exists()) { copBlip.Delete(); }
                            return;
                        }

                    }
                    else
                    {
                        if (Vector3.Distance(suspect.Position, cop.Position) > Vector3.Distance(cop.Position, transportVan.Position) && Vector3.Distance(cop.Position, suspect.Position) > 25f)
                        {
                            cop.Tasks.FollowNavigationMeshToPosition(transportVan.GetOffsetPosition(Vector3.RelativeLeft * 2f), transportVan.Heading - 90f, 1.65f).WaitForCompletion(7000);
                            cop.Tasks.EnterVehicle(transportVan, 6000, -1).WaitForCompletion();
                            driveToSuspect(cop, transportVan, suspect);
                            if (!suspect.Exists() || !suspect.IsValid())
                            {
                                Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");
                                foreach (Entity ent in EntitiesUsedForTransport)
                                {
                                    if (ent.Exists()) { ent.Dismiss(); }
                                }
                                canChoose = true;
                                transportMessageReceived = false;
                                if (copBlip.Exists()) { copBlip.Delete(); }
                                return;
                            }
                        }


                    }
                    if (copBlip.Exists()) { copBlip.Delete(); }
                    //Game.DisplayNotification("Transport has arrived for ~r~" + suspectName + ".");

                    //Put suspect in the vehicle
                    suspect = putSuspectInVehicle(cop, transportVan, suspect);
                    EntitiesUsedForTransport.Add(suspect);
                    if (!suspect.Exists() || !suspect.IsValid())
                    {
                        Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");
                        foreach (Entity ent in EntitiesUsedForTransport)
                        {
                            if (ent.Exists()) { ent.Dismiss(); }
                        }
                        canChoose = true;
                        transportMessageReceived = false;

                        return;
                    }
                    Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");
                    if (!suspectsPendingTransport.Contains(suspect))
                    {
                        suspectsPendingTransport.Add(suspect);
                    }

                    //Dispose of the entities
                    if (!transportVan.HasDriver)
                    {
                        cop.WarpIntoVehicle(transportVan, -1);
                    }

                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }
                    if (copBlip.Exists()) { copBlip.Delete(); }
                    transportSuspectToNearestStation(cop, transportVan, EntitiesUsedForTransport);
                }
                catch (ThreadAbortException)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }
                    canChoose = true;
                    transportMessageReceived = false;
                }
                catch (Exception e)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Arrest Manager handled the single recruited transport exception.");
                    canChoose = true;
                    transportMessageReceived = false;
                }
                finally
                {
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }
                }

            });
        }



        internal static void SuspectTransporterNewOfficer(Ped suspect = null, bool anims = true)
        {
            GameFiber.StartNew(delegate
            {
                Guid CalloutID = Guid.NewGuid();
                bool CalloutConcluded = false;
                List<Entity> EntitiesUsedForTransport = new List<Entity>();
                try
                {
                     Ped playerPed = Game.LocalPlayer.Character;

                    //Get the suspect and check if he exists.
                    if (!suspect.Exists())
                    {
                        suspect = getSuspectAPI();
                        if (suspect == null)
                        {
                            canChoose = true;
                            return;
                        }
                    }
                    if (suspectsArrestedByPlayer.Contains(suspect))
                    {
                        suspectsArrestedByPlayer.Remove(suspect);
                    }
                    //Name to use in notifications
                    string suspectName = Functions.GetPersonaForPed(suspect).FullName;
                    suspect.BlockPermanentEvents = true;
                    suspect.IsInvincible = true;
                    suspect.IsPersistent = true;
                    EntitiesUsedForTransport.Add(suspect);
                    suspectsPendingTransport.Add(suspect);
                    //Spawn a cop and a transport van for the pickup and safeguard them
                    if (anims)
                    {
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                    }
                    GameFiber.Wait(1000);
                    if (DispatchVoice)
                    {
                        Functions.PlayScannerAudioUsingPosition("ASSISTANCE_REQUIRED FOR SUSPECT_UNDER_ARREST IN_OR_ON_POSITION OFFICER_INTRO UNIT_RESPONDING_DISPATCH_01 INTRO REPORT_RESPONSE_COPY_03", suspect.Position);
                    }
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")))
                    {
                        CalloutID = API.ComputerPlusFuncs.CreateCallout("Prisoner Transport Required", "Transport Required", suspect.Position, 0, "Requesting prisoner transport for " + suspectName + ". Please respond.", 2, new List<Ped>() { suspect }, null);
                        API.ComputerPlusFuncs.AssignCallToAIUnit(CalloutID);
                    }
                    if (EntryPoint.IsLSPDFRPlusRunning)
                    {
                        API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Prisoners picked up");
                    }
                    playerPed.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_UNARMED"), 0, true);
                    TransportRegion trnsreg = null;
                    string ZoneName = Albo1125.Common.CommonLibrary.Zones.GetLowerZoneName(suspect.Position);
                    List<TransportRegion> AvailableRegions = (from x in TransportRegions where x.ZoneName.ToLower() == ZoneName select x).ToList();
                    if (AvailableRegions.Count > 0)
                    {
                        AvailableRegions = AvailableRegions.Shuffle();
                        trnsreg = AvailableRegions[0];
                    }
                    //Game.LogTrivial("WorldDistrict: " + Zones.GetWorldDistrict(suspect.Position).ToString());
                    List<TransportWorldDistrict> AvailableWorldDistricts = (from x in TransportWorldDistricts where x.WorldDistrict == Zones.GetWorldDistrict(suspect.Position) select x).ToList();
                    TransportWorldDistrict SelectedTransportWorldDistrict = AvailableWorldDistricts[EntryPoint.rnd.Next(AvailableWorldDistricts.Count)];

                    Ped cop = spawnTransportCop(true, trnsreg, SelectedTransportWorldDistrict);
                    Vehicle transportVan = spawnTransportVehicle(cop, trnsreg, SelectedTransportWorldDistrict);




                    warpIntoVehicle(cop, transportVan);

                    //Safeguards
                    if (!cop.Exists())
                    {
                        Game.LogTrivial("Spawning cop again");
                        cop = spawnTransportCop(true, trnsreg, SelectedTransportWorldDistrict);

                    }
                    if (!transportVan.Exists() || !transportVan.IsValid())
                    {
                        transportVan = spawnTransportVehicle(cop, trnsreg, SelectedTransportWorldDistrict);
                    }
                    if (!cop.IsInVehicle(transportVan, false))
                    {
                        cop.WarpIntoVehicle(transportVan, -1); Game.Console.Print("Cop Warped");
                    }
                    EntitiesUsedForTransport.Add(cop);
                    EntitiesUsedForTransport.Add(transportVan);
                    //add suspect to suspects pending transport so there isn't another van called for it

                    if (namesUsed.Contains(Functions.GetPersonaForPed(suspect).FullName))
                    {
                        namesUsed.Remove(Functions.GetPersonaForPed(suspect).FullName);
                    }
                    //playerPed.Tasks.Clear();

                    //radio in for assistance



                    //player can now radio for another suspect
                    canChoose = true;
                    transportMessageReceived = false;
                    multiTransportMessageReceived = false;
                    //create a blip
                    Blip copBlip = cop.AttachBlip();
                    copBlip.Color = System.Drawing.Color.DeepSkyBlue;
                    copBlip.Flash(1500, -1);
                    copBlip.IsFriendly = true;
                    //While the transport van isn't near the player, drive to the player
                    driveToSuspect(cop, transportVan, suspect);
                    if (!suspect.Exists() || !suspect.IsValid())
                    {
                        Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");

                        foreach (Entity ent in EntitiesUsedForTransport)
                        {
                            if (ent.Exists()) { ent.Dismiss(); }
                        }


                        canChoose = true;
                        transportMessageReceived = false;
                        if (copBlip.Exists()) { copBlip.Delete(); }
                        return;
                    }

                    if (copBlip.Exists()) { copBlip.Delete(); }
                    //Game.DisplayNotification("Transport has arrived for ~r~" + suspectName + ".");

                    //Put suspect in the vehicle
                    suspect = putSuspectInVehicle(cop, transportVan, suspect);

                    EntitiesUsedForTransport.Add(suspect);
                    if (!suspect.Exists() || !suspect.IsValid())
                    {
                        Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");

                        foreach (Entity ent in EntitiesUsedForTransport)
                        {
                            if (ent.Exists()) { ent.Dismiss(); }
                        }

                        canChoose = true;
                        transportMessageReceived = false;
                        if (copBlip.Exists()) { copBlip.Delete(); }
                        return;
                    }
                    Game.DisplayNotification("~r~" + suspectName + "~s~ has been picked up!");
                    if (!suspectsPendingTransport.Contains(suspect))
                    {
                        suspectsPendingTransport.Add(suspect);
                    }

                    //Dispose of the entities
                    if (!transportVan.HasDriver)
                    {
                        cop.WarpIntoVehicle(transportVan, -1);
                    }
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }
                    if (copBlip.Exists()) { copBlip.Delete(); }
                    transportSuspectToNearestStation(cop, transportVan, EntitiesUsedForTransport);


                }
                catch (ThreadAbortException)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }

                    canChoose = true;
                    transportMessageReceived = false;
                }
                catch (Exception e)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }

                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Arrest Manager handled the single new officer transport exception.");
                    canChoose = true;
                    transportMessageReceived = false;
                }
                finally
                {
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }
                }
            });
        }

        internal static void multipleSuspectTransporter(bool anims = true)
        {
            GameFiber.StartNew(delegate
            {
                Guid CalloutID = Guid.NewGuid();
                bool CalloutConcluded = false;
                List<Entity> EntitiesUsedForTransport = new List<Entity>();
                try
                {
                    Ped playerPed = Game.LocalPlayer.Character;

                    //Get suspects
                    if (twoSuspectsApi.Count != 2)
                    {
                        Game.DisplayNotification("You need 2 suspects to call for multi transport.");
                        canChoose = true;
                        return;
                    }

                    //Get the suspects and take them out of the list
                    Ped firstSuspect = twoSuspectsApi[0];
                    Ped secondSuspect = twoSuspectsApi[1];
                    suspectsArrestedByPlayer.Remove(firstSuspect);
                    suspectsArrestedByPlayer.Remove(secondSuspect);
                    EntitiesUsedForTransport.Add(firstSuspect);
                    EntitiesUsedForTransport.Add(secondSuspect);

                    //Get their names and prevent them from disappearing
                    string firstSuspectName = Functions.GetPersonaForPed(firstSuspect).FullName;
                    string secondSuspectName = Functions.GetPersonaForPed(secondSuspect).FullName;

                    if (namesUsed.Contains(Functions.GetPersonaForPed(firstSuspect).FullName))
                    {
                        namesUsed.Remove(Functions.GetPersonaForPed(firstSuspect).FullName);
                    }
                    if (namesUsed.Contains(Functions.GetPersonaForPed(secondSuspect).FullName))
                    {
                        namesUsed.Remove(Functions.GetPersonaForPed(secondSuspect).FullName);
                    }
                    suspectsPendingTransport.Add(firstSuspect);
                    suspectsPendingTransport.Add(secondSuspect);
                    firstSuspect.BlockPermanentEvents = true;
                    firstSuspect.IsInvincible = true;
                    firstSuspect.IsPersistent = true;
                    secondSuspect.BlockPermanentEvents = true;
                    secondSuspect.IsInvincible = true;
                    secondSuspect.IsPersistent = true;
                    if (anims)
                    {
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                    }
                    GameFiber.Wait(1000);
                    if (DispatchVoice)
                    {
                        Functions.PlayScannerAudioUsingPosition("ASSISTANCE_REQUIRED FOR SUSPECT_UNDER_ARREST IN_OR_ON_POSITION OFFICER_INTRO UNIT_RESPONDING_DISPATCH_01 INTRO REPORT_RESPONSE_COPY_03", firstSuspect.Position);
                    }
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")))
                    {
                        CalloutID = API.ComputerPlusFuncs.CreateCallout("Prisoner Transport Required", "Transport Required", firstSuspect.Position, 0, "Requesting prisoner transport for " + firstSuspectName + " and " + secondSuspectName + ". Please respond.", 2, new List<Ped>() { firstSuspect, secondSuspect }, null);
                        API.ComputerPlusFuncs.AssignCallToAIUnit(CalloutID);
                    }
                    if (EntryPoint.IsLSPDFRPlusRunning)
                    {
                        API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Prisoners picked up");
                        API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Prisoners picked up");
                    }
                    playerPed.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_UNARMED"), 0, true);
                    //Put cops in the vehicle
                    TransportRegion trnsreg = null;
                    string ZoneName = Albo1125.Common.CommonLibrary.Zones.GetLowerZoneName(firstSuspect.Position);
                    List<TransportRegion> AvailableRegions = (from x in TransportRegions where x.ZoneName.ToLower() == ZoneName select x).ToList();
                    if (AvailableRegions.Count > 0)
                    {
                        AvailableRegions = AvailableRegions.Shuffle();
                        trnsreg = AvailableRegions[0];
                    }
                    List<TransportWorldDistrict> AvailableWorldDistricts = (from x in TransportWorldDistricts where x.WorldDistrict == Zones.GetWorldDistrict(firstSuspect.Position) select x).ToList();
                    TransportWorldDistrict SelectedTransportWorldDistrict = AvailableWorldDistricts[EntryPoint.rnd.Next(AvailableWorldDistricts.Count)];

                    Ped driverCop = spawnTransportCop(true, trnsreg, SelectedTransportWorldDistrict);
                    Vehicle transportVan = spawnTransportVehicle(driverCop, trnsreg, SelectedTransportWorldDistrict);
                    driverCop.WarpIntoVehicle(transportVan, -1);

                    Ped passengerCop = spawnTransportCop(false, trnsreg, SelectedTransportWorldDistrict);
                    passengerCop.WarpIntoVehicle(transportVan, 0);

                    EntitiesUsedForTransport.Add(driverCop);
                    EntitiesUsedForTransport.Add(transportVan);
                    EntitiesUsedForTransport.Add(passengerCop);


                    //radio in for assistance
                    //playerPed.Tasks.Clear();


                    //player can now radio for another suspect
                    canChoose = true;
                    multiTransportMessageReceived = false;
                    transportMessageReceived = false;

                    //create blips
                    Blip copBlip = driverCop.AttachBlip();
                    copBlip.Color = System.Drawing.Color.DeepSkyBlue;
                    copBlip.Flash(1500, -1);
                    copBlip.IsFriendly = true;

                    //Drive to the suspect
                    driveToSuspect(driverCop, transportVan, firstSuspect);
                    //If either suspect no longer exist
                    if (!firstSuspect.Exists() || !firstSuspect.IsValid() || !secondSuspect.Exists() || !secondSuspect.IsValid())
                    {
                        Game.DisplayNotification("~r~" + firstSuspectName + "~s~ and ~r~" + secondSuspectName + "~s~ have been picked up!");
                        driverCop.Dismiss();

                        transportVan.IsSirenOn = false;
                        transportVan.Dismiss();
                        copBlip.Delete();
                        canChoose = true;
                        transportMessageReceived = false;
                        if (copBlip.Exists()) { copBlip.Delete(); }
                        return;
                    }

                    if (copBlip.Exists()) { copBlip.Delete(); }
                    //Game.DisplayNotification("Transport has arrived for ~r~" + firstSuspectName + "~s~ and ~r~" + secondSuspectName + "~s~.");

                    //Put suspects in the vehicle

                    Ped[] suspects = putSuspectInVehicle(driverCop, passengerCop, transportVan, firstSuspect, secondSuspect);
                    EntitiesUsedForTransport.AddRange(suspects);
                    //Cleanup
                    if (suspects.Length < 2 || !suspects[0].Exists() || !suspects[0].IsValid() || !suspects[1].Exists() || !suspects[1].IsValid())
                    {
                        Game.DisplayNotification("~r~" + firstSuspectName + "~s~ and ~r~" + secondSuspectName + "~s~ have been picked up!");
                        driverCop.Dismiss();
                        foreach (Entity ent in EntitiesUsedForTransport)
                        {
                            if (ent.Exists()) { ent.Dismiss(); }
                        }
                        transportVan.IsSirenOn = false;
                        transportVan.Dismiss();
                        canChoose = true;
                        transportMessageReceived = false;
                        return;
                    }
                    Game.DisplayNotification("~r~" + firstSuspectName + "~s~ and ~r~" + secondSuspectName + "~s~ have been picked up!");
                    if (!suspectsPendingTransport.Contains(suspects[1]))
                    {
                        suspectsPendingTransport.Add(suspects[1]);
                    }
                    if (!suspectsPendingTransport.Contains(suspects[0]))
                    {
                        suspectsPendingTransport.Add(suspects[0]);
                    }

                    //Dispose of the entities

                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }

                    if (copBlip.Exists()) { copBlip.Delete(); }
                    transportSuspectToNearestStation(driverCop, transportVan, EntitiesUsedForTransport);
                    
                    
                }
                catch (ThreadAbortException)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }
                    canChoose = true;
                    transportMessageReceived = false;
                }
                catch (Exception e)
                {
                    foreach (Entity ent in EntitiesUsedForTransport)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Arrest Manager handled the multi transport exception.");
                    canChoose = true;
                    transportMessageReceived = false;
                }
                finally
                {
                    if (IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0")) && !CalloutConcluded)
                    {

                        API.ComputerPlusFuncs.ConcludeCallout(CalloutID); CalloutConcluded = true;
                    }
                }



            });
        }

        private static void transportSuspectToNearestStation(Ped driverCop, Vehicle transportVan, List<Entity> EntitiesUsedForTransport)
        {
            try
            {
                Vector3 nearestDropoff = new Vector3(479.8365f, -1021.213f, 27.58666f);
                if (transportVan.Exists() && driverCop.Exists())
                {
                    nearestDropoff = JailDropoff.AllJailDropoffs.Where(x => x.AIDropoff && x.SuitableForVeh(transportVan)).Select(x => x.Position).OrderBy(x => x.DistanceTo(transportVan)).FirstOrDefault();
                    driverCop.Tasks.DriveToPosition(nearestDropoff, 17f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians);
                    NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(driverCop, 786603);
                    NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(driverCop, 0f);
                    NativeFunction.Natives.SET_DRIVER_ABILITY(driverCop, 1f);

                    transportVan.IsSirenOn = false;
                }
                while (true)
                {
                    GameFiber.Yield();

                    if (Vector3.Distance(Game.LocalPlayer.Character.Position, transportVan.Position) > 120f)
                    {
                        break;
                    }

                    if (transportVan.DistanceTo(nearestDropoff) < 10f)
                    {
                        GameFiber.Wait(6000);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("Exception handled. Cleaning up.");
            }
            foreach (Entity ent in EntitiesUsedForTransport)
            {
                if (ent) { ent.Delete(); }
            }
        }

        private static void warpIntoVehicle(Ped person, Vehicle car)
        {
            //warp person into car
            while (person.Exists() && !person.IsInVehicle(car, false) && !person.IsDead)
            {
                person.WarpIntoVehicle(car, -1);
                //Game.Console.Print("Warped");
            }




        }

        public static bool RecruitNearbyOfficer(out Ped Officer, out Vehicle PoliceCar)
        {

            if (!AllowRecruitedOfficers)
            {
                Officer = null;
                PoliceCar = null;
                return false;
            }

            List<Model> AcceptedPedModels = new List<Model>();
            List<Model> AcceptedVehicleModels = new List<Model>();
            string ZoneName = Albo1125.Common.CommonLibrary.Zones.GetLowerZoneName(Game.LocalPlayer.Character.Position);




            List<TransportRegion> AvailableRegions = (from x in TransportRegions where x.ZoneName.ToLower() == ZoneName select x).ToList();
            if (AvailableRegions.Count > 0)
            {


                Game.LogTrivial("Transport region(s) available");
                foreach (TransportRegion reg in AvailableRegions)
                {
                    AcceptedPedModels.AddRange(reg.DriverModels);
                    AcceptedVehicleModels.AddRange(reg.VehSettings.Select(x => x.VehicleModel));
                }


            }
            else
            {
                //uint zoneHash = Rage.Native.NativeFunction.CallByHash<uint>(0x7ee64d51e8498728, Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z);
                Game.LogTrivial("Transport region(s) not available");
                List<TransportWorldDistrict> AvailableWorldDistricts = (from x in TransportWorldDistricts where x.WorldDistrict == Zones.GetWorldDistrict(Game.LocalPlayer.Character.Position) select x).ToList();
                foreach (TransportWorldDistrict distr in AvailableWorldDistricts)
                {

                    AcceptedPedModels.AddRange(distr.DriverModels);
                    AcceptedVehicleModels.AddRange(distr.VehSettings.Select(x => x.VehicleModel));

                }


            }


            Entity[] nearbypeds = World.GetEntities(Game.LocalPlayer.Character.Position, transportSpawnDistance * 0.75f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed);
            nearbypeds = (from x in nearbypeds orderby (Game.LocalPlayer.Character.DistanceTo(x.Position)) select x).ToArray();
            foreach (Entity nearent in nearbypeds)
            {
                if (nearent.Exists())
                {
                    Ped nearped = (Ped)nearent;
                    if (RecruitedOfficerMustComplyWithStandards)
                    {

                        if (!AcceptedPedModels.Contains(nearped.Model))
                        {
                            continue;
                        }
                    }

                    if (!nearped.IsInCombat && nearped.RelationshipGroup == "COP" && !OfficersCurrentlyTransporting.Contains(nearped))
                    {

                        if (nearped.IsInAnyVehicle(false))
                        {
                            if (RecruitedOfficerMustComplyWithStandards)
                            {

                                if (!AcceptedVehicleModels.Contains(nearped.CurrentVehicle.Model))
                                {
                                    continue;
                                }
                            }
                            if (nearped.CurrentVehicle.IsPoliceVehicle && nearped.CurrentVehicle != Game.LocalPlayer.Character.CurrentVehicle && nearped.CurrentVehicle != Game.LocalPlayer.LastVehicle && nearped.CurrentVehicle.FreePassengerSeatsCount > 1 && nearped.LastVehicle.GetDoors().Length > 4)
                            {
                                if (nearped.CurrentVehicle.IsSirenSilent || !nearped.CurrentVehicle.IsSirenOn)
                                {


                                    PoliceCar = nearped.CurrentVehicle;
                                    PoliceCar.IsPersistent = true;
                                    Officer = nearped.ClonePed(true);

                                    Officer.IsPersistent = true;
                                    Officer.BlockPermanentEvents = true;
                                    return true;
                                    //recruit this ped
                                }
                            }
                        }
                        else if (nearped.LastVehicle.Exists())
                        {
                            if (RecruitedOfficerMustComplyWithStandards)
                            {
                                Game.LogTrivial("Checking recruited officer vehicle model");
                                if (!AcceptedVehicleModels.Contains(nearped.LastVehicle.Model))
                                {
                                    continue;
                                }
                            }
                            if (nearped.LastVehicle.IsPoliceVehicle && nearped.LastVehicle.FreePassengerSeatsCount > 1 && nearped.LastVehicle != Game.LocalPlayer.Character.LastVehicle && nearped.LastVehicle != Game.LocalPlayer.Character.CurrentVehicle && Vector3.Distance(nearped.Position, nearped.LastVehicle.Position) < 20f && nearped.LastVehicle.GetDoors().Length > 4)
                            {



                                PoliceCar = nearped.LastVehicle;
                                PoliceCar.IsPersistent = true;
                                Officer = nearped.ClonePed(true);

                                Officer.IsPersistent = true;
                                Officer.BlockPermanentEvents = true;
                                return true;
                                //recruit this ped
                            }
                        }

                    }
                }
            }
            Officer = null;
            PoliceCar = null;
            return false;
        }
        #endregion

        //EXTRA POLICE JAILS
        #region Extra Police Jails
        private static void createJailDropoffs()
        {
            JailDropoff.DeserializeDropoffs();
            foreach (JailDropoff dr in JailDropoff.AllJailDropoffs)
            {
                dr.CreateBlip();
            }
            Game.Console.Print("Jail Dropoff blips created.");
        }

        private static void clearJailSpots()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vehicle playerVehicle = playerPed.CurrentVehicle;
            if (playerVehicle)
            {
                foreach (JailDropoff dr in JailDropoff.AllJailDropoffs.Where(x => x.Position.DistanceTo(Game.LocalPlayer.Character) < 30 && x.SuitableForVeh(playerVehicle)))
                {
                    Vehicle nearcar = (Vehicle)World.GetClosestEntity(dr.Position, 5, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle);
                    if (nearcar)
                    {
                        nearcar.Delete();
                    }
                }
            }
        }
        private static bool isPlayerNearPoliceJail()
        {

            Ped playerPed = Game.LocalPlayer.Character;
            Vehicle car = playerPed.CurrentVehicle;
            if (car == null) { return false; }
            foreach (JailDropoff dr in JailDropoff.AllJailDropoffs.Where(x => x.SuitableForVeh(Game.LocalPlayer.Character.CurrentVehicle)))
            {
                if (Vector3.Distance(car.Position, dr.Position) < 4f)
                {
                    return true;
                }
            }
            return false;

        }

        private static JailDropoff getNearbyJailDropoff()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vehicle car = playerPed.CurrentVehicle;

            if (!car)
            {
                return null;
            }
            checkForJail = false;
            //For all police stations check distance & return closest index
            foreach (JailDropoff dr in JailDropoff.AllJailDropoffs.Where(x => x.SuitableForVeh(Game.LocalPlayer.Character.CurrentVehicle)))
            {
                if (Vector3.Distance(car.Position, dr.Position) < 4f)
                {
                    checkForJail = true;
                    return dr;
                }
            }
            checkForJail = true;
            return null;
        }
        private static void suspectOutOfCar(Ped suspect, Vehicle car, Ped cop)
        {
            //Game.DisplayNotification("An officer is dealing with ~r~" + Functions.GetPersonaForPed(suspect).FullName + ".");

            //Suspect get out
            Rage.Task leaveVehicle = suspect.Tasks.LeaveVehicle(Rage.LeaveVehicleFlags.None);
            leaveVehicle.WaitForCompletion();

            //Go out of view
            Rage.Task suspecttocop = suspect.Tasks.FollowNavigationMeshToPosition(cop.GetOffsetPosition(Vector3.RelativeFront * 1.5f), cop.Heading + 180, 1.4f);
            suspecttocop.WaitForCompletion(7000);

            Rage.Task suspectgoaway = suspect.Tasks.FollowNavigationMeshToPosition(car.GetOffsetPosition(Vector3.RelativeBack * 8f), car.Heading + 180f, 1.4f);
            GameFiber.Sleep(500);
            Rage.Task copgoaway = cop.Tasks.FollowNavigationMeshToPosition(car.GetOffsetPosition(Vector3.RelativeBack * 9f), car.Heading + 180f, 1.5f);
            suspectgoaway.WaitForCompletion(5500);



        }

        private static void policeJailer()
        {
            GameFiber.StartNew(delegate
            {
                //Safeguarding

                Ped playerPed = Game.LocalPlayer.Character;
                Vehicle car = playerPed.CurrentVehicle;

                JailDropoff dropoff = getNearbyJailDropoff();
                if (dropoff == null)
                {
                    canChoose = true;
                    checkForJail = true;
                    return;
                }

                Ped suspect = getSuspectFromVehicle();
                if (suspect == null)
                {
                    canChoose = true;
                    checkForJail = true;
                    return;
                }

                //Jailing
                if (car.Speed < 0.25f)
                {
                    try
                    {
                        Game.LocalPlayer.HasControl = false;
                        Game.FadeScreenOut(2500);
                        //Put car in specified position
                        if (EntryPoint.IsLSPDFRPlusRunning)
                        {
                            API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Prisoners taken to jail");
                        }
                        GameFiber.Sleep(3200);
                        car.Heading = dropoff.Heading;
                        car.Position = dropoff.Position;
                        if (suspect.Exists())
                        {
                            suspect.Model.LoadAndWait();
                        }
                        else
                        {
                            Game.LogTrivial("Suspect to jail no longer exists - possible conflict with default lspdfr jail.");
                            if (!Game.IsScreenFadedIn)
                            {
                                Game.FadeScreenIn(100);
                            }
                            return;
                        }
                        
                        if (dropoff.HasCells)
                        {
                            int ans = SpeechHandler.DisplayAnswers(new List<string>() { "Take " + Functions.GetPersonaForPed(suspect).FullName + " in yourself.", "Hand " + Functions.GetPersonaForPed(suspect).FullName + " over to another officer." });

                            if (ans == 1)
                            {
                                TakeSuspectOut(suspect, car);
                            }
                            else if (ans == 0)
                            {
                                if (dropoff.OfficerCutscene)
                                {
                                    TakeSuspectIntoCells(suspect);
                                }
                                else
                                {
                                    suspect.Delete();
                                    Game.FadeScreenIn(2000);
                                }
                            }
                        }
                        else
                        {
                            if (dropoff.OfficerCutscene)
                            {
                                TakeSuspectOut(suspect, car);
                            }
                            else
                            {
                                suspect.Delete();
                                Game.FadeScreenIn(2000);
                            }
                            
                        }
                    }
                    catch (Exception e)
                    {
                        Game.LogTrivial(e.ToString());
                        Game.LogTrivial("Place in cells exception handled.");
                        if (!Game.IsScreenFadedIn)
                        {
                            Game.FadeScreenIn(100);
                        }
                    }
                }
                Game.LocalPlayer.HasControl = true;

                transportMessageReceived = false;
                canChoose = true;
                checkForJail = true;
            });
        }

        private static void TakeSuspectOut(Ped suspect, Vehicle car)
        {
            Camera cam = new Camera(true);
            cam.Position = Game.LocalPlayer.Character.CurrentVehicle.GetOffsetPosition(Vector3.RelativeBack * 7.5f);
            cam.Rotation = Game.LocalPlayer.Character.Rotation;
            cam.SetPositionZ(cam.Position.Z + 2f);
            Vector3 pos;
            if (suspect.SeatIndex == 1)
            {
                pos = car.GetOffsetPosition(Vector3.RelativeLeft * 1.3f);

            }
            else
            {
                pos = car.GetOffsetPosition(Vector3.RelativeRight * 1.3f);
            }
            Ped cop = new Ped("s_m_m_prisguard_01", pos, car.Heading + 180);
            cop.BlockPermanentEvents = true;
            cop.IsInvincible = true;
            int seatIndex = suspect.SeatIndex;
            //Suspect handling

            suspect.BlockPermanentEvents = true;
            suspect.IsInvincible = true;

            GameFiber.Sleep(1000);
            Game.FadeScreenIn(2000);
            GameFiber.Sleep(2000);

            if (suspect.Exists())
            {
                suspect = suspect.ClonePed(true);
                suspect.Tasks.PlayAnimation("mp_arresting", "idle", 8f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);
                //Get suspect out of car and repair vehicle
                suspectOutOfCar(suspect, car, cop);
                Game.FadeScreenOut(1500);
                GameFiber.Sleep(1500);
                if (car.Exists())
                {
                    car.Repair();
                    car.Wash();
                }
                if (cop.Exists())
                {
                    cop.Delete();
                }
                if (suspect.Exists())
                {
                    suspect.Delete();
                }

                GameFiber.Sleep(500);
                Game.FadeScreenIn(2000);
                messageReceived = false;
                jailRouteMessageReceived = false;

                if (suspectsArrestedByPlayer.Contains(suspect))
                {
                    suspectsArrestedByPlayer.Remove(suspect);
                }
            }
            else
            {
                Game.FadeScreenIn(2000);
                if (cop.Exists())
                {
                    cop.Delete();
                }
            }
            if (cam.Exists()) { cam.Delete(); }
            GameFiber.Sleep(2000);
        }

        private static void TakeSuspectIntoCells(Ped suspect)
        {
            Game.LocalPlayer.HasControl = true;
            Vehicle Playercar = Game.LocalPlayer.Character.CurrentVehicle;
            Vector3 localplayerlastpos = Game.LocalPlayer.Character.Position;
            bool PlayercarWasPersistent = Playercar.IsPersistent;
            Playercar.IsPersistent = true;
            suspect.IsPersistent = true;
            Game.LocalPlayer.Character.Position = new Vector3(464.4409f, -1007.447f, 25.56135f);
            Game.LocalPlayer.Character.Heading = 355.8362f;

            suspect.Tasks.PlayAnimation("mp_arresting", "idle", 8f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);
            suspect.Position = new Vector3(464.4852f, -1009.838f, 25.56136f);
            suspect.Heading = 2.890372f;
            GameFiber.Sleep(3000);
            Game.FadeScreenIn(3000);
            Game.HideHelp();
            GameFiber.Sleep(3000);

            Vector3 OutsideDoor = new Vector3(464.4852f, -1009.838f, 25.56136f);
            Blip OutsideDoorBlip = new Blip(OutsideDoor);
            OutsideDoorBlip.Sprite = BlipSprite.ScriptObjective;
            Game.DisplayHelp("Place ~r~" + Functions.GetPersonaForPed(suspect).FullName + " ~s~in a cell and leave.", true);
            ulong _DOOR_CONTROL = 0x9b12f9a24fabedb0;
            while (true)
            {
                GameFiber.Yield();
                Game.DisplayHelp("Place ~r~" + Functions.GetPersonaForPed(suspect).FullName + " ~s~in a cell and leave.", true);
                NativeFunction.CallByHash<uint>(_DOOR_CONTROL, 631614199, 464.5701f, -992.6641f, 25.06443f, true, 0f, 0f, 0f); //Far cell door
                NativeFunction.CallByHash<uint>(_DOOR_CONTROL, 2271212864, 467.3716f, -1014.452f, 26.53623f, true, 0f, 0f, 0f); //Outside door 1
                NativeFunction.CallByHash<uint>(_DOOR_CONTROL, 2271212864, 469.9679f, -1014.452f, 26.53623f, true, 0f, 0f, 0f); //Outside door 2
                if (Vector3.Distance(Game.LocalPlayer.Character.Position, OutsideDoor) < 2f)
                {
                    if (Vector3.Distance(Game.LocalPlayer.Character.Position, suspect.Position) > 6f)
                    {
                        break;
                    }

                }
                else if (Vector3.Distance(Game.LocalPlayer.Character.Position, OutsideDoor) > 50f)
                {
                    break;
                }

            }
            Game.HideHelp();
            Game.FadeScreenOut(3000);
            GameFiber.Wait(3000);
            NativeFunction.CallByHash<uint>(_DOOR_CONTROL, 631614199, 464.5701f, -992.6641f, 25.06443f, false, 0f, 0f, 0f); //Far cell door
            NativeFunction.CallByHash<uint>(_DOOR_CONTROL, 2271212864, 467.3716f, -1014.452f, 26.53623f, false, 0f, 0f, 0f); //Outside door 1
            NativeFunction.CallByHash<uint>(_DOOR_CONTROL, 2271212864, 469.9679f, -1014.452f, 26.53623f, false, 0f, 0f, 0f); //Outside door 2
                                                                                                                             //if (suspect.Exists()) { suspect.Delete(); }
            if (OutsideDoorBlip.Exists()) { OutsideDoorBlip.Delete(); }
            if (Playercar.Exists())
            {
                Game.LocalPlayer.Character.WarpIntoVehicle(Playercar, -1);


                Playercar.IsPersistent = PlayercarWasPersistent;
            }
            else
            {
                Game.LocalPlayer.Character.Position = localplayerlastpos;
            }
            //Game.LogTrivial("Ped in prison: " + Functions.IsPedInPrison(suspect).ToString());
            GameFiber.Sleep(2000);
            Game.FadeScreenIn(3000);
            GameFiber.Sleep(3000);
        }

        private static float oldDistance { get; set; }
        private static Blip currentJailRouteBlip { get; set; }
        private static void createRouteToNearestJail()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            oldDistance = 100000f;
            float newDistance;
            foreach (JailDropoff dr in JailDropoff.AllJailDropoffs.Where(x => x.SuitableForVeh(Game.LocalPlayer.Character.CurrentVehicle)))
            {
                newDistance = Vector3.Distance(dr.Position, playerPed.Position);
                if (newDistance < oldDistance)
                {
                    dr.blip.IsRouteEnabled = true;
                    currentJailRouteBlip = dr.blip;
                    oldDistance = newDistance;
                }
            }
            float jail1Distance = Vector3.Distance(playerPed.Position, new Vector3(479.8365f, -1021.213f, 27.58666f));

            if (oldDistance > jail1Distance)
            {

                currentJailRouteBlip.IsRouteEnabled = false;
                currentJailRouteBlip = new Blip(new Vector3(479.8365f, -1021.213f, 27.58666f));
                currentJailRouteBlip.Alpha = 0.0f;
                currentJailRouteBlip.Color = System.Drawing.Color.Yellow;
                currentJailRouteBlip.IsRouteEnabled = true;
            }


        }
        #endregion
        //Main thing
        internal static List<Ped> suspectsArrestedByPlayer { get; set; }
        private static List<Ped> SuspectsNotArrestedByPlayer = new List<Ped>();
        private static void isSomeoneGettingArrestedByPlayer()
        {

            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    try
                    {
                        Ped playerPed = Game.LocalPlayer.Character;
                        if (playerPed.Exists() && playerPed.IsValid())
                        {
                            if (playerPed.GetNearbyPeds(1).Length == 1)
                            {
                                Ped nearestPed = playerPed.GetNearbyPeds(1)[0];
                                if (Functions.IsPedArrested(nearestPed))
                                {
                                    arrestingOfficer = Functions.GetPedArrestingOfficer(nearestPed);
                                    if ((arrestingOfficer == playerPed) && !suspectsArrestedByPlayer.Contains(nearestPed))
                                    {
                                        suspectsArrestedByPlayer.Add(nearestPed);

                                        suspectsWithVehicles.Add(nearestPed, nearestPed.LastVehicle);
                                        if (suspectsWithVehicles.ContainsKey(nearestPed)) { Game.LogTrivial("Contains key after add"); }
                                        NativeFunction.Natives.SET_PED_DROPS_WEAPON(nearestPed); Game.LogTrivial("Weapon Dropped");
                                        API.Functions.OnPlayerArrestedPed(nearestPed);



                                    }

                                    else if (!SuspectsNotArrestedByPlayer.Contains(nearestPed) && !suspectsArrestedByPlayer.Contains(nearestPed))
                                    {


                                        Game.LogTrivial("Adding suspect not arrested by player");
                                        SuspectsNotArrestedByPlayer.Add(nearestPed);


                                    }
                                }
                            }
                        }

                    }
                    catch (ThreadAbortException) { break; }
                    catch (Exception e)
                    {

                        Game.LogTrivial(e.ToString());
                        //Game.LogTrivial("Arrest Manager handled the ArrByP exception successfully");
                    }
                }
            });
        }
        private static void CheckForAllTypesOfSuspect()
        {

            GameFiber.StartNew(delegate
                 {
                     while (true)
                     {

                         GameFiber.Yield();
                         try
                         {
                             try
                             {
                                 suspectAPI = getSuspectAPI();
                                 GameFiber.Yield();
                                 twoSuspectsApi = getTwoSuspectAPI();
                                 GameFiber.Yield();
                             }
                             catch (Exception)
                             {

                                 suspectAPI = null;
                                 twoSuspectsApi = new List<Ped>();

                             }



                             try
                             {

                                 if (suspectAPI != null)
                                 {

                                     if (suspectAPI.Exists())
                                     {
                                         suspectAPI.RelationshipGroup = "ARRESTEDSUSPECTS";
                                         suspectName = Functions.GetPersonaForPed(suspectAPI).FullName;
                                     }

                                     if (suspectName != oldSuspectName)
                                     {
                                         transportMessageReceived = false;
                                     }

                                 }

                             }

                             catch (System.NullReferenceException)
                             {
                                 suspectName = oldSuspectName;
                             }
                             suspectFromVehicle = getSuspectFromVehicle();
                         }
                         catch (ThreadAbortException) { break; }
                         catch (Exception e) { Game.LogTrivial(e.ToString()); }
                     }
                 });
        }

        internal static bool canTransportBeCalled()
        {
            return EntryPoint.canChoose && EntryPoint.suspectAPI != null && EntryPoint.suspectsArrestedByPlayer.Contains(EntryPoint.suspectAPI) && !EntryPoint.suspectsPendingTransport.Contains(EntryPoint.suspectAPI);
        }

        private static void CheckForKeyInput()
        {
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();
                    try
                    {


                        if (canChoose)
                        {

                            if ((Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(releaseModifierKey)) || (releaseModifierKey == Keys.None))
                            {
                                if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(releaseKey))
                                {
                                    if (suspectFromVehicle != null && !suspectsPendingTransport.Contains(suspectFromVehicle))
                                    {
                                        canChoose = false; releaseSuspectFromVehicle();
                                    }
                                }
                            }
                            if ((Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(transportModifierKey)) || (transportModifierKey == Keys.None))
                            {
                                if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(transportKey))
                                {
                                    if (suspectAPI != null && suspectsArrestedByPlayer.Contains(suspectAPI))
                                    {
                                        if (!suspectsPendingTransport.Contains(suspectAPI))
                                        {
                                            if (EntryPoint.twoSuspectsApi.Count == 2 && !ExtensionMethods.IsPointOnWater(Game.LocalPlayer.Character.Position))
                                            {

                                                if (EntryPoint.twoSuspectsApi[0].Exists() && EntryPoint.twoSuspectsApi[1].Exists() && EntryPoint.suspectsArrestedByPlayer.Contains(EntryPoint.twoSuspectsApi[0]) &&
                                                    EntryPoint.suspectsArrestedByPlayer.Contains(EntryPoint.twoSuspectsApi[1]) && !EntryPoint.suspectsPendingTransport.Contains(EntryPoint.twoSuspectsApi[0]) &&
                                                        !EntryPoint.suspectsPendingTransport.Contains(EntryPoint.twoSuspectsApi[1]))
                                                {

                                                    if (Vector3.Distance(EntryPoint.twoSuspectsApi[0].Position, EntryPoint.twoSuspectsApi[1].Position) < 25f)
                                                    {
                                                        TransportMenu.Visible = true;
                                                        TransportMenu.CurrentSelection = 2;
                                                        continue;

                                                    }
                                                }
                                            }

                                            canChoose = false;

                                            Game.RemoveNotification(multiVanOnStandbyMsg);
                                            Game.RemoveNotification(vanOnStandbyMsg);
                                            Ped officer;
                                            Vehicle van;
                                            if (RecruitNearbyOfficer(out officer, out van))
                                            {
                                                Game.LogTrivial("Recruited Officer");
                                                SuspectTransporterRecruitedOfficer(officer, van);
                                            }
                                            else if (ExtensionMethods.IsPointOnWater(Game.LocalPlayer.Character.Position))
                                            {
                                                BoatTransport();
                                            }
                                            else
                                            {
                                                SuspectTransporterNewOfficer();
                                            }
                                        }
                                    }
                                }
                            }

                            if ((Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(jailRouteModifierKey)) || (jailRouteModifierKey == Keys.None))
                            {
                                if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(jailRouteKey))
                                {
                                    if (suspectFromVehicle != null)
                                    {

                                        createRouteToNearestJail();

                                    }
                                }
                            }
                        }
                    }

                    catch (System.Threading.ThreadAbortException) { break; }
                    catch (Exception e)
                    {

                        Game.LogTrivial(e.ToString());
                        Game.LogTrivial("Arrest Manager handled keyinput exception.");

                    }
                }
            });
        }


        private static string suspectName = "";
        private static string oldSuspectName = "";
        internal static Random rnd = new Random();
        private static bool checkingForArrestedByPlayer { get; set; }

        private static Ped arrestingOfficer { get; set; }
        private static bool checkingKeyInput { get; set; }
        private static bool canIWarpToJail { get; set; }
        internal static Ped suspectAPI { get; set; }
        internal static List<Ped> twoSuspectsApi = new List<Ped>();
        internal static Ped suspectFromVehicle { get; set; }
        private static bool multiTransportMessageReceived { get; set; }
        private static bool releaseMessageReceived { get; set; }
        //private static List<Model> cityCopModelName = new List<Model>();
        //private static List<Model> countrysideCopModelName = new List<Model>();
        private static bool OfficerAudio { get; set; }
        internal static uint vanOnStandbyMsg;
        internal static uint multiVanOnStandbyMsg;
        private static bool DispatchVoice { get; set; }
        private static float transportSpawnDistance { get; set; }
        public static KeysConverter kc = new KeysConverter();
        private static List<string> namesUsed { get; set; }
        private static bool RecruitedOfficerMustComplyWithStandards = true;
        private static bool AllowRecruitedOfficers = true;
        private static Dictionary<Ped, Vehicle> suspectsWithVehicles = new Dictionary<Ped, Vehicle>();
        private static List<TransportRegion> TransportRegions;
        private static List<TransportWorldDistrict> TransportWorldDistricts;
        private static List<Ped> OfficersCurrentlyTransporting = new List<Ped>();
        public static bool IsLSPDFRPlusRunning = false;
        public static Guid LSPDFRPlusSecurityGuid;
        private static bool TransportSirenSoundEnabled = false;
        private static bool TransportSirenLightsEnabled = true;
        public static bool AllowWarping = true;

        public static void choice()
        {
            //Read keys & set variables
            GameFiber.StartNew(delegate
            {

                Ped playerPed = Game.LocalPlayer.Character;
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
                TransportRegions = XMLStuff.LoadTransportRegionsFromXMLFile("Plugins/LSPDFR/Arrest Manager/Transport Regions.xml");
                TransportWorldDistricts = XMLStuff.LoadTransportWorldDistrictsFromXMLFile("Plugins/LSPDFR/Arrest Manager/Transport World Districts.xml");
                Game.LogTrivial("TransportWorldDistricts info:");
                foreach (TransportWorldDistrict reg in TransportWorldDistricts)
                {
                    GameFiber.Yield();
                    string s = "";
                    s += Environment.NewLine + "World District: " + reg.WorldDistrict;
                    s += Environment.NewLine + "Drivers:";
                    foreach (Model drivermod in reg.DriverModels)
                    {
                        s += drivermod.Name + " ";
                    }
                    s += Environment.NewLine + "Passengers:";
                    foreach (Model drivermod in reg.PassengerModels)
                    {
                        s += drivermod.Name + " ";
                    }
                    s += Environment.NewLine + "Vehsettings:";
                    foreach (VehicleSettings vehsets in reg.VehSettings)
                    {
                        s += " Model: " + vehsets.VehicleModel.Name;
                        s += "Livs: " + vehsets.LiveryNumber.ToString() + " Extras: " + string.Join(", ", vehsets.ExtraNumbers.Select(v => v.ToString()));
                    }
                    Game.LogTrivial(s);
                }

                Game.LogTrivial("Transportregion info:");
                foreach (TransportRegion reg in TransportRegions)
                {
                    GameFiber.Yield();
                    string s = "";
                    s += Environment.NewLine + "Zonename: " + reg.ZoneName;
                    s += Environment.NewLine + "Drivers:";
                    foreach (Model drivermod in reg.DriverModels)
                    {
                        s += drivermod.Name + " ";
                    }
                    s += Environment.NewLine + "Passengers:";
                    foreach (Model drivermod in reg.PassengerModels)
                    {
                        s += drivermod.Name + " ";
                    }
                    s += Environment.NewLine + "Vehsettings:";
                    foreach (VehicleSettings vehsets in reg.VehSettings)
                    {
                        s += " Model: " + vehsets.VehicleModel.Name;
                        s += "Livs: " + vehsets.LiveryNumber.ToString() + " Extras: " + string.Join(", ", vehsets.ExtraNumbers.Select(v => v.ToString()));
                    }

                    Game.LogTrivial(s);
                }

                try
                {
                    transportKey = (Keys)kc.ConvertFromString(getTransportKey());
                    jailRouteKey = (Keys)kc.ConvertFromString(getJailRouteKey());
                    releaseKey = (Keys)kc.ConvertFromString(getReleaseKey());
                    transportModifierKey = (Keys)kc.ConvertFromString(getTransportModifierKey());
                    jailRouteModifierKey = (Keys)kc.ConvertFromString(getJailRouteModifierKey());
                    releaseModifierKey = (Keys)kc.ConvertFromString(getReleaseModifierKey());
                    SceneManagementKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "SceneManagementKey", "H"));
                    SceneManagementModifierKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "SceneManagementModifierKey", "LControlKey"));
                    PedManager.GrabPedKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "GrabPedKey", "T"));
                    PedManager.GrabPedModifierKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "GrabPedModifierKey", "LShiftKey"));
                    PedManager.PlacePedInVehicleKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PlacePedInVehicleKey"));
                    PedManager.TackleKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "TackleKey", "E"));
                    PedManager.TackleButton = initialiseFile().ReadEnum<ControllerButtons>("Keybindings", "TackleButton", ControllerButtons.A);
                    
                    autoDoorEnabled = bool.Parse(getAutoDoorEnabled());
                    OfficerAudio = initialiseFile().ReadBoolean("Misc", "OfficerAudio", true);
                    DispatchVoice = initialiseFile().ReadBoolean("Misc", "DispatchAudio", true);
                    AllowRecruitedOfficers = initialiseFile().ReadBoolean("Misc", "AllowRecruitedOfficers");
                    RecruitedOfficerMustComplyWithStandards = initialiseFile().ReadBoolean("Misc", "RecruitedOfficerMustComplyWithStandards");
                    string Towtruckcolor = initialiseFile().ReadString("Misc", "TowTruckColourOverride", "");
                    if (!string.IsNullOrWhiteSpace(Towtruckcolor))
                    {
                        VehicleManager.TowTruckColour = System.Drawing.Color.FromName(Towtruckcolor);
                        VehicleManager.OverrideTowTruckColour = true;
                    }
                    VehicleManager.TowtruckModel = initialiseFile().ReadString("Misc", "TowTruckModel", "TOWTRUCK");
                    if (!VehicleManager.TowtruckModel.IsValid)
                    {
                        VehicleManager.TowtruckModel = "TOWTRUCK";
                        Game.LogTrivial("Invalid Tow Truck Model in Arrest Manager.ini. Setting to TOWTRUCK.");
                    }

                    VehicleManager.FlatbedModel = initialiseFile().ReadString("Misc", "FlatbedModel", "FLATBED");
                    if (!VehicleManager.FlatbedModel.IsValid)
                    {
                        VehicleManager.FlatbedModel = "FLATBED";
                        Game.LogTrivial("Invalid Flatbed Model in Arrest Manager.ini. Setting to FLATBED.");
                    }
                    VehicleManager.AlwaysFlatbed = initialiseFile().ReadBoolean("Misc", "AlwaysUseFlatbed", false);
                    VehicleManager.FlatbedModifier = new Vector3(initialiseFile().ReadSingle("Misc", "FlatbedX", -0.5f), initialiseFile().ReadSingle("Misc", "FlatbedY", -5.75f),
                        initialiseFile().ReadSingle("Misc", "FlatbedZ", 1.005f));


                    TransportSirenSoundEnabled = initialiseFile().ReadBoolean("Misc", "TransportSirenSoundEnabled", false);
                    TransportSirenLightsEnabled = initialiseFile().ReadBoolean("Misc", "TransportSirenLightsEnabled", true);
                    AllowWarping = initialiseFile().ReadBoolean("Misc", "AllowWarping", true);

                    getTransportSpawnDistance();
                    getSceneManagementSpawnDistance();
                    VehicleManager.RecruitNearbyTowTrucks = initialiseFile().ReadBoolean("Misc", "RecruitNearbyTowTrucks");
                    Coroner.coronerModel = initialiseFile().ReadString("Misc", "CoronerPedModel", "S_M_M_DOCTOR_01");
                    if (!Coroner.coronerModel.IsValid)
                    {
                        Coroner.coronerModel = "S_M_M_DOCTOR_01";
                    }

                    Coroner.coronerVehicleModel = initialiseFile().ReadString("Misc", "CoronerVehicleModel", "SPEEDO");
                    if (!Coroner.coronerVehicleModel.IsValid || !Coroner.coronerVehicleModel.IsVehicle)
                    {
                        Game.LogTrivial("Arrest Manager: The specified coroner vehicle is either invalid or not a vehicle. Use at own risk! " + Coroner.coronerVehicleModel.Name);
                        
                    }

                }
                catch
                {
                    transportKey = Keys.D8; jailRouteKey = Keys.D0; releaseKey = Keys.D6;
                    transportSpawnDistance = 85f;
                    SceneManagementKey = Keys.H;
                    SceneManagementModifierKey = Keys.LControlKey;
                    transportModifierKey = Keys.None;
                    jailRouteModifierKey = Keys.None;
                    SceneManagementSpawnDistance = 70f;
                    releaseKey = Keys.None;
                    DispatchVoice = true;
                    OfficerAudio = true;
                    autoDoorEnabled = true;
                    Game.DisplayNotification("~r~~h~Error while reading Arrest Manager.ini. Replace with default from download! Loading default settings...");
                }

                GameFiber.Wait(4000);

                //Game.DisplayNotification("~b~Arrest Manager ~s~by ~b~Albo1125 ~s~has been loaded ~g~successfully!");
                Game.LogTrivial("Arrest Manager by Albo1125 has loaded successfully.");
                IsLSPDFRPlusRunning = IsLSPDFRPluginRunning("LSPDFR+", new Version("1.7.0.0"));
                //if (IsLSPDFRPlusRunning)
                //{
                //    LSPDFRPlusSecurityGuid = API.LSPDFRPlusFuncs.GenerateSecurityGuid("Arrest Manager", "Albo1125", "mOd2ZqIqjCtW/9ysfN4z5wSFZnZ+1GPr8acNMkdPoauakZAeXsOVs9m+ythvWn1P1b/LAiDKwvQIF7vAJ5ka+E33OFOqTC7DByE4eJfRFOUEIn8eKWA6h2x+YJhJcMhIoCCrn3itFNDTgWbcA9uDJE9z1I2MDq2uH8Nd6icz1IQ=");
                //}
                if (IsLSPDFRPluginRunning("PoliceSmartRadio"))
                {
                    API.SmartRadioFuncs.AddActionToButton(Coroner.smartRadioMain, Coroner.CanBeCalled, "coroner");
                    API.SmartRadioFuncs.AddActionToButton(VehicleManager.smartRadioTow, "tow");
                    API.SmartRadioFuncs.AddActionToButton(API.SmartRadioFuncs.RequestTransport, canTransportBeCalled, "transport");
                }
                if (IsLSPDFRPluginRunning("VocalDispatch", new Version("1.6.0.0")))
                {
                    API.VocalDispatchHelper vc_coroner = new API.VocalDispatchHelper();
                    vc_coroner.SetupVocalDispatchAPI("ArrestManager.Coroner", new API.VocalDispatchHelper.VocalDispatchEventDelegate(Coroner.vc_main));
                }
                messageReceived = false;
                transportMessageReceived = false;
                jailRouteMessageReceived = false;
                multiTransportMessageReceived = false;
                canChoose = true;
                checkForJail = true;
                createJailDropoffs();
                suspectsPendingTransport = new List<Ped>();
                twoSuspectsApi = new List<Ped>();
                namesUsed = new List<string>();

                int clearJailSpotsCount = 0;
                arrestingOfficer = null;
                suspectsArrestedByPlayer = new List<Ped>();
                canIWarpToJail = true;

                checkingForArrestedByPlayer = false;
                releaseMessageReceived = false;
                bool doorsClosed = false;

                SceneManager.CreateMenus();
                InitialiseTransportMenu();
                CheckForAllTypesOfSuspect();
                isSomeoneGettingArrestedByPlayer();
                CheckForKeyInput();
                //Listens for key input and calls appropriate method

                try
                {
                    while (true)
                    {

                        Game.LocalPlayer.WantedLevel = 0;
                        playerPed = Game.LocalPlayer.Character;
                        GameFiber.Yield();
                        Game.SetRelationshipBetweenRelationshipGroups("PLAYER", "ARRESTEDSUSPECTS", Relationship.Respect);
                        Game.SetRelationshipBetweenRelationshipGroups("ARRESTEDSUSPECTS", "PLAYER", Relationship.Respect);
                        Game.SetRelationshipBetweenRelationshipGroups("COP", "ARRESTEDSUSPECTS", Relationship.Respect);
                        Game.SetRelationshipBetweenRelationshipGroups("ARRESTEDSUSPECTS", "COP", Relationship.Respect);
                        //while you have the option to choose, listen for input
                        while (canChoose)
                        {
                            GameFiber.Yield();
                            playerPed = Game.LocalPlayer.Character;


                            //Check: first for warp, release, then for multi, then for single



                            if ((transportKey != Keys.None) && (suspectFromVehicle == null) && (suspectAPI != null) && !transportMessageReceived && suspectsArrestedByPlayer.Contains(suspectAPI))
                            {
                                if (suspectAPI.Exists() && !suspectsPendingTransport.Contains(suspectAPI))
                                {
                                    if (suspectName != null)
                                    {
                                        if (!namesUsed.Contains(suspectName))
                                        {
                                            if (!string.IsNullOrWhiteSpace(suspectName))
                                            {
                                                if (suspectName == Functions.GetPersonaForPed(suspectAPI).FullName)
                                                {


                                                    vanOnStandbyMsg = Game.DisplayNotification(String.Format("Transport on standby for~r~ {0}. ~s~Press ~b~" + kc.ConvertToString(transportKey) + ".", suspectName));
                                                    transportMessageReceived = true;
                                                    namesUsed.Add(suspectName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if ((suspectFromVehicle == null) && (twoSuspectsApi.Count == 2) && !multiTransportMessageReceived) //&& (multiTransportKey != Keys.None)
                            {
                                if ((suspectsArrestedByPlayer.Contains(twoSuspectsApi[0])) && suspectsArrestedByPlayer.Contains(twoSuspectsApi[1]))
                                {
                                    if (!suspectsPendingTransport.Contains(twoSuspectsApi[0]) && !suspectsPendingTransport.Contains(twoSuspectsApi[1]))
                                    {
                                        if (Vector3.Distance(twoSuspectsApi[0].Position, twoSuspectsApi[1].Position) < 20f)
                                        {
                                            if (twoSuspectsApi[0].Exists() && twoSuspectsApi[1].Exists())
                                            {

                                                multiVanOnStandbyMsg = Game.DisplayNotification(String.Format("Transport ready for~r~ {0} ~s~and ~r~{1}~s~. Press ~b~" + kc.ConvertToString(transportKey) + ".", Functions.GetPersonaForPed(twoSuspectsApi[0]).FullName, Functions.GetPersonaForPed(twoSuspectsApi[1]).FullName));
                                                multiTransportMessageReceived = true;
                                                Game.RemoveNotification(vanOnStandbyMsg);
                                                namesUsed.Add(Functions.GetPersonaForPed(twoSuspectsApi[0]).FullName);
                                                namesUsed.Add(Functions.GetPersonaForPed(twoSuspectsApi[1]).FullName);
                                            }

                                        }
                                    }
                                }
                            }




                            //Manage blips, jailing
                            if (suspectFromVehicle != null)
                            {
                                foreach (Blip blip in JailDropoff.AllJailDropoffs.Where(x => x.SuitableForVeh(Game.LocalPlayer.Character.CurrentVehicle)).Select(x => x.blip))
                                {
                                    //if (blip.Alpha != 1f)
                                    //{
                                    //    blip.Alpha = 1f;
                                    //}
                                    if (!NativeFunction.Natives.IS_BLIP_ON_MINIMAP<bool>(blip))
                                    {
                                        NativeFunction.Natives.SET_BLIP_DISPLAY(blip, 8);
                                    }
                                }
                                clearJailSpotsCount++;
                                if (clearJailSpotsCount >= 35)
                                {
                                    clearJailSpots();
                                    clearJailSpotsCount = 0;
                                }

                                if (!jailRouteMessageReceived && (jailRouteKey != Keys.None))
                                {
                                    Game.DisplayNotification("Press ~b~" + kc.ConvertToString(jailRouteKey) + "~s~ to calculate the route to the nearest ~b~jail.");
                                    jailRouteMessageReceived = true;
                                }
                                if (!releaseMessageReceived && (releaseKey != Keys.None))
                                {
                                    Game.DisplayNotification("Press ~b~" + kc.ConvertToString(releaseKey) + "~s~ to ~g~release ~r~" + suspectName + ".");
                                    releaseMessageReceived = true;
                                }

                                if (checkForJail && isPlayerNearPoliceJail() && Game.LocalPlayer.Character.IsInAnyVehicle(false))
                                {
                                    checkForJail = false;
                                    canChoose = false;
                                    policeJailer();


                                }
                            }
                            else
                            {
                                foreach (Blip blip in JailDropoff.AllJailDropoffs.Select(x => x.blip))
                                {
                                    if (NativeFunction.Natives.IS_BLIP_ON_MINIMAP<bool>(blip))
                                    {
                                        NativeFunction.Natives.SET_BLIP_DISPLAY(blip, 3);
                                    }
                                    blip.IsRouteEnabled = false;
                                }
                                if (currentJailRouteBlip.Exists())
                                {
                                    currentJailRouteBlip.IsRouteEnabled = false;
                                }
                                jailRouteMessageReceived = false;
                                messageReceived = false;
                                releaseMessageReceived = false;
                            }
                            if (autoDoorEnabled)
                            {
                                if (playerPed.IsInAnyVehicle(false))
                                {
                                    if (playerPed.CurrentVehicle.Driver == playerPed)
                                    {
                                        if (playerPed.CurrentVehicle.Speed > 3f)
                                        {
                                            if (!doorsClosed)
                                            {

                                                doorsClosed = true;
                                                Rage.Native.NativeFunction.Natives.SET_VEHICLE_DOORS_SHUT(playerPed.CurrentVehicle, true);
                                            }
                                        }
                                    }
                                }
                                else
                                {

                                    doorsClosed = false;
                                }
                            }


                            oldSuspectName = suspectName;
                            Game.LocalPlayer.WantedLevel = 0;
                            //GameFiber.Yield();
                        }
                    }

                }
                catch (System.Threading.ThreadAbortException e)
                {
                    foreach (Blip blip in JailDropoff.AllJailDropoffs.Select(x => x.blip))
                    {
                        if (blip.Exists())
                        {
                            blip.Delete();
                        }
                    }
                    throw e;

                }


            });
        }
        //Eventhandlers for on/off duty


        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName();
                if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0) { return true; }
                }
            }
            return false;
        }
        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args) { foreach (Assembly assembly in Functions.GetAllUserPlugins()) { if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower())) { return assembly; } } return null; }


        public static UIMenu TransportMenu;
        private static UIMenuItem SingleTransportItem;
        private static UIMenuItem MultipleSingleTransportItem;
        private static UIMenuItem MultiTransportItem;
        private static MenuPool TransportMenuPool = new MenuPool();
        private static void InitialiseTransportMenu()
        {
            TransportMenu = new UIMenu("Transport", "~b~Select");
            TransportMenu.AddItem(SingleTransportItem = new UIMenuItem("Single transport"));
            TransportMenu.AddItem(MultipleSingleTransportItem = new UIMenuItem("Multiple single transports"));
            TransportMenu.AddItem(MultiTransportItem = new UIMenuItem("Multi transport"));

            TransportMenuPool.Add(TransportMenu);
            TransportMenu.OnItemSelect += OnItemSelect;
            TransportMenu.MouseControlsEnabled = false;
            TransportMenu.AllowCameraMovement = true;
            Game.FrameRender += Process;

        }
        private static Stopwatch CanChooseStopwatch = new Stopwatch();
        public static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender != TransportMenu) { return; }

            if (selectedItem == SingleTransportItem)
            {
                SuspectTransporterNewOfficer();
            }
            else if (selectedItem == MultipleSingleTransportItem)
            {

                SuspectTransporterNewOfficer();
                GameFiber.StartNew(delegate
                {

                    CanChooseStopwatch.Restart();
                    while (suspectAPI != null)
                    {
                        GameFiber.Yield();
                        canChoose = false;
                        if (CanChooseStopwatch.ElapsedMilliseconds > 6000)
                        {
                            SuspectTransporterNewOfficer();
                            CanChooseStopwatch.Restart();
                        }
                    }

                });
            }
            else if (selectedItem == MultiTransportItem)
            {
                multipleSuspectTransporter();
            }
            TransportMenu.Visible = false;
        }

        public static void Process(object sender, GraphicsEventArgs e)
        {
            TransportMenuPool.ProcessMenus();
        }

        internal static void Initialise()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("Arrest Manager, developed by Albo1125, has been loaded successfully!");
                GameFiber.Wait(6000);
                Game.DisplayNotification("~b~Arrest Manager~s~, developed by ~b~Albo1125, ~s~has been loaded ~g~successfully.");
            });
            Game.LogTrivial("Arrest Manager is not in beta.");
            choice();
        }
    }
}