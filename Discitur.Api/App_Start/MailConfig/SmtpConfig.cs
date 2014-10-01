using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Discitur.Api
{
    public class SmtpConfig : ConfigurationSection
    {
        public static SmtpConfig GetConfiguration()
        {
            SmtpConfig configuration = 
                ConfigurationManager
                .GetSection("SMTPServer")
                as SmtpConfig;

            if (configuration != null)
                return configuration;
            throw new  Exception("SMTPServer Config NOT Present");
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

        [ConfigurationProperty("host", IsRequired = false)]
        public string Host
        {
            get
            {
                return this["host"] as string;
            }
        }

        [ConfigurationProperty("enableSsl", IsRequired = false)]
        public bool EnableSsl
        {
            get
            {
                return (bool)this["enableSsl"];
            }
        }

        [ConfigurationProperty("userName", IsRequired = false)]
        public string UserName
        {
            get
            {
                return this["userName"] as string;
            }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return this["password"] as string;
            }
        }

        [ConfigurationProperty("port", IsRequired = false)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
        }

    
    }
}