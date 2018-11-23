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
using static Arrest_Manager.PedManager;
using static Arrest_Manager.VehicleManager;
using Albo1125.Common.CommonLibrary;

namespace Arrest_Manager
{
    internal static class SceneManager
    {
        public static System.Media.SoundPlayer bleepPlayer = new System.Media.SoundPlayer("LSPDFR/Police Scanner/Arrest Manager Audio/RADIO_BLIP.wav");
        private static Rage.Object MobilePhone;

        public static void ToggleMobilePhone(Ped ped, bool toggle)
        {

            if (toggle)
            {
                if (MobilePhone.Exists()) { MobilePhone.Delete(); }
                NativeFunction.Natives.SET_PED_CAN_SWITCH_WEAPON(ped, false);
                ped.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_UNARMED"), -1, true);
                MobilePhone = new Rage.Object(new Model("prop_police_phone"), new Vector3(0, 0, 0));
                int boneIndex = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(ped, (int)PedBoneId.RightPhHand);
                NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(MobilePhone, ped, boneIndex, 0f, 0f, 0f, 0f, 0f, 0f, true, true, false, false, 2, 1);
                ped.Tasks.PlayAnimation("cellphone@", "cellphone_call_listen_base", 1.45f, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);

            }
            else
            {
                NativeFunction.Natives.SET_PED_CAN_SWITCH_WEAPON(ped, true);
                ped.Tasks.Clear();
                if (GameFiber.CanSleepNow)
                {
                    GameFiber.Wait(800);
                }
                if (MobilePhone.Exists()) { MobilePhone.Delete(); }
            }
        }

        public static void driveToEntity(Ped driver, Vehicle driverCar, Entity entitytodriveto, bool GetCloseToEntity)
        {

            Ped playerPed = Game.LocalPlayer.Character;
            int drivingLoopCount = 0;
            bool transportVanTeleported = false;
            int waitCount = 0;
            bool forceCloseSpawn = false;



            //Get close to player with various checks
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (!forceCloseSpawn)
                    {
                        GameFiber.Yield();
                        if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(EntryPoint.SceneManagementKey))
                        {
                            GameFiber.Sleep(300);
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.SceneManagementKey))
                            {
                                GameFiber.Sleep(1000);
                                if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.SceneManagementKey))
                                {
                                    forceCloseSpawn = true;
                                }
                                else
                                {
                                    Game.DisplayNotification("Hold down the ~b~Scene Management Key ~s~to force a close spawn.");
                                }
                            }
                        }
                    }
                });
                Rage.Task driveToPed = null;
                driver.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardStraight).WaitForCompletion(500);
                
                while (Vector3.Distance(driverCar.Position, entitytodriveto.Position) > 35f)
                {
                    if (!entitytodriveto.Exists() || !entitytodriveto.IsValid())
                    {
                        return;
                    }

                    driverCar.Repair();
                    if (driveToPed == null || !driveToPed.IsActive)
                    {
                        driver.Tasks.DriveToPosition(entitytodriveto.Position, 15f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                    }
                    NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(driver, 786607);
                    NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(driver, 0f);
                    NativeFunction.Natives.SET_DRIVER_ABILITY(driver, 1f);
                    GameFiber.Wait(600);
                    waitCount++;
                    if (waitCount == 55)
                    {
                        Game.DisplayHelp("Service taking too long? Hold down ~b~" + EntryPoint.kc.ConvertToString(EntryPoint.SceneManagementKey) + " ~s~to speed it up.", 5000);
                    }
                    //If van isn't moving
                    if (driverCar.Speed < 0.1f)
                    {
                        //driver.Tasks.PerformDrivingManeuver(driverCar, VehicleManeuver.ReverseStraight, 1000).WaitForCompletion();
                        //drivingLoopCount += 2;
                        //driver.Tasks.DriveToPosition(entitytodriveto.Position, MathHelper.ConvertKilometersPerHourToMetersPerSecond(60f), VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians).WaitForCompletion(1000);
                    }
                    if (driverCar.Speed < 2f)
                    {
                        drivingLoopCount++;
                    }
                    //if van is very far away
                    if ((Vector3.Distance(entitytodriveto.Position, driverCar.Position) > EntryPoint.SceneManagementSpawnDistance + 65f))
                    {
                        drivingLoopCount++;
                    }
                    //If Van is stuck, relocate it
                    if ((drivingLoopCount >= 33 && drivingLoopCount <= 38) && EntryPoint.AllowWarping)
                    {
                        Vector3 SpawnPoint;
                        float Heading;
                        bool UseSpecialID = true;
                        float travelDistance;
                        int wC = 0;
                        while (true)
                        {
                            GetSpawnPoint(entitytodriveto.Position, out SpawnPoint, out Heading, UseSpecialID);
                            travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>( SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, entitytodriveto.Position.X, entitytodriveto.Position.Y, entitytodriveto.Position.Z);
                            wC++;
                            if (Vector3.Distance(entitytodriveto.Position, SpawnPoint) > EntryPoint.SceneManagementSpawnDistance - 15f)
                            {

                                if (travelDistance < (EntryPoint.SceneManagementSpawnDistance * 4.5f))
                                {

                                    Vector3 directionFromVehicleToPed1 = (entitytodriveto.Position - SpawnPoint);
                                    directionFromVehicleToPed1.Normalize();

                                    float HeadingToPlayer = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);

                                    if (Math.Abs(MathHelper.NormalizeHeading(Heading) - MathHelper.NormalizeHeading(HeadingToPlayer)) < 150f)
                                    {


                                        break;
                                    }
                                }
                            }
                            if (wC >= 400)
                            {
                                UseSpecialID = false;
                            }
                            GameFiber.Yield();
                        }
                        //float travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>( SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, playerPed.Position.X, playerPed.Position.Y, playerPed.Position.Z);

                        Game.Console.Print("Relocating because service was stuck...");
                        driverCar.Position = SpawnPoint;

                        driverCar.Heading = Heading;
                        drivingLoopCount = 39;



                    }

                    // if van is stuck for a 2nd time or takes too long, spawn it very near to the car
                    else if (((drivingLoopCount >= 70 || waitCount >= 110) && EntryPoint.AllowWarping) || forceCloseSpawn)
                    {
                        Game.Console.Print("Relocating service to a close position");

                        Vector3 SpawnPoint = World.GetNextPositionOnStreet(entitytodriveto.Position.Around2D(15f));

                        int waitCounter = 0;
                        while ((SpawnPoint.Z - entitytodriveto.Position.Z < -3f) || (SpawnPoint.Z - entitytodriveto.Position.Z > 3f) || (Vector3.Distance(SpawnPoint, entitytodriveto.Position) > 26f))
                        {
                            waitCounter++;
                            SpawnPoint = World.GetNextPositionOnStreet(entitytodriveto.Position.Around(20f));
                            GameFiber.Yield();
                            if (waitCounter >= 500)
                            {
                                SpawnPoint = entitytodriveto.Position.Around(20f);
                                break;
                            }
                        }
                        Vector3 directionFromVehicleToPed = (entitytodriveto.Position - SpawnPoint);
                        directionFromVehicleToPed.Normalize();

                        float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                        driverCar.Heading = vehicleHeading + 180f;
                        driverCar.Position = SpawnPoint;

                        transportVanTeleported = true;

                        break;
                    }

                }

                forceCloseSpawn = true;
                //park the van
                Game.HideHelp();
                if (!GetCloseToEntity)
                {
                    while ((Vector3.Distance(entitytodriveto.Position, driverCar.Position) > 19f && (driverCar.Position.Z - entitytodriveto.Position.Z < -2.5f) || (driverCar.Position.Z - entitytodriveto.Position.Z > 2.5f)) && !transportVanTeleported)
                    {
                        if (!entitytodriveto.Exists() || !entitytodriveto.IsValid())
                        {
                            return;
                        }

                        Rage.Task parkNearcar = driver.Tasks.DriveToPosition(entitytodriveto.Position, 6f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                        parkNearcar.WaitForCompletion(900);

                        if (Vector3.Distance(entitytodriveto.Position, driverCar.Position) > 60f)
                        {
                            Vector3 SpawnPoint = World.GetNextPositionOnStreet(entitytodriveto.Position.Around(10f));

                            int waitCounter = 0;
                            while ((SpawnPoint.Z - entitytodriveto.Position.Z < -3f) || (SpawnPoint.Z - entitytodriveto.Position.Z > 3f) || (Vector3.Distance(SpawnPoint, entitytodriveto.Position) > 26f))
                            {
                                waitCounter++;
                                SpawnPoint = World.GetNextPositionOnStreet(entitytodriveto.Position.Around(20f));
                                GameFiber.Yield();
                                if (waitCounter >= 500)
                                {
                                    SpawnPoint = entitytodriveto.Position.Around(20f);
                                    break;
                                }
                            }
                            Vector3 directionFromVehicleToPed = (entitytodriveto.Position - SpawnPoint);
                            directionFromVehicleToPed.Normalize();

                            float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                            driverCar.Heading = vehicleHeading + 180f;
                            driverCar.Position = SpawnPoint;

                            transportVanTeleported = true;
                        }
                        //if (driverCar.Speed < 0.1f)
                        //{
                        //    reverseCount++;
                        //    if (reverseCount == 10)
                        //    {
                        //        driver.Tasks.PerformDrivingManeuver(driverCar, VehicleManeuver.ReverseStraight, 1300).WaitForCompletion();
                        //        reverseCount = 0;
                        //    }
                        //}
                    }
                }

                else
                {
                    while ((Vector3.Distance(entitytodriveto.Position, driverCar.Position) > 17f) || transportVanTeleported)
                    {
                        if (!entitytodriveto.Exists() || !entitytodriveto.IsValid())
                        {
                            return;
                        }
                        Rage.Task parkNearSuspect = driver.Tasks.DriveToPosition(entitytodriveto.Position, 6f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                        parkNearSuspect.WaitForCompletion(800);
                        transportVanTeleported = false;
                        if (Vector3.Distance(entitytodriveto.Position, driverCar.Position) > 50f)
                        {
                            Vector3 SpawnPoint = World.GetNextPositionOnStreet(entitytodriveto.Position.Around(12f));
                            driverCar.Position = SpawnPoint;
                        }
                        //if (driverCar.Speed < 0.2f)
                        //{
                        //    reverseCount++;
                        //    if (reverseCount == 3)
                        //    {
                        //        driver.Tasks.PerformDrivingManeuver(driverCar, VehicleManeuver.ReverseStraight, 1700).WaitForCompletion();
                        //        reverseCount = 0;
                        //    }
                        //}
                    }
                    GameFiber.Sleep(600);
                }
            }

            catch (Exception)
            {
                return;
            }
        }
        public static void GetSpawnPoint(Vector3 StartPoint, out Vector3 SpawnPoint1, out float Heading1, bool UseSpecialID)
        {
            Vector3 tempspawn = World.GetNextPositionOnStreet(StartPoint.Around2D(EntryPoint.SceneManagementSpawnDistance + 5f));
            Vector3 SpawnPoint = Vector3.Zero;
            float Heading = 0;
            unsafe
            {
                if (!UseSpecialID || !NativeFunction.Natives.GET_NTH_CLOSEST_VEHICLE_NODE_FAVOUR_DIRECTION<bool>(tempspawn.X, tempspawn.Y, tempspawn.Z, StartPoint.X, StartPoint.Y, StartPoint.Z, 0, out SpawnPoint, out Heading, 0, 0x40400000, 0) || !ExtensionMethods.IsNodeSafe(SpawnPoint))
                {
                    Game.LogTrivial("Unsuccessful specialID");
                    SpawnPoint = World.GetNextPositionOnStreet(StartPoint.Around2D(EntryPoint.SceneManagementSpawnDistance + 5f));
                    Vector3 directionFromVehicleToPed1 = (StartPoint - SpawnPoint);
                    directionFromVehicleToPed1.Normalize();

                    Heading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);
                }

            }
            SpawnPoint1 = SpawnPoint;
            Heading1 = Heading;
        }










        private static TimerBarPool timerBarPool = new TimerBarPool();
        private static BarTimerBar arrestBar = new BarTimerBar("Arresting...");
        private static bool arrestBarInPool = false;
        private static bool arrestBarDisplayTime = false;

        private static MenuPool _menuPool;
        private static UIMenu ActiveMenu = PedManagementMenu;
        public static UIMenuListItem MenuSwitchListItem;
        public static void CreateMenus()
        {
            arrestBar.ForegroundColor = System.Drawing.Color.DarkBlue;
            arrestBar.BackgroundColor = ControlPaint.Dark(arrestBar.ForegroundColor);

            _menuPool = new MenuPool();
            var menus = new List<dynamic>() { "Ped Manager", "Vehicle Manager" };
            MenuSwitchListItem = new UIMenuListItem("Scene Management", menus, 0);
            CreatePedManagementMenu();
            
            _menuPool.Add(PedManagementMenu);
            PedManagementMenu.OnListChange += OnListChange;
            createVehicleManagementMenu();
            _menuPool.Add(vehicleManagementMenu);
            vehicleManagementMenu.OnListChange += OnListChange;
            Game.FrameRender += Process;
            MainLogic();

        }
        public static void OnListChange(UIMenu sender, UIMenuListItem list, int index)
        {
            
            if ((sender != PedManagementMenu && sender != vehicleManagementMenu) || list != MenuSwitchListItem) { return; }
           
            string selectedmenustring = list.IndexToItem(index).ToString();

            UIMenu selectedmenu;
            if (selectedmenustring == "Ped Manager")
            {
                selectedmenu = PedManagementMenu;
            }
            else
            {
                selectedmenu = vehicleManagementMenu;
            }
            if (selectedmenu != sender)
            {
                
                sender.Visible = false;
                selectedmenu.Visible = true;
                ActiveMenu = selectedmenu;
                list.Selected = false;
            }
            
        }
        private static Ped nearestWaterPed;
        private static Ped nearestPed;
        public static bool callCoronerTime = false;
        private static void MainLogic()
        {
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();
                    
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.SceneManagementModifierKey) || (EntryPoint.SceneManagementModifierKey == Keys.None))
                    {
                        if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(EntryPoint.SceneManagementKey))
                        {
                            if (ActiveMenu != null)
                            {
                                ActiveMenu.Visible = !ActiveMenu.Visible;

                            }
                            else
                            {
                                PedManagementMenu.Visible = !PedManagementMenu.Visible;

                            }


                        }
                    }
                    if (_menuPool.IsAnyMenuOpen()) { Rage.Native.NativeFunction.Natives.SET_PED_STEALTH_MOVEMENT(Game.LocalPlayer.Character, 0, 0); }
                    else if ((Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(GrabPedModifierKey) || GrabPedModifierKey == Keys.None) && Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(GrabPedKey))
                    {
                        if (!EnableGrab)
                        {
                            PedManager.GrabPed();
                        }
                        else
                        {
                            EnableGrab = false;
                        }
                    }
                    

                    //if ped is in water offer proper arresting mechanism
                    
                    else if (Game.LocalPlayer.Character.SubmersionLevel > 0.2 && Game.IsControlPressed(2, GameControl.Context) && (nearestWaterPed = PedManager.GetNearestValidPed(6f, true, -1)).Exists() && !Functions.IsPedArrested(nearestWaterPed) && nearestWaterPed.SubmersionLevel > 0.2)
                    {
                        
                        arrestBarDisplayTime = true;
                        Functions.SetPedCantBeArrestedByPlayer(nearestWaterPed, true);
                        arrestBar.Percentage += 0.03f;
                        if (arrestBar.Percentage > 0.99)
                        {
                            nearestWaterPed.Tasks.ClearImmediately();
                            ArrestPed(nearestWaterPed);
                            arrestBarDisplayTime = false;
                            arrestBar.Percentage = 0;
                        }
                    }
                    else
                    {
                        arrestBarDisplayTime = false;
                        arrestBar.Percentage = 0;
                        if (nearestWaterPed && !Functions.IsPedArrested(nearestWaterPed))
                        {
                            Functions.SetPedCantBeArrestedByPlayer(nearestWaterPed, false);
                        }
                    }

                    if (Game.LocalPlayer.Character.SubmersionLevel < 0.2 && (ExtensionMethods.IsKeyDownComputerCheck(PedManager.TackleKey) || Game.IsControllerButtonDown(TackleButton)) && Game.LocalPlayer.Character.Speed >= 5.3f)
                    {
                        nearestPed = PedManager.GetNearestValidPed(2f, true, -1);
                        if (nearestPed && !Functions.IsPedArrested(nearestPed) && !Functions.IsPedGettingArrested(nearestPed))
                        {
                            Game.LocalPlayer.Character.IsRagdoll = true;
                            nearestPed.IsRagdoll = true;
                            GameFiber.Sleep(500);
                            Game.LocalPlayer.Character.IsRagdoll = false;
                            GameFiber.Wait(2000);
                            nearestPed.IsRagdoll = false;
                        }
                        
                    }

                    foreach (Ped suspect in SuspectsManuallyArrested.ToArray())
                    {
                        if (suspect.Exists())
                        {
                            if (!NativeFunction.Natives.IS_ENTITY_PLAYING_ANIM<bool>(suspect, "mp_arresting", "idle", 3))
                            {
                                suspect.Tasks.PlayAnimation("mp_arresting", "idle", 8f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);
                            }
                        }
                        else
                        {
                            SuspectsManuallyArrested.Remove(suspect);
                        }
                    }

                    if (callCoronerTime)
                    {
                        Coroner.Main();
                        callCoronerTime = false;
                    }
                }
            });
        }

        public static void Process(object sender, GraphicsEventArgs e)
        {
            
            _menuPool.ProcessMenus();

            if (arrestBarDisplayTime && !arrestBarInPool)
            {

                timerBarPool.Add(arrestBar);
                arrestBarInPool = true;
            }

            if (!arrestBarDisplayTime && arrestBarInPool)
            {
                timerBarPool.Remove(arrestBar);
                arrestBarInPool = false;
            }

            if (arrestBarDisplayTime)
            {
                timerBarPool.Draw();
            }

        }


    }
}
