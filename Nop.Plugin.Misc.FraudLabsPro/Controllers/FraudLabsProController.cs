using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.FraudLabsPro.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.FraudLabsPro.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class FraudLabsProController : BasePluginController
    {
        #region Fields

        private readonly FraudLabsProSettings _fraudLabsProSettings;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public FraudLabsProController(
            FraudLabsProSettings fraudLabsProSettings,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService
            )
        {
            _fraudLabsProSettings = fraudLabsProSettings;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare FraudLabsProModel
        /// </summary>
        /// <param name="model">Model</param>
        protected async void PrepareModel(ConfigurationModel model)
        {
            //prepare common properties
            model.ApiKey = _fraudLabsProSettings.ApiKey;
            model.ApproveStatusID = _fraudLabsProSettings.ApproveStatusID;
            model.ReviewStatusID = _fraudLabsProSettings.ReviewStatusID;
            model.RejectStatusID = _fraudLabsProSettings.RejectStatusID;
            model.Balance = _fraudLabsProSettings.Balance;

            //prepare available order statuses
            var availableStatusItems = await OrderStatus.Pending.ToSelectListAsync(false);
            foreach (var statusItem in availableStatusItems)
            {
                model.AvailableStatusLists.Add(statusItem);
            }
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public IActionResult Configure()
        {
            var model = new ConfigurationModel();
            PrepareModel(model);

            return View("~/Plugins/Misc.FraudLabsPro/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //set API key
            _fraudLabsProSettings.ApiKey = model.ApiKey;
            await _settingService.SaveSettingAsync(_fraudLabsProSettings, x => x.ApiKey, clearCache: false);

            //set Approve status
            _fraudLabsProSettings.ApproveStatusID = model.ApproveStatusID;
            await _settingService.SaveSettingAsync(_fraudLabsProSettings, x => x.ApproveStatusID, clearCache: false);

            //set Review status
            _fraudLabsProSettings.ReviewStatusID = model.ReviewStatusID;
            await _settingService.SaveSettingAsync(_fraudLabsProSettings, x => x.ReviewStatusID, clearCache: false);

            //set Reject status
            _fraudLabsProSettings.RejectStatusID = model.RejectStatusID;
            await _settingService.SaveSettingAsync(_fraudLabsProSettings, x => x.RejectStatusID, clearCache: false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion

    }
}
