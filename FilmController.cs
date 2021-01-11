using log4net;
using MovieMe.Models;
using MovieMe.Repository;
using MovieMe.Services;
using MovieMe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MovieMe.Logger;


namespace MovieMe.Controllers
{
    public class FilmController : Controller
    {
        private readonly IFilmService db;
        public FilmController(IFilmService _db)
        {
            db = _db;
        }
        [HttpGet]
        public async Task<ActionResult> Add()
        {
            ViewBag.Producers = await db.IncludeProducersDropdown();
            ViewBag.Actors = await db.IncludeActorsDropdown();
            ViewBag.Directors = await db.IncludeDirectorsDropdown();
            ViewBag.Genres = await db.IncludeGenresDropdown();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult>Add(FilmAddViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var newFilm = await db.MapDataAddAsync(viewModel);
                db.Create(newFilm);
                await db.SaveAsync();
                return Json(new { success = true, message = "Added Successfully" });
            }
            var validationErrors = GetValidationMessages();
            if (validationErrors != null) { Logger<FilmController>.Log.Error(validationErrors); }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }
        [HttpGet]
        public async Task<ActionResult> Update(int id)
        {
            if (id > 0)
            {
                var film = await db.GetByIdAsNoTrackingAsync(id);
                if (film == null)
                {
                    Logger<FilmController>.Log.Error("Controller:Film | ActionResult:Update | ERORR = Film not found");
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);

                }
               
                var viewModel = new FilmUpdateViewModel(film);
                ViewBag.Directors = await db.IncludeDirectorsDropdown(viewModel.DirectorId);
                ViewBag.Producers = await db.IncludeProducersDropdown(viewModel.ProducerId);
                ViewBag.Genres = await db.IncludeGenresDropdown(viewModel.GenreId);
                ViewBag.Actors = await db.IncludeActorsDropdown();
                
                return View(viewModel);
            }
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);

        }
        [HttpPost]
        public async Task<ActionResult> Update(FilmUpdateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var filmUpdated = await db.MapDataUpdateAsync(viewModel);
                if(filmUpdated!=null)
                db.Update(filmUpdated);
                await db.SaveAsync();
                return Json(new { success = true, message = "Updated Successfully" }, JsonRequestBehavior.AllowGet);

            }
            var validationErrors = GetValidationMessages();
            if(validationErrors!= null) { Logger<FilmController>.Log.Error(validationErrors); } 
            return new HttpStatusCodeResult(HttpStatusCode.NotModified);

        }
        [HttpGet]
        public async Task<ActionResult> GetData()
        {
            var filmData = await db.GetAllFilmsDataAsync();
            return Json(filmData, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var filmToDelete =await db.GetByIdAsync(id);
            if (filmToDelete != null) { 
            db.Delete(filmToDelete);
            await db.SaveAsync();
            return Json(new { success = true, message = "Deleted Successfully" }, JsonRequestBehavior.AllowGet);
            }
            Logger<FilmController>.Log.Error("Film for delete wasn't found");
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        
        private string GetValidationMessages()
        {
           return string.Join(" | ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        }
    }
}