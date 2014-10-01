using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Discitur.Api
{
    public class MailConfigProvider 
    {

        public static T GetConfiguration<T>() where T : DisciturMailConfig
        {
            string configName = typeof(T).Name;
            T configuration = (T)ConfigurationManager.GetSection(configName) as T;

            if (configuration != null)
                return configuration;
            throw new Exception(configName+ "Config NOT Present");
        }
    }
}