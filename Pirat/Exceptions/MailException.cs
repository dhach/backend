﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pirat.Exceptions
{
    [Serializable]
    public class MailException : Exception
    {
        public MailException(string message)
        : base(message)
        { }

        public MailException() { }

        public MailException(string message, Exception innerException) : base(message, innerException) { }
    }
}
