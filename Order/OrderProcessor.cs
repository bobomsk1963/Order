using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Order
{
    class OrderProcessor
    {
        void ProcessOrder(Order order)
        {
            if (order.Status == 0)
            {
                order.Summ = (order.Summ / 100) * (100 + order.Percent);
                order.Status = 1;
            }
        }

        void DeProcessOrder(Order order)
        {
            if (order.Status == 1)
            {
                order.Summ = (order.Summ / (100 + order.Percent))*100;
                order.Status = 0;
            }
        }

    }
}
