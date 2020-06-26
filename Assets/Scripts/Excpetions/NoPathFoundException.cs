using System;
using System.Collections;
using System.Collections.Generic;


public class NoPathFoundException : Exception
{
    public NoPathFoundException(string message)
      : base(message)
    {
    }
}
