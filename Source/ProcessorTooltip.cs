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
    using SupplyChainColoring;
    using HarmonyLib;
    using JetBrains.Annotations;

    [HarmonyPatch(typeof(IndustrialBuildingAI), "GetLocalizedStatusActive")]
    public static class ProcessorTooltip
    {
        /*
         * In CS 1.13.1-f1-steam-win, all fishing boats are colored generically.
         * This patch colors them by the type of fish they harvest.
         */
        [HarmonyPostfix]
        [UsedImplicitly]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Output argument is __result as required by Harmony")]
        public static void Postfix(ref IndustrialBuildingAI __instance, ushort buildingID, ref Building data, ref string __result)
        {
            string resourceName;
            switch (data.Info.m_class.m_subService)
            {
                case ItemClass.SubService.IndustrialGeneric:

                    resourceName = Mappings.GetSubservice(buildingID) switch
                    {
                        ItemClass.SubService.IndustrialOre => "INFO_CONNECTIONS_ORE",
                        ItemClass.SubService.IndustrialForestry => "INFO_CONNECTIONS_FORESTRY",
                        ItemClass.SubService.IndustrialOil => "INFO_CONNECTIONS_OIL",
                        ItemClass.SubService.IndustrialFarming => "INFO_CONNECTIONS_AGRICULTURE",
                        _ => string.Empty,
                    };
                    if (SupplyChainColoringMod.HaveIndustriesDLC)
                    {
                        __result += "\n (" +
                                    ColossalFramework.Globalization.Locale.Get(resourceName) +
                                    ", " +
                                    ColossalFramework.Globalization.Locale.Get("WAREHOUSEPANEL_RESOURCE", Mappings.GetSecondaryIncomingTransferReason(buildingID).ToString()) +
                                    " -> " +
                                    ColossalFramework.Globalization.Locale.Get("INFO_CONNECTIONS_GOODS") +
                                    ")";
                    } else
                    {
                        __result += "\n (" +
                                    ColossalFramework.Globalization.Locale.Get(resourceName) +
                                    " -> " +
                                    ColossalFramework.Globalization.Locale.Get("INFO_CONNECTIONS_GOODS") +
                                    ")";
                    }
                    break;
                default:
                    resourceName = data.Info.m_class.m_subService switch {
                        ItemClass.SubService.IndustrialOre => "Ore",
                        ItemClass.SubService.IndustrialForestry => "Forestry",
                        ItemClass.SubService.IndustrialOil => "Oil",
                        ItemClass.SubService.IndustrialFarming => "Farming",
                        _ => string.Empty,
                     };
                    string outName = data.Info.m_class.m_subService switch
                    {
                        ItemClass.SubService.IndustrialOre => "INFO_CONNECTIONS_ORE",
                        ItemClass.SubService.IndustrialForestry => "INFO_CONNECTIONS_FORESTRY",
                        ItemClass.SubService.IndustrialOil => "INFO_CONNECTIONS_OIL",
                        ItemClass.SubService.IndustrialFarming => "INFO_CONNECTIONS_AGRICULTURE",
                        _ => string.Empty,
                    };
                    if (!string.IsNullOrEmpty(resourceName)) {
                        __result += "\n (" +
                                    ColossalFramework.Globalization.Locale.Get("INDUSTRYPANEL_RAWRESOURCE", resourceName) +
                                    " -> " +
                                    ColossalFramework.Globalization.Locale.Get(outName) +
                                    ")";
                    }
                    break;
            }
        }
    }
}
