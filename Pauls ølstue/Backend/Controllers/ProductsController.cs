﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Controllers
{
    [Authorize]
    public class ProductsController : BaseController
    {
        // GET: Products
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EditDrink(int id)
        {
            return View(id);
        }
    }
}