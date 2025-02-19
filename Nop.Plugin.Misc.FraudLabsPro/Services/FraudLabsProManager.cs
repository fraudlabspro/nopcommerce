using System;
using System.Linq;
using System.Threading.Tasks;
using FraudLabsPro.FraudLabsPro;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using NopOrder = Nop.Core.Domain.Orders.Order;

namespace Nop.Plugin.Misc.FraudLabsPro.Services
{
    public class FraudLabsProManager
    {
        #region Fields

        private readonly FraudLabsProSettings _fraudLabsProSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public FraudLabsProManager(
            FraudLabsProSettings fraudLabsProSettings,
            IAddressService addressService,
            ICountryService countryService,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IProductService productService,
            ISettingService settingService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IWorkContext workContext
            )
        {
            _fraudLabsProSettings = fraudLabsProSettings;
            _addressService = addressService;
            _countryService = countryService;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _productService = productService;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Utilites

        /// <summary>
        /// Gets cookies check sum for device validation 
        /// </summary>
        /// <returns>Check sum string</returns>
        public string GetFLPCheckSum()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            //try to get cookie
            var cookieName = FraudLabsProDefaults.CookiesName;
            httpContext.Request.Cookies.TryGetValue(cookieName, out var cookieChecksum);

            return cookieChecksum ?? string.Empty;
        }

        /// <summary>
        /// Parse result from FraudLabs Pro
        /// </summary>
        /// <returns>String</returns>
        private string ParseFlpResult(JToken fraudLabsProResult)
        {
            if (fraudLabsProResult.Equals(null))
            {
                return "N/A";
            }
            else if ((bool) fraudLabsProResult)
            {
                return "Yes";
            }
            else
            {
                return "No";
            }
        }

        /// <summary>
        /// Update order status 
        /// </summary>
        /// <param name="order">NopCommerce order object</param>
        /// <param name="fraudLabsProStatus">FraudLAbs Pro status</param>
        public async Task UpdateOrerStatus(NopOrder order, string fraudLabsProStatus)
        {
            switch (fraudLabsProStatus)
            {
                case Order.Action.APPROVE:
                    order.OrderStatusId = _fraudLabsProSettings.ApproveStatusID;
                    break;
                case Order.Action.REJECT:
                case Order.Action.REJECT_BLACKLIST:
                    order.OrderStatusId = _fraudLabsProSettings.RejectStatusID;
                    break;
            }
            if (!string.IsNullOrEmpty(fraudLabsProStatus))
            {
                await _orderService.UpdateOrderAsync(order);
                await _orderProcessingService.CheckOrderStatusAsync(order);
            }

        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle order placed event
        /// </summary>
        /// <param name="order">Order</param>
        internal Task HandleOrderPlacedEventAsync(NopOrder order)
        {
            if (order != null)
            {
                var result = ScreenOrder(order);
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Screen an order transaction for payment fraud
        /// </summary>
        /// <param name="order">NopCommerce order object</param>
        /// <returns>OrderResult</returns>
        public async Task<JObject> ScreenOrder(NopOrder order)
        {
            //whether plugin is configured
            if (string.IsNullOrEmpty(_fraudLabsProSettings.ApiKey))
                throw new NopException($"Plugin not configured");

            try
            {
                // Configure FraudLabs Pro API KEY
                FraudLabsProConfig.APIKey = _fraudLabsProSettings.ApiKey;

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                if (customer != null)
                {
                    var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? order.BillingAddressId);
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                    //prepare parameters
                    var screenOrderPara = new OrderPara();

                    //customer information
                    screenOrderPara.IPAddress = customer.LastIpAddress ?? string.Empty;
                    screenOrderPara.FirstName = ((billingAddress != null) ? billingAddress.FirstName : string.Empty) ?? string.Empty;
                    screenOrderPara.LastName = ((billingAddress != null) ? billingAddress.LastName : string.Empty) ?? string.Empty;
                    screenOrderPara.UserPhone = ((billingAddress != null) ? billingAddress.PhoneNumber : string.Empty) ?? string.Empty;
                    screenOrderPara.EmailAddress = ((billingAddress != null) ? billingAddress.Email : string.Empty) ?? string.Empty;
                    screenOrderPara.FLPCheckSum = GetFLPCheckSum();

                    // Billing Information
                    if (billingAddress != null)
                    {
                        screenOrderPara.BillAddress = billingAddress.Address1 + " " + billingAddress.Address2;
                        screenOrderPara.BillCity = billingAddress.City ?? string.Empty;
                        screenOrderPara.BillState = (await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress))?.Name ?? string.Empty;
                        screenOrderPara.BillCountry = (await _countryService.GetCountryByAddressAsync(billingAddress))?.TwoLetterIsoCode ?? string.Empty;
                        screenOrderPara.BillZIPCode = billingAddress.ZipPostalCode ?? string.Empty;
                    }

                    // Shipping Information
                    if (shippingAddress != null)
                    {
                        screenOrderPara.ShippingAddress = shippingAddress.Address1 + " " + shippingAddress.Address2;
                        screenOrderPara.ShippingCity = shippingAddress.City ?? string.Empty;
                        screenOrderPara.ShippingState = (await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress))?.Name ?? string.Empty;
                        screenOrderPara.ShippingCountry = (await _countryService.GetCountryByAddressAsync(shippingAddress))?.TwoLetterIsoCode ?? string.Empty;
                        screenOrderPara.ShippingZIPCode = shippingAddress.ZipPostalCode ?? string.Empty;
                    }

                    //Payment information
                    var cardNumber = _encryptionService.DecryptText(order.CardNumber);

                    if (!string.IsNullOrEmpty(cardNumber))
                    {
                        screenOrderPara.BinNo = cardNumber.Substring(0, 6);
                        screenOrderPara.CardNumber = cardNumber;
                        screenOrderPara.PaymentMode = Order.PaymentMethods.CREDIT_CARD;
                        screenOrderPara.PaymentGateway = "creditcard";
                    } else {
                        screenOrderPara.PaymentMode = order.PaymentMethodSystemName ?? string.Empty;
                        screenOrderPara.PaymentGateway = order.PaymentMethodSystemName ?? string.Empty;
                    }

                    // Order Information
                    screenOrderPara.Department = (await _storeContext.GetCurrentStoreAsync()).Name ?? string.Empty;
                    screenOrderPara.UserOrderID = order.Id.ToString();
                    screenOrderPara.UserOrderMemo = order.OrderGuid.ToString();
                    screenOrderPara.Amount = order.OrderTotal;
                    screenOrderPara.Currency = order.CustomerCurrencyCode ?? string.Empty;
                    screenOrderPara.Quantity = (await _orderService.GetOrderItemsAsync(order.Id)).Sum(x => x.Quantity);
                    var itemSku = "";
                    foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        var qty = item.Quantity;
                        var sku = await _productService.FormatSkuAsync(product, item.AttributesXml);
                        if (string.IsNullOrEmpty(sku))
                        {
                            sku = product.Id.ToString();
                        }
                        itemSku += sku + ':' + qty + ',';
                    }
                    itemSku = itemSku.TrimEnd(',');
                    screenOrderPara.Items = itemSku;

                    // ScreenOrder API
                    var screenOrder = new Order();
                    // Send order to FraudLabs Pro
                    JObject result = screenOrder.ScreenOrder(screenOrderPara);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultAttribute, JsonConvert.SerializeObject(result));
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultFraudLabsProID, result["fraudlabspro_id"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultFraudLabsProScore, result["fraudlabspro_score"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultFraudLabsProStatus, result["fraudlabspro_status"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultFraudLabsProCredit, result["remaining_credits"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPAddress, result["ip_geolocation"]["ip"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPNetSpeed, result["ip_geolocation"]["netspeed"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPDomain, result["ip_geolocation"]["domain"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPTimeZone, result["ip_geolocation"]["timezone"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPLatitude, result["ip_geolocation"]["latitude"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPLongtitude, result["ip_geolocation"]["longitude"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPContinent, result["ip_geolocation"]["continent"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPCountry, result["ip_geolocation"]["country_name"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPRegion, result["ip_geolocation"]["region"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPCity, result["ip_geolocation"]["city"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPISPName, result["ip_geolocation"]["isp_name"]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIPUsageType, result["ip_geolocation"]["usage_type"][0]);
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIsProxyIPAddress, ParseFlpResult(result["ip_geolocation"]["is_proxy"]));
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIsAddressShipForward, ParseFlpResult(result["shipping_address"]["is_address_ship_forward"]));
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIsBinFound, ParseFlpResult(result["credit_card"]["is_bin_exist"]));
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIsCreditCardBlacklist, ParseFlpResult(result["credit_card"]["is_in_blacklist"]));
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultDistanceInKM, result["billing_address"]["ip_distance_in_km"].ToString());
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultDistanceInMile, result["billing_address"]["ip_distance_in_mile"].ToString());
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIsFreeEmail, ParseFlpResult(result["email_address"]["is_free"]));
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultIsEmailBlacklist, ParseFlpResult(result["email_address"]["is_in_blacklist"]));
                    _fraudLabsProSettings.Balance = result["remaining_credits"].ToString();
                    await _settingService.SaveSettingAsync(_fraudLabsProSettings);

                    //save order status
                    await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderStatusAttribute, result["fraudlabspro_status"]);

                    _ = UpdateOrerStatus(order, result["fraudlabspro_status"].ToString());

                    return result;
                }
            }
            catch (Exception exception)
            {
                //log full error
                await _logger.ErrorAsync($"FraundLabs Pro ScreenOrder error: {exception.Message}.", exception, await _workContext.GetCurrentCustomerAsync());
                return null;
            }
            return null;
        }

        /// <summary>
        /// Update status of a transaction
        /// </summary>
        /// <param name="transactionId">Unique transaction Id generated by Fraud Check API.</param>
        /// <param name="actionName">Perform APPROVE, REJECT, or REJECT_BLACKLIST action to transaction.</param>
        public async Task<JObject> OrderFeedbackAsync(int orderId, string transactionId, string actionName)
        {
            //whether plugin is configured
            if (string.IsNullOrEmpty(_fraudLabsProSettings.ApiKey))
                throw new NopException($"Plugin not configured");

            try
            {
                // Configure FraudLabs Pro API KEY
                FraudLabsProConfig.APIKey = _fraudLabsProSettings.ApiKey;

                // Set FeedBack Order parameter
                var feedbackOrderParameter = new OrderPara
                {
                    ID = transactionId,
                    Action = actionName,
                    Note = string.Empty
                };

                // Feedback Order API
                var feedbackOrder = new Order();
                JObject result = feedbackOrder.FeedbackOrder(feedbackOrderParameter);

                if ((bool) result["fraudlabspro_id"])
                {
                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    _ = UpdateOrerStatus(order, actionName);

                    var stringOrderDetail = await _genericAttributeService.GetAttributeAsync<string>(order, FraudLabsProDefaults.OrderResultAttribute);
                    if (!string.IsNullOrEmpty(stringOrderDetail))
                    {
                        var orderObject = JObject.Parse(stringOrderDetail);

                        //save order status
                        await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderStatusAttribute, actionName);

                        actionName = (actionName == Order.Action.REJECT_BLACKLIST) ? Order.Action.REJECT : actionName;
                        // orderObject[nameof(OrderResult.FraudLabsProStatus)] = actionName;
                        await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultAttribute, orderObject.ToString());
                        await _genericAttributeService.SaveAttributeAsync(order, FraudLabsProDefaults.OrderResultFraudLabsProStatus, actionName);

                    }
                    return (JObject)result["fraudlabspro_id"];
                }

                return result;
            }
            catch (Exception exception)
            {
                //log full error
                await _logger.ErrorAsync($"FraundLabs Pro feedback error: {exception.Message}.", exception, await _workContext.GetCurrentCustomerAsync());
            }
            return null;
        }

        #endregion
    }
}
