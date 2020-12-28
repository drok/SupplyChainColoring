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
namespace SupplyChainColoring.PatchIndustrialBuildingAI
{
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;
    using System.Collections.Generic;
    using System.Reflection;

    [HarmonyPatch]
    /*
     * BUG FIX: In CS 1.13.1-f1-steam-win, in ProcessingFacilityAI, Industry buildings
     * are not colored based on the natural resources they use.
     * This patch adds coloring based on natural resource
     *
     * Also, In 1.13.1-f1-steam-win, in IndustrialBuildingAI.GetColor,
     * processing buildings are intended to be colored by the incoming resource color,
     * but ItemClass.SubService.IndustrialGeneric does not yield a resource color.
     * This looks like a bug; the patch here fixes it by using GetIncomingTransferReason()
     * to determine the color of unspecialized industrial buildings:
     *
     * 		case InfoManager.InfoMode.NaturalResources:
			if (ShowConsumption(buildingID, ref data))
			{
				return GetResourceColor(m_info.m_class.m_subService);
			}
			return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
     *
     */
    public static class ProcessorColors
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (SupplyChainColoringMod.HaveIndustriesDLC)
                yield return AccessTools.Method(typeof(IndustryBuildingAI), "GetColor");
            yield return AccessTools.Method(typeof(IndustrialExtractorAI), "GetColor");
            yield return AccessTools.Method(typeof(IndustrialBuildingAI), "GetColor");
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Output argument is __result as required by Harmony")]
        public static bool Prefix(CommonBuildingAI __instance, ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref UnityEngine.Color __result)
        {

            if (data.Info.m_class.m_service != ItemClass.Service.PlayerIndustry && data.Info.m_class.m_service != ItemClass.Service.Industrial)
            {
                return true;
            }

            switch (infoMode)
            {
                case InfoManager.InfoMode.NaturalResources:
                    if (ShowConsumption(buildingID, ref data))
                    {
                        ItemClass.SubService subservice;

                        switch (data.Info.m_class.m_subService)
                        {
                            case ItemClass.SubService.IndustrialOre:
                            case ItemClass.SubService.PlayerIndustryOre:
                                subservice = ItemClass.SubService.IndustrialOre;
                                break;
                            case ItemClass.SubService.IndustrialForestry:
                            case ItemClass.SubService.PlayerIndustryForestry:
                                subservice = ItemClass.SubService.IndustrialForestry;
                                break;
                            case ItemClass.SubService.IndustrialOil:
                            case ItemClass.SubService.PlayerIndustryOil:
                                subservice = ItemClass.SubService.IndustrialOil;
                                break;
                            case ItemClass.SubService.IndustrialFarming:
                            case ItemClass.SubService.PlayerIndustryFarming:
                                subservice = ItemClass.SubService.IndustrialFarming;
                                break;
                            case ItemClass.SubService.IndustrialGeneric:
                                // Unspecialized IndustrialBuildingAI requires one of two input materials at different times in the simulation.
                                // one is the primary resource (Zoned Industry processed good)
                                // the other is the secondary resource (Industries DLC processed good)
                                // For extractors and processors, the color is a resource color (match with Legend on the "Natural Resources" panel
                                // For goods producers, the color is a material color, toggling periodically between the two resources accepted.

                                __result = IndustryWorldInfoPanel.instance.GetResourceColor(
                                    (!SupplyChainColoringMod.HaveIndustriesDLC || (Singleton<SimulationManager>.instance.m_currentFrameIndex & 0x400) == 0) ?
                                    Mappings.GetIncomingTransferReason(buildingID) :
                                    Mappings.GetSecondaryIncomingTransferReason(buildingID));
                                return false;
                            case ItemClass.SubService.None: // Unique building
                                if (data.Info.m_class.m_service == ItemClass.Service.PlayerIndustry)
                                {
                                    ProcessingFacilityAI ai = __instance as ProcessingFacilityAI;
                                    if (ai != null)
                                    {
                                        int num =
                                            ai.m_inputResource4 != TransferManager.TransferReason.None ? 4 :
                                            ai.m_inputResource3 != TransferManager.TransferReason.None ? 3 :
                                            ai.m_inputResource2 != TransferManager.TransferReason.None ? 2 : 1;

                                        __result = IndustryWorldInfoPanel.instance.GetResourceColor(((Singleton<SimulationManager>.instance.m_currentFrameIndex >> 10) % num) switch
                                        {
                                            0 => ai.m_inputResource1,
                                            1 => ai.m_inputResource2,
                                            2 => ai.m_inputResource3,
                                            _ => ai.m_inputResource4,
                                        });
                                        return false;
                                    }
                                }
                                subservice = ItemClass.SubService.None;
                                break;
                            default:
                                subservice = ItemClass.SubService.None;
                                break;
                        }
                        __result = IndustrialBuildingAI.GetResourceColor(subservice);
                    }
                    else
                    {
                        __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }

                    break;

                case InfoManager.InfoMode.Industry:
                    ItemClass.SubService submode;
                    InfoManager.SubInfoMode filterByResource = InfoManager.SubInfoMode.None;

                    switch (data.Info.m_class.m_subService) {

                        case ItemClass.SubService.IndustrialOre:
                        case ItemClass.SubService.PlayerIndustryOre:
                            submode = ItemClass.SubService.PlayerIndustryOre;
                            break;
                        case ItemClass.SubService.IndustrialForestry:
                        case ItemClass.SubService.PlayerIndustryForestry:
                            submode = ItemClass.SubService.PlayerIndustryForestry;
                            break;
                        case ItemClass.SubService.IndustrialOil:
                        case ItemClass.SubService.PlayerIndustryOil:
                            submode = ItemClass.SubService.PlayerIndustryOil;
                            break;
                        case ItemClass.SubService.IndustrialFarming:
                        case ItemClass.SubService.PlayerIndustryFarming:
                            submode = ItemClass.SubService.PlayerIndustryFarming;
                            break;
                        case ItemClass.SubService.IndustrialGeneric:
                            submode = ItemClass.SubService.None;
                            filterByResource = IndustryBuildingAI.ResourceToInfoMode(Mappings.GetSecondaryIncomingTransferReason(buildingID));
                            break;
                        case ItemClass.SubService.None: // Unique Factory
                            submode = ItemClass.SubService.None;
                            switch (data.Info.m_class.m_level) {
                                case ItemClass.Level.Level5: // Unique Factory
                                    ProcessingFacilityAI ai = __instance as ProcessingFacilityAI;
                                    if (ai != null)
                                    {
                                        if (IndustryBuildingAI.ResourceToInfoMode(ai.m_inputResource1) == Singleton<InfoManager>.instance.CurrentSubMode ||
                                            IndustryBuildingAI.ResourceToInfoMode(ai.m_inputResource2) == Singleton<InfoManager>.instance.CurrentSubMode ||
                                            IndustryBuildingAI.ResourceToInfoMode(ai.m_inputResource3) == Singleton<InfoManager>.instance.CurrentSubMode ||
                                            IndustryBuildingAI.ResourceToInfoMode(ai.m_inputResource4) == Singleton<InfoManager>.instance.CurrentSubMode)
                                        {
                                            filterByResource = Singleton<InfoManager>.instance.CurrentSubMode;
                                        }
                                    }
                                    break;
                            }
                            break;

                        default:
                            submode = ItemClass.SubService.None;
                            break;
                    }

                    if (filterByResource == Singleton<InfoManager>.instance.CurrentSubMode || Singleton<InfoManager>.instance.CurrentSubMode == IndustryBuildingAI.ServiceToInfoMode(submode))
                    {
                        if ((data.m_flags & Building.Flags.Active) != 0 ||
                            (data.Info.m_class.m_service == ItemClass.Service.Industrial) /* HACK, I don't know where to to put UpdateBuildingColors() when an Industry building is finished building and becomes active, so it will be shown in its active colour while building or collapsed  */)
                        {
                            TransferManager.TransferReason material;
                            material =
                                    (data.Info.m_class.m_service == ItemClass.Service.Industrial) ? Mappings.GetOutgoingTransferReason(data.Info.m_class) :
                                    (__instance as ProcessingFacilityAI)?.m_outputResource ??
                                    (__instance as ExtractingFacilityAI)?.m_outputResource ??
                                    TransferManager.TransferReason.None;

                            if (material == TransferManager.TransferReason.Goods)
                            {
                                __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)material];
                            } else
                            {
                                __result = IndustryWorldInfoPanel.instance.GetResourceColor(material);
                            }
                        }
                        else
                        {
                            __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                        }
                    }
                    else
                    {
                        __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }

                    break;
                default:
                    return true;
            }

            return false;
        }

        private static bool ShowConsumption(ushort buildingID, ref Building data)
        {
            return (data.m_flags & (Building.Flags.Completed | Building.Flags.Abandoned | Building.Flags.Collapsed)) == Building.Flags.Completed;
        }
    }

    /* TODO: Toggling colors is only needed if the Industries DLC is active */

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "The toggler really belongs in this file.")]
    [HarmonyPatch]
    public static class UpdateProcessorColor
    {
        /* If InfoManager is in Natural Resources mode, ask renderer to update building colors
         * on Generic (goods) producer, to reflect the input resource that would be requested
         */

        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (SupplyChainColoringMod.HaveIndustriesDLC)
                yield return AccessTools.Method(typeof(IndustrialBuildingAI), "SimulationStepActive");
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        public static void Prefix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {

            if ((buildingData.m_flags & Building.Flags.Active) != 0)
            {
                if (buildingData.Info.m_class.m_subService == ItemClass.SubService.IndustrialGeneric &&
                    Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.NaturalResources &&
                    (uint)(Singleton<SimulationManager>.instance.m_currentFrameIndex & 0x300) == ((uint)((buildingID & 3) << 8)))
                {
                    Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
                }
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "The toggler really belongs in this file.")]
    [HarmonyPatch]
    public static class UpdateProcessingFacilityColor
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (SupplyChainColoringMod.HaveIndustriesDLC)
                yield return AccessTools.DeclaredMethod(typeof(ProcessingFacilityAI), "SimulationStep");
        }

        /* Used for Unique Buildings to toggle input material colors in Natural Resource view
         */
        [HarmonyPrefix]
        [UsedImplicitly]
        public static void Prefix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {

            if ((buildingData.m_flags & Building.Flags.Active) != 0)
            {
                if (buildingData.Info.m_class.m_subService == ItemClass.SubService.None &&
                    Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.NaturalResources &&
                    (Singleton<SimulationManager>.instance.m_currentFrameIndex & 0x300) == ((uint)((buildingID & 3) << 8)))
                {
                    Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
                }
            }
        }
    }
}
