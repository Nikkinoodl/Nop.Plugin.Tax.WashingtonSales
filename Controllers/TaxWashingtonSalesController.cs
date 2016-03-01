using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Plugin.Tax.WashingtonSales.Models;
using Nop.Services.Directory;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;

namespace Nop.Plugin.Tax.WashingtonSales.Controllers
{
    [AdminAuthorize]
    public class TaxWashingtonSalesController : BasePluginController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        public TaxWashingtonSalesController(ITaxCategoryService taxCategoryService,
            ISettingService settingService,
            IPermissionService permissionService)
        {
            this._taxCategoryService = taxCategoryService;
            this._settingService = settingService;
            _permissionService = permissionService;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            //little hack here
            //always set culture to 'en-US' (Telerik has a bug related to editing decimal values in other cultures). Like currently it's done for admin area in Global.asax.cs
            CommonHelper.SetTelerikCulture();
            base.Initialize(requestContext);
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            return View("~/Plugins/Tax.WashingtonSales/Views/TaxWashingtonSales/Configure.cshtml");
        }

      //  [NonAction]
      //  protected decimal GetTaxRate(int taxCategoryId)
     //   {
      //      var rate = this._settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.WashingtonSales.TaxCategoryId{0}", taxCategoryId));
      //      return rate;
      //  }
    }
}

