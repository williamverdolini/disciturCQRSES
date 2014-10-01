using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Discitur.Api
{
    public class DisciturMailConfig : ConfigurationSection
    {
        [ConfigurationProperty("template", IsRequired = false)]
        public string Template
        {
            get
            {
                return this["template"] as string;
            }
        }

        [ConfigurationProperty("from", IsRequired = false)]
        public string From
        {
            get
            {
                return this["from"] as string;
            }
        }

        [ConfigurationProperty("subject", IsRequired = false)]
        public string Subject
        {
            get
            {
                return this["subject"] as string;
            }
        }
    
    }
}