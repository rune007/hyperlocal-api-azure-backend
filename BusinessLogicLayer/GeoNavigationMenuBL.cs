using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data.Objects;
using System.Globalization;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data.Common;
using HLServiceRole.DataTransferObjects;
using System.Data;
using System.Diagnostics;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// Getting all the regions, used for navigation.
        /// </summary>
        public List<RegionDto> GetAllRegionsBL()
        {
            try
            {
                /* Calling the SPROC procGetAllRegions(). */
                var regions = entityFramework.procGetAllRegions();

                var regionDtos = new List<RegionDto>();

                foreach (var r in regions)
                {
                    regionDtos.Add
                    (
                        new RegionDto()
                        {
                            REGIONNAVN = r.REGIONNAVN,
                            UrlRegionName = r.UrlRegionName
                        }
                    );
                }
                return regionDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in GeoNavigationMenuBL.GetAllRegionsBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Getting the municipalities for a region, used for navigation.
        /// </summary>
        public List<MunicipalityDto> GetMunicipalitiesForRegionBL(string region)
        {
            try
            {
                /* Calling the SPROC procGetMunicipalitiesForRegion(). */
                var municipalities = entityFramework.procGetMunicipalitiesForRegion(region);

                var municipalityDtos = new List<MunicipalityDto>();

                foreach (var m in municipalities)
                {
                    municipalityDtos.Add
                    (
                        new MunicipalityDto()
                        {
                            KOMNAVN = m.KOMNAVN,
                            UrlMunicipalityName = m.UrlMunicipalityName
                        }
                    );
                }
                return municipalityDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in GeoNavigationMenuBL.GetMunicipalitiesForRegionBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Getting the postal codes for a municipality, used for navigation.
        /// </summary>
        public List<PostalCodeDto> GetPostalCodesForMunicipalityBL(string municipality)
        {
            try
            {
                /* Calling the SPROC procGetPostalCodesForMunicipality(). */
                var postalCodesForMunicipality = entityFramework.procGetPostalCodesForMunicipality(municipality);

                var postalCodeDtos = new List<PostalCodeDto>();

                foreach (var p in postalCodesForMunicipality)
                {
                    postalCodeDtos.Add
                    (
                        new PostalCodeDto()
                        {
                            POSTNR_TXT = p.POSTNR_TXT,
                            POSTBYNAVN = p.POSTBYNAVN
                        }
                    );
                }
                return postalCodeDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in GeoNavigationMenuBL.GetPostalCodesForMunicipalityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets all postal codes.
        /// </summary>
        public List<PostalCodeDto> GetAllPostalCodesBL()
        {
            try
            {
                var postalCodes = entityFramework.procGetAllPostalCodes();

                var postalCodeDtos = new List<PostalCodeDto>();

                foreach (var p in postalCodes)
                {
                    postalCodeDtos.Add
                    (
                        new PostalCodeDto()
                        {
                            POSTNR_TXT = p.POSTNR_TXT,
                            POSTBYNAVN = p.POSTBYNAVN
                        }
                    );
                }
                return postalCodeDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in GeoNavigationMenuBL.GetAllPostalCodesBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets all municipalities.
        /// </summary>
        public List<MunicipalityDto> GetAllMunicipalitiesBL()
        {
            try
            {
                var municipalities = entityFramework.procGetAllMunicipalities();

                var municipalityDtos = new List<MunicipalityDto>();

                foreach (var m in municipalities)
                {
                    municipalityDtos.Add
                    (
                        new MunicipalityDto()
                        {
                            KOMNAVN = m.KOMNAVN,
                            UrlMunicipalityName = m.UrlMunicipalityName
                        }
                    );
                }
                return municipalityDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in GeoNavigationMenuBL.GetAllMunicipalitiesBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Takes in REGIONNAVN (e.g. "Sjælland") and returns UrlRegionName (e.g. "Sjaelland")
        /// </summary>
        /// <param name="REGIONNAVN">e.g. "Sjælland"</param>
        /// <returns>UrlRegionName e.g. "Sjaelland"</returns>
        public RegionDto GetUrlRegionNameBL(string REGIONNAVN)
        {
            try
            {
                var name = entityFramework.procGetUrlRegionName(REGIONNAVN).Single();

                if (name != null)
                {
                    var dto = new RegionDto()
                    {
                        UrlRegionName = name
                    };
                    return dto;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in GeoNavigationMenuBL.GetUrlRegionNameBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Takes in KOMNAVN (e.g. "Hillerød") and returns UrlMunicipalityName (e.g. "Hilleroed")
        /// </summary>
        /// <param name="KOMNAVN">e.g. Hillerød</param>
        /// <returns>e.g. Hilleroed</returns>
        public MunicipalityDto GetUrlMunicipalityNameBL(string KOMNAVN)
        {
            try
            {
                var name = entityFramework.procGetUrlMunicipalityName(KOMNAVN).Single();

                if (name != null)
                {
                    var dto = new MunicipalityDto()
                    {
                        UrlMunicipalityName = name
                    };
                    return dto;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in GeoNavigationMenuBL.GetUrlMunicipalityNameBL(): " + ex.ToString());
                return null;
            }
        }
    }
}