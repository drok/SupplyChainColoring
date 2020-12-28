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
#if true
namespace SupplyChainColoring.PatchCarAI
{
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;

    [HarmonyPatch(typeof(FishingBoatAI), "GetColor")]
    public static class FishingBoatColors
    {
        /*
         * In CS 1.13.1-f1-steam-win, all fishing boats are colored generically.
         * This patch colors them by the type of fish they harvest.
         */
        [HarmonyPrefix]
        [UsedImplicitly]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Output argument is __result as required by Harmony")]
        public static bool Prefix(ref VehicleAI __instance, ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref UnityEngine.Color __result)
        {
            // InfoManager.SubInfoMode filterByResource = InfoManager.SubInfoMode.None;
#if false
            return true;
#else
            switch (infoMode)
            {
                case InfoManager.InfoMode.Fishing:
                    BuildingInfo ownerInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                    System.Type ownerType = ownerInfo.m_buildingAI.GetType();
                    if (ownerType == typeof(FishingHarborAI))
                    {
                        var ai = ownerInfo.m_buildingAI as FishingHarborAI;
                        switch (ai.m_boatClass.m_level)
                        {
                            case ItemClass.Level.Level1: // generic
//                                return true;
//                                __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)TransferManager.TransferReason.Fish];
//                                return false;
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorShallowStill;
                                break;
                            case ItemClass.Level.Level2: // salmon
//                                return true;
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorShallowFlowing;
                                break;
                            case ItemClass.Level.Level3: // shellfish
                                // return true;
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorDeepStill;
                                break;
                            case ItemClass.Level.Level4: // Tuna
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorDeepFlowing;
                                break;
                            case ItemClass.Level.Level5: // Anchovies
                                __result = Singleton<NaturalResourceManager>.instance.m_properties.m_fishColorShallowStill;
                                break;
                        }
                        break;
                    } else if (ownerType == typeof(FishFarmAI))
                    {
                        __result = Singleton<TransferManager>.instance.m_properties.m_resourceColors[data.m_transferType];
                        break;
                    } else
                    {
                        return true;
                    }

                default:
                    return true;
            }

            return false;
#endif
        }
    }
}
#endif
