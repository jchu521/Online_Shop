//
//NOTE: To run Unit Tests from here, 
//  (1) Wake up the test webserver by right-clicking on the top project node "MVCManukauTech" in Solution Explorer, then select "View in Browser ..."
//  (2) When the browser is running the web app, LEAVE IT RUNNING while you switch to Visual Studio -- Menu: Test --> Run --> All Tests
//
//NOTE: get-started statement below --  driver.Navigate().GoToUrl("http://localhost:7142/"); -- remember to change the URL for testing other setups

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualBasic;

//Selenium!  Ref: http://www.seleniumhq.org/docs/03_webdriver.jsp
using OpenQA.Selenium;

//140415 JPC handle JSON data with System.Web.Helpers.Json
using System.Web.Helpers;
using OpenQA.Selenium.IE;

//150609 JPC use "sleep" to give a more human-looking speed to this robot browser operator.
//   ref. http://stackoverflow.com/questions/8815895/why-is-thread-sleep-so-harmful
//   "Thread.Sleep has its use: simulating lengthy operations while testing/debugging on an MTA thread. 
//   In .NET there's no other reason to use it"
// We are "simulating lengthy operations while testing" therefore I feel OK about using "Sleep".

namespace MVC5ManukauTech.Tests.Controllers
{
    [TestClass]
    public class RemoteFunctionsTest
    {
        [TestMethod]
        public void TestLogin()
        {
            // Create a new instance of the Firefox driver.
            // Documentation says we can run with IE with a small code change but this is not working for us
            // Documentation says Google Chrome is possible with an extra download
            // Ref more info at bottom of page: http://www.seleniumhq.org/docs/03_webdriver.jsp

            //20161024 change from FireFox to InternetExplorer
            IWebDriver driver = new InternetExplorerDriver();
            System.Threading.Thread.Sleep(6000);
            //IWebDriver driver = new FirefoxDriver();

            //Simplest way of dealing with time taken for pages to load.  One line here does it all.
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

            IWebElement element;

            driver.Navigate().GoToUrl("http://localhost:7142/");

            element = driver.FindElement(By.Id("loginLink"));
            element.Click();
            System.Threading.Thread.Sleep(2000);       

            // Find the text input element by its name and enter the text
            element = driver.FindElement(By.Name("Email"));
            element.SendKeys("admin@example.com");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Name("Password"));
            element.SendKeys("Admin@123456");
            System.Threading.Thread.Sleep(2000);
            // Now submit the form. WebDriver will find the form for us from the element
            element.Submit();

            //Check out the page for results
            //string s = driver.PageSource;
            //bool test = s.Contains("Hello admin@example.com!");

            //Assert.IsTrue(test, "Not logged in as attempted");

            System.Threading.Thread.Sleep(2000);

            //buy something
            //look in the catalog under "communications"
            element = driver.FindElement(By.LinkText("Communications"));
            element.Click();
            System.Threading.Thread.Sleep(6000);

            //Challenge - input element button does not have id or name
            //Success - use XPath to find the input with onclick="NavCart('JWLTRANS6')"
            element = driver.FindElement(By.XPath("//input[@onclick=\"NavCart('JWLTRANS6')\"]"));
            element.Click();

            System.Threading.Thread.Sleep(2000);

            //We are now in the shopping cart
            //Change quantity
            element = driver.FindElement(By.Id("quantity_1"));
            //We want to clear the a"1" that is already there
            element.Clear();
            element.SendKeys("2");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.XPath("//input[@value=\"Go To Checkout\"]"));
            System.Threading.Thread.Sleep(2000);
            element.Click();

            //Major page navigation, give some time
            System.Threading.Thread.Sleep(2000);

            //Checkout Form

            element = driver.FindElement(By.Id("CustomerName"));
            element.SendKeys("John Tester");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("AddressStreet"));
            element.SendKeys("111 Imaginary Rd");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("Location"));
            element.SendKeys("Henderson");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("Country"));
            element.SendKeys("NZ");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("PostCode"));
            element.SendKeys("0612");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("CardOwner"));
            element.SendKeys("Mr Tester");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("CardType"));
            element.SendKeys("Visa");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("CardNumber"));
            element.SendKeys("1111");
            System.Threading.Thread.Sleep(2000);

            element = driver.FindElement(By.Id("CSC"));
            element.SendKeys("111");
            System.Threading.Thread.Sleep(2000);

            element.Submit();
            System.Threading.Thread.Sleep(2000);

            //Check out the page for results
            string s = driver.PageSource;
            bool test = s.Contains("Payment of 919.98 is accepted");

            Assert.IsTrue(test, "Transaction not completed as expected");

            //Close the browser
            //driver.Quit();  
        }
 
    }
}
