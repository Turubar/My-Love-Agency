using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyLoveAgency.Models;
using MyLoveAgency.Models.Database;
using System.Security.Claims;
using System.IO;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Microsoft.Identity.Client.Extensions.Msal;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace MyLoveAgency.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Home()
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Main/ChangeLanguage?refererPage=Home");

            ViewBag.NamePage = "Home";
            return View();
        }

        [HttpGet]
        [Route("/Main/Services/{nameType?}")]
        public IActionResult Services(string nameType = "")
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Main/ChangeLanguage?refererPage=Services");

            if (DataClass.TypeService.Count() <= 0) return Redirect("~/Main/Home/");

            if (DataClass.TypeService.Where(x => x.NameEn == nameType).Count() == 1) ViewBag.SelectedType = nameType;
            else ViewBag.SelectedType = DataClass.TypeService[0].NameEn;

            ViewBag.NamePage = "Services/" + ViewBag.SelectedType;
            return View();
        }

        public IActionResult Gallery()
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Main/ChangeLanguage?refererPage=Gallery");

            ViewBag.NamePage = "Gallery";
            return View();
        }

        public IActionResult FAQ()
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Main/ChangeLanguage?refererPage=FAQ");

            ViewBag.NamePage = "FAQ";
            return View();
        }

        [HttpGet]
        [Route("/Main/Contact/{selectedData?}")]
        public IActionResult Contact(string selectedData = "")
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Main/ChangeLanguage?refererPage=Contact");

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

                return Redirect("~/Main/" + refererPage);
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

                return Redirect("~/Main/" + refererPage);
            }
        }

        public IActionResult AcceptCookies(string refererPage)
        {
            CreateCookie("UseCookies", "true", 7);
            return Redirect("~/Main/" + refererPage);
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
                    return Redirect("~/Main/Manager/");
                }
                else
                {
                    HttpContext.SignOutAsync();
                    return Redirect("~/");
                }
            }
        }

        [HttpGet]
        [Route("/Main/Service/{id?}")]
        public IActionResult ServiceDetails(string id)
        {
            // Проверяем валидность cookie "Language"
            if (Request.Cookies["Language"] != "en" && Request.Cookies["Language"] != "ua")
                return Redirect("~/Main/ChangeLanguage?refererPage=Service");

            if (DataClass.Services.Count() <= 0) return Redirect("~/Main/Home/");

            bool result = int.TryParse(id, out int idService);
            if (!result || idService < 1) return Redirect("~/Main/Home/");

            if (DataClass.Services.Where(x => x.Id == idService).Count() == 1) ViewBag.IdService = idService;
            else return Redirect("~/Main/Home/");

            ViewBag.NamePage = "Service/" + ViewBag.IdService;
            return View();
        }

        [HttpPost]
        public async Task<string> AddImageService(IFormFile imageServiceInput, string idService)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (imageServiceInput != null && imageServiceInput.Length > 0 && idService != null)
                {
                    bool result = int.TryParse(idService, out int id);
                    if (!result) return "false";

                    using (var context = new LovelyLoveDbContext())
                    {
                        Service? service = context.Services.FirstOrDefault(x => x.Id == id);
                        if (service == null) return "false";

                        var listImages = context.StorageImageServices.Where(x => x.IdService == service.Id).OrderBy(x => x.Number);

                        string[] part = imageServiceInput.FileName.Split(".");
                        string extensionFile = part[part.Length - 1];

                        Random ran = new Random();
                        int nameFile = -1;

                        while (nameFile == -1)
                        {
                            int numberRan = ran.Next(100000, 1000000);
                            string futureName = numberRan.ToString() + "." + extensionFile.ToString();

                            if (!System.IO.File.Exists(futureName)) nameFile = numberRan;
                        }

                        string path = nameFile.ToString() + "." + extensionFile;
                        int number = listImages.Count() + 1;

                        bool loaded = false;
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var fileStream = new FileStream("wwwroot/data/services/" + path, FileMode.Create))
                            {
                                await imageServiceInput.CopyToAsync(fileStream);
                                loaded = true;
                            }
                        }

                        if (!loaded) return "false";

                        StorageImageService sis = new StorageImageService()
                        {
                            Path = path,
                            Number = number,
                            IdService = service.Id
                        };

                        context.StorageImageServices.Add(sis);
                        context.SaveChanges();

                        DataClass.ServiceImages = context.StorageImageServices.ToList();

                        return path;
                    }
                }
                else return "false";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string DeleteImageService(string idService, string number)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (idService != null && idService != "" && number != null && number != "")
                {
                    bool resultId = int.TryParse(idService, out int id);
                    bool resultNumber = int.TryParse(number, out int num);

                    if (!resultId || !resultNumber) return "false";
                    if (id < 1 || num < 1) return "false";

                    using (var context = new LovelyLoveDbContext())
                    {
                        List<StorageImageService> imageList = context.StorageImageServices.Where(x => x.IdService == id).ToList();
                        if (imageList == null || imageList.Count() < 1 || imageList.Where(x => x.Number == num).Count() != 1) return "false";

                        StorageImageService image = imageList.First(x => x.Number == num);
                        string pathToImage = "wwwroot/data/services/" + image.Path;
                        imageList.Remove(image);

                        for (int i = 0; i < imageList.Count(); i++)
                        {
                            imageList[i].Number = i + 1;
                        }

                        context.StorageImageServices.Remove(image);
                        context.StorageImageServices.UpdateRange(imageList);
                        context.SaveChanges();

                        DataClass.ServiceImages = context.StorageImageServices.ToList();

                        if (System.IO.File.Exists(pathToImage)) System.IO.File.Delete(pathToImage);

                        return "true";
                    }
                }
                else return "false";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string AddPackageService(string idService, string name, string price, string durationEn, string durationUa, string descriptionEn, string descriptionUa)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (idService == null || idService == "" || name == null || name == "" || price == null || price == "") return "false";
                if (durationEn == null) durationEn = "";
                if (durationUa == null) durationUa = "";
                if (descriptionEn == null) descriptionEn = "";
                if (descriptionUa == null) descriptionUa = "";

                bool resultId = int.TryParse(idService, out int id);
                if (!resultId || id < 1 || name.Length < 1 || name.Length > 50 || price.Length < 1 || price.Length > 50) return "false";
                if (durationEn.Length > 50 || durationUa.Length > 50) return "false";

                using (var context = new LovelyLoveDbContext())
                {
                    Service service = context.Services.First(x => x.Id == id);
                    if (service == null) return "false";

                    List<PackageService> packageList = context.PackageServices.Where(x => x.IdService == service.Id).ToList();
                    if (packageList.Where(x => x.Name == name).Count() >= 1) return "false";

                    PackageService newPackage = new PackageService()
                    {
                        Name = name,
                        PriceZloty = price,
                        DurationEn = durationEn,
                        DurationUa = durationUa,
                        DescriptionEn = descriptionEn,
                        DescriptionUa = descriptionUa,
                        IdService = service.Id
                    };

                    context.PackageServices.Add(newPackage);
                    context.SaveChanges();
                    DataClass.Packages = context.PackageServices.ToList();

                    PackageService package = DataClass.Packages.First(x => x.IdService == service.Id && x.Name == name);
                    return "true" + package.Id.ToString();
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string UpdateServiceAndPackages(string idService, string idType, string nameEn, string nameUa, string price, string descriptionEn, string descriptionUa, List<List<string>> packages)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (idService == null || idService == "" || idType == null || idType == null || idType == "") return "false";
                if (nameEn == null || nameEn == "" || nameUa == null || nameUa == "" || price == null || price == "") return "false";

                bool resultId = int.TryParse(idService, out int id);
                bool resultIdType = int.TryParse(idType, out int type);

                if (!resultId || !resultIdType) return "false";
                if (nameEn.Length < 1 || nameEn.Length > 100 || nameUa.Length < 1 || nameUa.Length > 100 || price.Length < 1 || price.Length > 50) return "false";

                if (descriptionEn == null) descriptionEn = "";
                if (descriptionUa == null) descriptionUa = "";

                using (var context = new LovelyLoveDbContext())
                {
                    Service service = context.Services.First(x => x.Id == id);
                    if (service == null) return "false";

                    TypeService serviceType = context.TypeServices.First(x => x.Id == type);
                    if (serviceType == null) return "false";

                    service.NameEn = nameEn;
                    service.NameUa = nameUa;
                    service.PriceZloty = price;
                    service.DescriptionEn = descriptionEn;
                    service.DescriptionUa = descriptionUa;
                    service.IdType = type;

                    List<PackageService> packageList = new List<PackageService>();
                    if (packages.Count > 0 && packages != null)
                    {
                        for (int i = 0; i < packages.Count(); i++)
                        {
                            if (packages[i].Count() != 7) continue;

                            bool resultIdPackage = int.TryParse(packages[i][0], out int packageId);
                            if (!resultIdPackage) continue;

                            PackageService package = context.PackageServices.First(x => x.Id == packageId);
                            if (package == null || package.IdService != service.Id) continue;

                            if (packages[i][1] == null || packages[i][1] == "") continue;
                            if (packages[i][1].Length < 1 || packages[i][1].Length > 50) continue;

                            if (packages[i][2] == null || packages[i][2] == "") continue;
                            if (packages[i][2].Length < 1 || packages[i][2].Length > 50) continue;

                            if (packages[i][3] != null)
                            {
                                if (packages[i][3].Length > 50) continue;
                            }
                            else packages[i][3] = "";

                            if (packages[i][4] != null)
                            {
                                if (packages[i][4].Length > 50) continue;
                            }
                            else packages[i][4] = "";

                            if (packages[i][5] == null) packages[i][5] = "";
                            if (packages[i][6] == null) packages[i][6] = "";

                            package.Name = packages[i][1];
                            package.PriceZloty = packages[i][2];
                            package.DurationEn = packages[i][3];
                            package.DurationUa = packages[i][4];
                            package.DescriptionEn = packages[i][5];
                            package.DescriptionUa = packages[i][6];

                            packageList.Add(package);
                        }

                        for (int i = 0; i < packageList.Count(); i++)
                        {
                            if (packageList.Where(x => x.Name == packageList[i].Name).Count() > 1) return "false";
                        }
                    }

                    context.Services.Update(service);
                    if (packageList.Count() > 0) context.PackageServices.UpdateRange(packageList);
                    context.SaveChanges();

                    DataClass.Services = context.Services.ToList();
                    DataClass.Packages = context.PackageServices.ToList();

                    return "true";
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string DeletePackageService(string idService, string name)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (idService == null || name == null) return "false";

                bool resultId = int.TryParse(idService, out int id);
                if (!resultId) return "false";

                using (var context = new LovelyLoveDbContext())
                {
                    Service service = context.Services.First(x => x.Id == id);
                    if (service == null) return "false";

                    PackageService package = context.PackageServices.First(x => x.IdService == service.Id && x.Name == name);
                    if (package == null) return "false";
                    int idPackage = package.Id;

                    context.PackageServices.Remove(package);
                    context.SaveChanges();

                    DataClass.Packages = context.PackageServices.ToList();
                    return "true" + idPackage.ToString();
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string DeleteService(string idService)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (idService == null) return "false";

                bool resultId = int.TryParse(idService, out int id);
                if (!resultId || id < 1) return "false";

                using (var context = new LovelyLoveDbContext())
                {
                    Service service = context.Services.First(x => x.Id == id);
                    if (service == null) return "false";

                    List<PackageService> packageList = context.PackageServices.Where(x => x.IdService == id).ToList();
                    if (packageList.Count >= 1) context.PackageServices.RemoveRange(packageList);

                    List<StorageImageService> imageList = context.StorageImageServices.Where(x => x.IdService == id).ToList();
                    if (imageList.Count >= 1)
                    {
                        context.StorageImageServices.RemoveRange(imageList);

                        foreach (var image in imageList)
                        {
                            string imagePath = "wwwroot/data/services/" + image.Path;
                            if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
                        }
                    }

                    context.Services.Remove(service);
                    context.SaveChanges();

                    DataClass.Services = context.Services.ToList();
                    if (packageList.Count() >= 1) DataClass.Packages = context.PackageServices.ToList();
                    if (imageList.Count() >= 1) DataClass.ServiceImages = context.StorageImageServices.ToList();

                    return "true";
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string AddService(string nameEn, string nameUa, string price, string idType, string descriptionEn, string descriptionUa)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (nameEn == null || nameUa == null || price == null || idType == null) return "false";
                if (nameEn.Length < 1 || nameEn.Length > 100 || nameUa.Length < 1 || nameUa.Length > 100 || price.Length < 1 || price.Length > 100) return "false";

                bool resultIdType = int.TryParse(idType, out int type);
                if (!resultIdType) return "false";

                if (descriptionEn == null) descriptionEn = "";
                if (descriptionUa == null) descriptionUa = "";

                using (var context = new LovelyLoveDbContext())
                {
                    TypeService typeService = context.TypeServices.First(x => x.Id == type);
                    if (typeService == null) return "false";

                    Service service = new Service
                    {
                        NameEn = nameEn,
                        NameUa = nameUa,
                        DescriptionEn = descriptionEn,
                        DescriptionUa = descriptionUa,
                        PriceZloty = price,
                        IdType = typeService.Id
                    };

                    context.Services.Add(service);
                    context.SaveChanges();

                    DataClass.Services = context.Services.ToList();
                    return "true";
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        public async Task<List<StorageImageGallery>?> AddImageGallery(IFormFile imageGalleryInput)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return null;

                if (imageGalleryInput != null && imageGalleryInput.Length > 0)
                {
                    using (var context = new LovelyLoveDbContext())
                    {
                        string[] part = imageGalleryInput.FileName.Split(".");
                        string extensionFile = part[part.Length - 1];

                        Random ran = new Random();
                        int nameFile = -1;

                        while (nameFile == -1)
                        {
                            int numberRan = ran.Next(100000, 1000000);
                            string futureName = numberRan.ToString() + "." + extensionFile.ToString();

                            if (!System.IO.File.Exists(futureName)) nameFile = numberRan;
                        }

                        string path = nameFile.ToString() + "." + extensionFile;
                        int number = context.StorageImageGalleries.Count() + 1;

                        bool loaded = false;
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var fileStream = new FileStream("wwwroot/data/gallery/" + path, FileMode.Create))
                            {
                                await imageGalleryInput.CopyToAsync(fileStream);
                                loaded = true;
                            }
                        }

                        if (!loaded) return null;

                        StorageImageGallery imageGallery = new StorageImageGallery()
                        {
                            Path = path,
                            Number = number
                        };

                        context.StorageImageGalleries.Add(imageGallery);
                        context.SaveChanges();

                        DataClass.GalleryImages = context.StorageImageGalleries.ToList();

                        int id = DataClass.GalleryImages.First(x => x.Path == path).Id;

                        bool correctNumber = true;
                        for (int i = 0; i < DataClass.GalleryImages.Count(); i++)
                        {
                            if (DataClass.GalleryImages[i].Number != i + 1) correctNumber = false;
                        }

                        if (!correctNumber)
                        {
                            List<StorageImageGallery> gallery = context.StorageImageGalleries.ToList();
                            for (int i = 0; i < gallery.Count(); i++)
                            {
                                gallery[i].Number = i + 1;
                            }

                            context.StorageImageGalleries.AddRange(gallery);
                            context.SaveChanges();
                            DataClass.GalleryImages = context.StorageImageGalleries.ToList();
                            number = DataClass.GalleryImages.First(x => x.Path == path).Number;
                        }

                        return DataClass.GalleryImages;
                    }
                }
                else return null;
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return null;
            }
        }

        [HttpPost]
        public List<StorageImageGallery>? DeleteImageGallery(string idImage)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return null;

                if (idImage != null && idImage != "")
                {
                    bool resultId = int.TryParse(idImage, out int id);
                    if (!resultId || id < 1) return null;

                    using (var context = new LovelyLoveDbContext())
                    {
                        StorageImageGallery image = context.StorageImageGalleries.First(x => x.Id == id);
                        if (image == null) return null;

                        string pathToImage = "wwwroot/data/gallery/" + image.Path;

                        context.StorageImageGalleries.Remove(image);
                        context.SaveChanges();

                        List<StorageImageGallery> gallery = context.StorageImageGalleries.ToList();
                        for (int i = 0; i < gallery.Count(); i++)
                        {
                            gallery[i].Number = i + 1;
                        }

                        context.StorageImageGalleries.UpdateRange(gallery);
                        context.SaveChanges();

                        DataClass.GalleryImages = context.StorageImageGalleries.ToList();
                        if (System.IO.File.Exists(pathToImage)) System.IO.File.Delete(pathToImage);

                        return DataClass.GalleryImages;
                    }
                }
                else return null;
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return null;
            }
        }

        [HttpPost]
        public List<StorageImageGallery>? ChangeImageGallery(string oldId, string newId)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return null;

                if (oldId == null || oldId == "" || newId == null || newId == "") return null;

                bool resultOldId = int.TryParse(oldId, out int idOld);
                bool resultNewId = int.TryParse(newId, out int idNew);

                if (!resultOldId || !resultNewId) return null;
                if (idOld < 1 || idNew < 1) return null;

                using (var context = new LovelyLoveDbContext())
                {
                    StorageImageGallery oldGallery = context.StorageImageGalleries.First(x => x.Id == idOld);
                    StorageImageGallery newGallery = context.StorageImageGalleries.First(x => x.Id == idNew);
                    if (oldGallery == null || newGallery == null) return null;

                    string oldPath = oldGallery.Path;
                    string newPath = newGallery.Path;

                    oldGallery.Path = newPath;
                    newGallery.Path = oldPath;

                    context.StorageImageGalleries.UpdateRange(oldGallery, newGallery);
                    context.SaveChanges();

                    DataClass.GalleryImages = context.StorageImageGalleries.ToList();
                    return DataClass.GalleryImages;
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return null;
            }
        }

        [HttpPost]
        public string UpdateFAQ(string id, string questionEn, string questionUa, string answerEn, string answerUa)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (id == null || id == "" || questionEn == null || questionEn == "" || questionUa == null || questionUa == "") return "false";
                if (answerEn == null || answerEn == "" || answerUa == null || answerUa == "") return "false";
                if (questionEn.Length < 1 || questionUa.Length < 1 || answerEn.Length < 1 || answerUa.Length < 1) return "false";

                bool resultId = int.TryParse(id, out int faqId);
                if (!resultId || faqId < 1) return "false";

                using (var context = new LovelyLoveDbContext())
                {
                    Faq faq = context.Faqs.First(x => x.Id == faqId);
                    if (faq == null) return "false";

                    faq.QuestionEn = questionEn;
                    faq.QuestionUa = questionUa;
                    faq.AnswerEn = answerEn;
                    faq.AnswerUa = answerUa;

                    context.Faqs.Update(faq);
                    context.SaveChanges();

                    DataClass.FAQ = context.Faqs.ToList();
                    return "true";
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string DeleteFAQ(string id)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (id == null || id == "") return "false";

                bool resultId = int.TryParse(id, out int faqId);
                if (!resultId || faqId < 1) return "false";

                using (var context = new LovelyLoveDbContext())
                {
                    Faq faq = context.Faqs.First(x => x.Id == faqId);
                    if (faq == null) return "false";

                    context.Faqs.Remove(faq);
                    context.SaveChanges();

                    DataClass.FAQ = context.Faqs.ToList();
                    return "true";
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string AddFAQ(string questionEn, string questionUa, string answerEn, string answerUa)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (questionEn == null || questionEn == "" || questionUa == null || questionUa == "") return "false";
                if (answerEn == null || answerEn == "" || answerUa == null || answerUa == "") return "false";
                if (questionEn.Length < 1 || questionUa.Length < 1 || answerEn.Length < 1 || answerUa.Length < 1) return "false";

                using (var context = new LovelyLoveDbContext())
                {
                    Faq newFaq = new Faq()
                    {
                        QuestionEn = questionEn,
                        QuestionUa = questionUa,
                        AnswerEn = answerEn,
                        AnswerUa = answerUa
                    };

                    context.Faqs.Add(newFaq);
                    context.SaveChanges();

                    DataClass.FAQ = context.Faqs.ToList();

                    int idFaq = DataClass.FAQ.First(x => x.QuestionEn == questionEn && x.QuestionUa == questionUa && x.AnswerEn == answerEn && x.AnswerUa == answerUa).Id;
                    return "true:" + idFaq;
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
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

                    using (var context = new LovelyLoveDbContext())
                    {
                        int countContacts = context.Contacts.Where(x => x.Date > DateTime.Now.AddHours(-1) && x.Email == email).Count();
                        if (countContacts >= 5) return "false 4";

                        Service service = context.Services.First(x => x.Id == serviceId);
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

                        int count = context.PackageServices.Where(x => x.IdService == service.Id).Count();
                        if (count > 0)
                        {
                            result = int.TryParse(idPackage, out int packageId);
                            if (!result) return "false 1";

                            PackageService package = context.PackageServices.First(x => x.Id == packageId);
                            if (package == null) return "false 1";

                            contact.IdPackage = packageId;

                            body.Append("Пакет: " + package.Name + "<br>");
                            body.Append("Стоимость: " + package.PriceZloty + "<br>");
                        }
                        else body.Append("Стоимость: " + service.PriceZloty + "<br>");

                        if (message != null) body.Append("Сообщение: " + message);

                        context.Contacts.Add(contact);
                        context.SaveChanges();

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
                }
                else return "false 3";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false 0";
            }
        }

        [HttpPost]
        public string DeleteContact(string id)
        {
            try
            {
                if (id == null) return "false";
                bool result = int.TryParse(id, out int contactId);
                if (!result) return "false";

                using (var context = new LovelyLoveDbContext())
                {
                    Contact? contact = DataClass.Contacts.Find(x => x.Id == contactId);
                    if (contact == null) return "false";

                    context.Contacts.Remove(contact);
                    context.SaveChanges();

                    DataClass.Contacts.Remove(contact);
                    return "true";
                }
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public string ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                if (oldPassword == null || newPassword == null) return "false";
                if (oldPassword != DataClass.password) return "false";
                if (newPassword.Length < 10 || newPassword.Length > 32) return "false";

                System.IO.File.WriteAllText(DataClass.pathToPasswordFile, newPassword);
                DataClass.password = newPassword;

                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }
    }
}
