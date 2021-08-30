using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PocUpload
{
    public interface IManageFileRepository
    {
        Task CreateChunk(int id, string fileName, Stream stream);

        void CreateFile(string fileName);

        void RemoveOldFiles();
    }
}
