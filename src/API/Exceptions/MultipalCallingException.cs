using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Exceptions;
internal class MultipleCallingException : Exception
{
    public MultipleCallingException() : base("This method is not suppose to called twice.")
    {
    }
}
