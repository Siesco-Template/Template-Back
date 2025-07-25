using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Helpers.Validators
{
    //Bu hisse dile gore ise yaramayacaq deye heke ki havadadir
    public class BaseStringValidator : AbstractValidator<string>
    {
        public BaseStringValidator()
        {
            
        }
    }
}
