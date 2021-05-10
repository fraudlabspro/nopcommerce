﻿using FraudLabsPro.FraudLabsPro;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.FraudLabsPro.Services;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.FraudLabsPro.Controllers
{
    public class OrderController : BaseAdminController
    {
        #region Fields

        private readonly FraudLabsProManager _fraudLabsProManager;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public OrderController(
            FraudLabsProManager fraudLabsProManager,
            IOrderService orderService,
            IPermissionService permissionService
            )
        {
            _fraudLabsProManager = fraudLabsProManager;
            _orderService = orderService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> FraudLabsProOrderScreen(int orderId)
        {
            //whether user has the authority
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order != null)
            {
                var orderResult = _fraudLabsProManager.ScreenOrder(order);

                if (orderResult == null)
                    throw new NopException("FraundLabs Pro order screen error: Screen result returned null.");
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> FraudLabsProOrderApprove(string transactionId, int orderId)
        {
            //whether user has the authority
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (!string.IsNullOrEmpty(transactionId))
            {
                var orderResult = _fraudLabsProManager.OrderFeedbackAsync(orderId, transactionId, Order.Action.APPROVE);

                if (orderResult == null)
                    throw new NopException("FraundLabs Pro approve error: approve result returned null.");
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> FraudLabsProOrderReject(string transactionId, int orderId)
        {
            //whether user has the authority
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (!string.IsNullOrEmpty(transactionId))
            {
                var orderResult = _fraudLabsProManager.OrderFeedbackAsync(orderId, transactionId, Order.Action.REJECT);

                if (orderResult == null)
                    throw new NopException("FraundLabs Pro feedback error: feedback result returned null.");
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> FraudLabsProOrderBlackList(string transactionId, int orderId)
        {
            //whether user has the authority
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (!string.IsNullOrEmpty(transactionId))
            {
                var orderResult = _fraudLabsProManager.OrderFeedbackAsync(orderId, transactionId, Order.Action.REJECT_BLACKLIST);

                if (orderResult == null)
                    throw new NopException("FraundLabs Pro black list error: black list result returned null.");
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}