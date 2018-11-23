using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using ComputerPlus.API;
using ComputerPlus;
namespace Arrest_Manager.API
{
    internal static class ComputerPlusFuncs
    {
        public static Guid CreateCallout(string CallName, string ShortName, Vector3 Location, int ResponseType, string Description = "", int CallStatus = 2, List<Ped> CallPeds = null, List<Vehicle> CallVehicles = null)
        {
            return ComputerPlus.API.Functions.CreateCallout(new CalloutData(CallName, ShortName, Location, (ComputerPlus.EResponseType)ResponseType, Description, (ComputerPlus.ECallStatus)CallStatus, CallPeds, CallVehicles));
        }

        public static void UpdateCalloutStatus(Guid ID, int Status)
        {
            ComputerPlus.API.Functions.UpdateCalloutStatus(ID, (ECallStatus)Status);
        }

        public static void UpdateCalloutDescription(Guid ID, string Description)
        {
            ComputerPlus.API.Functions.UpdateCalloutDescription(ID, Description);

        }

        public static void SetCalloutStatusToAtScene(Guid ID)
        {
            ComputerPlus.API.Functions.SetCalloutStatusToAtScene(ID);
        }

        public static void ConcludeCallout(Guid ID)
        {
            ComputerPlus.API.Functions.ConcludeCallout(ID);
        }

        public static void CancelCallout(Guid ID)
        {
            ComputerPlus.API.Functions.CancelCallout(ID);
        }

        public static void SetCalloutStatusToUnitResponding(Guid ID)
        {
            ComputerPlus.API.Functions.SetCalloutStatusToUnitResponding(ID);
        }

        public static void AddPedToCallout(Guid ID, Ped PedToAdd)
        {
            ComputerPlus.API.Functions.AddPedToCallout(ID, PedToAdd);
        }

        public static void AddUpdateToCallout(Guid ID, string Update)
        {
            ComputerPlus.API.Functions.AddUpdateToCallout(ID, Update);
        }

        public static void AddVehicleToCallout(Guid ID, Vehicle VehicleToAdd)
        {
            ComputerPlus.API.Functions.AddVehicleToCallout(ID, VehicleToAdd);
        }

        public static void AssignCallToAIUnit(Guid ID)
        {
            ComputerPlus.API.Functions.AssignCallToAIUnit(ID);
        }
    }
}
