using Albo1125.Common.CommonLibrary;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arrest_Manager
{

    internal class Coroner
    {
        private static SoundPlayer cameraSound = new SoundPlayer("LSPDFR/audio/scanner/Arrest Manager Audio/Camera.wav");       
        public static Model coronerVehicleModel = new Model("SPEEDO");
        public static Model coronerModel = new Model("S_M_M_Doctor_01");
        private static List<Ped> bodiesBeingHandled = new List<Ped>();

        private List<Ped> deadBodies = new List<Ped>();
        private Vehicle coronerVeh;
        private Ped driver;
        private Ped passenger;
        private Vector3 destination;
        private bool anims;
        private List<Rage.Object> bodyBags = new List<Rage.Object>();
        private Blip coronerBlip;

        public static bool CanBeCalled(Vector3 destination)
        {
            return getNearbyDeadPeds(destination).Count != 0;
        }

        public static bool CanBeCalled()
        {
            return CanBeCalled(Game.LocalPlayer.Character.Position);
        }

        public static void Main()
        {

            if (getNearbyDeadPeds(Game.LocalPlayer.Character.Position).Count == 0) { Game.DisplaySubtitle("No nearby dead people were found, sorry!"); return; }
            new Coroner(Game.LocalPlayer.Character.Position).handleCoroner();
        }

        public static bool vc_main() { smartRadioMain(); return true; }
        public static void smartRadioMain()
        {
            if (getNearbyDeadPeds(Game.LocalPlayer.Character.Position).Count == 0) { Game.DisplaySubtitle("No nearby dead people were found, sorry!"); return; }
            new Coroner(Game.LocalPlayer.Character.Position, false).handleCoroner();
        }

        public Coroner(Vector3 destination, bool anims = true)
        {
            this.destination = destination;
            this.deadBodies = getNearbyDeadPeds(destination);
            this.anims = anims;
        }

        public void handleCoroner()
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    float Heading;
                    bool UseSpecialID = true;
                    Vector3 SpawnPoint;
                    float travelDistance;
                    int waitCount = 0;
                    while (true)
                    {
                        SceneManager.GetSpawnPoint(destination, out SpawnPoint, out Heading, UseSpecialID);
                        travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>(SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, destination.X, destination.Y, destination.Z);
                        waitCount++;
                        if (Vector3.Distance(destination, SpawnPoint) > EntryPoint.SceneManagementSpawnDistance - 15f)
                        {

                            if (travelDistance < (EntryPoint.SceneManagementSpawnDistance * 4.5f))
                            {

                                Vector3 directionFromVehicleToPed1 = (destination - SpawnPoint);
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
                            Game.DisplayNotification("Press ~b~Y ~s~to force a spawn in the ~g~wilderness.");
                        }
                        if ((waitCount >= 600) && Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.Y))
                        {
                            SpawnPoint = destination.Around(15f);
                            break;
                        }
                        GameFiber.Yield();
                    }
                    coronerVeh = new Vehicle(coronerVehicleModel, SpawnPoint, Heading);
                    coronerVeh.IsPersistent = true;
                    if (coronerVeh.HasSiren)
                    {
                        coronerVeh.IsSirenOn = true;
                    }
                    coronerBlip = coronerVeh.AttachBlip();
                    coronerBlip.Color = System.Drawing.Color.Black;
                    coronerBlip.Flash(1000, 30000);
                    driver = new Ped(coronerModel, Vector3.Zero, 0);
                    driver.MakeMissionPed();
                    driver.IsInvincible = true;
                    driver.WarpIntoVehicle(coronerVeh, -1);

                    passenger = new Ped(coronerModel, Vector3.Zero, 0);
                    passenger.MakeMissionPed();
                    passenger.IsInvincible = true;
                    passenger.WarpIntoVehicle(coronerVeh, 0);
                    Game.DisplayNotification("~s~A ~b~coroner ~s~is en route to your location.");
                    if (anims)
                    {
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                        GameFiber.Wait(1000);
                        SceneManager.bleepPlayer.Play();
                    }
                    driveToPosition(driver, coronerVeh, destination);
                    coronerBlip.Delete();
                    while (deadBodies.Count > 0)
                    {
                        deadBodies.OrderBy(x => x.DistanceTo(driver.Position));
                        foreach (Ped body in deadBodies.ToArray())
                        {
                            if (body.Exists() && !bodiesBeingHandled.Contains(body))
                            {
                                dealWithBody(body);
                            }
                            else
                            {
                                deadBodies.Remove(body);
                            }
                        }
                        deadBodies.AddRange(getNearbyDeadPeds(driver.Position));
                    }
                    LeaveScene();
                }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    if (driver.Exists()) { driver.Delete(); }
                    if (passenger.Exists()) { passenger.Delete(); }
                    if (coronerVeh.Exists()) { coronerVeh.Delete(); }
                    foreach (Entity ent in deadBodies)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }
                    deadBodies.Clear();
                    foreach (Entity ent in bodyBags)
                    {
                        if (ent.Exists()) { ent.Delete(); }
                    }
                }
            });
        }

        private void LeaveScene()
        {


            GameFiber.Wait(2500);
            foreach (Rage.Object obj in bodyBags)
            {
                if (obj.Exists())
                {
                    obj.Delete();
                }
            }
            GameFiber.Wait(2500);
            int randomRoll = EntryPoint.rnd.Next(1, 23);

            string msg = "";
            if (randomRoll == 1)
            {
                msg = "All done here - I wonder if FinKone'll ever touch some code again.";
            }
            else if (randomRoll == 2)
            {
                msg = "Let's roll, we've got another call. Los Santos never stops!";
            }
            else if (randomRoll == 3)
            {
                msg = "This is not nearly as bad as the last call, the poor guy was stuck in a toilet.";
            }
            else if (randomRoll == 4)
            {
                msg = "I hope Albo1125 uploaded another video today... let's go check!";
            }
            else if (randomRoll == 5)
            {
                msg = "Albo1125 shouldn't promote himself like this. It's disgusting...";
            }
            else if (randomRoll == 6)
            {
                msg = "All I need now is a holiday to the PNW parks. Sheesh.";
            }
            else if (randomRoll == 7)
            {
                msg = "It's a bloody shame this had to happen.";
            }
            else if (randomRoll == 8)
            {
                msg = "I'm feeling hungry now. Let's get a bite to eat.";
            }
            else if (randomRoll == 9)
            {
                msg = "I heard Albo1125 updated his mods again - my bandwidth is starting to run out!";
            }
            else if (randomRoll == 10)
            {
                msg = "Our detectives seem to think it's like LS Noire sim over here. We're so damn busy!";
            }
            else if (randomRoll == 11)
            {
                msg = "I'm getting too old for this job. I'm joining the Old Age Pensioner club for sure.";
            }
            else if (randomRoll == 12)
            {
                msg = "It's back to watching San Andreas's CCTV stream now, then.";
            }
            else if (randomRoll == 13)
            {
                msg = "I heard dinosaurs are back. Apparently they've evolved and are now fit to moderate forums.";
            }
            else if (randomRoll == 14)
            {
                msg = "I like how people remained calm there. I wonder how they learned to stop shitting bricks...";
            }
            else if (randomRoll == 15)
            {
                msg = "With these budget cuts my only contact method will soon be LSCoroner@Idontcare.com...";
            }
            else if (randomRoll == 16)
            {
                msg = "Heard about the new glasses they're selling? Apparently they make visuals great again.";
            }
            else if (randomRoll == 17)
            {
                msg = "I hope these medics don't get any better or we'll be out of a job!";
            }
            else if (randomRoll == 18)
            {
                msg = "These new emergency lights the police are using are so damn bright.";
            }
            else if (randomRoll == 19)
            {
                msg = "Could've been worse. I got a call in the ocean once, had to swim a mile!";
            }
            else if (randomRoll == 20)
            {
                msg = "My stupidest call was the rednecks who blew themselves up fishing with grenades.";
            }
            else if (randomRoll == 21)
            {
                msg = "This still doesn't top the guy who somehow electrocuted himself with a toaster.";
            }
            else if (randomRoll == 22)
            {
                msg = "Can you believe the coast guard dispatched me once for a dead whale? It doesn't even fit!";
            }


            if (driver.Exists())
            {
                if (Vector3.Distance(driver.Position, Game.LocalPlayer.Character.Position) < 60f)
                {
                    Game.DisplaySubtitle("~b~Driver: " + msg, 7000);
                }
            }
            passenger.Tasks.FollowNavigationMeshToPosition(coronerVeh.GetOffsetPositionRight(2), coronerVeh.Heading, 1.7f);
            driver.Tasks.FollowNavigationMeshToPosition(coronerVeh.GetOffsetPositionRight(-2), coronerVeh.Heading, 1.7f).WaitForCompletion(8000);
            passenger.Tasks.EnterVehicle(coronerVeh, 7000, 0);
            driver.Tasks.EnterVehicle(coronerVeh, 7000, -1).WaitForCompletion();
            GameFiber.Wait(3000);
            driver.Tasks.CruiseWithVehicle(coronerVeh, 15.0f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);


            driver.Dismiss();
            coronerVeh.Dismiss();
        }

        private void dealWithBody(Ped body)
        {
            bodiesBeingHandled.Add(body);
            passenger.Tasks.GoToOffsetFromEntity(body, 10000, -2.0f, -1.0f, 8.0f);
            driver.Tasks.GoToOffsetFromEntity(body, 10000, 2.4f, 1.0f, 8.0f).WaitForCompletion();
            if (Vector3.Distance(driver.Position, Game.LocalPlayer.Character.Position) < 60f)
            {
                Rage.Object camera = new Rage.Object("prop_ing_camera_01", driver.GetOffsetPosition(Vector3.RelativeTop * 30));
                driver.Tasks.PlayAnimation("anim@mp_player_intupperphotography", "idle_a_fp", 8.0F, AnimationFlags.None);
                camera.Heading = driver.Heading - 180;
                camera.Position = driver.GetOffsetPosition(Vector3.RelativeTop * 0.68f + Vector3.RelativeFront * 0.33f);
                camera.IsPositionFrozen = true;

                Vector3 dirVect = body.Position - driver.Position;
                Vector3 offsetPos = driver.GetOffsetPosition(Vector3.RelativeFront * 1.4f + Vector3.RelativeBottom * 1.5f);
                dirVect.Normalize();

                GameFiber.Wait(900);
                Rage.Native.NativeFunction.Natives.DRAW_SPOT_LIGHT(driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).X, driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).Y,
                    driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).Z, dirVect.X, dirVect.Y, dirVect.Z, 100, 100, 100, 90.0f, 50.0f, 90.0f, 80.0f, 90.0f);
                cameraSound.Play();
                GameFiber.Wait(1500);
                Rage.Native.NativeFunction.Natives.DRAW_SPOT_LIGHT(driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).X, driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).Y,
                    driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).Z, dirVect.X, dirVect.Y, dirVect.Z, 100, 100, 100, 90.0f, 50.0f, 90.0f, 80.0f, 90.0f);
                cameraSound.Play();
                GameFiber.Wait(1500);
                Rage.Native.NativeFunction.Natives.DRAW_SPOT_LIGHT(driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).X, driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).Y,
                    driver.GetOffsetPosition(Vector3.RelativeFront * 0.5f).Z, dirVect.X, dirVect.Y, dirVect.Z, 100, 100, 100, 90.0f, 50.0f, 90.0f, 80.0f, 90.0f);
                cameraSound.Play();

                GameFiber.Wait(1000);
                camera.Delete();
                Game.DisplaySubtitle("~b~Driver: I've got enough pictures, I'll time stamp them.", 4000);

                passenger.Tasks.PlayAnimation("amb@medic@standing@tendtodead@enter", "enter", 8.0F, AnimationFlags.None);
                GameFiber.Wait(1000);
                passenger.Tasks.PlayAnimation("amb@medic@standing@tendtodead@base", "base", 8.0F, AnimationFlags.None);
                GameFiber.Wait(1000);
                passenger.Tasks.PlayAnimation("amb@medic@standing@tendtodead@exit", "exit", 8.0F, AnimationFlags.None).WaitForCompletion();
                GameFiber.Wait(1000);
            }


            Game.DisplaySubtitle("~b~Passenger: " + causeOfDeathSpeech() + determineCauseOfDeath(body) + "~b~.", 6000);
            if (body.Exists())
            {
                if (deadBodies.Contains(body))
                {
                    deadBodies.Remove(body);
                }

                if (bodiesBeingHandled.Contains(body))
                {
                    bodiesBeingHandled.Remove(body);
                }

                if (!body.IsInAnyVehicle(true))
                {
                    bodyBags.Add(new Rage.Object("prop_ld_binbag_01", body.Position));

                }
                if (body.Exists())
                {
                    body.Delete();
                }
            }
            GameFiber.Wait(2500);
            
        }

        private static string causeOfDeathSpeech()
        {
            int roll = EntryPoint.rnd.Next(3);
            if (roll == 0)
            {
                return "Hm, cause of death appears to be from ~r~";
            }
            else if (roll == 1)
            {
                return "Seems the cause of death on this one was ~r~";
            }
            else
            {
                return "This one appears to have died from ~r~";
            }
        }

        private static uint[] bluntForceObjects = new uint[] { 0x678B81B1, 0x4E875F73, 0x958A4A8F, 0x440E4788, 0x84BD7BFD };
        private static string determineCauseOfDeath(Ped body)
        {
            Model causeModel = Rage.Native.NativeFunction.Natives.GET_PED_CAUSE_OF_DEATH<Model>(body);
            uint cause = EntryPoint.IsLSPDFRPluginRunning("BetterEMS", new Version("3.0.0.0")) && API.BetterEMSFuncs.HasBeenTreated(body) ? API.BetterEMSFuncs.GetOriginalDeathWeaponAssetHash(body) : causeModel.Hash;
            if (causeModel.IsVehicle || cause == 0x07FC7D7A || cause == 0xA36D413E)
            {
                return "a collision with a vehicle";
            }
            if (cause == 0xA2719263)
            {
                return "a fist fight";
            }
            else if (cause == 0xF9FBAEBE)
            {
                return "an animal's bite";
            }
            else if (cause == 0x99B507EA)
            {
                return "a knife stab wound";
            }
            else if (cause == 0xCDC174B0)
            {
                return "a high fall";
            }
            else if (cause == 0xDF8E89EB)
            {
                return "a fire";
            }
            else if (cause == 0x2024F4E8)
            {
                return "an explosion";
            }
            else if (cause == 0x8B7333FB)
            {
                return "a wound bleeding out";
            }
            else if (cause == 0x8B7333FB)
            {
                return "a wound bleeding out";
            }
            else if (bluntForceObjects.Contains(cause))
            {
                return "a blunt force weapon";
            }
            else
            {
                return "a firearm";
            }

        }



        private static List<Ped> getNearbyDeadPeds(Vector3 pos, float radius = 35)
        {
            List<Ped> nearbyDeads = new List<Ped>();
            foreach (Ped ped in World.EnumeratePeds())
            {
                if (ped.Exists() && ped.IsDead && !bodiesBeingHandled.Contains(ped) && ped.DistanceTo(pos) < radius)
                {
                    nearbyDeads.Add(ped);
                }
            }
            return nearbyDeads;
        }

        private static void driveToPosition(Ped driver, Vehicle veh, Vector3 pos)
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
                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(EntryPoint.SceneManagementKey)) // || Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(multiTransportKey))
                    {
                        GameFiber.Sleep(500);
                        if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.SceneManagementKey))// || Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(multiTransportKey))
                        {
                            GameFiber.Sleep(500);
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.SceneManagementKey))// || Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(multiTransportKey))
                            {
                                forceCloseSpawn = true;
                            }
                            else
                            {
                                Game.DisplayNotification("Hold down the ~b~" + Albo1125.Common.CommonLibrary.ExtensionMethods.GetKeyString(EntryPoint.SceneManagementKey, Keys.None) + " ~s~to force a close spawn.");
                            }
                        }
                    }
                }
            });
            Rage.Task driveToPed = null;
            driver.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardStraight).WaitForCompletion(500);
            while (Vector3.Distance(veh.Position, pos) > 35f)
            {

                veh.Repair();
                if (driveToPed == null || !driveToPed.IsActive)
                {
                    driveToPed = driver.Tasks.DriveToPosition(pos, MathHelper.ConvertKilometersPerHourToMetersPerSecond(60f), VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                }
                NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(driver, 786607);
                NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(driver, 0f);
                NativeFunction.Natives.SET_DRIVER_ABILITY(driver, 1f);
                GameFiber.Wait(600);
                

                waitCount++;
                if (waitCount == 70)
                {
                    Game.DisplayHelp("Service taking too long? Hold down ~b~" + EntryPoint.kc.ConvertToString(EntryPoint.SceneManagementKey) + " ~s~to speed it up.", 5000);
                }
                //If van isn't moving

                if (veh.Speed < 0.2f)
                {
                    //    driver.Tasks.PerformDrivingManeuver(veh, VehicleManeuver.ReverseStraight, 700).WaitForCompletion();
                    //    drivingLoopCount += 2;
                    //    driver.Tasks.DriveToPosition(pos, MathHelper.ConvertKilometersPerHourToMetersPerSecond(60f), VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians).WaitForCompletion(100);
                }
                if (veh.Speed < 2f)
                {
                    drivingLoopCount++;
                }
                //if van is very far away
                if ((Vector3.Distance(pos, veh.Position) > EntryPoint.SceneManagementSpawnDistance + 70f))
                {
                    drivingLoopCount++;
                }
                //If Van is stuck, relocate it

                if ((drivingLoopCount >= 33 && drivingLoopCount <= 38) && EntryPoint.AllowWarping)
                {
                    //Vector3 tempspawn = World.GetNextPositionOnStreet(suspect.Position.Around(transportSpawnDistance));
                    Vector3 SpawnPoint;
                    float Heading;
                    bool UseSpecialID = true;
                    //GetTransportVanSpawnPoint(suspect.Position, out SpawnPoint, out Heading);
                    float travelDistance;
                    int WaitCount = 0;
                    while (true)
                    {
                        SceneManager.GetSpawnPoint(pos, out SpawnPoint, out Heading, UseSpecialID);
                        travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>(SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, playerPed.Position.X, playerPed.Position.Y, playerPed.Position.Z);

                        if (Vector3.Distance(playerPed.Position, SpawnPoint) > EntryPoint.SceneManagementSpawnDistance - 15f)
                        {

                            if (travelDistance < EntryPoint.SceneManagementSpawnDistance * 4.5f)
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
                    veh.Position = SpawnPoint;
                    //Vector3 directionFromVehicleToPed = (suspect.Position - SpawnPoint);
                    //directionFromVehicleToPed.Normalize();

                    //float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                    veh.Heading = Heading;
                    drivingLoopCount = 39;
                    Game.DisplayHelp("Transport taking too long? Hold down ~b~" + EntryPoint.kc.ConvertToString(EntryPoint.SceneManagementKey) + " ~s~to speed it up.", 5000);


                }
                // if van is stuck for a 2nd time or takes too long, spawn it very near to the suspect
                else if (((drivingLoopCount >= 70 || waitCount >= 110) && EntryPoint.AllowWarping) || forceCloseSpawn)
                {
                    Game.Console.Print("Relocating to a close position");

                    Vector3 SpawnPoint = World.GetNextPositionOnStreet(pos.Around2D(15f));

                    int waitCounter = 0;
                    while ((SpawnPoint.Z - pos.Z < -3f) || (SpawnPoint.Z - pos.Z > 3f) || (Vector3.Distance(SpawnPoint, pos) > 25f))
                    {
                        waitCounter++;
                        SpawnPoint = World.GetNextPositionOnStreet(pos.Around2D(15f));
                        GameFiber.Yield();
                        if (waitCounter >= 500)
                        {
                            SpawnPoint = pos.Around2D(15f);
                            break;
                        }
                    }
                    veh.Position = SpawnPoint;
                    Vector3 directionFromVehicleToPed = (pos - SpawnPoint);
                    directionFromVehicleToPed.Normalize();

                    float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                    veh.Heading = vehicleHeading;
                    transportVanTeleported = true;

                    break;
                }
            }

            forceCloseSpawn = true;
            //park the van
            Game.HideHelp();
            while ((Vector3.Distance(pos, veh.Position) > 18f) && !transportVanTeleported)
            {
                Rage.Task parkNearSuspect = driver.Tasks.DriveToPosition(pos, 6f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                parkNearSuspect.WaitForCompletion(800);
                transportVanTeleported = false;
                if (Vector3.Distance(pos, veh.Position) > 80f)
                {
                    Vector3 SpawnPoint = World.GetNextPositionOnStreet(pos.Around2D(12f));
                    veh.Position = SpawnPoint;
                }

            }
            GameFiber.Wait(600);



        }


    }
}
