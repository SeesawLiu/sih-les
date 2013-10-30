using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace com.Sconit.Entity
{
    public class OrderHolder
    {
        private static ThreadLocal<IList<string>> orders = new ThreadLocal<IList<string>>();

        public static IList<string> GetOrders()
        {
            return orders.Value;
        }

        public static void CleanOrders()
        {
            orders.Value = new List<string>();
        }

        public static void AddOrder(string orderNo)
        {
            if (orders.Value == null)
            {
                orders.Value = new List<string>();
            }
            orders.Value.Add(orderNo);
        }
    }
}
