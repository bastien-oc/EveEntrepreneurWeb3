﻿using System.Web.Mvc;

namespace EveEntrepreneurWebPersistency3 {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
