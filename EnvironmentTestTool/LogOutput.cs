using System;
using System.Collections.Generic;
using System.Text;

namespace RNASTestingTool
{
    public class LogOutput
    {
        public enum Status { Pass, Warning, Fail }
        public Status TestStatus { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Address { get; set; }
    }
    
}
