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
namespace SupplyChainColoring
{
    using CitiesHarmony.API;
    using ICities;
    using JetBrains.Annotations;
    using UnityEngine;
    using System.Reflection;
    using System;

    public class SupplyChainColoringMod : IUserMod
    {
#if DEBUG
        public SupplyChainColoringMod()
        {
            Debug.Log($"SCC: ..ctor Mod {Assembly.GetExecutingAssembly().GetName().Version}");
        }
#endif
        public static Version ModVersion => typeof(SupplyChainColoringMod).Assembly.GetName().Version;

        public static Version ModVersion => typeof(SupplyChainColoringMod).Assembly.GetName().Version;

#if DEBUG
        public string Name => "Supply Chain Coloring " + ModVersion.ToString(4) + " DEBUG";
#else
        public string Name => "Supply Chain Coloring " + ModVersion.ToString(3);
#endif

        public string Description => "Highlight warehouses and cargo trucks on 'Industry Areas' and 'Natural Resources' views";

        [UsedImplicitly]
        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        [UsedImplicitly]
        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }
    }
}
