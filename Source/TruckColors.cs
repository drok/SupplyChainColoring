/*(
 * Supply Chain Coloring - Cities Skylines mod
 *  Copyright (C) 2020 Radu Hociung <radu.csmods@ohmi.org>
 *  
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */
namespace SupplyChainColoring.PatchCarAI
{
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;

    [HarmonyPatch(typeof(CargoTruckAI), "GetColor")]
    public static class TruckColors
    {
        /*
         * BUG FIX: In CS 1.13.1-f1-steam-win, in VehicleAI, the trucks transporting
         * Industry goods are not color coded on the Natural Resources info view.
         * This patch fixes it and colors cargo trucks corresponding to the resource they
         * transport.
         */
        [HarmonyPrefix]
        [UsedImplicitly]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Output argument is __result as required by Harmony")]
        public static bool Prefix(ref VehicleAI __instance, ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref UnityEngine.Color __result)
        {
            InfoManager.SubInfoMode filterByResource = InfoManager.SubInfoMode.None;

            switch (infoMode)
            {
                case InfoManager.InfoMode.NaturalResources:
                    ItemClass.SubService subservice;

                    switch ((TransferManager.TransferReason)data.m_transferType)
                    {
                        case TransferManager.TransferReason.Ore:
                        case TransferManager.TransferReason.Coal:
                        case TransferManager.TransferReason.Glass:
                        case TransferManager.TransferReason.Metals:
                            subservice = ItemClass.SubService.IndustrialOre;
                            break;

                        case TransferManager.TransferReason.Logs:
                        case TransferManager.TransferReason.Lumber:
                        case TransferManager.TransferReason.Paper:
                        case TransferManager.TransferReason.PlanedTimber:
                            subservice = ItemClass.SubService.IndustrialForestry;
                            break;

                        case TransferManager.TransferReason.Oil:
                        case TransferManager.TransferReason.Petrol:
                        case TransferManager.TransferReason.Petroleum:
                        case TransferManager.TransferReason.Plastics:
                            subservice = ItemClass.SubService.IndustrialOil;
                            break;

                        case TransferManager.TransferReason.Grain:
                        case TransferManager.TransferReason.Food:
                        case TransferManager.TransferReason.Flours:
                        case TransferManager.TransferReason.AnimalProducts:
                            subservice = ItemClass.SubService.IndustrialFarming;
                            break;

                        case TransferManager.TransferReason.LuxuryProducts:
                            __result = IndustryWorldInfoPanel.instance.GetResourceColor((TransferManager.TransferReason)data.m_transferType);
                            return false;

                        case TransferManager.TransferReason.Goods:
                        case TransferManager.TransferReason.Fish:
                            __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[data.m_transferType];
                            return false;

                        default:
                            subservice = ItemClass.SubService.None;
                            break;
                    }

                    if (subservice != ItemClass.SubService.None)
                    {
                        if (subservice == ItemClass.SubService.IndustrialGeneric)
                        {
                            __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)InfoManager.InfoMode.Industry].m_activeColor;
                        }
                        else
                        {
                            __result = IndustrialBuildingAI.GetResourceColor(subservice);
                        }
                    }
                    else
                    {
                        __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }

                    break;
                case InfoManager.InfoMode.Industry:
                    ushort sourceBuilding = data.m_sourceBuilding;
                    if (sourceBuilding != 0)
                    {
                        BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[sourceBuilding].Info;
                        if ((object)info != null)
                        {
                            switch (info.m_class.m_subService)
                            {
                                case ItemClass.SubService.IndustrialOre:
                                    subservice = ItemClass.SubService.PlayerIndustryOre;
                                    break;
                                case ItemClass.SubService.IndustrialForestry:
                                    subservice = ItemClass.SubService.PlayerIndustryForestry;
                                    break;
                                case ItemClass.SubService.IndustrialOil:
                                    subservice = ItemClass.SubService.PlayerIndustryOil;
                                    break;
                                case ItemClass.SubService.IndustrialFarming:
                                    subservice = ItemClass.SubService.PlayerIndustryFarming;
                                    break;
                                case ItemClass.SubService.IndustrialGeneric: // Unspecialized industry (goods manufacturers)
                                    __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[data.m_transferType];
                                    return false;
                                default:
                                    switch (info.m_class.m_service)
                                    {
                                        case ItemClass.Service.PlayerIndustry:
                                            switch (info.m_class.m_subService)
                                            {
                                                case ItemClass.SubService.None: // Warehouse, Unique Factories
                                                    filterByResource = IndustryBuildingAI.ResourceToInfoMode((TransferManager.TransferReason)data.m_transferType);
                                                    subservice = ItemClass.SubService.None;

                                                    if (filterByResource == InfoManager.SubInfoMode.None)
                                                    {
                                                        if ((TransferManager.TransferReason)data.m_transferType == TransferManager.TransferReason.Goods)
                                                        {
                                                            __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[data.m_transferType];
                                                            return false;
                                                        }
                                                        else
                                                        {
                                                            filterByResource = Singleton<InfoManager>.instance.CurrentSubMode;
                                                        }
                                                    }

                                                    break;
                                                default:
                                                    subservice = info.m_class.m_subService;
                                                    break;
                                            }

                                            break;
                                        // Should I color Goods from fish factories on the Industry Areas view?
                                        // No, they are not participants in the Industries supply chains
                                        default:
                                            ushort targetBuilding = data.m_targetBuilding;
                                            if (targetBuilding != 0)
                                            {
                                                BuildingInfo tinfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].Info;
                                                if ((object)tinfo != null)
                                                {
                                                    switch (tinfo.m_class.m_subService)
                                                    {
                                                        case ItemClass.SubService.IndustrialOre:
                                                            subservice = ItemClass.SubService.PlayerIndustryOre;
                                                            break;
                                                        case ItemClass.SubService.IndustrialForestry:
                                                            subservice = ItemClass.SubService.PlayerIndustryForestry;
                                                            break;
                                                        case ItemClass.SubService.IndustrialOil:
                                                            subservice = ItemClass.SubService.PlayerIndustryOil;
                                                            break;
                                                        case ItemClass.SubService.IndustrialFarming:
                                                            subservice = ItemClass.SubService.PlayerIndustryFarming;
                                                            break;
                                                        default:
                                                            filterByResource = IndustryBuildingAI.ResourceToInfoMode((TransferManager.TransferReason)data.m_transferType);

                                                            if (filterByResource == InfoManager.SubInfoMode.None)
                                                            {
                                                                switch (tinfo.m_class.m_service)
                                                                {
                                                                    case ItemClass.Service.Road: // Ie, exporting, from import
                                                                        filterByResource = InfoManager.SubInfoMode.None;
                                                                        break;
                                                                    case ItemClass.Service.PlayerIndustry:
                                                                    case ItemClass.Service.Industrial: // Unspecialized industry
                                                                        break;
                                                                    default:
                                                                        return true;
                                                                }
                                                            }
                                                            subservice = ItemClass.SubService.None;
                                                            break;
                                                    } // Filter by industry type
                                                }
                                                else
                                                {
                                                    return true;
                                                }
                                            }
                                            else
                                            {
                                                filterByResource = IndustryBuildingAI.ResourceToInfoMode((TransferManager.TransferReason)data.m_transferType);
                                                subservice = ItemClass.SubService.None;
                                                break;
                                            } // Filter by Material

                                            break;
                                    }

                                    break;
                            } // Filter by industry type

                            if (filterByResource == Singleton<InfoManager>.instance.CurrentSubMode || IndustryBuildingAI.ServiceToInfoMode(subservice) == Singleton<InfoManager>.instance.CurrentSubMode)
                            {
                                __result = IndustryWorldInfoPanel.instance.GetResourceColor((TransferManager.TransferReason)data.m_transferType);
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }

                    __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    return false;
                case InfoManager.InfoMode.Fishing:
                    BuildingInfo ownerInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                    System.Type ownerType = ownerInfo.m_buildingAI.GetType();
                    if (ownerType == typeof(FishingHarborAI))
                    {
                        var ai = ownerInfo.m_buildingAI as FishingHarborAI;
                        switch (ai.m_boatClass.m_level)
                        {
                            case ItemClass.Level.Level1: // generic
                                __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[data.m_transferType];
                                break;
                            case ItemClass.Level.Level2: // salmon
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorShallowFlowing;
                                break;
                            case ItemClass.Level.Level3: // shellfish
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorDeepStill;
                                break;
                            case ItemClass.Level.Level4: // Tuna
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorDeepFlowing;
                                break;
                            case ItemClass.Level.Level5: // Anchovies
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorShallowStill;
                                break;
                        }

                        return false;
                    } else if (ownerType == typeof(FishFarmAI))
                    {
                        __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[data.m_transferType];
                        return false;
                    }

                    return true;

                default:
                    return true;
            }

            return false;
        }
    }
}
