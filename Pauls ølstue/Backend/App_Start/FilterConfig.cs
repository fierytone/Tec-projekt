﻿using System.Web;
using System.Web.Mvc;
using Backend.HelperClasses;

namespace Backend
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
