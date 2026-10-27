using System;
using System.Collections.Generic;


/// <summary>One stack of an item: what it is and how many.</summary>
[Serializable]
public class InventoryItem
{
    public ItemData data;

    public int stackSize;

    public InventoryItem(ItemData _newItemData, int _stackSize = 1)
    {
        data = _newItemData;
        stackSize = _stackSize;
    }

    public void AddStack(int _amount = 1) => stackSize += _amount;
    public void RemoveStack() => stackSize--;

    // Every container (inventory, stash, shop) keeps a list for slot order plus a dictionary for lookup.
    // These keep the two in step, so no container repeats the bookkeeping.

    /// <summary>Adds _count of _item, stacking onto an existing entry when there is one.</summary>
    public static void AddTo(List<InventoryItem> _list, Dictionary<ItemData, InventoryItem> _lookup, ItemData _item, int _count = 1)
    {
        if (_lookup.TryGetValue(_item, out InventoryItem stack))
        {
            stack.AddStack(_count);
            return;
        }

        stack = new InventoryItem(_item, _count);
        _list.Add(stack);
        _lookup.Add(_item, stack);
    }

    /// <summary>Takes one of _item (or the whole stack), dropping the entry once empty. False when it isn't there.</summary>
    public static bool RemoveFrom(List<InventoryItem> _list, Dictionary<ItemData, InventoryItem> _lookup, ItemData _item, bool _removeAll, out int _removed)
    {
        _removed = 0;

        if (!_lookup.TryGetValue(_item, out InventoryItem stack))
            return false;

        _removed = _removeAll ? stack.stackSize : 1;

        if (_removeAll || stack.stackSize <= 1)
        {
            _list.Remove(stack);
            _lookup.Remove(_item);
        }
        else
        {
            stack.RemoveStack();
        }

        return true;
    }
}
