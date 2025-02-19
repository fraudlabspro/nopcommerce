using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.FraudLabsPro.Models.Order;
using Nop.Plugin.Misc.FraudLabsPro.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.FraudLabsPro.Factories
{
    /// <summary>
    /// Represents FraudLabsPro order model factory
    /// </summary>
    public class FraudLabsProOrderModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public FraudLabsProOrderModelFactory(
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare FraudLabsPro order model
        /// </summary>
        /// <param name="orderModel">Order model</param>
        /// <param name="order">Order</param>
        /// <returns>FraudLabsPro Order model</returns>
        public FraudLabsProOrderModel PrepareOrderModel(OrderModel orderModel, Order order)
        {
            if (orderModel == null)
                throw new ArgumentNullException(nameof(orderModel));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //prepare model
            var model = new FraudLabsProOrderModel
            {
                Id = orderModel.Id,
                UserOrderID = order.Id.ToString(),
                HideBlock = _genericAttributeService.GetAttributeAsync<bool>(_workContext.GetCurrentCustomerAsync().Result, FraudLabsProDefaults.HideBlockAttribute).Result
            };

            var stringResponse = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultAttribute).Result;
            if (!string.IsNullOrEmpty(stringResponse))
            {
                var response = JObject.Parse(stringResponse);
                var orderResultModel = response.ToObject<FraudLabsProOrderModel>();

                // orderResultModel.Id = orderModel.Id;
                // orderResultModel.IPAddress = (_customerService.GetCustomerByIdAsync(order.CustomerId).Result)?.LastIpAddress;
                // orderResultModel.IPCountry = ISO3166.FromCountryCode(orderResultModel.IPCountry)?.Name ?? "-";

                orderResultModel.Id = orderModel.Id;
                orderResultModel.UserOrderID = order.Id.ToString();
                orderResultModel.FraudLabsProOriginalStatus =  _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderStatusAttribute).Result ?? string.Empty;
                orderResultModel.FraudLabsProID = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultFraudLabsProID).Result;
                orderResultModel.FraudLabsProScore = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultFraudLabsProScore).Result;
                orderResultModel.FraudLabsProStatus = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultFraudLabsProStatus).Result;
                orderResultModel.FraudLabsProCredit = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultFraudLabsProCredit).Result;
                orderResultModel.IPAddress = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPAddress).Result;
                orderResultModel.IPNetSpeed = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPNetSpeed).Result;
                orderResultModel.IPDomain = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPDomain).Result;
                orderResultModel.IPTimeZone = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPTimeZone).Result;
                orderResultModel.IPLatitude = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPLatitude).Result;
                orderResultModel.IPLongtitude = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPLongtitude).Result;
                orderResultModel.IPContinent = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPContinent).Result;
                orderResultModel.IPCountry = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPCountry).Result;
                orderResultModel.IPRegion = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPRegion).Result;
                orderResultModel.IPCity = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPCity).Result;
                orderResultModel.IPISPName = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPISPName).Result;
                orderResultModel.IPUsageType = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIPUsageType).Result;
                orderResultModel.IsProxyIPAddress = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIsProxyIPAddress).Result;
                orderResultModel.IsAddressShipForward = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIsAddressShipForward).Result;
                orderResultModel.IsBinFound = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIsBinFound).Result;
                orderResultModel.IsCreditCardBlacklist = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIsCreditCardBlacklist).Result;
                orderResultModel.DistanceInKM = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultDistanceInKM).Result;
                orderResultModel.DistanceInMile = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultDistanceInMile).Result;
                orderResultModel.IsFreeEmail = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIsFreeEmail).Result;
                orderResultModel.IsEmailBlacklist = _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultIsEmailBlacklist).Result;
                orderResultModel.IsHighRiskCountry = "N/A";
                model = orderResultModel;
            }

            return model;
        }

        #endregion
    }
}