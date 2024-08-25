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
    }
}
