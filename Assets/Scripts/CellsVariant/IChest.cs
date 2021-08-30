using System.Collections.Generic;

public interface IChest
{
    List<Treasure_Cell.ItemPack> ContentItems { get; set; }
}