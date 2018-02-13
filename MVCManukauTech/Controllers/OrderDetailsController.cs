using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVCManukauTech.Models;

using Microsoft.VisualBasic;
//140415 JPC handle JSON data with System.Web.Helpers.Json
//20170305 JPC change to Newtonsoft.Json
using Newtonsoft.Json;

namespace MVCManukauTech.Controllers
{
    public class OrderDetailsController : Controller
    {
        private BookShopEntities db = new BookShopEntities();

        //140825 started JPC John Calder - adapted from XSpy version 1 webforms 2004-2008

        //140825 JPC note that at first the SessionID kept changing.  
        //  Discovered need to put something in the Session object to "wake it up"
        //  Refer global.asax.cs method Session_Start.
        //  Ref: http://stackoverflow.com/questions/281881/sessionid-keeps-changing-in-asp-net-mvc-why

        // GET: OrderDetails/ShoppingCart?ProductId=1MOR4ME
        // or to simply view the cart
        // GET: OrderDetails/ShoppingCart
        public ActionResult ShoppingCart()
        {
            string SQLGetOrder = "";
            string SQLStartOrder = "";
            string SQLCart = "";
            string SQLBuy = "";
            string SQLUnitCostLookup = "";
            int rowsChanged = 0;
            double Discount = 0;
            //add code 17-08-17
            @Session["Discount"] = "N";
            if (User.IsInRole("Full Member"))
            {
                //Code for action for a Full Member
                //12% discount
                @Session["Discount"] = "Y";
                Discount = 0.88;
            }

            //end 

            string ProductId = Request.QueryString.Get("ProductId");
            // Security checking
            if(ProductId != null  && (ProductId.Length > 20 || ProductId.IndexOf("'") > -1 || ProductId.IndexOf("#") > -1) )
            {
                //
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Have we created an order for this user yet?
            //If not then create a placeholder for a mostly empty order
            //Note that any Order in progress has an OrderStatusId of 0 or 1
            //We are not interested in Orders with higher status because they have already gone through checkout
            //150807 JPC Security improvement implementation of @p0
            SQLGetOrder = "SELECT * FROM Orders WHERE SessionId = @p0 AND OrderStatusId <= 1;";

            //140825 JPC.  We may need 2 attempts at reading the order out of the database.
            //  Managing this as a for..loop with 2 loops.  If successful first time then break out.
            //  (Other opinion Rosemarie T - "this is a bit dodgy!")
            //150807 JPC Security improvement implementation of @p0
            var orders = db.Orders.SqlQuery(SQLGetOrder, Session.SessionID).ToList();
            for (int iForLoop = 0; iForLoop <= 1;iForLoop++ )
            {   
                //Do we have an order?
                if (orders.Count == 1)
                {
                    //we have an order, we can continue to the next step
                    break;
                } else if(iForLoop == 1){
                    //failed on the second attempt
                    throw new Exception("ERROR with database table 'Orders'.  This session fails to write a new order.");
                } else if(orders.Count > 1) {
                    //This would be a major error. In theory impossible but we need to be ready for any outcome
                    throw new Exception("ERROR with database table 'Orders'.  This session is running more than one shopping cart.");
                } else {
                    //else we get an order started
                    //150807 JPC Security improvement implementation of @p0
                    SQLStartOrder = "INSERT INTO Orders(SessionID, OrderStatusId) VALUES(@p0, 0);";
                    rowsChanged = db.Database.ExecuteSqlCommand(SQLStartOrder, Session.SessionID);
                    // a good result would be one row changed
                    if(rowsChanged != 1)
                    {
                        //Error handling code to go in here.  Poss return a view with error messages.
                        //Code from our old webforms version is -- 
                        throw new Exception("ERROR with database table 'Orders'.");
                    }
                    //ready to try reading that order again
                    //150807 JPC Security improvement implementation of @p0, parameter Session.SessionID
                    orders = db.Orders.SqlQuery(SQLGetOrder, Session.SessionID).ToList();
                    //go round and test orders again
                }
            }

            //What is the OrderId
            int orderId = orders[0].OrderId;

            //040514 JPC EDUC add ORDER BY clause
            //080704 JPC Note that with use of SQL Server, "LineNo" is a reserved word!
            //  Therefore change to "LineNumber"
            //150807 JPC Security improvement implementation of @p0
            SQLCart = "SELECT OrderDetails.OrderId AS OrderId, OrderDetails.LineNumber As LineNumber, OrderDetails.ProductId As ProductId, " +
                "Products.Title As ProductTitle, Products.ImageFileName As ImageFileName, " +
                "OrderDetails.Quantity As Quantity, OrderDetails.UnitCost As UnitCost " +
                "FROM OrderDetails INNER JOIN Products ON Products.ProductId = OrderDetails.ProductId " + 
                "WHERE OrderDetails.OrderId = @p0 ORDER BY OrderDetails.LineNumber;";
            // Note that this is an "view" query across 2 tables 
            // so we need to create a new VIEW MODEL class "OrderDetailsQueryForCart" to match up
            // See this under folder "Models"
            //150807 JPC Security improvement implementation of @p0, parameter orderId
            var cart = db.Database.SqlQuery<OrderDetailsQueryForCart>(SQLCart, orderId).ToList();

            //140903 JPC
            //What to do about a product for the case where the user clicked add to cart ..
            //IF the product is already in the cart THEN raise the quantity by one ELSE add it in

            int lineNumber = 0;
            int? quantity = 0;
            //140903 JPC cover case of user is only looking at the cart without adding any changes
            if (ProductId == null)
            {
                //use lineNumber = -1 as a flag to skip the data writing in the following if block
                lineNumber = -1;
            } else { 
                foreach(var item in cart)
                {
                    if(item.ProductId == ProductId)
                    {
                        lineNumber = item.LineNumber;
                        quantity = item.Quantity;
                        break;
                    }
                }
            } //end if

            rowsChanged = 0;
            if (lineNumber > 0)
            {
                quantity += 1;
                //150807 JPC Security improvement implementation of @p0, @p1, @p2 - (was {0}, {1}, {2})
                SQLBuy = "UPDATE OrderDetails SET Quantity = @p0 WHERE OrderId = @p1 AND LineNumber = @p2 ";
                rowsChanged = db.Database.ExecuteSqlCommand(SQLBuy, quantity, orderId, lineNumber);
            }
            else if(lineNumber == 0)
            {
                //writing a new line.  we need to know the unitcost
                //in real life work this could grow into a major method in a separate class involving special member discounts etc
                //here there is a choice between sending it in the querystring, or doing a new database lookup 
                //the querystring is easier but I am concerned about users being able to interfere with querystrings so I will go with the database

                //Safe bet is to stick to the SELECT * approach to match the existing generated classes.  
                //Can also call this the "go with the flow" method
                //150807 JPC Security improvement implementation of @p0
                SQLUnitCostLookup = "SELECT * FROM Products WHERE ProductId = @p0";
                var products = db.Products.SqlQuery(SQLUnitCostLookup, ProductId).ToList();
                decimal unitCost = Convert.ToDecimal(products[0].UnitCost);

                lineNumber = cart.Count + 1;
                //150807 JPC Security improvement implementation of @p0 etc
                //17-08-17 changed add @p5 which is the Discount
                SQLBuy = "INSERT INTO OrderDetails VALUES(@p0, @p1, @p2, @p3, @p4, @p5)";

                // add Discount
                rowsChanged = db.Database.ExecuteSqlCommand(SQLBuy, orderId, lineNumber, ProductId, 1, unitCost, Discount);
                //end
            }

            //If User has selected a product to add then Query is UPDATE or INSERT but they both run like this
            if (SQLBuy != "") { 
                if (rowsChanged != 1)
                {
                    //Error handling code to go in here.  Poss return a view with error messages.
                    //Code from our old webforms version is -- 
                    throw new Exception("ERROR with database table 'Orders'.");
                }

                //If we have changed the cart in the database, then we need to reload it here
                //140903 JPC note the syntax for working with a "View Model"
                //150807 JPC Security improvement implementation of @p0, parameter orderId
                cart = db.Database.SqlQuery<OrderDetailsQueryForCart>(SQLCart, orderId).ToList();
            }

            //Give that Session object some work to do to wake it up and get it functional
            Session["OrderId"] = orderId;

            return View(cart);
         
        }

        //AJAX!
        // This sample GET string is not useful as a copy/paste test because it only runs as a step in a longer process
        // that would be difficult to test manually.  Quoted as documentation example only
        // GET: OrderDetails/ShoppingCartUpdate?Quantity=4&LineNumber=7
        [HttpPost]
        public string ShoppingCartUpdate(String Quantity, String LineNumber)
        {
            string SQLUpdateOrderDetails = "";
            int rowsChanged = 0;

            //140903 JPC check that Quantity and LineNumber are numeric. Non-numeric or decimal could indicate hacker mischief-making
            //  I came from vb, I save time by importing library VisualBasic so I can have my old fave functions in C#
            //  Some may criticise but "Microsoft.VisualBasic" is a valid .NET function library which we can use like any other
            if (!Information.IsNumeric(Quantity) || !Information.IsNumeric(LineNumber) 
                || Convert.ToInt32(Quantity) != Convert.ToDouble(Quantity) 
                || Convert.ToInt32(LineNumber) != Convert.ToDouble(LineNumber))
            {
                //TODO Code to log this event and send alert email to admin
                return "ERROR";
            }
            int orderId = Convert.ToInt32(Session["OrderId"]);
            //150807 JPC Security improvement implementation of @p0 etc
            SQLUpdateOrderDetails = "UPDATE OrderDetails SET Quantity = @p0 WHERE OrderId = @p1 AND LineNumber = @p2";
            rowsChanged = db.Database.ExecuteSqlCommand(SQLUpdateOrderDetails, Quantity, orderId, LineNumber);
            if (rowsChanged == 1)
            {
                //expected good result
                return "SUCCESS";
            }
            else if(rowsChanged == 0)
            {
                //nothing happened, a bit sad but there is no change so simple feedback is needed
                return "ERROR";
            }
            else
            {
                //more than 1 rows changed is in theory impossible.
                //the only possibility I can think of is some kind of hacking injection attack where % signs
                //get into the mix and give a wider scope to what the WHERE statement finds.
                //if it does happen then we have a major problem on our hands and we need 
                //to cancel this shopping cart immediately 
                //needs SQL DELETE for the cart
                return "ERROR HIGH SEVERITY"; //placeholder only, 
            }
   
        }

        //First time setup of the form web page
        public ActionResult Checkout()
        {
            Checkout checkout = new Checkout();
            //Calculate the grand total for display and processing
            //We need to remember the orderId - see above where we "persisted" (remembered) it in "Session"
            int orderId = Convert.ToInt32(Session["OrderId"]);

            //Use the orderId to get the GrandTotal out of the database
            //150807 JPC Security improvement implementation of @p0, parameter orderId
            string SQLGetGrandTotal = "SELECT SUM(Quantity * UnitCost) AS GrandTotal FROM OrderDetails WHERE OrderId = @p0";

            //150609 JPC Interesting code here.  Took me a while to do this one mostly by a range of logical guesses and interpretation of error messages.
            //  We need to say that the SQLQuery will give us a type decimal result, 
            //  and we also need to say that there is only a single result
            //  rather than the usual collection that comes from an SQL query.
            //150807 JPC Security improvement implementation of @p0, parameter orderId
            decimal grandTotal = db.Database.SqlQuery<decimal>(SQLGetGrandTotal, orderId).Single<decimal>();

            //We include grandTotal as an element of class Checkout.  
            checkout.GrandTotal = grandTotal;
            //Also persist because we will need it later for the "Bank of Fiction", or other Credit Card Authority
            Session["GrandTotal"] = grandTotal;

            return View(checkout);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(Checkout checkout)
        {
            //Remember that GrandTotal that we persisted earlier
            decimal grandTotal = Convert.ToDecimal(Session["GrandTotal"]);
            //Pessimistic default
            bool isSuccess = false;
            //Pessimistic default - if credit card validation succeeds then change this to 3
            int statusId = 4;
            //Empty defaults
            string SQL = "";
            int rowsChanged = 0;

            //No expiry date in this form design, or in Class Checkout.  Do a hard-coded test.  
            //Students!  Make this better!
            string expiryDatePlaceHolder = "2018-10-01";

            //POST data to the "Bank of Fiction"
            //Ref: http://stackoverflow.com/questions/5401501/how-to-post-data-to-specific-url-using-webclient-in-c-sharp
            //Ref: http://en.wikipedia.org/wiki/Percent-encoding


            System.Net.WebClient w = new System.Net.WebClient();
            //POST has some optional configurations - recommended
            w.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            w.Encoding = System.Text.Encoding.UTF8;

            //POST separates the web address (URI) and the data
            string webAddress = "https://island.manukau.ac.nz/BankFiction2/Transactions/Reservation";
            //ALERT Over-Simplification to get learning started! - note the use of hard-coded bank username and password - in real life we need to do much much better than this!!!
            string data = "MerchantId=Kim@a.a&MerchantPassword=nice.coffee&CardNo={0}&CardType={1}&CardSecurity={2}&CardHolder={3}&CardExpiry={4}&Amount={5}";

            //UrlEncode provides "escapes" for some HTTP-unfriendly characters like spaces
            //Use UrlEncode for all fields where users have freedom to input text
            //or for any field with the possibility of including HTTP-unfriendly characters
            string cardOwner = HttpUtility.UrlEncode(checkout.CardOwner);

            data = String.Format(data, checkout.CardNumber, checkout.CardType, checkout.CSC, cardOwner, expiryDatePlaceHolder, grandTotal);

            //POST - another difference from GET is method UploadString 
            string responseJson = w.UploadString(webAddress, data);

            //To work with JSON we add a "using" statement at the top of this document -- using Newtonsoft.Json;
            Reservation reservation = JsonConvert.DeserializeObject<Reservation>(responseJson);
            if (reservation.IsReserved)
            {
                statusId = 3;
            }
            
            string DeliveryAddress = checkout.AddressStreet + " " + checkout.Location 
            + " " + checkout.Country + " " + checkout.PostCode;

            //Now to record the transactionId and other checkout data in table "Orders"
            //We need to remember the orderId - see above where we "persisted" (remembered) it in "Session"
            int orderId = Convert.ToInt32(Session["OrderId"]);

            try
            {
                //Change of statusId effectively clears the cart so the Customer cannot accidently buy these goods a second time!
                //Code goes here for UPDATE SQL statement and run this SQL Command with db.Database.ExecuteSqlCommand
                SQL = "UPDATE Orders SET TransactionId = @p0, OrderStatusId = @p1 " 
                    + ", Notes = @p2, CustomerName = @p3, DeliveryAddress = @p4 "
                    + ", Phone = @p5, EmailAddress = @p6, CardOwner = @p7 "
                    + ", CardType = @p8 "
                    + "WHERE OrderId = @p9";
                rowsChanged = db.Database.ExecuteSqlCommand(SQL
                , reservation.TransactionId, statusId, reservation.Notes, checkout.CustomerName
                , DeliveryAddress, " ", " ", checkout.CardOwner, checkout.CardType
                , orderId);
                if (rowsChanged == 1 && statusId == 3) isSuccess = true;
            }
            catch 
            {
                //Throwing an error into here is an unlikely but very serious situation.
                //It means that we have just failed to write this Order into our database but we must accept and process it
                //because we may have contracted with the Bank of Fiction to go through with it.
                //Probably best to send emails and alerts to the system admin with allowing to continue to give the standard success message.

            }

            if ( reservation.IsReserved)
            {
                //Clear the Cart because we really do not want the user accidently buying their Cart-load again
                Session["OrderId"] = 0;

                //Changing pages so ViewBag will not work, we need the wider-scope Session to do "persistence"
                Session["Message"] = "Payment of " + grandTotal.ToString("0.00") + " is accepted. Your transaction id is " + reservation.TransactionId.ToString();
                //Success means we move on to a different web page
                return RedirectToAction("CheckoutResult");
            }
            else
            {
                ViewBag.Message = "ERROR: Could not process this credit card.";              
                return View(checkout);
            }

        }

        public ActionResult CheckoutResult()
        {
            ViewBag.Message = Session["Message"];
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
