using System.Collections.Generic;

public interface IChest
{
    List<Chest.ItemPack> ContentItems { get; set; }
    ISpecialTile Parent { get; set; }

    void GenerateChestLootWindowPopulatedWithItems(IChest source, List<Chest.ItemPack> items);
}