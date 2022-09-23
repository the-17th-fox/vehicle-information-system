using Microsoft.AspNetCore.Authentication.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class GoogleOptionsExtension
    {
        public static void SetPredefined(this GoogleOptions opt, string clientId, string clientSecret)
        {
            opt.ClientId = clientId;
            opt.ClientSecret = clientSecret;
            opt.SaveTokens = true;
        }
    }
}
