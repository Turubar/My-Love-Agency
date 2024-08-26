using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLoveAgency.Application;
using MyLoveAgency.Models;
using MyLoveAgency.Models.Database;
using System.Security.Claims;
using System.Text;

namespace MyLoveAgency.Controllers
{
    public class PageController : Controller
    {
        private readonly LovelyLoveDbContext _dbContext;
        public PageController(LovelyLoveDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Home()
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Page/ChangeLanguage?refererPage=Home");

            ViewBag.NamePage = "Home";
            return View();
        }

        [HttpGet]
        [Route("/Page/Services/{nameType?}")]
        public IActionResult Services(string nameType = "")
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Page/ChangeLanguage?refererPage=Services");

            if (DataClass.TypeService.Count() <= 0) return Redirect("~/Page/Home/");

            if (DataClass.TypeService.Where(x => x.NameEn == nameType).Count() == 1) ViewBag.SelectedType = nameType;
            else ViewBag.SelectedType = DataClass.TypeService[0].NameEn;

            ViewBag.NamePage = "Services/" + ViewBag.SelectedType;
            return View();
        }

        public IActionResult Gallery()
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Page/ChangeLanguage?refererPage=Gallery");

            ViewBag.NamePage = "Gallery";
            return View();
        }

        public IActionResult FAQ()
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Page/ChangeLanguage?refererPage=FAQ");

            ViewBag.NamePage = "FAQ";
            return View();
        }

        [HttpGet]
        [Route("/Page/Contact/{selectedData?}")]
        public IActionResult Contact(string selectedData = "")
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Page/ChangeLanguage?refererPage=Contact");

            string[] partData = selectedData.Split("|");
            ViewBag.SelectedData = partData;

            ViewBag.NamePage = "Contact";
            return View();
        }

        public IActionResult ChangeLanguage(string refererPage)
        {
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
            {
                Response.Cookies.Delete("Language");
                CreateCookie("Language", "en", 7);

                return Redirect("~/Page/" + refererPage);
            }
            else
            {
                if (Request.Cookies["Language"] == "en")
                {
                    Response.Cookies.Delete("Language");
                    CreateCookie("Language", "ua", 7);
                }
                else
                {
                    Response.Cookies.Delete("Language");
                    CreateCookie("Language", "en", 7);
                }

                return Redirect("~/Page/" + refererPage);
            }
        }

        public IActionResult AcceptCookies(string refererPage)
        {
            CreateCookie("UseCookies", "true", 7);
            return Redirect("~/Page/" + refererPage);
        }

        public string AcceptCookiesWR()
        {
            try
            {
                CreateCookie("UseCookies", "true", 7);
                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [NonAction]
        public void CreateCookie(string key, string value, int expiresDay)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(expiresDay);
            Response.Cookies.Append(key, value, options);
        }

        public IActionResult Manager(string password)
        {
            if (User?.FindFirst("Access") != null)
            {
                if (User?.FindFirst("Access")?.Value == DataClass.password)
                {
                    ViewBag.NamePage = "Manager";
                    return View();
                }
                else
                {
                    HttpContext.SignOutAsync();
                    return Redirect("~/");
                }
            }
            else
            {
                if (password == DataClass.password)
                {
                    var claims = new List<Claim>() { new Claim("Access", DataClass.password) };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    HttpContext.SignInAsync(claimsPrincipal);
                    return Redirect("~/Page/Manager/");
                }
                else
                {
                    HttpContext.SignOutAsync();
                    return Redirect("~/");
                }
            }
        }

        [HttpGet]
        [Route("/Page/Service/{id?}")]
        public IActionResult ServiceDetails(string id)
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Page/ChangeLanguage?refererPage=Service");

            if (DataClass.Services.Count() <= 0) return Redirect("~/Page/Home/");

            bool result = int.TryParse(id, out int idService);
            if (!result || idService < 1) return Redirect("~/Page/Home/");

            if (DataClass.Services.Where(x => x.Id == idService).Count() == 1) ViewBag.IdService = idService;
            else return Redirect("~/Page/Home/");

            ViewBag.NamePage = "Service/" + ViewBag.IdService;
            return View();
        }

        [HttpPost]
        public async Task<string> SendEmail(string email)
        {
            try
            {
                if (Request.Cookies["UseCookies"] != "true") return "false 1";

                if (email == null || email == "") return "false 2";

                if (ServiceClass.IsValidEmail(email))
                {
                    ClearExpiredCodes();

                    if (!DataClass.StorageCode.ContainsKey(email))
                    {
                        Random ran = new Random();
                        int code = ran.Next(100000, 1000000);

                        string subject = "Confirmation code";
                        string body = "Your confirmation code for sending the contact form: ";

                        if (Request.Cookies["Language"] == "ua")
                        {
                            subject = "Код підтвердження";
                            body = "Ваш код підтвердження для надсилання контактної форми: ";
                        }
                        else
                        {
                            subject = "Confirmation code";
                            body = "Your confirmation code for sending the contact form: ";
                        }

                        bool result = false;

                        for (int i = 0; i < DataClass.Mailing.Count(); i++)
                        {
                            result = await ServiceClass.SendEmailAsync(DataClass.Mailing[i].email, email, subject, body + code.ToString(), DataClass.Mailing[i].key);
                            if (result) break;
                        }

                        if (!result)
                        {
                            ServiceClass.WriteLog("Не удалось отправить код подтверждения со всех почтовых аккаунтов!");
                            return "false 3";
                        }

                        DataClass.StorageCode.Add(email, (code.ToString(), DateTime.Now));
                    }

                    return "true";
                }
                else return "false 2";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false 0";
            }
        }

        [NonAction]
        public void ClearExpiredCodes()
        {
            try
            {
                foreach (KeyValuePair<string, (string, DateTime)> row in DataClass.StorageCode)
                {
                    if (row.Value.Item2.AddMinutes(5) < DateTime.Now)
                    {
                        DataClass.StorageCode.Remove(row.Key);
                    }
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
            }
        }

        [HttpPost]
        public async Task<string> SendContactForm(string codeEmail, string name, string phoneNumber, string email, string communication, string message, string idService, string idPackage)
        {
            try
            {
                if (codeEmail != null)
                {
                    codeEmail = codeEmail.Replace(" ", "");
                    if (codeEmail.Length <= 0 || codeEmail.Length > 20) return "false 1";
                }
                else return "false 1";

                if (email != null)
                {
                    email = email.Replace(" ", "");
                    if (email.Length <= 0 || email.Length > 50) return "false 1";
                }
                else return "false 1";

                ClearExpiredCodes();

                if (!DataClass.StorageCode.ContainsKey(email)) return "false 2";

                if (DataClass.StorageCode[email].Item1 == codeEmail)
                {
                    if (name != null)
                    {
                        if (name.Length <= 0 || name.Length > 50) return "false 1";
                    }
                    else return "false 1";

                    if (phoneNumber != null)
                    {
                        phoneNumber = phoneNumber.Replace(" ", "");
                        if (phoneNumber.Length <= 0 || phoneNumber.Length > 20) return "false 1";
                    }
                    else return "false 1";

                    if (communication != null)
                    {
                        if (communication.Length > 50) return "false 1";
                    }

                    if (message != null)
                    {
                        if (message.Length > 500) return "false 1";
                    }

                    bool result = int.TryParse(idService, out int serviceId);
                    if (!result) return "false 1";

                    int countContacts = await _dbContext.Contacts.Where(x => x.Date > DateTime.Now.AddHours(-1) && x.Email == email).CountAsync();
                    if (countContacts >= 5) return "false 4";

                    Service? service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceId);
                    if (service == null) return "false 1";

                    Contact contact = new Contact()
                    {
                        Name = name,
                        PhoneNumber = phoneNumber,
                        Email = email,
                        Communication = communication,
                        Message = message,
                        Date = DateTime.Now,
                        IdService = serviceId
                    };

                    StringBuilder body = new StringBuilder();

                    body.Append("Имя: " + name + "<br>");
                    body.Append("Телефон: " + phoneNumber + "<br>");
                    body.Append("Email: " + email + "<br>");
                    body.Append("Способ связи: " + communication + "<br>");
                    body.Append("Услуга: " + service.NameUa + "<br>");

                    int count = await _dbContext.PackageServices.Where(x => x.IdService == service.Id).CountAsync();
                    if (count > 0)
                    {
                        result = int.TryParse(idPackage, out int packageId);
                        if (!result) return "false 1";

                        PackageService? package = await _dbContext.PackageServices.FirstOrDefaultAsync(x => x.Id == packageId);
                        if (package == null) return "false 1";

                        contact.IdPackage = packageId;

                        body.Append("Пакет: " + package.Name + "<br>");
                        body.Append("Стоимость: " + package.PriceZloty + "<br>");
                    }
                    else body.Append("Стоимость: " + service.PriceZloty + "<br>");

                    if (message != null) body.Append("Сообщение: " + message);

                    await _dbContext.Contacts.AddAsync(contact);
                    await _dbContext.SaveChangesAsync();

                    DataClass.Contacts.Add(contact);
                    DataClass.StorageCode.Remove(email);

                    for (int i = 0; i < DataClass.Mailing.Count(); i++)
                    {
                        for (int j = 0; j < DataClass.EmailForNotifications.Count(); j++)
                        {
                            result = await ServiceClass.SendEmailAsync(DataClass.Mailing[i].email, DataClass.EmailForNotifications[j], "Новая заявка!", body.ToString(), DataClass.Mailing[i].key);
                        }

                        if (result) break;
                    }

                    if (!result) ServiceClass.WriteLog("Не удалось отправить уведомление о новой контактной форме со всех почтовых аккаунтов!");

                    ServiceClass.WriteLog("Новая контактная форма! Время: " + DateTime.Now + "\n");
                    return "true";
                }
                else return "false 3";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false 0";
            }
        }
    }
}
