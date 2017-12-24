using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace myProvider
{
    public class Customers
    {
        public string CustomerID;
        public string ContactName;
        public string Phone;
        public string City;
        public string Country;
    }

    public class Orders
    {
        public int OrderID;
        public string CustomerID;
        public DateTime OrderDate;
    }

    public class Northwind
    {
        public Query<Customers> Customers;
        public Query<Orders> Orders;

        public Northwind(DbConnection connection)
        {
            QueryProvider provider = new DbQueryProvider(connection);
            this.Customers = new Query<Customers>(provider);
            this.Orders = new Query<Orders>(provider);
        }
    }
}
