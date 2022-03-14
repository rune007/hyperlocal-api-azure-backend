using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using System.Diagnostics;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {

        /// <summary>
        /// Checks whether a latitude and longitude falls within the territory of Denmark.
        /// </summary>
        public bool IsLatLongWithinDenmarkBL(double latitude, double longitude)
        {
            try
            {
                var isWithinDenmark = entityFramework.procIsGeographyInDenmark(ConvertLatLongToPointWkt(longitude, latitude)).Single();

                if (isWithinDenmark > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.IsLatLongWithinDenmarkBL(): " + ex.ToString());
                return false;
            } 
        }


        /// <summary>
        /// Checks whether a geograhphy WKT falls in the territory of Denmark.
        /// </summary>
        public bool IsGeographyInDenmarkBL(string geographyWkt)
        {
            try
            {
                var isWithinDenmark = entityFramework.procIsGeographyInDenmark(geographyWkt).Single();

                if (isWithinDenmark > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.IsGeographyInDenmarkBL(): " + ex.ToString());
                return false;
            }           
        }


        /// <summary>
        /// Checks whether the geography WKT can be made into a valid polygon.
        /// </summary>
        public bool IsPolygonValidBL(string geographyWkt)
        {
            try
            {
                var isValid = entityFramework.procIsPolygonValid(geographyWkt).Single();

                if (isValid > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.IsPolygonValidBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// We have a size limit of 15 SQ KM for communities, this method checks whether the polygon exceeds this size.
        /// </summary>
        public bool IsPolygonTooBigBL(string geographyWkt)
        {
            try
            {
                var isTooBig = entityFramework.procIsPolygonTooBig(geographyWkt).Single();

                if (isTooBig > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.IsPolygonTooBigBL(): " + ex.ToString());
                return true;
            }
        }


        /// <summary>
        /// In conjunction with Community polling, we only want to allow the users which live in the Community area and the creater 
        /// of the Community to vote. This method checks whether a particular User lives inside the area of a particular Community.
        /// </summary>
        public bool IsUserLivingInCommunityAreaBL(int userId, int communityId)
        {
            try
            {
                var doesUser = entityFramework.procIsUserLivingInCommunityArea(userId, communityId).Single();

                if (doesUser > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.IsUserLivingInCommunityAreaBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Gets the polygons covering the whole of Danish territory.
        /// </summary>
        public List<PolygonDto> GetDKCountryPolygonsBL()
        {
            try
            {
                var polygons = entityFramework.procGetDKCountryPolygons();

                var polygonDtos = new List<PolygonDto>();

                if (polygons != null)
                {
                    foreach (var p in polygons)
                    {
                        polygonDtos.Add
                        (
                            new PolygonDto()
                            {
                                PolygonWkt = p
                            }
                        );
                    }
                    return polygonDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetDKCountryPolygonsBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Getting the polygons for a particular region.
        /// </summary>
        public List<PolygonDto> GetRegionPolygonsBL(string region)
        {
            try
            {
                var polygons = entityFramework.procGetRegionPolygons(region);

                var polygonDtos = new List<PolygonDto>();

                if (polygons != null)
                {
                    foreach (var p in polygons)
                    {
                        polygonDtos.Add
                        (
                            new PolygonDto()
                            {
                                PolygonWkt = p
                            }
                        );
                    }
                    return polygonDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetRegionPolygonsBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the polygons for a particular region, but without holes in the polygons. This is for rendering on 
        /// the Windows Phone. My C# parsing the WKT, on the phone, does not understand holes in polygons.
        /// </summary>
        public List<PolygonDto> GetRegionPolygonsWithoutHolesBL(string region)
        {
            try
            {
                var polygons = entityFramework.procGetRegionPolygonsWithoutHoles(region);

                var polygonDtos = new List<PolygonDto>();

                if (polygons != null)
                {
                    foreach (var p in polygons)
                    {
                        polygonDtos.Add
                        (
                            new PolygonDto()
                            {
                                PolygonWkt = p
                            }
                        );
                    }
                    return polygonDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetRegionPolygonsWithoutHolesBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the polygons for a particular municipality.
        /// </summary>
        public List<PolygonDto> GetMunicipalityPolygonsBL(string municipality)
        {
            try
            {
                var polygons = entityFramework.procGetMunicipalityPolygons(municipality);

                var polygonDtos = new List<PolygonDto>();

                if (polygons != null)
                {
                    foreach (var p in polygons)
                    {
                        polygonDtos.Add
                        (
                            new PolygonDto()
                            {
                                PolygonWkt = p
                            }
                        );
                    }
                    return polygonDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetMunicipalityPolygonsBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the polygons for a particular municipality, but without holes in the polygons. This is for rendering on 
        /// the Windows Phone. My C# parsing the WKT, on the phone, does not understand holes in polygons.
        /// </summary>
        public List<PolygonDto> GetMunicipalityPolygonsWithoutHolesBL(string municipality)
        {
            try
            {
                var polygons = entityFramework.procGetMunicipalityPolygonsWithoutHoles(municipality);

                var polygonDtos = new List<PolygonDto>();

                if (polygons != null)
                {
                    foreach (var p in polygons)
                    {
                        polygonDtos.Add
                        (
                            new PolygonDto()
                            {
                                PolygonWkt = p
                            }
                        );
                    }
                    return polygonDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetMunicipalityPolygonsWithoutHolesBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the polygons for a particular postal code.
        /// </summary>
        public List<PolygonDto> GetPostalCodePolygonsBL(string postalCode)
        {
            try
            {
                var polygons = entityFramework.procGetPostalCodePolygons(postalCode);

                var polygonDtos = new List<PolygonDto>();

                if (polygons != null)
                {
                    foreach (var p in polygons)
                    {
                        polygonDtos.Add
                        (
                            new PolygonDto()
                            {
                                PolygonWkt = p
                            }
                        );
                    }
                    return polygonDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetPostalCodePolygonsBL(): " + ex.ToString());
                return null;
            }
        }


        private int GetNumberOfUsersInCommunity(int communityId)
        {
            try
            {
                var number = Convert.ToInt32(entityFramework.procGetNumberOfUsersInCommunity(communityId).Single());
                return number;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetNumberOfUsersInCommunity(): " + ex.ToString());
                return 0;
            }
        }


        /// <summary>
        /// Gets the center point of Denmark, used for adjusting map.
        /// </summary>
        public PointDto GetCenterPointOfDkCountryBL()
        {
            try
            {
                var pointWkt = entityFramework.procGetCenterPointOfDkCountry().SingleOrDefault();

                if (pointWkt != null)
                {
                    return new PointDto()
                    {
                        Latitude = ExtractLatitudeFromPointWkt(pointWkt),
                        Longitude = ExtractLongitudeFromPointWkt(pointWkt)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetCenterPointOfDkCountryBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the center point of a Region, used for adjusting map.
        /// </summary>
        public PointDto GetCenterPointOfRegionBL(string urlRegionName)
        {
            try
            {
                var pointWkt = entityFramework.procGetCenterPointOfRegion(urlRegionName).SingleOrDefault();

                if (pointWkt != null)
                {
                    return new PointDto()
                    {
                        Latitude = ExtractLatitudeFromPointWkt(pointWkt),
                        Longitude = ExtractLongitudeFromPointWkt(pointWkt)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetCenterPointOfRegionBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the center point of a Municipality, used for adjusting map.
        /// </summary>
        public PointDto GetCenterPointOfMunicipalityBL(string urlMunicipalityName)
        {
            try
            {
                var pointWkt = entityFramework.procGetCenterPointOfMunicipality(urlMunicipalityName).SingleOrDefault();

                if (pointWkt != null)
                {
                    return new PointDto()
                    {
                        Latitude = ExtractLatitudeFromPointWkt(pointWkt),
                        Longitude = ExtractLongitudeFromPointWkt(pointWkt)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetCenterPointOfMunicipalityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the center point of a PostalCode, used for adjusting map.
        /// </summary>
        public PointDto GetCenterPointOfPostalCodeBL(string POSTNR_TXT)
        {
            try
            {
                var pointWkt = entityFramework.procGetCenterPointOfPostalCode(POSTNR_TXT).SingleOrDefault();

                if (pointWkt != null)
                {
                    return new PointDto()
                    {
                        Latitude = ExtractLatitudeFromPointWkt(pointWkt),
                        Longitude = ExtractLongitudeFromPointWkt(pointWkt)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetCenterPointOfPostalCodeBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the center point of a Community, used for adjusting map.
        /// </summary>
        public PointDto GetCenterPointOfCommunityBL(int communityId)
        {
            try
            {
                var pointWkt = entityFramework.procGetCenterPointOfCommunity(communityId).SingleOrDefault();

                if (pointWkt != null)
                {
                    return new PointDto()
                    {
                        Latitude = ExtractLatitudeFromPointWkt(pointWkt),
                        Longitude = ExtractLongitudeFromPointWkt(pointWkt)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in SpatialQueryBL.GetCenterPointOfCommunityBL(): " + ex.ToString());
                return null;
            }
        }
    }
}