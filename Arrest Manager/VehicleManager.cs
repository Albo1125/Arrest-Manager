using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using System.Windows.Forms;
using RAGENativeUI.Elements;
using RAGENativeUI;
using LSPD_First_Response.Mod.API;
using Rage.Native;
using static Arrest_Manager.SceneManager;
using Albo1125.Common.CommonLibrary;

namespace Arrest_Manager
{
    internal class VehicleManager
    {
        public static Model TowtruckModel = "TOWTRUCK";
        public static Model FlatbedModel = "FLATBED";
        public static bool AlwaysFlatbed = false;
        public static Vector3 FlatbedModifier = new Vector3(-0.5f, -5.75f, 1.005f);
        public static System.Drawing.Color TowTruckColour;
        public static bool OverrideTowTruckColour = false;
        public static bool RecruitNearbyTowTrucks = false;
        public static Random rnd = new Random();
        private  Blip towblip;
        private  Blip carblip;
        private  Vehicle towTruck;
        private  Ped driver;
        private  Vehicle car;
        private static List<Vehicle> TowTrucksBeingUsed = new List<Vehicle>();
        private string modelName;

        internal bool RecruitNearbyTowtruck(out Ped TowDriver, out Vehicle TowTruck)
        {
            if (RecruitNearbyTowTrucks)
            {
                Entity[] nearbypeds = World.GetEntities(Game.LocalPlayer.Character.Position, EntryPoint.SceneManagementSpawnDistance * 0.75f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed);
                nearbypeds = (from x in nearbypeds orderby (Game.LocalPlayer.Character.DistanceTo(x.Position)) select x).ToArray();
                foreach (Entity nearent in nearbypeds)
                {
                    if (nearent.Exists())
                    {
                        Ped nearped = (Ped)nearent;
                        if (nearped.IsInAnyVehicle(false))
                        {
                            if (nearped.CurrentVehicle.HasTowArm && !nearped.CurrentVehicle.TowedVehicle.Exists() && !TowTrucksBeingUsed.Contains(nearped.CurrentVehicle))
                            {
                                TowDriver = nearped;
                                TowDriver.MakeMissionPed();
                                TowTruck = TowDriver.CurrentVehicle;
                                TowTruck.IsPersistent = true;
                                return true;
                            }
                        }
                    }
                }
            }
            TowDriver = null;
            TowTruck = null;
            return false;
        }
        internal static void smartRadioTow()
        {
            new VehicleManager().towVehicle(false);
        }
        internal void towVehicle(bool playanims = true)
        {
            Vehicle[] nearbyvehs = Game.LocalPlayer.Character.GetNearbyVehicles(2);
            if (nearbyvehs.Length == 0)
            {
                Game.DisplayNotification("~r~Couldn't detect a close enough vehicle.");
                return;
            }
            Vehicle car = nearbyvehs[0];
            if (Vector3.Distance(Game.LocalPlayer.Character.Position, car.Position) > 6f)
            {
                Game.DisplayNotification("~r~Couldn't detect a close enough vehicle.");
                return;
            }
            if (car.HasOccupants)
            {
                if (nearbyvehs.Length == 2)
                {
                    car = nearbyvehs[1];
                    if (car.HasOccupants)
                    {
                        Game.DisplayNotification("~r~Couldn't detect a close enough vehicle without occupants.");
                        return;
                    }
                }
                else
                {
                    Game.DisplayNotification("~r~Couldn't detect a close enough vehicle without occupants.");
                    return;
                }

            }
            if (!car.Model.IsCar && !car.Model.IsBike && !car.Model.IsQuadBike)
            {
                Game.DisplayNotification("Unfortunately, this vehicle can't be towed or impounded.");
                return;
            }
            towVehicle(car, playanims);
        }
        
        internal void towVehicle(Vehicle car, bool playanims = true)
        {

            GameFiber.StartNew(delegate
            {
                if(!car.Exists()) { return; }
                try
                {
                    bool flatbed = true;
                    if (car.HasOccupants)
                    {

                        Game.DisplayNotification("Vehicle has occupants. Aborting tow.");
                        return;

                    }
                    if (car.IsPoliceVehicle)
                    {
                        uint noti = Game.DisplayNotification("Are you sure you want to tow the police vehicle? ~h~~b~Y/N");
                        while (true)
                        {
                            GameFiber.Yield();
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(System.Windows.Forms.Keys.Y))
                            {
                                Game.RemoveNotification(noti);
                                break;
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(System.Windows.Forms.Keys.N))
                            {
                                Game.RemoveNotification(noti);
                                return;
                            }
                        }
                        if (!car.Exists()) { return; }
                    }
                    if (!car.Model.IsCar && !car.Model.IsBike && !car.Model.IsQuadBike && !car.Model.IsBoat && !car.Model.IsJetski)
                    {
                        Game.DisplayNotification("Unfortunately, this vehicle can't be towed or impounded.");
                        return;
                    }
                    car.IsPersistent = true;
                    if (playanims)
                    {
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                        GameFiber.Wait(1000);

                        bleepPlayer.Play();
                        GameFiber.Wait(500);
                    }

                    carblip = car.AttachBlip();
                    carblip.Color = System.Drawing.Color.Black;
                    carblip.Scale = 0.7f;
                    if (EntryPoint.IsLSPDFRPlusRunning)
                    {
                        API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Vehicles towed");
                    }
                    Ped playerPed = Game.LocalPlayer.Character;
                    if (car.Model.IsCar && RecruitNearbyTowtruck(out driver, out towTruck))
                    {
                        Game.LogTrivial("Recruited nearby tow truck.");
                    }
                    else
                    {
                        float Heading;
                        bool UseSpecialID = true;
                        Vector3 SpawnPoint;
                        float travelDistance;
                        int waitCount = 0;
                        while (true)
                        {
                            GetSpawnPoint(car.Position, out SpawnPoint, out Heading, UseSpecialID);
                            travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>( SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, car.Position.X, car.Position.Y, car.Position.Z);
                            waitCount++;
                            if (Vector3.Distance(car.Position, SpawnPoint) > EntryPoint.SceneManagementSpawnDistance - 15f)
                            {

                                if (travelDistance < (EntryPoint.SceneManagementSpawnDistance * 4.5f))
                                {

                                    Vector3 directionFromVehicleToPed1 = (car.Position - SpawnPoint);
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
                                Game.DisplayNotification("Take the car ~s~to a more reachable location.");
                                Game.DisplayNotification("Alternatively, press ~b~Y ~s~to force a spawn in the ~g~wilderness.");
                            }
                            if ((waitCount >= 600) && Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.Y))
                            {
                                SpawnPoint = Game.LocalPlayer.Character.Position.Around(15f);
                                break;
                            }
                            GameFiber.Yield();

                        }
                        modelName = car.Model.Name.ToLower();
                        modelName = char.ToUpper(modelName[0]) + modelName.Substring(1);
                        

                        if (car.Model.IsCar && !car.IsDead && !AlwaysFlatbed)
                        {
                            Game.DisplayNotification("A ~g~tow truck ~s~has been dispatched for the target ~r~" + modelName + ". ~s~Await arrival.");
                            towTruck = new Vehicle(TowtruckModel, SpawnPoint, Heading);
                            Game.DisplayHelp("~b~If you want to attach the vehicle yourself, get in now.");
                            flatbed = false;
                        }
                        else
                        {
                            Game.DisplayNotification("A ~g~flatbed ~s~has been dispatched for the target ~r~" + modelName + ". ~s~Await arrival.");
                            towTruck = new Vehicle(FlatbedModel, SpawnPoint, Heading);
                        }
                    }
                    TowTrucksBeingUsed.Add(towTruck);
                    towTruck.IsPersistent = true;
                    towTruck.CanTiresBurst = false;
                    towTruck.IsInvincible = true;
                    if (OverrideTowTruckColour)
                    {
                        towTruck.PrimaryColor = TowTruckColour;
                        towTruck.SecondaryColor = TowTruckColour;
                        towTruck.PearlescentColor = TowTruckColour;
                    }


                    towblip = towTruck.AttachBlip();
                    towblip.Color = System.Drawing.Color.Blue;
                    //Vector3 directionFromVehicleToPed = (car.Position - towTruck.Position);
                    //directionFromVehicleToPed.Normalize();
                    //towTruck.Heading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                    if (!driver.Exists())
                    {
                        driver = towTruck.CreateRandomDriver();
                    }
                    driver.MakeMissionPed();
                    driver.IsInvincible = true;
                    driver.Money = 1233;

                    driveToEntity(driver, towTruck, car, false);
                    Rage.Native.NativeFunction.Natives.START_VEHICLE_HORN(towTruck, 5000, 0, true);


                    if (towTruck.Speed > 15f)
                    {
                        NativeFunction.Natives.SET_VEHICLE_FORWARD_SPEED(towTruck, 15f);
                    }
                    driver.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardStraightBraking);
                    GameFiber.Sleep(600);
                    driver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
                    towTruck.IsSirenOn = true;
                    GameFiber.Wait(2000);
                    bool automaticallyAttach = false;
                    bool showImpoundMsg = true;
                    if (flatbed)
                    {
                        while (car && car.HasOccupants)
                        {
                            GameFiber.Yield();
                            Game.DisplaySubtitle("~r~Please remove all occupants from the vehicle.", 1);
                        }
                        if (car)
                        {
                            car.AttachTo(towTruck, 20, FlatbedModifier, Rotator.Zero);
                        }
                    }
                    else
                    {
                        if (!Game.LocalPlayer.Character.IsInVehicle(car, true))
                        {
                            automaticallyAttach = true;
                        }

                        while (true)
                        {
                            GameFiber.Sleep(1);
                            driver.Money = 1233;
                            if (!car.Exists()) { break; }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.D0) || automaticallyAttach)
                            {
                                if (Game.LocalPlayer.Character.IsInVehicle(car, false))
                                {
                                    Game.DisplaySubtitle("~r~Get out of the vehicle first.", 5000);

                                }
                                else
                                {
                                    car.Position = towTruck.GetOffsetPosition(Vector3.RelativeBack * 7f);
                                    car.Heading = towTruck.Heading;
                                    if (towTruck.HasTowArm)
                                    {
                                        towTruck.TowVehicle(car, true);
                                    }
                                    else
                                    {
                                        car.Delete();
                                        Game.LogTrivial("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                                        Game.DisplayNotification("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                                    }
                                    Game.HideHelp();
                                    break;
                                }

                            }
                            else if (Vector3.Distance(towTruck.GetOffsetPosition(Vector3.RelativeBack * 7f), car.Position) < 2.1f)
                            {


                                //Game.LogTrivial((towTruck.Heading - car.Heading).ToString());
                                if ((towTruck.Heading - car.Heading < 30f) && (towTruck.Heading - car.Heading > -30f))
                                {
                                    Game.DisplaySubtitle("~b~Exit the vehicle", 1);
                                    if (!Game.LocalPlayer.Character.IsInVehicle(car, true))
                                    {
                                        GameFiber.Sleep(1000);
                                        towTruck.TowVehicle(car, true);
                                        break;
                                    }
                                }
                                else if (((towTruck.Heading - car.Heading < -155f) && (towTruck.Heading - car.Heading > -205f)) || ((towTruck.Heading - car.Heading > 155f) && (towTruck.Heading - car.Heading < 205f)))
                                {
                                    Game.DisplaySubtitle("~b~Exit the vehicle", 1);
                                    if (!Game.LocalPlayer.Character.IsInVehicle(car, true))
                                    {
                                        GameFiber.Sleep(1000);
                                        if (towTruck.HasTowArm)
                                        {
                                            towTruck.TowVehicle(car, false);
                                        }
                                        else
                                        {
                                            car.Delete();
                                            Game.LogTrivial("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                                            Game.DisplayNotification("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    Game.DisplaySubtitle("~b~Align the ~b~vehicle~s~ with the ~g~tow truck.", 1);

                                }
                            }

                            else
                            {
                                Game.DisplaySubtitle("Drive the vehicle behind the tow truck.", 1);
                            }

                            if (Vector3.Distance(Game.LocalPlayer.Character.Position, car.Position) > 70f)
                            {
                                car.Position = towTruck.GetOffsetPosition(Vector3.RelativeBack * 7f);
                                car.Heading = towTruck.Heading;

                                if (towTruck.HasTowArm)
                                {
                                    towTruck.TowVehicle(car, true);
                                }
                                else
                                {
                                    car.Delete();
                                    Game.LogTrivial("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                                    Game.DisplayNotification("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                                }
                                break;
                            }
                            if (Vector3.Distance(car.Position, towTruck.Position) > 80f)
                            {
                                Game.DisplaySubtitle("Towing service cancelled", 5000);
                                showImpoundMsg = false;
                                break;
                            }
                        }
                    }
                

                    Game.HideHelp();
                    if (showImpoundMsg)
                    {
                        Game.DisplayNotification("The target ~r~" + modelName + " ~s~has been impounded!");
                    }
                    driver.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardStraight).WaitForCompletion(600);
                    driver.Tasks.CruiseWithVehicle(25f);
                    GameFiber.Wait(1000);
                    if (car.Exists() && towTruck.Exists() && !flatbed)
                    {
                        if (!car.FindTowTruck().Exists())
                        {
                            
                            car.Position = towTruck.GetOffsetPosition(Vector3.RelativeBack * 7f);
                            car.Heading = towTruck.Heading;

                            if (towTruck.HasTowArm)
                            {
                                towTruck.TowVehicle(car, true);
                            }
                            else
                            {
                                car.Delete();
                                Game.LogTrivial("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                                Game.DisplayNotification("Tow truck model is not registered as a tow truck ingame - if this is a custom vehicle, contact the vehicle author.");
                            }
                        }
                    }
                    if (driver.Exists()) { driver.Dismiss(); }
                    if (car.Exists()) { car.Dismiss(); }
                    
                    if (towTruck.Exists()) { towTruck.Dismiss(); }
                    if (towblip.Exists()) { towblip.Delete(); }
                    if (carblip.Exists()) { carblip.Delete(); }
                    
                    while (towTruck.Exists() && car.Exists())
                    {
                        GameFiber.Sleep(1000);
                    }
                    if (car.Exists())
                    {
                        car.Delete();
                    }

                }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Tow Truck Crashed");
                    Game.DisplayNotification("The towing service was interrupted.");
                    if (towblip.Exists()) { towblip.Delete(); }
                    if (carblip.Exists()) { carblip.Delete(); }
                    if (driver.Exists()) { driver.Delete(); }
                    if (car.Exists()) { car.Delete(); }
                    if (towTruck.Exists()) { towTruck.Delete(); }
                }
            });
        }
        public string[] insurancevehicles = new string[] { "JACKAL", "ASTEROPE", "TAILGATER", "PREMIER", "FUSILADE" };
        private Blip businesscarblip;
        private Ped passenger;
        private Vehicle businessCar;
        internal void insurancePickUp()
        {

            GameFiber.StartNew(delegate
            {

                try
                {
                    Vehicle[] nearbyvehs = Game.LocalPlayer.Character.GetNearbyVehicles(2);
                    if (nearbyvehs.Length == 0)
                    {
                        Game.DisplayNotification("~r~Couldn't detect a close enough vehicle.");
                        return;
                    }
                    
                    car = nearbyvehs[0];
                    if (Vector3.Distance(Game.LocalPlayer.Character.Position, car.Position) > 6f)
                    {
                        Game.DisplayNotification("~r~Couldn't detect a close enough vehicle.");
                        return;
                    }
                    if (car.HasOccupants)
                    {
                        if (nearbyvehs.Length == 2)
                        {
                            car = nearbyvehs[1];
                            if (car.HasOccupants)
                            {
                                Game.DisplayNotification("~r~Couldn't detect a close enough vehicle without occupants.");
                                return;
                            }
                        }
                        else
                        {
                            Game.DisplayNotification("~r~Couldn't detect a close enough vehicle without occupants.");
                            return;
                        }

                    }
                    if (car.IsPoliceVehicle)
                    {
                        Game.DisplayNotification("Are you sure you want to remove the police vehicle? ~h~~b~Y/N");
                        while (true)
                        {
                            GameFiber.Yield();
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(System.Windows.Forms.Keys.Y))
                            {
                                break;
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(System.Windows.Forms.Keys.N))
                            {
                                return;
                            }
                        }
                    }
                    ToggleMobilePhone(Game.LocalPlayer.Character, true);
                    GameFiber.Sleep(3000);
                    ToggleMobilePhone(Game.LocalPlayer.Character, false);
                    car.IsPersistent = true;
                    carblip = car.AttachBlip();
                    carblip.Color = System.Drawing.Color.Black;
                    carblip.Scale = 0.7f;
                    string modelName = car.Model.Name.ToLower();
                    modelName = char.ToUpper(modelName[0]) + modelName.Substring(1);
                    Ped playerPed = Game.LocalPlayer.Character;
                    if (EntryPoint.IsLSPDFRPlusRunning)
                    {
                        API.LSPDFRPlusFuncs.AddCountToStatistic(Main.PluginName, "Insurance pickups");
                    }
                    Vector3 SpawnPoint = World.GetNextPositionOnStreet(playerPed.Position.Around(EntryPoint.SceneManagementSpawnDistance));
                    float travelDistance;
                    int waitCount = 0;
                    while (true)
                    {
                        SpawnPoint = World.GetNextPositionOnStreet(playerPed.Position.Around(EntryPoint.SceneManagementSpawnDistance));
                        travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>( SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, playerPed.Position.X, playerPed.Position.Y, playerPed.Position.Z);
                        waitCount++;
                        if (Vector3.Distance(playerPed.Position, SpawnPoint) > EntryPoint.SceneManagementSpawnDistance - 10f)
                        {

                            if (travelDistance < (EntryPoint.SceneManagementSpawnDistance * 4.5f))
                            {

                                break;
                            }
                        }
                        if (waitCount == 600)
                        {
                            Game.DisplayNotification("Take the car ~s~to a more reachable location.");
                            Game.DisplayNotification("Alternatively, press ~b~Y ~s~to force a spawn in the ~g~wilderness.");
                        }
                        if ((waitCount >= 600) && Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.Y))
                        {
                            SpawnPoint = Game.LocalPlayer.Character.Position.Around(15f);
                            break;
                        }
                        GameFiber.Yield();

                    }
                    car.LockStatus = VehicleLockStatus.Unlocked;
                    car.MustBeHotwired = false;
                    Game.DisplayNotification("mphud", "mp_player_ready", "~h~Mors Mutual Insurance", "~b~Vehicle Pickup Status Update", "Two of our employees are en route to pick up our client's ~h~" + modelName + ".");
                    businessCar = new Vehicle(insurancevehicles[rnd.Next(insurancevehicles.Length)], SpawnPoint);
                    businesscarblip = businessCar.AttachBlip();
                    businesscarblip.Color = System.Drawing.Color.Blue;
                    businessCar.IsPersistent = true;
                    Vector3 directionFromVehicleToPed = (car.Position - businessCar.Position);
                    directionFromVehicleToPed.Normalize();
                    businessCar.Heading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                    driver = new Ped("a_m_y_business_02", businessCar.Position, businessCar.Heading);
                    driver.BlockPermanentEvents = true;
                    driver.WarpIntoVehicle(businessCar, -1);
                    driver.Money = 1;

                    passenger = new Ped("a_m_y_business_02", businessCar.Position, businessCar.Heading);
                    passenger.BlockPermanentEvents = true;
                    passenger.WarpIntoVehicle(businessCar, 0);
                    passenger.Money = 1;

                    driveToEntity(driver, businessCar, car, true);
                    Rage.Native.NativeFunction.Natives.START_VEHICLE_HORN(businessCar, 3000, 0, true);
                    while (true)
                    {
                        GameFiber.Yield();
                        driver.Tasks.DriveToPosition(car.GetOffsetPosition(Vector3.RelativeFront * 2f), 10f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians).WaitForCompletion(500);
                        if (Vector3.Distance(businessCar.Position, car.Position) < 15f)
                        {


                            driver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
                            break;
                        }
                        if (Vector3.Distance(car.Position, businessCar.Position) > 50f)
                        {
                            SpawnPoint = World.GetNextPositionOnStreet(car.Position);
                            businessCar.Position = SpawnPoint;
                            directionFromVehicleToPed = (car.Position - SpawnPoint);
                            directionFromVehicleToPed.Normalize();

                            float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                            businessCar.Heading = vehicleHeading;
                        }
                    }

                    driver.PlayAmbientSpeech("GENERIC_HOWS_IT_GOING", true);
                    passenger.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion();
                    Rage.Native.NativeFunction.Natives.SET_PED_CAN_RAGDOLL(passenger, false);
                    passenger.Tasks.FollowNavigationMeshToPosition(car.GetOffsetPosition(Vector3.RelativeLeft * 2f), car.Heading, 2f).WaitForCompletion(2000);
                    driver.Dismiss();
                    passenger.Tasks.FollowNavigationMeshToPosition(car.GetOffsetPosition(Vector3.RelativeLeft * 2f), car.Heading, 2f).WaitForCompletion(3000);

                    passenger.Tasks.EnterVehicle(car, 9000, -1).WaitForCompletion();
                    if (car.HasDriver)
                    {
                        if (car.Driver != passenger)
                        {
                            car.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.WarpOut).WaitForCompletion();
                        }
                    }
                    passenger.WarpIntoVehicle(car, -1);
                    GameFiber.Sleep(2000);
                    passenger.PlayAmbientSpeech("GENERIC_THANKS", true);
                    passenger.Dismiss();
                    car.Dismiss();
                    carblip.Delete();
                    businesscarblip.Delete();
                    GameFiber.Sleep(9000);
                    Game.DisplayNotification("mphud", "mp_player_ready", "~h~Mors Mutual Insurance", "~b~Vehicle Pickup Status Update", "Thank you for letting us collect our client's ~h~" + modelName + "!");
                }

                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Insurance company Crashed");
                    Game.DisplayNotification("The insurance pickup service was interrupted.");
                    if (businesscarblip.Exists()) { businesscarblip.Delete(); }
                    if (carblip.Exists()) { carblip.Delete(); }
                    if (driver.Exists()) { driver.Delete(); }
                    if (car.Exists()) { car.Delete(); }
                    if (businessCar.Exists()) { businessCar.Delete(); }
                    if (passenger.Exists()) { passenger.Delete(); }
                }
            });
        }
        



        


        //private static MenuPool _menuPool;
        public static UIMenu vehicleManagementMenu;
        private static UIMenuItem callForTowTruckItem;
        private static UIMenuItem callForInsuranceItem;
        public static void createVehicleManagementMenu()
        {
            //Game.FrameRender += Process;
            //_menuPool = new MenuPool();
            vehicleManagementMenu = new UIMenu("Vehicle Manager", "");
            //_menuPool.Add(vehicleManagementMenu);
            vehicleManagementMenu.AddItem(SceneManager.MenuSwitchListItem);
            callForTowTruckItem = new UIMenuItem("Tow truck");
            vehicleManagementMenu.AddItem(callForTowTruckItem);
            callForInsuranceItem = new UIMenuItem("Insurance company");
            vehicleManagementMenu.AddItem(callForInsuranceItem);
            vehicleManagementMenu.OnItemSelect += OnItemSelect;
            vehicleManagementMenu.MouseControlsEnabled = false;
            vehicleManagementMenu.AllowCameraMovement = true;
            

        }
        public static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender != vehicleManagementMenu) { return; }
            
            if (selectedItem == callForTowTruckItem)
            {
                Rage.Native.NativeFunction.Natives.SET_PED_STEALTH_MOVEMENT(Game.LocalPlayer.Character, 0, 0);
                new VehicleManager().towVehicle();
                vehicleManagementMenu.Visible = false;
            }
            else if (selectedItem == callForInsuranceItem)
            {
                Rage.Native.NativeFunction.Natives.SET_PED_STEALTH_MOVEMENT(Game.LocalPlayer.Character, 0, 0);
                new VehicleManager().insurancePickUp();
                vehicleManagementMenu.Visible = false;
            }
        }

        //public static void Process(object sender, GraphicsEventArgs e)
        //{
        //    _menuPool.ProcessMenus();
        //    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.vehicleManagementModifierKey) || (EntryPoint.vehicleManagementModifierKey == Keys.None))
        //    {
        //        if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(EntryPoint.vehicleManagementKey))
        //        {
                    
        //            if (!vehicleManagementMenu.Visible)
        //            {
        //                Vehicle[] nearbyvehs = Game.LocalPlayer.Character.GetNearbyVehicles(2);
        //                if (nearbyvehs.Length != 0)
        //                {

        //                    vehicleManagementMenu.Visible = !vehicleManagementMenu.Visible;

        //                }
        //            }
        //            else
        //            {
        //                vehicleManagementMenu.Visible = !vehicleManagementMenu.Visible;
        //            }


        //        }
        //    }
        }
        //public static void TPTest()
        //{
        //    Traffic_Policer.VehicleInsurance.SetInsuranceStatusForVehicle(Game.LocalPlayer.Character.GetNearbyVehicles(1)[0], Traffic_Policer.VehicleInsurance.InsuranceStatus.Uninsured);
        //    Traffic_Policer.VehicleInsurance.DisplayInsuranceStatusNotification(Traffic_Policer.VehicleInsurance.GetInsuranceStatusForVehicle(Game.LocalPlayer.Character.GetNearbyVehicles(1)[0]));
        //}
    }

