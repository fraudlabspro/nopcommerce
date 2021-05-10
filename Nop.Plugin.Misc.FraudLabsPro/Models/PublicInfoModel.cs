﻿using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.FraudLabsPro.Models
{
    /// <summary>
    /// Represents a public info model
    /// </summary>
    public record PublicInfoModel : BaseNopModel
    {
        public string HrefUrl { get; set; }
        public string LinkSrc { get; set; }
    }
}
