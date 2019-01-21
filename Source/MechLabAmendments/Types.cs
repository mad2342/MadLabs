using BattleTech;



namespace MechLabAmendments
{
    public class InventoryItemElement_Simple
    {
        public void ModifyQuantity(int quantityToAdd)
        {
            this.quantity += quantityToAdd;
        }

        public MechComponentRef ComponentRef
        {
            get
            {
                return this.componentRef;
            }
            set
            {
                this.componentRef = value;
            }
        }

        public int Quantity
        {
            get
            {
                return this.quantity;
            }
            set
            {
                this.quantity = value;
            }
        }

        public MechLabDropTargetType Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = value;
            }
        }


        private MechComponentRef componentRef;
        private int quantity;
        private MechLabDropTargetType origin;
    }
}
