﻿using System;
using Formo;

namespace btsmon
{
    /// <summary>
    /// Configuration properties
    /// </summary>
    internal class AppSettings
    {
        public static void Init()
        {
            _config = new Configuration();
            // bind to this static class
            _config.Bind<AppSettings>();
        }

        /// <summary>
        /// Formo instance https://github.com/ChrisMissal/Formo
        /// </summary>
        public static Configuration Config
        {
            get
            {
                return _config;
            }
        }
        private static Configuration _config;


        #region User Defined Options in <appSettings>

        public static int StartFrom { get; set; }

        public static int PollingIntervalSeconds { get; set; }

        public static int MailThrottleMinutes { get; set; }

        public static string MailServer { get; set; }

        public static string MailTo { get; set; }

        public static string MailFrom { get; set; }

        #endregion
    }
}
