using Microsoft.AspNetCore.Mvc;
using MyLoveAgency.Models.Database;
using MyLoveAgency.Models;
using Microsoft.EntityFrameworkCore;

namespace MyLoveAgency.Controllers
{
    public class APIController : Controller
    {
        private LovelyLoveDbContext _dbContext;

        public APIController(LovelyLoveDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Добавление/обновление/удаление услуг и пакетов

        [HttpPost]
        public async Task<string> AddService(string nameEn, string nameUa, string price, string idType, string descriptionEn, string descriptionUa)
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

                TypeService? typeService = await _dbContext.TypeServices.FirstOrDefaultAsync(x => x.Id == type);
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

                await _dbContext.Services.AddAsync(service);
                await _dbContext.SaveChangesAsync();

                DataClass.Services = await _dbContext.Services.ToListAsync();
                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public async Task<string> AddPackageService(string idService, string name, string price, string durationEn, string durationUa, string descriptionEn, string descriptionUa)
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

                Service service = await _dbContext.Services.FirstAsync(x => x.Id == id);
                if (service == null) return "false";

                List<PackageService> packageList = await _dbContext.PackageServices.Where(x => x.IdService == service.Id).ToListAsync();
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

                await _dbContext.PackageServices.AddAsync(newPackage);
                await _dbContext.SaveChangesAsync();
                DataClass.Packages = await _dbContext.PackageServices.ToListAsync();

                PackageService package = DataClass.Packages.First(x => x.IdService == service.Id && x.Name == name);
                return "true" + package.Id.ToString();
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public async Task<string> UpdateServiceAndPackages(string idService, string idType, string nameEn, string nameUa, string price, string descriptionEn, string descriptionUa, List<List<string>> packages)
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

                Service? service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);
                if (service == null) return "false";

                TypeService? serviceType = await _dbContext.TypeServices.FirstOrDefaultAsync(x => x.Id == type);
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

                        PackageService? package = await _dbContext.PackageServices.FirstOrDefaultAsync(x => x.Id == packageId);
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

                _dbContext.Services.Update(service);
                if (packageList.Count() > 0) _dbContext.PackageServices.UpdateRange(packageList);
                await _dbContext.SaveChangesAsync();

                DataClass.Services = await _dbContext.Services.ToListAsync();
                DataClass.Packages = await _dbContext.PackageServices.ToListAsync();

                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public async Task<string> DeleteService(string idService)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (idService == null) return "false";

                bool resultId = int.TryParse(idService, out int id);
                if (!resultId || id < 1) return "false";

                Service? service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);
                if (service == null) return "false";

                List<PackageService> packageList = await _dbContext.PackageServices.Where(x => x.IdService == id).ToListAsync();
                if (packageList.Count >= 1) _dbContext.PackageServices.RemoveRange(packageList);

                List<StorageImageService> imageList = await _dbContext.StorageImageServices.Where(x => x.IdService == id).ToListAsync();
                if (imageList.Count >= 1)
                {
                    _dbContext.StorageImageServices.RemoveRange(imageList);

                    foreach (var image in imageList)
                    {
                        string imagePath = "wwwroot/data/services/" + image.Path;
                        if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
                    }
                }

                _dbContext.Services.Remove(service);
                await _dbContext.SaveChangesAsync();

                DataClass.Services = await _dbContext.Services.ToListAsync();
                if (packageList.Count() >= 1) DataClass.Packages = await _dbContext.PackageServices.ToListAsync();
                if (imageList.Count() >= 1) DataClass.ServiceImages = await _dbContext.StorageImageServices.ToListAsync();

                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public async Task<string> DeletePackageService(string idService, string name)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (idService == null || name == null) return "false";

                bool resultId = int.TryParse(idService, out int id);
                if (!resultId) return "false";

                Service? service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);
                if (service == null) return "false";

                PackageService? package = await _dbContext.PackageServices.FirstOrDefaultAsync(x => x.IdService == service.Id && x.Name == name);
                if (package == null) return "false";
                int idPackage = package.Id;

                _dbContext.PackageServices.Remove(package);
                await _dbContext.SaveChangesAsync();

                DataClass.Packages = await _dbContext.PackageServices.ToListAsync();
                return "true" + idPackage.ToString();
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        #region Добавление/удаление изображений для услуг

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

                    Service? service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);
                    if (service == null) return "false";

                    var listImages = await _dbContext.StorageImageServices.Where(x => x.IdService == service.Id).OrderBy(x => x.Number).ToListAsync();

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

                    await _dbContext.StorageImageServices.AddAsync(sis);
                    await _dbContext.SaveChangesAsync();

                    DataClass.ServiceImages = await _dbContext.StorageImageServices.ToListAsync();

                    return path;
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
        public async Task<string> DeleteImageService(string idService, string number)
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

                    List<StorageImageService> imageList = await _dbContext.StorageImageServices.Where(x => x.IdService == id).ToListAsync();
                    if (imageList == null || imageList.Count() < 1 || imageList.Where(x => x.Number == num).Count() != 1) return "false";

                    StorageImageService image = imageList.First(x => x.Number == num);
                    string pathToImage = "wwwroot/data/services/" + image.Path;
                    imageList.Remove(image);

                    for (int i = 0; i < imageList.Count(); i++)
                    {
                        imageList[i].Number = i + 1;
                    }

                    _dbContext.StorageImageServices.Remove(image);
                    _dbContext.StorageImageServices.UpdateRange(imageList);
                    await _dbContext.SaveChangesAsync();

                    DataClass.ServiceImages = await _dbContext.StorageImageServices.ToListAsync();

                    if (System.IO.File.Exists(pathToImage)) System.IO.File.Delete(pathToImage);

                    return "true";
                }
                else return "false";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        #endregion

        #endregion

        #region Добавление/обновление/удаление галереи

        public async Task<List<StorageImageGallery>?> AddImageGallery(IFormFile imageGalleryInput)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return null;

                if (imageGalleryInput != null && imageGalleryInput.Length > 0)
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
                    int number = await _dbContext.StorageImageGalleries.CountAsync() + 1;

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

                    await _dbContext.StorageImageGalleries.AddAsync(imageGallery);
                    await _dbContext.SaveChangesAsync();

                    DataClass.GalleryImages = await _dbContext.StorageImageGalleries.ToListAsync();

                    //int id = DataClass.GalleryImages.First(x => x.Path == path).Id;

                    bool correctNumber = true;
                    for (int i = 0; i < DataClass.GalleryImages.Count(); i++)
                    {
                        if (DataClass.GalleryImages[i].Number != i + 1) correctNumber = false;
                    }

                    if (!correctNumber)
                    {
                        List<StorageImageGallery> gallery = await _dbContext.StorageImageGalleries.ToListAsync();
                        for (int i = 0; i < gallery.Count(); i++)
                        {
                            gallery[i].Number = i + 1;
                        }

                        await _dbContext.StorageImageGalleries.AddRangeAsync(gallery);
                        await _dbContext.SaveChangesAsync();

                        DataClass.GalleryImages = await _dbContext.StorageImageGalleries.ToListAsync();
                        //number = DataClass.GalleryImages.First(x => x.Path == path).Number;
                    }

                    return DataClass.GalleryImages;
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
        public async Task<List<StorageImageGallery>?> ChangeImageGallery(string oldId, string newId)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return null;

                if (oldId == null || oldId == "" || newId == null || newId == "") return null;

                bool resultOldId = int.TryParse(oldId, out int idOld);
                bool resultNewId = int.TryParse(newId, out int idNew);

                if (!resultOldId || !resultNewId) return null;
                if (idOld < 1 || idNew < 1) return null;

                StorageImageGallery? oldGallery = await _dbContext.StorageImageGalleries.FirstOrDefaultAsync(x => x.Id == idOld);
                StorageImageGallery? newGallery = await _dbContext.StorageImageGalleries.FirstOrDefaultAsync(x => x.Id == idNew);
                if (oldGallery == null || newGallery == null) return null;

                string oldPath = oldGallery.Path;
                string newPath = newGallery.Path;

                oldGallery.Path = newPath;
                newGallery.Path = oldPath;

                _dbContext.StorageImageGalleries.UpdateRange(oldGallery, newGallery);
                await _dbContext.SaveChangesAsync();

                DataClass.GalleryImages = await _dbContext.StorageImageGalleries.ToListAsync();
                return DataClass.GalleryImages;
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return null;
            }
        }

        [HttpPost]
        public async Task<List<StorageImageGallery>?> DeleteImageGallery(string idImage)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return null;

                if (idImage != null && idImage != "")
                {
                    bool resultId = int.TryParse(idImage, out int id);
                    if (!resultId || id < 1) return null;

                    StorageImageGallery? image = await _dbContext.StorageImageGalleries.FirstOrDefaultAsync(x => x.Id == id);
                    if (image == null) return null;

                    string pathToImage = "wwwroot/data/gallery/" + image.Path;

                    _dbContext.StorageImageGalleries.Remove(image);
                    await _dbContext.SaveChangesAsync();

                    List<StorageImageGallery> gallery = await _dbContext.StorageImageGalleries.ToListAsync();
                    for (int i = 0; i < gallery.Count(); i++)
                    {
                        gallery[i].Number = i + 1;
                    }

                    _dbContext.StorageImageGalleries.UpdateRange(gallery);
                    await _dbContext.SaveChangesAsync();

                    DataClass.GalleryImages = await _dbContext.StorageImageGalleries.ToListAsync();
                    if (System.IO.File.Exists(pathToImage)) System.IO.File.Delete(pathToImage);

                    return DataClass.GalleryImages;
                }
                else return null;
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return null;
            }
        }

        #endregion

        #region Добавление/обновление/удаление FAQ

        [HttpPost]
        public async Task<string> AddFAQ(string questionEn, string questionUa, string answerEn, string answerUa)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (questionEn == null || questionEn == "" || questionUa == null || questionUa == "") return "false";
                if (answerEn == null || answerEn == "" || answerUa == null || answerUa == "") return "false";
                if (questionEn.Length < 1 || questionUa.Length < 1 || answerEn.Length < 1 || answerUa.Length < 1) return "false";

                Faq newFaq = new Faq()
                {
                    QuestionEn = questionEn,
                    QuestionUa = questionUa,
                    AnswerEn = answerEn,
                    AnswerUa = answerUa
                };

                await _dbContext.Faqs.AddAsync(newFaq);
                await _dbContext.SaveChangesAsync();

                DataClass.FAQ = await _dbContext.Faqs.ToListAsync();

                int idFaq = DataClass.FAQ.First(x => x.QuestionEn == questionEn && x.QuestionUa == questionUa && x.AnswerEn == answerEn && x.AnswerUa == answerUa).Id;
                return "true:" + idFaq;
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public async Task<string> UpdateFAQ(string id, string questionEn, string questionUa, string answerEn, string answerUa)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (id == null || id == "" || questionEn == null || questionEn == "" || questionUa == null || questionUa == "") return "false";
                if (answerEn == null || answerEn == "" || answerUa == null || answerUa == "") return "false";
                if (questionEn.Length < 1 || questionUa.Length < 1 || answerEn.Length < 1 || answerUa.Length < 1) return "false";

                bool resultId = int.TryParse(id, out int faqId);
                if (!resultId || faqId < 1) return "false";

                Faq? faq = await _dbContext.Faqs.FirstOrDefaultAsync(x => x.Id == faqId);
                if (faq == null) return "false";

                faq.QuestionEn = questionEn;
                faq.QuestionUa = questionUa;
                faq.AnswerEn = answerEn;
                faq.AnswerUa = answerUa;

                _dbContext.Faqs.Update(faq);
                await _dbContext.SaveChangesAsync();

                DataClass.FAQ = await _dbContext.Faqs.ToListAsync();
                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        [HttpPost]
        public async Task<string> DeleteFAQ(string id)
        {
            try
            {
                if (User?.FindFirst("Access")?.Value != DataClass.password) return "false";

                if (id == null || id == "") return "false";

                bool resultId = int.TryParse(id, out int faqId);
                if (!resultId || faqId < 1) return "false";

                Faq? faq = await _dbContext.Faqs.FirstOrDefaultAsync(x => x.Id == faqId);
                if (faq == null) return "false";

                _dbContext.Faqs.Remove(faq);
                await _dbContext.SaveChangesAsync();

                DataClass.FAQ = await _dbContext.Faqs.ToListAsync();
                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        #endregion

        #region Удаление заявок

        [HttpPost]
        public async Task<string> DeleteContact(string id)
        {
            try
            {
                if (id == null) return "false";
                bool result = int.TryParse(id, out int contactId);
                if (!result) return "false";

                Contact? contact = DataClass.Contacts.FirstOrDefault(x => x.Id == contactId);
                if (contact == null) return "false";

                _dbContext.Contacts.Remove(contact);
                await _dbContext.SaveChangesAsync();

                DataClass.Contacts.Remove(contact);
                return "true";
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Что-то пошло не так! Время: " + DateTime.Now + "\r\n" + e + "\r\n");
                return "false";
            }
        }

        #endregion

        #region Изменить пароль

        [HttpPost]
        public async Task<string> ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                if (oldPassword == null || newPassword == null) return "false";
                if (oldPassword != DataClass.password) return "false";
                if (newPassword.Length <= 10 || newPassword.Length > 32) return "false";

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

        #endregion
    }
}
