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
namespace SupplyChainColoring.PatchProcessingFacilityAI
{
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;

    [HarmonyPatch(typeof(CommonBuildingAI), "GetColor")]
    /*
     * BUG FIX: In CS 1.13.1-f1-steam-win, in ProcessingFacilityAI, Industry buildings
     * are not colored based on the natural resources they use.
     * This patch adds coloring based on natural resource
     */
    public static class ProcessorColors
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Output argument is __result as required by Harmony")]
        public static bool Prefix(CommonBuildingAI __instance, ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref UnityEngine.Color __result)
        {

            if (data.Info.m_class.m_service != ItemClass.Service.PlayerIndustry && data.Info.m_class.m_service != ItemClass.Service.Industrial)
            {
                return true;
            }

            ItemClass.SubService subservice;

            switch (infoMode)
            {
                case InfoManager.InfoMode.NaturalResources:
                case InfoManager.InfoMode.Industry:
                    ItemClass.SubService submode;
                    switch (data.Info.m_class.m_subService)
                    {
                        case ItemClass.SubService.IndustrialOre:
                        case ItemClass.SubService.PlayerIndustryOre:
                            subservice = ItemClass.SubService.IndustrialOre;
                            submode = ItemClass.SubService.PlayerIndustryOre;
                            break;
                        case ItemClass.SubService.IndustrialForestry:
                        case ItemClass.SubService.PlayerIndustryForestry:
                            subservice = ItemClass.SubService.IndustrialForestry;
                            submode = ItemClass.SubService.PlayerIndustryForestry;
                            break;
                        case ItemClass.SubService.IndustrialOil:
                        case ItemClass.SubService.PlayerIndustryOil:
                            subservice = ItemClass.SubService.IndustrialOil;
                            submode = ItemClass.SubService.PlayerIndustryOil;
                            break;
                        case ItemClass.SubService.IndustrialFarming:
                        case ItemClass.SubService.PlayerIndustryFarming:
                            subservice = ItemClass.SubService.IndustrialFarming;
                            submode = ItemClass.SubService.PlayerIndustryFarming;
                            break;
                        default:
                            subservice = ItemClass.SubService.None;
                            submode = ItemClass.SubService.None;
                            break;
                    }

                    if (subservice != ItemClass.SubService.None && (infoMode == InfoManager.InfoMode.NaturalResources || Singleton<InfoManager>.instance.CurrentSubMode == IndustryBuildingAI.ServiceToInfoMode(submode)))
                    {
                        if ((data.m_flags & Building.Flags.Active) != 0)
                        {
                            __result = IndustrialBuildingAI.GetResourceColor(subservice);
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
    }
}
