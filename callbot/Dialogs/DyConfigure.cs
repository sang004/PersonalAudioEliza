using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Web.Configuration;

namespace callbot
{
    public class DyConfigure
    {
        public DyConfigure()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var section = (ConnectionStringsSection)configuration.GetSection("appSettings");
            section.ConnectionStrings["Store"].ConnectionString = "Data Source=...";
            configuration.Save();
            
        }


    }
}