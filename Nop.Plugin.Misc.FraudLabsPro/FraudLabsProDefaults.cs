﻿namespace Nop.Plugin.Misc.FraudLabsPro
{
    /// <summary>
    /// Represents constants of the FraudLabsPro plugin
    /// </summary>
    public static class FraudLabsProDefaults
    {
        /// <summary>
        /// FraudLabs Pro system name
        /// </summary>
        public static string SystemName => "Misc.FraudLabsPro";

        /// <summary>
        /// Name of the view component to display seal in public store
        /// </summary>
        public const string SEAL_VIEW_COMPONENT_NAME = "FraudLabsPro.Secured.Seal";

        /// <summary>
        /// Name of the view component to disaply FraudLabsPro block on the order details page
        /// </summary>
        public const string ORDER_VIEW_COMPONENT_NAME = "FraudLabsPro.OrderDetails";

        /// <summary>
        /// Generic attribute name to hide FraudLabsPro order block on the order details page
        /// </summary>
        public static string HideBlockAttribute = "OrderPage.HideFraudLabsProBlock";

        /// <summary>
        /// Gets Secured Seal link url
        /// </summary>
        public static string SecuredSealHrefUrl => "https://www.fraudlabspro.com/?ref=15876#secured-seal-1";

        /// <summary>
        /// Gets Secured Seal image src
        /// </summary>
        public static string SecuredSealLinkSrc => "//www.fraudlabspro.com/images/secured-seals/seal.png?ref=15876";

        /// <summary>
        /// One page checkout route name
        /// </summary>
        public static string OnePageCheckoutRouteName => "CheckoutOnePage";

        /// <summary>
        /// Confirm checkout route name
        /// </summary>
        public static string ConfirmCheckoutRouteName => "CheckoutConfirm";

        /// <summary>
        /// Gets a key of the Order result
        /// </summary>
        public static string OrderResultAttribute => "FraudLabsProOrderResult";
        public static string OrderResultFraudLabsProID => "FraudLabsProID";
        public static string OrderResultFraudLabsProScore => "FraudLabsProScore";
        public static string OrderResultFraudLabsProStatus => "FraudLabsProStatus";
        public static string OrderResultFraudLabsProCredit => "FraudLabsProCredit";
        public static string OrderResultIPAddress => "IPAddress";
        public static string OrderResultIPNetSpeed => "IPNetSpeed";
        public static string OrderResultIPDomain => "IPDomain";
        public static string OrderResultIPTimeZone => "IPTimeZone";
        public static string OrderResultIPLatitude => "IPLatitude";
        public static string OrderResultIPLongtitude => "IPLongtitude";
        public static string OrderResultIPContinent => "IPContinent";
        public static string OrderResultIPCountry => "IPCountry";
        public static string OrderResultIPRegion => "IPRegion";
        public static string OrderResultIPCity => "IPCity";
        public static string OrderResultIPISPName => "IPISPName";
        public static string OrderResultIPUsageType => "IPUsageType";
        public static string OrderResultIsProxyIPAddress => "IsProxyIPAddress";
        public static string OrderResultIsAddressShipForward => "IsAddressShipForward";
        public static string OrderResultIsBinFound => "IsBinFound";
        public static string OrderResultIsCreditCardBlacklist => "IsCreditCardBlacklist";
        public static string OrderResultDistanceInKM => "DistanceInKM";
        public static string OrderResultDistanceInMile => "DistanceInMile";
        public static string OrderResultIsFreeEmail => "IsFreeEmail";
        public static string OrderResultIsEmailBlacklist => "IsEmailBlacklist";

        /// <summary>
        /// Gets a key of the Order status
        /// </summary>
        public static string OrderStatusAttribute => "FraudLabsProOrderStatus";

        /// <summary>
        /// Gets cookies name for device validation
        /// </summary>
        public static string CookiesName => "flp_checksum";

        /// <summary>
        /// Gets FraudLabs Pro tab name
        /// </summary>
        public static string FraudLabsProPanelId => "order-fraudlabspro";

        /// <summary>
        /// Gets path of agent js script
        /// </summary>
        public static string AgentScriptPath => "~/Plugins/Misc.FraudLabsPro/Scripts/agent_javascript.js";

        /// <summary>
        /// Gets FraudLabs Pro Merchant Area url
        /// </summary>
        public static string FraudLabsProMerchantArea => "https://www.fraudlabspro.com/merchant/";

        /// <summary>
        /// Gets FraudLabs Pro scope image url
        /// </summary>
        public static string FraudLabsProImageUrl => "https://www.fraudlabspro.com/images/fraudscore/fraudlabsproscore_a";

        /// <summary>
        /// Gets FraudLabs Pro upgrade url
        /// </summary>
        public static string FraudLabsProUpgrageUrl => "https://www.fraudlabspro.com/pricing";
    }
}