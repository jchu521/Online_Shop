using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVCManukauTech.Models;

namespace MVCManukauTech.Controllers
{
    public class CatalogController : Controller
    {
        private BookShopEntities db = new BookShopEntities();

        // GET: Catalog?CategoryName=Travel
        public ActionResult Index(string CategoryName,string page)
        {
            /*100917
             *  when the button been ckick at index page, passing back the page number 
             *  the "display" is useing in sql query  @p1,  
             *   ORDER BY ProductId ASC OFFSET @p1 ROWS FETCH NEXT 6 ROWS ONLY
             *   so can display the right products
             */
            int display;
            if(page == null || Int32.Parse(page)==0)
            {
                display = 0;
            }
            else
            {
                display = (Int32.Parse(page) - 1) * 6;
            }


            //140903 JPC add CategoryName to SELECT list of fields
            String SQL = "SELECT ProductId, Products.CategoryId AS CategoryId, Title, PublicationYear, ImageFileName, UnitCost"
                + ", SUBSTRING(Description, 1, 100) + '...' AS Description, IsDownload, DownloadFileName "
                + "FROM Products INNER JOIN Categories ON Products.CategoryId = Categories.CategoryId";

             CategoryName = Request.QueryString.Get("CategoryName");
            
            if (CategoryName != null && CategoryName!="")
            {
                //140903 JPC security check - if ProductId is dodgy then return bad request and log the fact 
                //  of a possible hacker attack.  Excessive length or containing possible control characters
                //  are cause for concern!  TODO move this into a separate reusable code method with more sophistication.
                if (CategoryName.Length > 20 || CategoryName.IndexOf("'") > -1 || CategoryName.IndexOf("#") > -1)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                //150807 JPC Security improvement @p0
                SQL += " WHERE CategoryName = @p0";

                //Send extra info to the view that this is the selected CategoryName
                ViewBag.CategoryName = CategoryName;
            }
            /*  100917 
             *   ref: http://www.tutorialsteacher.com/mvc/viewdata-in-asp.net-mvc
             *  count the total number of products by category name
                add send it to view  
                ViewData["TotalProduct"] = count.ToString();
                also, send the category name
                ViewData["CategoryName"] = CategoryName;
                use to display buttons in index page and passing back the values
             */
            var totalCategoryProduct = db.Products.SqlQuery(SQL, CategoryName);
            int count = totalCategoryProduct.Count();
            //100917 using sql to show maximum 6 products per page 
            //ref: http://www.dofactory.com/sql/order-by-offset-fetch
            SQL += " ORDER BY ProductId ASC OFFSET @p1 ROWS FETCH NEXT 6 ROWS ONLY";
            var products = db.Products.SqlQuery(SQL, CategoryName, display);
            ViewData["TotalProduct"] = count.ToString();
            ViewData["CategoryName"] = CategoryName;
            return View(products.ToList());
        }

        // GET: Catalog/Details?ProductId=1MORE4ME
        public ActionResult Details(string ProductId)
        {
            if (ProductId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //140903 JPC security check - if ProductId is dodgy then return bad request and log the fact 
            //  of a possible hacker attack.  Excessive length or containing possible control characters
            //  are cause for concern!  TODO move this into a separate reusable code method with more sophistication.
            if (ProductId.Length > 20 || ProductId.IndexOf("'") > -1|| ProductId.IndexOf("#") > -1)
            {
                //TODO Code to log this event and send alert email to admin
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //150807 JPC Security improvement implementation of @p0
            String SQL = "SELECT * FROM Products WHERE ProductId = @p0";
            
            //140904 JPC case of one product to look at the details.
            //  SQL gives some kind of collection where we need to clean that up with ToList() then take element [0]
            //150807 JPC Security improvement implementation of @p0 substitute ProductId
            var product = db.Products.SqlQuery(SQL, ProductId).ToList()[0];
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
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
