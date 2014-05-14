using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

class FraudPrevention
{
    static void Main(String[] args)
    {
        int numRecords;
        numRecords = int.Parse(Console.ReadLine());
        var FraudulentOrders = new HashSet<int>();
        
        var Orderz = new Dictionary<int, Order>();
        for (int i = 0; i < numRecords; i++)
        {//for each case
            Order o = new Order(Console.ReadLine().Split(','));        
            Orderz[o.OrderID] = o;
        }//end for each case
        foreach (var deal in Order.DealIDs)
        {//order items by email, address, and ccNums
            if ( deal.Value.Count > 1 )
            {//if this deal has more than one order asscociated with it.
                var byDeal = new List<Order>(); //create a list of orders (small list)
                foreach ( var id in deal.Value )   //foreach item with this deal ID                
                    byDeal.Add(Orderz[id]);      //create a list of objects        
                var byEmail =   byDeal.GroupBy(x => x.Email).Where(x => x.Count() > 1).SelectMany(x => x);
                var byAdds  = byDeal.GroupBy(x => x.Address).Where(x => x.Count() > 1).SelectMany(x => x);
                var byE_CC =   byEmail.GroupBy(x => x.ccNum).Where(x => x.Count() >= 1).SelectMany(x => x);
                foreach (var item in byE_CC)
                    FraudulentOrders.Add(item.OrderID);
                var byA_CC = byAdds.GroupBy(x => x.ccNum).Where(x => x.Count() == 1).SelectMany(x => x);
                foreach (var item in byA_CC)
                    FraudulentOrders.Add(item.OrderID);
            }//if deal has only one orderID, its distinct, so do nothing.
        }//end foreach        
        StringBuilder answer = new StringBuilder(); 
        foreach (var item in FraudulentOrders.OrderBy(x => x))
            answer.Append(item + ",");
        Console.WriteLine(answer.ToString().Substring(0, answer.Length - 1));        
     }//end main
    public class Order
    {
        private HashSet<int> OrderIDs = new HashSet<int>(); //ensures orderID uniqueness
        public static Dictionary<int, List<int>> DealIDs = new Dictionary<int, List<int>>();

        public int OrderID { get; set; }
        public int DealID { get; set; }
        public Tuple<string, string> Email
        {
            get{ return new Tuple<string,string>(this.User, this.Domain); }
        }
        public string User { get; set; }
        public string Domain { get; set; }
        public Tuple<string, string, string, int> Address
        {
            get { return new Tuple<string, string, string, int>(this.Street, this.City, this.State, this.Zip); }
        }
        public string City { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public int Zip { get; set; }
        public long ccNum { get; set; }

        public Order(string[] s)
        {
            if (s.Length != 8)
                throw new Exception("Wrong Number of fields in record");
            try
            {//try to parse this value as an integer
                var orderID = int.Parse(s[0]);
                if (!this.OrderIDs.Add(orderID))
                    throw new Exception("Order ID is not Unique!");
                else this.OrderID = orderID;
            }//end try parse orderid
            catch (Exception e)
            {//raise exception
                throw new Exception("Order ID creation Error:", e);
            }//end catch order id exception
            try
            {//parse DealID
                this.DealID = int.Parse(s[1]);
                if (DealIDs.ContainsKey(this.DealID))
                    DealIDs[this.DealID].Add(this.OrderID);
                else                
                {//if dictionary entry does not exist.
                    DealIDs.Add( this.DealID, new List<int>() );   //add key and initialize list
                    DealIDs[this.DealID].Add(this.OrderID);        //and add this to the list
                }
            }//end try parse dealid
            catch
            {//on fail,somethings wrong
                throw new Exception("Deal ID creation Error");
            }//end catch
            try
            {//normalize email                
                var email = s[2].ToLower();
                int pos = email.IndexOf("@");
                if (pos < 0)
                    throw new Exception("Must contain @");
                var user = email.Substring(0, pos);   //split
                var domain = email.Substring(pos + 1);
                if (!domain.Contains("."))
                    throw new Exception("Domain Error, no '.' found");
                while (user.Contains("."))  //while the username contains periods
                {//could be more than one period correct?
                    pos = user.IndexOf(".");//find them
                    user = user.Remove(pos, 1);//and remove them
                }//end while
                if (user.Contains("+"))
                {//more than one plus wouldn't matter cuz
                    pos = user.IndexOf("+");    //find the first
                    user = user.Remove(pos);    //and remove everything after
                }//end if
                this.User = user;
                this.Domain = domain;
            }//end try normalize email
            catch (Exception e)
            {//on error
                throw new Exception("Invalid Email Address:", e);
            }//end catch
            try
            {//normalize address
                var street = s[3].ToLower();
                var city = s[4].ToLower();
                var state = s[5].ToLower();
                var zip = s[6];

                int pos = zip.IndexOf('-');
                if (pos > 0) zip = zip.Remove(pos, 1);
                try
                {//parse zip
                    this.Zip = int.Parse(zip);
                }//end try
                catch
                {//if parse fails
                    throw new Exception("Zipcode Error");
                }//raise exception

                if (street.Contains("street"))
                    street = street.Replace("street", "st.");
                if (street.Contains("road"))
                    street = street.Replace("road", "rd.");

                switch (state)
                {//case state contains
                    case "illinois":
                        state = "il";
                        break;
                    case "new york":
                        state = "ny";
                        break;
                    case "california":
                        state = "ca";
                        break;
                }//end switch                
                this.Street = street;
                this.City = city;
                this.State = state;
            }//end try address
            catch (Exception e)
            {//additonal error handling here if desired
                throw new Exception("Problem with Address:", e);
            }//end catch address
            var cc = s[7].Trim();
            try
            {//credit card
                this.ccNum = long.Parse(cc);
            }//end try cc
            catch
            {//could add additional error handling here
                throw new Exception("Credit Card Number contains invalid characters");
            }//end catch cc            
        }//end order constructor
        public override string ToString()
        {//pretty print helper function for debugging
            StringBuilder sb = new StringBuilder();
            sb.Append("================================" +
                      "\n  Order Details:"  +
                      "\n================================" +
                      "\nOrderID = " + this.OrderID +
                      "\nDealID  = " + this.DealID +
                      "\nEmail   = " + this.User + "@" + this.Domain +
                      "\nccNum   = " + this.ccNum +
                      "\nAddress = " + this.Street +
                      "\n          " + this.City + ", " +
                                       this.State.ToUpper() + ", " +
                                       this.Zip);
            return sb.ToString();
        }//end ToString()
    }//end order class
}//end fraud prevention class
