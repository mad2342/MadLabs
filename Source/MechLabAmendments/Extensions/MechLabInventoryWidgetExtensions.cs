using BattleTech.UI;

namespace MechLabAmendments.Extensions
{
    public static class MechLabInventoryWidgetExtensions
    {
        public static void RemoveItem(this MechLabInventoryWidget inventoryWidget, InventoryItemElement_NotListView item)
        {
            if (item.controller != null)
            {
                //Logger.LogLine("[Extensions.MechLabInventoryWidget.RemoveItem] item.controller != null");
                if (item.controller.quantity > 1)
                {
                    item.controller.ModifyQuantity(-1);
                    //Logger.LogLine("[Extensions.MechLabInventoryWidget.RemoveItem] item.controller.quantity: " + item.controller.quantity);
                }
                else
                {
                    //Logger.LogLine("[Extensions.MechLabInventoryWidget.RemoveItem] item.controller.quantity <= 1: " + item.controller.quantity);
                    inventoryWidget.localInventory.Remove(item);
                    item.SetRadioParent(null);
                    item.controller.Pool();
                    ReflectionHelper.InvokePrivateMethode(inventoryWidget, "EndOfFrameScrollBarMovement", null);
                }
            }
            else if (item.Quantity > 1)
            {
                item.ModifyQuantity(-1);
            }
            else
            {
                inventoryWidget.localInventory.Remove(item);
                item.SetRadioParent(null);
                ReflectionHelper.InvokePrivateMethode(inventoryWidget, "EndOfFrameScrollBarMovement", null);
            }
            if (!inventoryWidget.localInventory.Contains(item))
            {
                //Logger.LogLine("[Extensions.MechLabInventoryWidget.RemoveItem] !inventoryWidget.localInventory.Contains(item): HIDING item");
                item.ElementVisible = false;
            }
            inventoryWidget.ApplySorting(true);
        }
    }
}
