namespace Mistria.API.Helpers
{
    public class DocumentSettings
    {
        public static string UploadFile(IFormFile file, string folderName)
        {
            //1. File Location Path
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", folderName);

            //2. Get File Name and make it Unique
            var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";

            //3. Get File Path
            var filePath = Path.Combine(folderPath, fileName);

            //4. Use File Stream to make a copy
            using var fileStream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(fileStream);

            return $"/Images/{folderName}/{fileName}";

        }

        public static bool DeleteFile(string fileUrl, string folderName)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", folderName);

            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطأ أثناء حذف الملف: {ex.Message}");
                    return false;
                }
            }

            Console.WriteLine($"الملف غير موجود: {filePath}");
            return false;
        }
    }
}
