using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Utilies.Templates
{
    public class EmailTemplate(IConfiguration _configuration)
    {
        private readonly string Facebook = $"{_configuration["BaseUrl"]}/Files/Images/EmailImages/facebook.png";
        private readonly string X = $"{_configuration["BaseUrl"]}/Files/Images/EmailImages/X.png";
        private readonly string Instagram = $"{_configuration["BaseUrl"]}/Files/Images/EmailImages/instagram.png";
        private readonly string Linkedin = $"{_configuration["BaseUrl"]}/Files/Images/EmailImages/linkedin.png";
        private readonly string Secure = $"{_configuration["BaseUrl"]}/Files/Images/EmailImages/secure.png";
        private readonly string UserCheck = $"{_configuration["BaseUrl"]}/Files/Images/EmailImages/userCheck.png";
        private readonly string Youtube = $"{_configuration["BaseUrl"]}/Files/Images/EmailImages/youtube.png";

        public string ResetPassword(string token, string fullName)
        {
            string host = _configuration["ResetPassword"]!;

            return $@"
<!DOCTYPE html>
<html>
  <head>
    <meta charset=""UTF-8"" />
    <title>Şifrənizi yeniləyin</title>
    <style>
    </style>
  </head>
  <body align=""center"" style=""width: 600px; margin:0 auto; padding:0; background-color:#f0f4ff; font-family:Poppins, Arial, sans-serif; box-sizing: border-box;"">
    <table align=""center"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f0f4ff; padding:32px; border: 0; max-width: 536px; width: 100%; margin-inline: auto; box-sizing: border-box;"">
      <tr>
        <td style=""text-align: center;"">
          <!-- Header -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""border-radius:16px; padding-bottom:20px; border: 0; box-sizing: border-box;"">
            <!-- Logo -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Auth
              </td>
            </tr>
            <!-- Title -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Şifrənizi yeniləyin
              </td>
            </tr>
          </table>

          <!-- Card -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""background-color:#ffffff; border-radius:16px; padding:32px 40px; border: 0; box-sizing: border-box;"">
            <!-- Image -->
            <tr>
              <td align=""center"" style=""padding-bottom: 32px; text-align: center; box-sizing: border-box;"">
                <img src=""{Secure}"" alt=""Secure Icon"" height=""92"" style=""display:block; margin-inline: auto; box-sizing: border-box;"" />
              </td>
            </tr>
            <!-- Content -->
            <tr>
              <td style=""padding-bottom: 20px; color:#303C4A; font-size:14px; line-height:1.4; font-weight: 400; text-align: center; box-sizing: border-box; box-sizing: border-box;"">
                <p style=""font-size: 16px; font-weight: 600; color: #1E1E1E; line-height: 1.4; margin: 0; padding-bottom: 8px;"">
                  <strong>Salam {fullName},</strong>
                </p>

                Şifrənizi yeniləmək üçün aşağıdakı linkə klikləyin:
              </td>
            </tr>
            <!-- Button -->
            <tr>
              <td style=""text-align: center; box-sizing: border-box;"">
                <a href='{host}/{token}' style=""background-color:#0068F7; color:#fff; padding:7.5px 40px; text-decoration:none; border-radius:12px; border: 1px solid #8ABAFB; font-size:14px; line-height: 1.5; font-weight: 500; display:inline-block; box-sizing: border-box;"">
                  Şifrənizi yeniləyin
                </a>
              </td>
            </tr>
          </table>

          <!-- Footer -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:20px; border: 0; box-sizing: border-box;"">
            <tr>
              <td style=""font-size:16px; font-weight: 500; color:#002C68; padding-bottom:12px; text-align: center; box-sizing: border-box;"">
                Bizi izləyin
              </td>
            </tr>
            <tr>
              <td style=""padding-bottom:6px; text-align: center; box-sizing: border-box;"">
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Facebook}"" alt=""Facebook"" width=""32"" height=""32""/>
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Instagram}"" alt=""Instagram"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{X}"" alt=""X"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Youtube}"" alt=""Youtube"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Linkedin}"" alt=""LinkedIn"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
              </td>
            </tr>
            <tr>
              <td style=""font-size:16px; line-height: 1.5; font-weight: 500; color:#002C68; text-align: center; box-sizing: border-box;"">
                <a href=""https://www.website.com"" style=""text-decoration:none; color:#002C68;"">
                  www.website.com
                </a>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>
";
        }

        public string RegisterCompleted(string fullName)
        {
            string host = _configuration["RegisterCompleted"]!;

            return $@"
<!DOCTYPE html>
<html>
  <head>
    <meta charset=""UTF-8"" />
    <title>Qeydiyyat tamamlandı</title>
    <style>
    </style>
  </head>
  <body align=""center"" style=""width: 600px; margin:0 auto; padding:0; background-color:#f0f4ff; font-family:Poppins, Arial, sans-serif; box-sizing: border-box;"">
    <table align=""center"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f0f4ff; padding:32px; border: 0; max-width: 536px; width: 100%; margin-inline: auto; box-sizing: border-box;"">
      <tr>
        <td style=""text-align: center;"">
          <!-- Header -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""border-radius:16px; padding-bottom:20px; border: 0; box-sizing: border-box;"">
            <!-- Logo -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Auth
              </td>
            </tr>
            <!-- Title -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Qeydiyyatınız tamamlandı!
              </td>
            </tr>
          </table>

          <!-- Card -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""background-color:#ffffff; border-radius:16px; padding:32px 40px; border: 0; box-sizing: border-box;"">
            <!-- Image -->
            <tr>
              <td align=""center"" style=""padding-bottom: 32px; text-align: center; box-sizing: border-box;"">
                <img src=""{UserCheck}"" alt=""User Icon"" height=""92"" style=""display:block; margin-inline: auto; box-sizing: border-box; text-align: center;"" />
              </td>
            </tr>
            <!-- Content -->
            <tr>
              <td style=""padding-bottom: 20px; color:#303C4A; font-size:14px; line-height:1.4; font-weight: 400; text-align: center; box-sizing: border-box; box-sizing: border-box;"">
                <p style=""font-size: 16px; font-weight: 600; color: #1E1E1E; line-height: 1.4; margin: 0; padding-bottom: 8px;"">
                  <strong>Salam {fullName},</strong>
                </p>

                Qeydiyyatınız uğurla tamamlandı. Platformamıza xoş gəldiniz! 😊 <br />
                Sizi burada görməkdən məmnunuq.<br />
                İndi hesabınıza daxil olaraq imkanlarımızdan tam şəkildə yararlana bilərsiniz.<br />
                Hesabınıza keçid etmək üçün:
              </td>
            </tr>
            <!-- Button -->
            <tr>
              <td style=""text-align: center; box-sizing: border-box;"">
                <a href=""#"" style=""background-color:#0068F7; color:#fff; padding:7.5px 75px; text-decoration:none; border-radius:12px; border: 1px solid #8ABAFB; font-size:14px; line-height: 1.5; font-weight: 500; display:inline-block; box-sizing: border-box;"">
                  Daxil ol
                </a>
              </td>
            </tr>
          </table>

          <!-- Footer -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:20px; border: 0; box-sizing: border-box;"">
            <tr>
              <td style=""font-size:16px; font-weight: 500; color:#002C68; padding-bottom:12px; text-align: center; box-sizing: border-box;"">
                Bizi izləyin
              </td>
            </tr>
            <tr>
              <td style=""padding-bottom:6px; text-align: center; box-sizing: border-box;"">
                <a href=""#"" style=""text-decoration: none;"">
                  <img src='{Facebook}' alt=""Facebook"" width=""32"" height=""32""/>
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src='{Instagram}' alt=""Instagram"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src='{X}' alt=""X"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src='{Youtube}' alt=""Youtube"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src='{Linkedin}' alt=""LinkedIn"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
              </td>
            </tr>
            <tr>
              <td style=""font-size:16px; line-height: 1.5; font-weight: 500; color:#002C68; text-align: center; box-sizing: border-box;"">
                <a href=""https://www.website.com"" style=""text-decoration:none; color:#002C68;"">
                  www.website.com
                </a>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
        }

        public string CreatedByAdmin(string fullName, string password)
        {
            return $@"
<!DOCTYPE html>
<html>
  <head>
    <meta charset=""UTF-8"" />
    <title>Hesabınız yaradıldı</title>
    <style>
    </style>
  </head>
  <body align=""center"" style=""width: 600px; margin:0 auto; padding:0; background-color:#f0f4ff; font-family:Poppins, Arial, sans-serif; box-sizing: border-box;"">
    <table align=""center"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f0f4ff; padding:32px; border: 0; margin-inline: auto; box-sizing: border-box;"">
      <tr>
        <td style=""text-align: center;"">
          <!-- Header -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""border-radius:16px; padding-bottom:20px; border: 0; box-sizing: border-box;"">
            <!-- Logo -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Auth
              </td>
            </tr>
            <!-- Title -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Hesabınız yaradıldı
              </td>
            </tr>
          </table>

          <!-- Card -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""background-color:#ffffff; border-radius:16px; padding:32px 40px; border: 0; box-sizing: border-box;"">
            <!-- Image -->
            <tr>
              <td align=""center"" style=""padding-bottom: 32px; text-align: center; box-sizing: border-box;"">
                <img src=""{Secure}"" alt=""Secure Icon"" height=""92"" style=""display:block; margin : auto; box-sizing: border-box;"" />
              </td>
            </tr>
            <!-- Content -->
            <tr>
              <td style=""padding-bottom: 20px; color:#303C4A; font-size:14px; line-height:1.4; font-weight: 400; text-align: center; box-sizing: border-box; box-sizing: border-box;"">
                <p style=""font-size: 16px; font-weight: 600; color: #1E1E1E; line-height: 1.4; margin: 0; padding-bottom: 8px;"">
                  <strong>Salam {fullName},</strong>
                </p>

                Hesabınız admin tərəfindən yaradıldı. Xahiş olunur, ilk girişdən sonra şifrənizi dəyişəsiniz. Şifrəniz:
              </td>
            </tr>
            <!-- Button --> 
            <tr>
              <td style=""text-align: center; box-sizing: border-box;"">
                <p href=""#"" style=""margin: 0; letter-spacing: 8px; color:#3490EC; border: 1px dashed #3490EC; padding:8px 68px; text-decoration:none; border-radius:26px; font-size:24px; line-height: 1.5; font-weight: 400; display:inline-block; box-sizing: border-box;"">
                  {password}
                </p>
              </td>
            </tr>
          </table>

          <!-- Footer -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:20px; border: 0; box-sizing: border-box;"">
            <tr>
              <td style=""font-size:16px; font-weight: 500; color:#002C68; padding-bottom:12px; text-align: center; box-sizing: border-box;"">
                Bizi izləyin
              </td>
            </tr>
            <tr>
              <td style=""padding-bottom:6px; text-align: center; box-sizing: border-box;"">
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Facebook}"" alt=""Facebook"" width=""32"" height=""32""/>
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Instagram}"" alt=""Instagram"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{X}"" alt=""X"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Youtube}"" alt=""Youtube"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Linkedin}"" alt=""LinkedIn"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
              </td>
            </tr>
            <tr>
              <td style=""font-size:16px; line-height: 1.5; font-weight: 500; color:#002C68; text-align: center; box-sizing: border-box;"">
                <a href=""https://www.website.com"" style=""text-decoration:none; color:#002C68;"">
                  www.website.com
                </a>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>
";
        }

        public string ChangedPasswordByAdmin(string fullName, string password)
        {
            return $@"
<!DOCTYPE html>
<html>
  <head>
    <meta charset=""UTF-8"" />
    <title>Şifrəniz yeniləndi</title>
    <style>
    </style>
  </head>
  <body align=""center"" style=""width: 600px; margin:0 auto; padding:0; background-color:#f0f4ff; font-family:Poppins, Arial, sans-serif; box-sizing: border-box;"">
    <table align=""center"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f0f4ff; padding:32px; border: 0; margin-inline: auto; box-sizing: border-box;"">
      <tr>
        <td style=""text-align: center;"">
          <!-- Header -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""border-radius:16px; padding-bottom:20px; border: 0; box-sizing: border-box;"">
            <!-- Logo -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Auth
              </td>
            </tr>
            <!-- Title -->
            <tr>
              <td style=""font-size:20px; color:#3D4C5E; font-weight:600; line-height: 1.5; text-align: center; box-sizing: border-box;"">
                Şifrəniz yeniləndi
              </td>
            </tr>
          </table>

          <!-- Card -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0""  style=""background-color:#ffffff; border-radius:16px; padding:32px 40px; border: 0; box-sizing: border-box;"">
            <!-- Image -->
            <tr>
              <td align=""center"" style=""padding-bottom: 32px; text-align: center; box-sizing: border-box;"">
                <img src=""{Secure}"" alt=""Secure Icon"" height=""92"" style=""display:block; margin-inline: auto; box-sizing: border-box;"" />
              </td>
            </tr>
            <!-- Content -->
            <tr>
              <td style=""padding-bottom: 20px; color:#303C4A; font-size:14px; line-height:1.4; font-weight: 400; text-align: center; box-sizing: border-box; box-sizing: border-box;"">
                <p style=""font-size: 16px; font-weight: 600; color: #1E1E1E; line-height: 1.4; margin: 0; padding-bottom: 8px;"">
                  <strong>Salam {fullName},</strong>
                </p>

                Şifrəniz admin tərəfindən yeniləndi.
                Xahiş olunur, təhlükəsizlik məqsədilə ilk girişdən sonra şifrəni dəyişəsiniz. Yeni şifrəniz:
              </td>
            </tr>
            <!-- Button -->
            <tr>
              <td style=""text-align: center; box-sizing: border-box;"">
                <p href=""#"" style=""margin: 0; letter-spacing: 8px; color:#3490EC; border: 1px dashed #3490EC; padding:8px 68px; text-decoration:none; border-radius:26px; font-size:24px; line-height: 1.5; font-weight: 400; display:inline-block; box-sizing: border-box;"">
                  {password}
                </p>
              </td>
            </tr>
          </table>

          <!-- Footer -->
          <table width=""536"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:20px; border: 0; box-sizing: border-box;"">
            <tr>
              <td style=""font-size:16px; font-weight: 500; color:#002C68; padding-bottom:12px; text-align: center; box-sizing: border-box;"">
                Bizi izləyin
              </td>
            </tr>
            <tr>
              <td style=""padding-bottom:6px; text-align: center; box-sizing: border-box;"">
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Facebook}"" alt=""Facebook"" width=""32"" height=""32""/>
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Instagram}"" alt=""Instagram"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{X}"" alt=""X"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Youtube}"" alt=""Youtube"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
                <a href=""#"" style=""text-decoration: none;"">
                  <img src=""{Linkedin}"" alt=""LinkedIn"" width=""32"" height=""32"" style=""margin-left: 20px;"" />
                </a>
              </td>
            </tr>
            <tr>
              <td style=""font-size:16px; line-height: 1.5; font-weight: 500; color:#002C68; text-align: center; box-sizing: border-box;"">
                <a href=""https://www.website.com"" style=""text-decoration:none; color:#002C68;"">
                  www.website.com
                </a>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
        }
    }
}
