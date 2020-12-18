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
    using System;

    // [HarmonyPatch(typeof(VehicleAI), "GetColor", new Type[] { typeof(ushort), typeof(Vehicle), typeof(InfoManager.InfoMode) })]
    [HarmonyPatch(typeof(CarAI), "GetColor")]
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

                        case TransferManager.TransferReason.Goods:
                        case TransferManager.TransferReason.LuxuryProducts:
                            subservice = ItemClass.SubService.IndustrialGeneric;
                            break;

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
                default:
                    return true;
            }

            return false;
        }
    }
}
