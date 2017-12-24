using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myProvider
{
    class Program
    {
        static void Main(string[] args)
        {
            string constr = @"Integrated Security=true;Initial Catalog=Northwind;Data Source=localhost\SQLEXPRESS;";

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                Northwind db = new Northwind(con);

                var city = "London";
                IQueryable<Customers> query = db.Customers.Where(c => c.City == city);
                var sel = db.Customers.Select(x => x.ContactName.TrimEnd());
                Console.WriteLine(sel.ToString());
                Console.WriteLine("Query: \n{0}\n", query);

                var list = query.ToList();
                foreach (var item in list)
                {
                    Console.WriteLine("Name: {0}", item.ContactName);
                }
            }
        }
    }
}
