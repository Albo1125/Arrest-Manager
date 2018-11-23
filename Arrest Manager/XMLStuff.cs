using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Rage;
using Albo1125.Common.CommonLibrary;
using System.IO;

namespace Arrest_Manager
{
    internal class VehicleSettings
    {
        public Model VehicleModel;
        public int LiveryNumber;
        public int[] ExtraNumbers;
        public VehicleSettings(Model _vehmodel, int _liverynumber, int[] _extranumbers)
        {
            this.VehicleModel = _vehmodel;
            this.LiveryNumber = _liverynumber;
            this.ExtraNumbers = _extranumbers;
        }
    }

    internal class TransportRegion
    {
        public string ZoneName;
        public Model[] DriverModels;
        public Model[] PassengerModels;
        public VehicleSettings[] VehSettings;
    }

    internal class TransportWorldDistrict
    {
        public Zones.WorldDistricts WorldDistrict;
        public Model[] DriverModels;
        public Model[] PassengerModels;
        public VehicleSettings[] VehSettings;
    }
    

    internal static class XMLStuff
    { 
        public static List<TransportWorldDistrict> LoadTransportWorldDistrictsFromXMLFile(string file)
        {
            TransportWorldDistrict DefaultCityDistrict = new TransportWorldDistrict()
            {
                WorldDistrict = Zones.WorldDistricts.City,
                DriverModels = new Model[] { "S_M_Y_COP_01" },
                PassengerModels = new Model[] { "S_F_Y_COP_01" },
                VehSettings = new VehicleSettings[] { new VehicleSettings("POLICET", -1, new int[] { }) }
            };
            TransportWorldDistrict DefaultCountrysideDistrict = new TransportWorldDistrict()
            {
                WorldDistrict = Zones.WorldDistricts.LosSantosCountryside,
                DriverModels = new Model[] { "S_M_Y_SHERIFF_01" },
                PassengerModels = new Model[] { "S_F_Y_SHERIFF_01" },
                VehSettings = new VehicleSettings[] { new VehicleSettings("SHERIFF2", -1, new int[] { }) }
            };
            TransportWorldDistrict DefaultBlaineDistrict = new TransportWorldDistrict()
            {
                WorldDistrict = Zones.WorldDistricts.BlaineCounty,
                DriverModels = new Model[] { "S_M_Y_SHERIFF_01" },
                PassengerModels = new Model[] { "S_F_Y_SHERIFF_01" },
                VehSettings = new VehicleSettings[] { new VehicleSettings("SHERIFF2", -1, new int[] { }) }
            };

            TransportWorldDistrict DefaultWaterDistrict = new TransportWorldDistrict()
            {
                WorldDistrict = Zones.WorldDistricts.Water,
                DriverModels = new Model[] { "s_m_y_uscg_01" },
                PassengerModels = new Model[] { "s_m_y_uscg_01" },
                VehSettings = new VehicleSettings[] { new VehicleSettings("PREDATOR", -1, new int[] { }) }
            };

            try
            {

                #region defaultdocument
                if (!File.Exists(file))
                {
                    Directory.CreateDirectory(Directory.GetParent(file).FullName);
                    new XDocument(
                    new XElement("ArrestManager",
                        new XComment(@"These Transport World Districts are used if you call for transport within a certain world district and you don't have a transport region set up for that district.

    Multiple Transport World Districts can be set up for more than one zone.

    In that case, Arrest Manager will select a random one from the ones that you've specified for that zone.
    There must be at least one Transport World District for each of the following:

    Valid district names are: City, LosSantosCountryside, BlaineCounty, Water.

    Certain restrictions & conditions apply: Driver & Passenger & Vehicle models must be valid.
    A vehicle must have at least 4 free seats and must be a Police vehicle(with the exception of the RIOT van).This means a FLAG_LAW_ENFORCEMENT in vehicles.meta must be present.
    Water districts must have boats as vehicles.

    LiveryNumber and ExtraNumbers are optional.
    For LiveryNumber & ExtraNumbers: Keep in mind the code starts counting at 0.If a LiveryNumber is 1 in OpenIV, it will be 0 in code so you must enter 0.
    If the LiveryNumber is 2 in OpenIV it will be 1 in code so you must enter 1 etc.

    ExtraNumbers must be separated by commas, e.g. 2,3,4,5.

    Naturally, you can add as many TransportWorldDistricts as you like - just keep them between the <ArrestManager> and </ArrestManager> tags.
    The below ones are meant as examples of what you can do.
    The default XML file that comes with the Arrest Manager download (this one, if you haven't changed it) works ingame.

    There's no need to change anything if you don't want to.

    If you don't set anything at all for a certain district (not recommended) a very basic default will be set by Arrest Manager itself.


    Here you can change the ped that's driving the transport vehicle. You can find all valid values here: http://ragepluginhook.net/PedModels.aspx

    Police unit uniforms
    Male City Police: s_m_y_cop_01
    Female City Police: s_f_y_cop_01
    Female Sheriff: s_f_y_sheriff_01
    Male Sheriff: s_m_y_sheriff_01
    Male Highway: s_m_y_hwaycop_01
    Prison Guard: s_m_m_prisguard_01

    Police Vehicle Examples: POLICE, POLICE2, POLICE3, POLICE4, POLICET, SHERIFF, SHERIFF2"


                        ), new XElement("TransportWorldDistrict",
                            new XAttribute("DistrictName", "City"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Driver", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICET"), new XAttribute("LiveryNumber", "0")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICE")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICE2"), new XAttribute("ExtraNumbers", "1")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICE3"), new XAttribute("LiveryNumber", "0"), new XAttribute("ExtraNumbers", "1"))
                        ),

                          new XElement("TransportWorldDistrict",
                            new XAttribute("DistrictName", "City"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Driver", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICET")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICE")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICE2")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICE3"))
                            ),

                          new XElement("TransportWorldDistrict",
                            new XAttribute("DistrictName", "LosSantosCountryside"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_SHERIFF_01")),
                            new XElement("Driver", new XAttribute("Model", "S_F_Y_SHERIFF_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_SHERIFF_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_F_Y_SHERIFF_01")),
                            new XElement("Vehicle", new XAttribute("Model", "SHERIFF2"))
                            ),

                          new XElement("TransportWorldDistrict",
                            new XAttribute("DistrictName", "BlaineCounty"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_SHERIFF_01")),
                            new XElement("Driver", new XAttribute("Model", "S_F_Y_SHERIFF_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_SHERIFF_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_F_Y_SHERIFF_01")),
                            new XElement("Vehicle", new XAttribute("Model", "SHERIFF2"), new XAttribute("LiveryNumber", "0"), new XAttribute("ExtraNumbers", "2,3,4"))
                            ),
                          new XElement("TransportWorldDistrict",
                            new XAttribute("DistrictName", "Water"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_USCG_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_USCG_01")),
                            new XElement("Vehicle", new XAttribute("Model", "PREDATOR"))
                            )
                        )

                    ).Save(file);
                    Game.LogTrivial("Transport world district file did not exist. Created default.");
                }
                #endregion
                XDocument xdoc = XDocument.Load(file);
                char[] trim = new char[] { '\'', '\"', ' ' };
                
                List<TransportWorldDistrict> trnswrlddistrs = xdoc.Descendants("TransportWorldDistrict").Select(x => new TransportWorldDistrict()
                {
                    WorldDistrict = (Zones.WorldDistricts)Enum.Parse(typeof(Zones.WorldDistricts), ((string)x.Attribute("DistrictName")).Trim(trim)),
                    DriverModels = (x.Elements("Driver").Select(y => new Model(((string)y.Attribute("Model")).Trim(trim))).ToArray()),
                    PassengerModels = (x.Elements("Passenger").Select(y => new Model(((string)y.Attribute("Model")).Trim(trim))).ToArray()),
                    VehSettings = (x.Elements("Vehicle").Select(y => new VehicleSettings(new Model((((string)y.Attribute("Model"))).Trim(trim)),
                    (string)y.Attribute("LiveryNumber") != null && !string.IsNullOrWhiteSpace((string)y.Attribute("LiveryNumber")) ? Int32.Parse(((string)y.Attribute("LiveryNumber")).Trim(trim)) : -1,
                    (string)y.Attribute("ExtraNumbers") != null && !string.IsNullOrWhiteSpace((string)y.Attribute("ExtraNumbers")) ? Array.ConvertAll(((string)y.Attribute("ExtraNumbers")).Trim(trim).Replace(" ", "").ToLower().Split(','), int.Parse) : new int[] { })).ToArray()),
                    
                }).ToList<TransportWorldDistrict>();
                
                foreach (Zones.WorldDistricts distr in Enum.GetValues(typeof(Zones.WorldDistricts)))
                {
                    if (!trnswrlddistrs.Select(x => x.WorldDistrict).Contains(distr))
                    {
                        Game.LogTrivial("Transport World Districts doesn't contain " + distr.ToString() + ". Adding default.");
                        if (distr == Zones.WorldDistricts.City)
                        {
                            
                            trnswrlddistrs.Add(DefaultCityDistrict);
                        }
                        else if (distr == Zones.WorldDistricts.LosSantosCountryside)
                        {
                            trnswrlddistrs.Add(DefaultCountrysideDistrict);
                        }
                        else if (distr == Zones.WorldDistricts.BlaineCounty)
                        {
                            trnswrlddistrs.Add(DefaultBlaineDistrict);
                        }
                        else if (distr == Zones.WorldDistricts.Water)
                        {
                            trnswrlddistrs.Add(DefaultWaterDistrict);
                        }

                    }
                }

                return trnswrlddistrs;
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception e)
            {
                Game.LogTrivial("Arrest Manager encountered an exception reading \'" + file + "\'. It was: " + e.ToString());
                Game.DisplayNotification("~r~Error reading Transport World Districts.xml. Setting default values.");
            }

            return new List<TransportWorldDistrict>() { DefaultCityDistrict, DefaultBlaineDistrict, DefaultCountrysideDistrict };
        }

        public static List<TransportRegion> LoadTransportRegionsFromXMLFile(string file)
        {
            try
            {
                #region defaultdocument
                if (!File.Exists(file))
                {
                    Directory.CreateDirectory(Directory.GetParent(file).FullName);
                    new XDocument(
                    new XElement("ArrestManager",
                        new XComment(@"These Transport Regions are used to override the Transport World Districts if you call for transport in a certain Zone that you've set up below.
	Multiple regions can be set up for one zone - in that case, Arrest Manager will select a random transport region from the ones that you've specified for that zone.
	A list of zone names can be found in the Documentation and Licence folder.
		
	The same restrictions apply here as in the world districts file: Driver & Passenger & Vehicle models must be valid. 
	A vehicle must have at least 4 free seats and must be a Police vehicle (with the exception of the RIOT van).
		
	LiveryNumber and ExtraNumbers are optional.
	For LiveryNumber&ExtraNumbers: Keep in mind the code starts counting at 0. If a LiveryNumber is 1 in OpenIV, it will be 0 in code so you must enter 0.
	If the LiveryNumber is 2 in OpenIV it will be 1 in code so you must enter 1 etc.
		
	ExtraNumbers must be separated by commas, e.g. 2, 3, 4, 5.


    The default XML file that comes with the Arrest Manager download(this one, if you haven't changed it) works ingame.

    There's no need to change anything if you don't want to.
    Naturally, you can add as many TransportRegions as you like(add them between the < ArrestManager > and </ ArrestManager > tags).The below regions are meant as examples of what you can do.

        Here you can change the ped that's driving the transport vehicle. You can find all valid values here: http://ragepluginhook.net/PedModels.aspx

    Police unit uniforms
    Male City Police: s_m_y_cop_01
    Female City Police: s_f_y_cop_01
    Female Sheriff: s_f_y_sheriff_01
    Male Sheriff: s_m_y_sheriff_01
    Male Highway: s_m_y_hwaycop_01
    Prison Guard: s_m_m_prisguard_01

    Police Vehicle Examples: POLICE, POLICE2, POLICE3, POLICE4, POLICET, SHERIFF, SHERIFF2"),

                        new XElement("TransportRegion",
                            new XAttribute("ZoneName", "East Vinewood"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Driver", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICE4"), new XAttribute("LiveryNumber", "0"), new XAttribute("ExtraNumbers", "2,3,4"))
                            ),
                        new XElement("TransportRegion",
                            new XAttribute("ZoneName", "West Vinewood"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Driver", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_COP_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_F_Y_COP_01")),
                            new XElement("Vehicle", new XAttribute("Model", "POLICET"), new XAttribute("LiveryNumber", "0"))
                            ),
                        new XElement("TransportRegion",
                            new XAttribute("ZoneName", "Sandy Shores"),
                            new XElement("Driver", new XAttribute("Model", "S_M_Y_SHERIFF_01")),
                            new XElement("Driver", new XAttribute("Model", "S_F_Y_SHERIFF_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_M_Y_SHERIFF_01")),
                            new XElement("Passenger", new XAttribute("Model", "S_F_Y_SHERIFF_01")),
                            new XElement("Vehicle", new XAttribute("Model", "SHERIFF"))
                            )
                            )).Save(file);



                }
                #endregion
                XDocument xdoc = XDocument.Load(file);
                char[] trim = new char[] { '\'', '\"', ' ' };

                List<TransportRegion> trnsregs = xdoc.Descendants("TransportRegion").Select(x => new TransportRegion()
                {
                    ZoneName = ((string)x.Attribute("ZoneName")).Trim(trim),
                    DriverModels = (x.Elements("Driver").Select(y => new Model(((string)y.Attribute("Model")).Trim(trim))).ToArray()),
                    PassengerModels = (x.Elements("Passenger").Select(y => new Model(((string)y.Attribute("Model")).Trim(trim))).ToArray()),
                    VehSettings = (x.Elements("Vehicle").Select(y => new VehicleSettings(new Model((((string)y.Attribute("Model"))).Trim(trim)),
                    (string)y.Attribute("LiveryNumber") != null && !string.IsNullOrWhiteSpace((string)y.Attribute("LiveryNumber")) ? Int32.Parse(((string)y.Attribute("LiveryNumber")).Trim(trim)) : -1,
                    (string)y.Attribute("ExtraNumbers") != null && !string.IsNullOrWhiteSpace((string)y.Attribute("ExtraNumbers")) ? Array.ConvertAll(((string)y.Attribute("ExtraNumbers")).Trim(trim).Replace(" ", "").ToLower().Split(','), int.Parse) : new int[] { })).ToArray()),
                    //VehicleModel = new Model(((string)x.Element("Vehicle").Attribute("Model")).Trim(trim)),
                    //LiveryNumber = (string)x.Element("Vehicle").Attribute("LiveryNumber") != null && !string.IsNullOrWhiteSpace((string)x.Element("Vehicle").Attribute("LiveryNumber")) ?  Int32.Parse(((string)x.Element("Vehicle").Attribute("LiveryNumber")).Trim(trim)) : -1,
                    //ExtraNumbers = (string)x.Element("Vehicle").Attribute("ExtraNumbers") != null && !string.IsNullOrWhiteSpace((string)x.Element("Vehicle").Attribute("ExtraNumbers")) ? Array.ConvertAll(((string)x.Element("Vehicle").Attribute("ExtraNumbers")).Trim(trim).Replace(" ", "").ToLower().Split(','), int.Parse) : new int[] { },
                }).ToList<TransportRegion>();
                
                return trnsregs;
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception e)
            {
                Game.LogTrivial("Arrest Manager encountered an exception reading \'" + file + "\'. It was: " + e.ToString());
                Game.DisplayNotification("~r~Error reading Transport Regions.xml. Setting default values.");
            }
            return new List<TransportRegion>();
        }
    }
}
