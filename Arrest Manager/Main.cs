using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Arrest_Manager
{
    using LSPD_First_Response.Mod.API;
    using Rage;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    internal class Main : Plugin
    {
        
        public Main()
        {
            Albo1125.Common.UpdateChecker.VerifyXmlNodeExists(PluginName, FileID, DownloadURL, Path);
            Albo1125.Common.DependencyChecker.RegisterPluginForDependencyChecks(PluginName);   
        }
      
        public override void Finally()
        {
            
        }

        public override void Initialize()
        {
            //Event handler for detecting if the player goes on duty
            Game.LogTrivial("Arrest Manager " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", developed by Albo1125, loaded successfully!");
            
            Game.LogTrivial("Please go on duty to start Arrest Manager.");

            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;

        }
        internal static Version Albo1125CommonVer = new Version("6.6.3.0");
        internal static Version MadeForGTAVersion = new Version("1.0.1604.1");
        internal static float MinimumRPHVersion = 0.51f;
        internal static string[] AudioFilesToCheckFor = new string[] { "LSPDFR/audio/scanner/Arrest Manager Audio/Camera.wav" };
        internal static Version RAGENativeUIVersion = new Version("1.6.3.0");
        internal static Version MadeForLSPDFRVersion = new Version("0.4.2");
        internal static string[] OtherFilesToCheckFor = new string[] { };

        internal static string FileID = "8107";
        internal static string DownloadURL = "http://bit.ly/ArrestManager42";
        internal static string PluginName = "Arrest Manager";
        internal static string Path = "Plugins/LSPDFR/Arrest Manager.dll";

        public static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                if (Albo1125.Common.DependencyChecker.DependencyCheckMain(PluginName, Albo1125CommonVer, MinimumRPHVersion, MadeForGTAVersion, MadeForLSPDFRVersion, RAGENativeUIVersion, AudioFilesToCheckFor, OtherFilesToCheckFor))
                {
                    EntryPoint.Initialise();
                }
            }
        }
    }
}