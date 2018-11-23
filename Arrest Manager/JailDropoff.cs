using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Arrest_Manager
{
    public class JailDropoff
    {
        [XmlIgnore]
        public Vector3 Position
        {
            get
            {
                return new Vector3(X, Y, Z);
            }
        }
        [XmlIgnore]
        public Blip blip;

        public float X;
        public float Y;
        public float Z;
        public float Heading;
        public bool HasCells;
        public bool OfficerCutscene;
        public bool GroundVehicles;
        public bool AirVehicles;
        public bool WaterVehicles;
        public bool AIDropoff;

        public void CreateBlip()
        {
            blip = new Blip(Position);
            blip.Sprite = BlipSprite.PlayerstateCustody;
            blip.Order = 11;

            NativeFunction.Natives.SET_BLIP_DISPLAY(blip, 3);
        }

        internal bool SuitableForVeh(Vehicle veh)
        {
            if (veh)
            {
                if (veh.IsBoat)
                {
                    return WaterVehicles;
                }
                else if (veh.IsHelicopter || veh.IsPlane)
                {
                    return AirVehicles;
                }
                else
                {
                    return GroundVehicles;
                }
            }
            return false;
        }

        internal static List<JailDropoff> AllJailDropoffs = new List<JailDropoff>();

        internal static List<JailDropoff> DeserializeDropoffs()
        {
            if (Directory.Exists("Plugins/LSPDFR/Arrest Manager/JailDropoffs"))
            {
                foreach (string file in Directory.EnumerateFiles("Plugins/LSPDFR/Arrest Manager/JailDropoffs", "*.xml", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        using (var reader = new StreamReader(file))
                        {
                            XmlSerializer deserializer = new XmlSerializer(typeof(List<JailDropoff>),
                                new XmlRootAttribute("JailDropoffs"));
                            
                            AllJailDropoffs.AddRange((List<JailDropoff>)deserializer.Deserialize(reader));
                        }
                    }
                    catch (Exception e)
                    {
                        Game.LogTrivial(e.ToString());
                        Game.LogTrivial("Arrest Manager - Error parsing XML from " + file);
                    }
                }
            }
            else
            {

            }
            if (AllJailDropoffs.Count == 0)
            {
                Game.DisplayNotification("Arrest Manager couldn't find a valid XML file with Jail Dropoffs in Plugins/LSPDFR/Arrest Manager/JailDropoffs.");
                Game.LogTrivial("Arrest Manager couldn't find a valid XML file with Jail Dropoffs in Plugins/LSPDFR/Arrest Manager/JailDropoffs.");
            }
            return AllJailDropoffs;
        }

        internal static void SerializeJailDropoffs()
        {
            Directory.CreateDirectory("Plugins/LSPDFR/Arrest Manager/JailDropoffs");
            using (StreamWriter writer = new StreamWriter("Plugins/LSPDFR/Arrest Manager/JailDropoffs/default.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<JailDropoff>),
                                new XmlRootAttribute("JailDropoffs"));
                serializer.Serialize(writer, AllJailDropoffs);
            }
        }
    }
}
