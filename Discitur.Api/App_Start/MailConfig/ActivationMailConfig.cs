using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Discitur.Api
{
    public class ActivationMailConfig : DisciturMailConfig
    {
        [ConfigurationProperty("activation-url", IsRequired = false)]
        public string ActivationURL
        {
            get
            {
                return this["activation-url"] as string;
            }
        }

        [ConfigurationProperty("activation-path", IsRequired = true)]
        public string ActivationPath
        {
            get
            {
                return this["activation-path"] as string;
            }
        }

    }
}