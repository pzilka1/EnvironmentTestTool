using System;
using System.Collections.Generic;
using System.Text;

namespace RNASTestingTool
{
    /// <summary>
    /// List global test tool settings
    /// </summary>
    public class Settings
    {
        public int Timeout { get; set; }
        public string Version { get; set; }

        public List<WebTarget> WebTests { get; set; }
        public List<PingTarget> PingTests { get; set; }
    }

    /// <summary>
    /// list of ping targets to test
    /// </summary>
    public class PingTarget
    {
        public string Title { get; set; }

        public string Address { get; set; }
    }

    /// <summary>
    /// list of web targets to test
    /// </summary>
    public class WebTarget
    {
        public string Title { get; set; }

        public string URL { get; set; }
    }



}
