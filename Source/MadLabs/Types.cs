using System;
using BattleTech;

namespace MadLabs
{
    public class Range<T> where T : IComparable<T>
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public Range(T min, T max) { Minimum = min; Maximum = max; }



        public override string ToString()
        {
            return string.Format("[{0} - {1}]", this.Minimum, this.Maximum);
        }

        public bool IsValid()
        {
            return this.Minimum.CompareTo(this.Maximum) <= 0;
        }

        public bool ContainsValue(T value)
        {
            return (this.Minimum.CompareTo(value) <= 0) && (value.CompareTo(this.Maximum) <= 0);
        }

        public bool IsInsideRange(Range<T> range)
        {
            return this.IsValid() && range.IsValid() && range.ContainsValue(this.Minimum) && range.ContainsValue(this.Maximum);
        }

        public bool ContainsRange(Range<T> range)
        {
            return this.IsValid() && range.IsValid() && this.ContainsValue(range.Minimum) && this.ContainsValue(range.Maximum);
        }
    }



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
