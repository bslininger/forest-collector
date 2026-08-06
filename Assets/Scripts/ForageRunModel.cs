using System.Collections.Generic;

public class ForageRunModel
{
    private int _deliveredAmount;
    private Dictionary<string, int> _deliveredItemCounts;

    public ForageRunModel()
    {
        _deliveredAmount = 0;
        _deliveredItemCounts = new();
    }


    public void RecordDeliveredItems(string itemId, int amount)
    {
        if (amount <= 0)
            return;
        if (!_deliveredItemCounts.ContainsKey(itemId))
            _deliveredItemCounts[itemId] = amount;
        else
            _deliveredItemCounts[itemId] += amount;

        _deliveredAmount += amount;
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
