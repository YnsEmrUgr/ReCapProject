using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using System.IO;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Helpers;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using DataAccess.Concrete.EntityFramework;
using Core.Utilities.Business;

namespace Business.Concrete
{
    public class CarImageManager : ICarImageService
    {
        ICarImageDal _imageDal;
        IFileHelper _fileHelper;

        public CarImageManager(ICarImageDal imageDal,IFileHelper fileHelper)
        {
            _imageDal = imageDal;
            _fileHelper = fileHelper;
        }

        //------------------------------------------------------------

        public IResult Add(IFormFile file, CarImage carImage)
        {
            var result = BusinessRules.Run(CheckIfCarImageCountOfCarCorret(carImage.CarId));
            if (result != null)
            {
                return result;
            }

            carImage.ImagePath = _fileHelper.Upload(file, PathConstants.ImagePath).ToString();
            carImage.Date = DateTime.Now;
            _imageDal.Add(carImage);
            return new SuccessResult("Dosya yuklendi");
        }

        public IResult Delete(CarImage carImage)
        {
            var result = _fileHelper.Delete(PathConstants.ImagePath + carImage.ImagePath);
            if (!result.Success)
            {
                return result;
            }
            _imageDal.Delete(carImage);
            return new SuccessResult("Dosya silindi");
        }

        public IResult Update(IFormFile file, CarImage carImage)
        {
            var result = _fileHelper.Update(file, PathConstants.ImagePath + carImage.ImagePath, PathConstants.ImagePath);
            _imageDal.Update(carImage);
            return new SuccessResult("Dosya guncellendi");
        }


        //-------------------------------------------------------------

        public IDataResult<List<CarImage>> GetAll()
        {
            return new SuccessDataResult<List<CarImage>>(_imageDal.GetAll(), "Dosyalar Listelendi");
        }

        public IDataResult<List<CarImage>> GetByCarId(int carId)
        {
            var result = BusinessRules.Run(CheckIfCarImageExists(carId));

            if (result != null)
            {
                return new ErrorDataResult<List<CarImage>>(GetDefaultImage(carId).Data);
            }
            return new SuccessDataResult<List<CarImage>>(_imageDal.GetAll(c => c.CarId == carId));
        }

        public IDataResult<CarImage> GetByImageId(int imageId)
        {
            return new SuccessDataResult<CarImage>(_imageDal.Get(c => c.Id == imageId));
        }


        //-------------------------RULES-----------------------------//


        private IResult CheckIfCarImageCountOfCarCorret(int carId)
        {
            var result = _imageDal.GetAll(c => c.CarId == carId).Count();
            if (result >= 5)
            {
                return new ErrorResult(Messages.CarImageCountExceeded);
            }
            return new SuccessResult();
        }

        private IResult CheckIfCarImageExists(int carId)
        {
            var result = _imageDal.GetAll(c => c.CarId == carId).Count();

            if (result > 0)
            {
                return new SuccessResult();
            }
            return new ErrorResult();
        }
        private IDataResult<List<CarImage>> GetDefaultImage(int carId)
        {
            List<CarImage> carImages = new List<CarImage>();

            carImages.Add(new CarImage { CarId = carId, Date = DateTime.Now, ImagePath = "wwwroot\\Images\\Default\\default.jpg}" });
            return new SuccessDataResult<List<CarImage>>(carImages);
        }

    }
}
