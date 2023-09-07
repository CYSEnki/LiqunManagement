﻿using LiqunManagement.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class BaseController : Controller
    {
        public Logger logger;

        public FormModels formdb = new FormModels();
        public MembersModel memberdb = new MembersModel();

        public BaseController()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        //public FormModels formdb = new FormModels();
        // GET: Base
        //public ActionResult Index()
        //{
        //    return View();
        //}
    }
}