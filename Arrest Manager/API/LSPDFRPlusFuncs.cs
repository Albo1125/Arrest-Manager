using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace Arrest_Manager.API
{
    internal static class LSPDFRPlusFuncs
    {
        public static void CreateCourtCase(Persona defendant, string crime, int guiltychance, string verdict)
        {
            LSPDFR_.API.Functions.CreateNewCourtCase(defendant, crime, guiltychance, verdict);
        }

        public static string DetermineFineSentence(int MinFine, int MaxFine)
        {
            return LSPDFR_.API.Functions.DetermineFineSentence(MinFine, MaxFine);
        }

        public static string DeterminePrisonSentence(int MinMonths, int MaxMonths, int SuspendedChance)
        {
            return LSPDFR_.API.Functions.DeterminePrisonSentence(MinMonths, MaxMonths, SuspendedChance);
        }

        //public static Guid GenerateSecurityGuid(string PluginName, string AuthorName, string Signature)
        //{
        //    return LSPDFR_.API.ProtectedFunctions.GenerateSecurityGuid(System.Reflection.Assembly.GetExecutingAssembly(), PluginName, AuthorName, Signature);
        //}

        //public static void AddCountToStatistic(Guid SecurityGuid, string Statistic)
        //{
        //    LSPDFR_.API.ProtectedFunctions.AddCountToStatistic(SecurityGuid, Statistic);
        //}

        public static void AddCountToStatistic(string PluginName, string Statistic)
        {
            LSPDFR_.API.ProtectedFunctions.AddCountToStatistic(PluginName, Statistic);
        }

        public static void AddQuestionToTrafficStop(Ped suspect, string Question, string Answer)
        {
            LSPDFR_.API.Functions.AddQuestionToTrafficStop(suspect, Question, Answer);
        }

        public static void AddQuestionToTrafficStop(Ped suspect, string Question, List<string> Answers)
        {
            LSPDFR_.API.Functions.AddQuestionToTrafficStop(suspect, Question, Answers);
        }

        public static void AddQuestionToTrafficStop(Ped suspect, List<string> Questions, List<string> Answers)
        {
            LSPDFR_.API.Functions.AddQuestionToTrafficStop(suspect, Questions, Answers);
        }
    }
}
