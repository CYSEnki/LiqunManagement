﻿using LiqunManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class BaseController : Controller
    {
        public LiqunModels liqundb = new LiqunModels();
        // GET: Base
        public ActionResult Index()
        {
            return View();
        }
    }
}