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
namespace SupplyChainColoring.PatchWarehouseAI
{
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;

    [HarmonyPatch(typeof(WarehouseAI), "GetColor")]
    /*
     * BUG FIX: In CS 1.13.1-f1-steam-win, in WarehouseAI, the m_info.m_class.m_subService is not set,
     * Causing warehouses to not be highlighted in the Industries Areas
     * This patch fixes the bug.
     *
     */
    public static class WarehouseColors
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Output argument is __result as required by Harmony")]
        public static bool Prefix(WarehouseAI __instance, ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref UnityEngine.Color __result)
        {
            if (data.Info.m_class.m_service != ItemClass.Service.PlayerIndustry)
            {
                return true;
            }

            ItemClass.SubService subservice;
            TransferManager.TransferReason material = __instance.GetActualTransferReason(buildingID, ref data);

            switch (infoMode)
            {
                case InfoManager.InfoMode.Industry:

                    InfoManager.SubInfoMode filterByResource = IndustryBuildingAI.ResourceToInfoMode(material);
                    bool isactive = (data.m_flags & Building.Flags.Active) != 0 ? true : false;

                    if (filterByResource == Singleton<InfoManager>.instance.CurrentSubMode || filterByResource == InfoManager.SubInfoMode.None)
                    {
                        if ((data.m_flags & Building.Flags.Active) != 0)
                        {
                            if (material == TransferManager.TransferReason.Goods)
                            {
                                __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)material];
                            }
                            else
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

                    return false;
                case InfoManager.InfoMode.NaturalResources:
                    switch (material)
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

                        case TransferManager.TransferReason.Goods:
                            subservice = ItemClass.SubService.IndustrialGeneric;
                            break;

                        default:
                            subservice = ItemClass.SubService.None;
                            break;
                    }

                    if (subservice != ItemClass.SubService.None)
                    {
                        if ((data.m_flags & Building.Flags.Active) != 0)
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
