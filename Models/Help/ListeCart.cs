namespace tp2.Models.Help
{
    public class ListeCart
    {
        public List<Item> Items { get; private set; } = new();

        public void AddItem(Product prod)
        {
            AddItem(prod, 1);
        }

        public void AddItem(Product prod, int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            var existing = Items.FirstOrDefault(i => i.Prod.ProductId == prod.ProductId);
            if (existing != null)
            {
                existing.quantite += quantity;
                return;
            }

            Items.Add(new Item(prod) { quantite = quantity });
        }

        public void SetToNUll()
        {
            Items.Clear();
        }

        public void SetLessOneItem(Product prod)
        {
            var item = Items.FirstOrDefault(i => i.Prod.ProductId == prod.ProductId);
            if (item == null)
            {
                return;
            }

            if (item.quantite <= 1)
            {
                Items.Remove(item);
                return;
            }

            item.quantite--;
        }

        public void SetItemQuantity(Product prod, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveItem(prod);
                return;
            }

            var item = Items.FirstOrDefault(i => i.Prod.ProductId == prod.ProductId);
            if (item != null)
            {
                item.quantite = quantity;
            }
        }

        public void RemoveItem(Product prod)
        {
            var item = Items.FirstOrDefault(i => i.Prod.ProductId == prod.ProductId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public float GetSubTotal()
        {
            return Items.Sum(i => i.TotalPrice);
        }

        public int GetTotalQuantity()
        {
            return Items.Sum(i => i.quantite);
        }
    }
}
