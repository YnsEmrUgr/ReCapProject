using Core.Utilities.Business;
using Core.Utilities.Results;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;

namespace Core.Utilities.Helpers
{
    public class FileHelperManager : IFileHelper
    {
        public IResult Delete(string filePath)
        {
            var result = CheckIfFileExist(filePath);
            if (!result.Success)
            {
                return result;
            }
            File.Delete(filePath);
            return new SuccessResult();
        }

        public IResult Update(IFormFile formFile, string filePath, string root)
        {
            var resultToDelete = Delete(filePath);
            if (!resultToDelete.Success)
            {
                return resultToDelete;
            }

            var resultToUpdate = Upload(formFile, root);
            if (!resultToUpdate.Success)
            {
                return resultToUpdate;
            }
            return new SuccessResult();
        }

        public IResult Upload(IFormFile formFile, string root)
        {
            var result = BusinessRules.Run(CheckIfFileEnter(formFile),CheckIfFileExtensionValid(Path.GetExtension(formFile.FileName)));
            if (result != null)
            {
                return result;
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);

            CreateFile(root + fileName,formFile);
            CheckIfDirectoryExist(root);
            return new SuccessResult(fileName);
        }




        //----------------------------------------------------------------------------------

        private IResult CheckIfFileExist(string filePath)
        {
            if (File.Exists(filePath))
            {
                return new SuccessResult();
            }
            return new ErrorResult("Böyle bir dosya mevcut değil");
        }

        private IResult CheckIfFileEnter(IFormFile fromFile)
        {
            if (fromFile.Length < 0)
            {
                return new ErrorResult("Dosya girilmemiş");
            }
            return new SuccessResult();
        }

        private IResult CheckIfFileExtensionValid(string extension)
        {
            if (extension == ".jpg" || extension == ".png" || extension == ".jpeg" || extension == ".webp")
            {
                return new SuccessResult();
            }
            return new ErrorResult("Dosya uzantısı geçerli değil");
        }

        private void CheckIfDirectoryExist(string root)
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
        }

        private void CreateFile(string directory,IFormFile formFile)
        {
            using (FileStream fileStream = File.Create(directory))
            {
                if (formFile.Length > 0)
                {
                    formFile.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }
        }
    }
}
