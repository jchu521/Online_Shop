using System.Globalization;
using MVCManukauTech.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data.Entity;

namespace MVCManukauTech.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private BookShopEntities db = new BookShopEntities();
        public HomeController()
        {

        }
        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private ApplicationSignInManager _signInManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }
        //GET METHOD

        public ActionResult Index()
        {
            
            string sql; 
            //070917 check user login 
            if (Request.IsAuthenticated)
            {
                //get current user id
                var UserID = User.Identity.GetUserId();
                bool check = false;
                if (User.IsInRole("Full Member") && !check)
                {
                    check = true;
                    //method to check is full membership expiry?
                    //ref https://stackoverflow.com/questions/22624470/get-current-user-id-in-asp-net-identity-2-0
                    bool isExpiry = IsMembershipExpiry(User.Identity.Name);
                    //if is expiry
                    if (isExpiry)
                    {
                        /*090917 success update user role
                          remove from full member change it to Associate
                          only update SQL database, the cookie is not update, so the role is still "Full Member",
                          casing everytime click the home page, will go to managecontroller RenewMembership page
                          ref:https://stackoverflow.com/questions/29285406/refresh-current-users-role-when-changed-in-asp-net-identity-framework/29286361
                        */
                        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                        UserManager.RemoveFromRole(UserID, "Full Member");
                        UserManager.AddToRole(UserID, "Associate");

                        /*  fixed the cookie problem, now only go to  managecontroller RenewMembership page once,
                         *  if click home page, will display the content
                          ref https://stackoverflow.com/questions/29285406/refresh-current-users-role-when-changed-in-asp-net-identity-framework/29286361
                        */
                        var user = UserManager.FindById(User.Identity.GetUserId());
                        SignInManager.SignIn(user, false, false);

                        //redirect to Manage controller RenewMembership  page, give comment
                        ViewData["expiry"] = "Full membership is expiry, Please rejoin";
                        return RedirectToAction("RenewMembership", "Manage");
                    }
                }
            }
            //220817  read from database 
            sql = "SELECT * FROM ArticleContent WHERE ArticleContentId = 1";
            var article = db.ArticleContents.SqlQuery(sql).ToList()[0];
            return View(article);
        }
        //220817 update articlecontent database POST Method
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Index(string ArticleContentId, string texteditor)
        {
            ArticleContent article = db.ArticleContents.Find(Convert.ToInt32(ArticleContentId));
            article.ContentText = texteditor.Replace("&xlt;", "<");
            article.ContentText.Replace("&xgt;", ">");
            db.Entry(article).State = EntityState.Modified;
            db.SaveChanges();
            return View(article);
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        //050917 finial change
        //if is expiry then return true
        public bool IsMembershipExpiry(string Email)
        {
            bool isExpiry = false;
            // find expiry date in database
            string sql = "SELECT ExpiryDate FROM AspNetUsers WHERE Email = @p0";
            List<ExpiryDateModels> ExpiryDate = db.Database.SqlQuery<ExpiryDateModels>(sql, Email).ToList();
            //expiry date in string 
            string e = JsonConvert.SerializeObject(ExpiryDate);
            //DD/MM/YY
            var expirydate = e.Substring(16, 10).Split('/');

            int eYear = Convert.ToInt32(expirydate[2]);
            int eMonth = Convert.ToInt32(expirydate[1]);
            int eDay = Convert.ToInt32(expirydate[0]);

            //convert string to Datetime
            int tYear = DateTime.Today.Year;
            int tMonth = DateTime.Today.Month;
            int tDay = DateTime.Today.Day;
            // Compare current date and expiry date
            
            bool checkYear = eYear >= tYear ? false : true;
            bool checkM = eMonth >= tMonth ? false : true;
            bool checkD = eDay >= tDay ? false : true;
            if (!checkYear)
            {
                if (!checkM)
                {
                    if (checkD)
                    {
                        isExpiry = true;
                    }
                }
                else
                {
                    isExpiry = true;
                }
            }
            else
            {
                isExpiry = true;
            }
            return isExpiry;
        }

    }
}
