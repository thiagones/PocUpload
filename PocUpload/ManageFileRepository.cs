using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PocUpload
{
    public class ManageFileRepository : IManageFileRepository
    {
        private readonly ILogger<ManageFileRepository> _logger;
        private readonly int _chunkSize;
        private readonly string _path;
        private readonly string _tempPath;
        private readonly int _fileAgeLimit;

        public ManageFileRepository(ILogger<ManageFileRepository> logger)
        {
            _logger = logger;
            _chunkSize = 900000;
            _fileAgeLimit = -2;
            _path = Path.Combine(Directory.GetCurrentDirectory(), "Files");
            _tempPath = Path.Combine(_path, "Temp");
            
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
        }


        public async Task CreateChunk(int id, string fileName, Stream stream)
        {
            string newpath = Path.Combine(_tempPath, $"{fileName}{id}");
            using FileStream fs = File.Create(newpath);
            byte[] bytes = new byte[_chunkSize];
            int bytesRead = 0;
            while ((bytesRead = await stream.ReadAsync(bytes, 0, bytes.Length)) > 0)
            {
                fs.Write(bytes, 0, bytesRead);
            }
        }

        public void CreateFile(string fileName)
        {
            string newPath = Path.Combine(_tempPath, fileName);
            string[] filePaths = Directory.GetFiles(_tempPath)
                .Where(p => p.Contains(fileName))
                .OrderBy(p => int.Parse(p.Replace(fileName, "$").Split('$')[1]))
                .ToArray();

            foreach (string filePath in filePaths)
            {
                MergeChunks(newPath, filePath);
            }
            
            File.Move(Path.Combine(_tempPath, fileName), Path.Combine(_path, fileName));
        }

        public void RemoveOldFiles()
        {
            string[] filePaths = Directory.GetFiles(_tempPath)
                .ToArray();

            foreach (var file in filePaths)
            {
                var creationTime = File.GetCreationTime(file);
                if (creationTime < DateTime.Now.AddMinutes(_fileAgeLimit))
                {
                    File.Delete(file);
                }
            }
        }

        private void MergeChunks(string chunk1, string chunk2)
        {
            FileStream fs1 = null;
            FileStream fs2 = null;
            try
            {
                fs1 = File.Open(chunk1, FileMode.Append);
                fs2 = File.Open(chunk2, FileMode.Open);
                byte[] fs2Content = new byte[fs2.Length];
                fs2.Read(fs2Content, 0, (int)fs2.Length);
                fs1.Write(fs2Content, 0, (int)fs2.Length);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message + " : " + ex.StackTrace);
            }
            finally
            {
                if (fs1 != null) fs1.Close();
                if (fs2 != null) fs2.Close();
                File.Delete(chunk2);
            }
        }
    }
}
