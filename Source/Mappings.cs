namespace SupplyChainColoring
{
    using ColossalFramework.Math;

    public static class Mappings
    {
        public static TransferManager.TransferReason GetOutgoingTransferReason(ItemClass.SubService subservice)
        {
            return subservice switch
            {
                ItemClass.SubService.IndustrialForestry =>
                    TransferManager.TransferReason.Lumber,
                ItemClass.SubService.IndustrialFarming =>
                    TransferManager.TransferReason.Food,
                ItemClass.SubService.IndustrialOil =>
                    TransferManager.TransferReason.Petrol,
                ItemClass.SubService.IndustrialOre =>
                     TransferManager.TransferReason.Coal,
                _ => TransferManager.TransferReason.None,
            };
        }
        public static TransferManager.TransferReason GetIncomingTransferReason(ItemClass.SubService subservice)
        {
            return subservice switch
            {
                ItemClass.SubService.IndustrialForestry =>
                    TransferManager.TransferReason.Logs,
                ItemClass.SubService.IndustrialFarming =>
                    TransferManager.TransferReason.Grain,
                ItemClass.SubService.IndustrialOil =>
                    TransferManager.TransferReason.Oil,
                ItemClass.SubService.IndustrialOre =>
                    TransferManager.TransferReason.Ore,
                _ => TransferManager.TransferReason.None,
            };
        }

        public static TransferManager.TransferReason GetOutgoingTransferReason(ItemClass c)
        {
            return c.m_subService switch
            {
                ItemClass.SubService.IndustrialForestry =>
                    c.m_level == ItemClass.Level.Level1 ? TransferManager.TransferReason.Lumber : TransferManager.TransferReason.Logs,
                ItemClass.SubService.IndustrialFarming =>
                    c.m_level == ItemClass.Level.Level1 ? TransferManager.TransferReason.Food : TransferManager.TransferReason.Grain,
                ItemClass.SubService.IndustrialOil =>
                    c.m_level == ItemClass.Level.Level1 ? TransferManager.TransferReason.Petrol : TransferManager.TransferReason.Oil,
                ItemClass.SubService.IndustrialOre =>
                    c.m_level == ItemClass.Level.Level1 ? TransferManager.TransferReason.Coal : TransferManager.TransferReason.Ore,
                _ => TransferManager.TransferReason.Goods,
            };
        }
        public static TransferManager.TransferReason GetIncomingTransferReason(ushort buildingID)
        {
            return new Randomizer(buildingID).Int32(4u) switch
            {
                0 => TransferManager.TransferReason.Lumber,
                1 => TransferManager.TransferReason.Food,
                2 => TransferManager.TransferReason.Petrol,
                _ => TransferManager.TransferReason.Coal,
            };
        }
        public static ItemClass.SubService GetSubservice(ushort buildingID)
        {
            // xxx performance Opportinity:
            // Fix the buildingID -> [0..3] without creating a Randomizer object.
            // This construct is used in many places in Assembly-Csharp, when all it wants is
            // The seed may be saved in the gamesave, ie, for different cities the map may be different
            // The seed may be detected once as new Randomizer(buildingID).Int32(4u) ^ (buildingId & 0x3);
            return new Randomizer(buildingID).Int32(4u) switch
            {
                0 => ItemClass.SubService.IndustrialForestry,
                1 => ItemClass.SubService.IndustrialFarming,
                2 => ItemClass.SubService.IndustrialOil,
                _ => ItemClass.SubService.IndustrialOre,
            };
        }
        public static TransferManager.TransferReason GetSecondaryIncomingTransferReason(ushort buildingID)
        {
            switch (new Randomizer(buildingID).Int32(8u))
            {
                case 0:
                    return TransferManager.TransferReason.PlanedTimber;
                case 1:
                    return TransferManager.TransferReason.Paper;
                case 2:
                    return TransferManager.TransferReason.Flours;
                case 3:
                    return TransferManager.TransferReason.AnimalProducts;
                case 4:
                    return TransferManager.TransferReason.Petroleum;
                case 5:
                    return TransferManager.TransferReason.Plastics;
                case 6:
                    return TransferManager.TransferReason.Metals;
                default:
                    return TransferManager.TransferReason.Glass;
            }
        }
    }
}
