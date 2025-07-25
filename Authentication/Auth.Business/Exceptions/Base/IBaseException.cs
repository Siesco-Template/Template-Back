using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Exceptions.Base
{
    public interface IBaseException
    {
        public int StatusCode { get; }
        public string ErrorMessage { get; }
    }
}
