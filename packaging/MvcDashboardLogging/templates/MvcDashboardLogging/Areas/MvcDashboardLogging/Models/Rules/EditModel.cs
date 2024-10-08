﻿using MyMvcApp.Data.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLogging.Models.Rules
{
    public class EditModel : BaseEditModel<LogActionRule>
    {
        public bool IsNew => this.Item.Id == 0;
    }
}
