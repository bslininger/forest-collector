using System.Collections.Generic;
using System.Linq;

public class ForageRunModel
{
    private const int CarryingCapacity = 5;
    private int _remainingCapacity;
    private int _deliveredAmount;
    private Dictionary<string, int> _collectibleItemCounts;
    private Dictionary<string, int> _deliveredItemCounts;

    public ForageRunModel()
    {
        _remainingCapacity = CarryingCapacity;
        _deliveredAmount = 0;
        _collectibleItemCounts = new();
        _deliveredItemCounts = new();
    }

    public bool CollectItem(string itemId)
    {
        // Returns true if the collection went through, false if it did not.
        if (_remainingCapacity <= 0)
            return false;
        if (!_collectibleItemCounts.ContainsKey(itemId))
            _collectibleItemCounts[itemId] = 1;
        else
            _collectibleItemCounts[itemId] += 1;
        _remainingCapacity -= 1;
        return true;
    }

    public int GetItemCount(string itemId)
    {
        if (!_collectibleItemCounts.ContainsKey(itemId))
            return 0;
        return _collectibleItemCounts[itemId];
    }

    public bool HasAnyRemainingCapacity()
    {
        return _remainingCapacity > 0;
    }

    public int GetRemainingCapacity()
    {
        return _remainingCapacity;
    }

    public bool DeliverAllCarriedItems()
    {
        // Returns true if there were items to deliver, false if no items were being carried and so nothing to deliver.
        if (_remainingCapacity == CarryingCapacity)
            return false;

        _deliveredAmount += (CarryingCapacity - _remainingCapacity);
        _remainingCapacity = CarryingCapacity;
        foreach (string itemId in _collectibleItemCounts.Keys.ToList())
        {
            if (!_deliveredItemCounts.ContainsKey(itemId))
                _deliveredItemCounts[itemId] = _collectibleItemCounts[itemId];
            else
                _deliveredItemCounts[itemId] += _collectibleItemCounts[itemId];
            _collectibleItemCounts[itemId] = 0;
        }
        return true;
    }

    public int GetDeliveredItemsCount()
    {
        return _deliveredAmount;
    }

    public int GetSpecificDeliveredItemCount(string itemId)
    {
        return _deliveredItemCounts.ContainsKey(itemId) ? _deliveredItemCounts[itemId] : 0;
    }
}
