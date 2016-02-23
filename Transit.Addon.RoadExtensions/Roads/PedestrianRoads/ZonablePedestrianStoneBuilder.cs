﻿using System;
using System.Collections.Generic;
using System.Linq;
using Transit.Addon.RoadExtensions.Menus;
using Transit.Addon.RoadExtensions.Roads.Common;
using Transit.Framework;
using Transit.Framework.Builders;
using Transit.Framework.Light;

namespace Transit.Addon.RoadExtensions.Roads.PedestrianRoads
{
    public partial class ZonablePedestrianStoneBuilder : Activable, INetInfoBuilderPart
    {
        public int Order { get { return 30; } }
        public int UIOrder { get { return 10; } }

        public string BasedPrefabName { get { return NetInfos.Vanilla.ROAD_2L_BIKE; } }
        public string Name { get { return "Stone Ped Road 16m"; } }
        public string DisplayName { get { return "Medium Stone Pedestrian Road"; } }
        public string Description { get { return "Pedestrian Roads are only accessible to pedestrians, cyclists, and emergency vehicles"; } }
        public string ShortDescription { get { return "No Passenger Vehicles, zoneable"; } }
        public string UICategory { get { return RExExtendedMenus.ROADS_PEDESTRIANS; } }

        public string ThumbnailsPath { get { return @"Roads\Highways\Highway1L\thumbnails.png"; } }
        public string InfoTooltipPath { get { return @"Roads\Highways\Highway1L\infotooltip.png"; } }

        public NetInfoVersion SupportedVersions
        {
            get { return NetInfoVersion.Ground | NetInfoVersion.Elevated | NetInfoVersion.Bridge; }
        }

        public void BuildUp(NetInfo info, NetInfoVersion version)
        {
            ///////////////////////////
            // Template              //
            ///////////////////////////
            var highwayInfo = Prefabs.Find<NetInfo>(NetInfos.Vanilla.HIGHWAY_3L);
            var highwayTunnelInfo = Prefabs.Find<NetInfo>(NetInfos.Vanilla.HIGHWAY_3L_TUNNEL);
            var basicRoadInfo = Prefabs.Find<NetInfo>(NetInfos.Vanilla.ROAD_2L);


            ///////////////////////////
            // 3DModeling            //
            ///////////////////////////
            info.Setup16mNoSWMesh(version);


            ///////////////////////////
            // Texturing             //
            ///////////////////////////
            SetupTextures(info, version);

            ///////////////////////////
            // Set up                //
            ///////////////////////////
            info.m_availableIn = ItemClass.Availability.All;
            info.m_class = highwayInfo.m_class.Clone(NetInfoClasses.NEXT_HIGHWAY1L);
            info.m_surfaceLevel = 0;
            info.m_createPavement = false;
            info.m_createGravel = false;
            //info.m_averageVehicleLaneSpeed = 0.3f;
            info.m_hasParkingSpaces = false;
            info.m_hasPedestrianLanes = true;
            info.m_halfWidth = 8;
            info.m_UnlockMilestone = basicRoadInfo.m_UnlockMilestone;
            info.m_pavementWidth = 2;
            info.m_dlcRequired = SteamHelper.DLC_BitMask.AfterDarkDLC;
            if (version == NetInfoVersion.Tunnel)
            {
                info.m_setVehicleFlags = Vehicle.Flags.Transition;
                info.m_class = highwayTunnelInfo.m_class.Clone(NetInfoClasses.NEXT_HIGHWAY1L_TUNNEL);
            }
            else
            {
                info.m_class = highwayInfo.m_class.Clone(NetInfoClasses.NEXT_HIGHWAY1L);
            }

            // Setting up lanes
            var vehicleLanes = info.m_lanes.Where(l => l.m_laneType == NetInfo.LaneType.Vehicle).ToList();
            var bikeLaneWidth = 2;
            var bikeLanePosAbs = 1;
            var sVehicleLaneWidth = 2.5f;
            var sVehicleLanePosAbs = 4f;
            var tempVLanes = new List<NetInfo.Lane>();
            for (int i = 0; i < vehicleLanes.Count; i++)
            {
                vehicleLanes[i].m_verticalOffset = 0f;
                if (vehicleLanes[i].m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                {
                    vehicleLanes[i].m_position = (Math.Abs(vehicleLanes[i].m_position) / vehicleLanes[i].m_position) * bikeLanePosAbs;
                    vehicleLanes[i].m_width = bikeLaneWidth;
                    tempVLanes.Add(vehicleLanes[i]);
                }
                else if (vehicleLanes[i].m_vehicleType == VehicleInfo.VehicleType.Car)
                {
                    var niLane = new NetInfoLane(vehicleLanes[i])
                    {
                        m_allowedVehicleTypes = VehicleType.ServiceVehicles,
                        m_position = (Math.Abs(vehicleLanes[i].m_position) / vehicleLanes[i].m_position) * sVehicleLanePosAbs,
                        m_width = sVehicleLaneWidth,
                        m_speedLimit = 0.3f
                    };
                    tempVLanes.Add(niLane);
                }
            }
            var pedLanes = info.m_lanes.Where(l => l.m_laneType == NetInfo.LaneType.Pedestrian).OrderBy(l => l.m_position).ToList();
            pedLanes[0].m_position = -5;
            pedLanes[0].m_width = -6;
            pedLanes[1].m_position = 5;
            pedLanes[1].m_width = 6;

            var roadCollection = new List<NetInfo.Lane>();
            roadCollection.AddRange(tempVLanes);
            roadCollection.AddRange(pedLanes);
            info.m_lanes = roadCollection.ToArray();
            ///////////////////////////
            // AI                    //
            ///////////////////////////
            var hwPlayerNetAI = highwayInfo.GetComponent<PlayerNetAI>();
            var playerNetAI = info.GetComponent<PlayerNetAI>();

            if (hwPlayerNetAI != null && playerNetAI != null)
            {
                playerNetAI.m_constructionCost = hwPlayerNetAI.m_constructionCost * 1 / 2;
                playerNetAI.m_maintenanceCost = hwPlayerNetAI.m_maintenanceCost * 1 / 2;
            }

            var roadBaseAI = info.GetComponent<RoadBaseAI>();

            if (roadBaseAI != null)
            {
                roadBaseAI.m_trafficLights = false;
            }

            var roadAI = info.GetComponent<RoadAI>();

            if (roadAI != null)
            {
                roadAI.m_enableZoning = true;
            }
        }
    }
}