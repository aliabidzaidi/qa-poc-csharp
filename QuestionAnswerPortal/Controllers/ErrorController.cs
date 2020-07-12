using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuestionAnswerPortal.Controllers
{
    public class ErrorController : Controller
    {

        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}