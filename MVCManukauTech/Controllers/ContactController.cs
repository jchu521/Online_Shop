using MVCManukauTech.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//040917 
namespace MVCManukauTech.Controllers
{
    public class ContactController : Controller
    {
        private BookShopEntities db = new BookShopEntities();
        //GET METHOD
        public ActionResult Index()
        {

            //  read from database 
            string sql = "SELECT * FROM ArticleContent WHERE ArticleContentId = 2";
            var article = db.ArticleContents.SqlQuery(sql).ToList()[0];
            return View(article);
        }
        //POST Method
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

    }
}