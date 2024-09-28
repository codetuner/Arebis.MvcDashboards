using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using MyMvcApp.Areas.MvcDashboardContent.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardContent.Controllers
{
    [Authorize(Roles = "Administrator,ContentAdministrator,ContentEditor,ContentAuthor")]
    public class MediaController : BaseController
    {
        #region Construction

        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;

        public MediaController(IConfiguration config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
        }

        #endregion

        #region Index

        [HttpGet]
        public IActionResult Index(IndexModel model)
        {
            return IndexView(model);
        }

        private IActionResult IndexView(IndexModel model)
        {
            // Retrieve path:
            var rootDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!));
            var pathDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!, model.Path ?? "."));
            if (GetPath(pathDir, rootDir) == null) return this.NotFound();

            // Build breadcrumb:
            model.BreadCrumb.Add(new SelectListItem() { Text = pathDir.Name, Value = GetPath(pathDir, rootDir), Selected = true });
            {
                var parentdir = pathDir;
                while (true)
                {
                    if (parentdir == null) break;
                    if (parentdir.FullName == rootDir.FullName) break;
                    parentdir = parentdir.Parent;
                    if (parentdir == null) break;
                    model.BreadCrumb.Insert(0, new SelectListItem() { Text = parentdir.Name, Value = GetPath(parentdir, rootDir) });
                }
            }

            try
            {
                // Retrieve sub-directories and files:
                model.Directories = pathDir.GetDirectories().Select(d => new SelectListItem(d.Name, GetPath(d, rootDir))).ToList();
                model.Files = pathDir.GetFiles();

                // Return view:
                return View("Index", model);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(IndexModel model, List<IFormFile>? files)
        {
            if (files == null)
            {
                this.SetToastrMessage("error", "Failed to upload files. Make sure total file size is not too large.");
                return NoContent();
            }
            else
            {
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var filePath = Path.Combine(env.WebRootPath, config["Content:Media"]!, model.Path ?? ".", formFile.FileName);
                        if (!filePath.StartsWith(Path.Combine(env.WebRootPath, config["Content:Media"]!))) return NotFound();
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                }
                this.SetToastrMessage("success", "File(s) uploaded.");
            }

            return IndexView(model);
        }

        [HttpGet]
        public IActionResult CreateDirectory(IndexModel model)
        { 
            return View(model);
        }

        [HttpPost, ActionName("CreateDirectory")]
        public IActionResult CreateDirectoryPost(IndexModel model)
        {
            // Retrieve path:
            var rootDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!));
            var pathDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!, model.Path ?? "."));
            if (GetPath(pathDir, rootDir) == null) return this.NotFound();

            // Create directory:
            if (model.NewName != null)
            {
                pathDir.CreateSubdirectory(model.NewName.Replace('\\', '-').Replace('/', '-').Replace(':', '-').Replace('?', '.').Replace('*', '-'));
            }

            // Return to index view:
            Response.Headers["X-Sircl-History-Replace"] = Url.Action("Index", null, new { Path = model.Path });
            return IndexView(model);
        }

        [HttpGet]
        public IActionResult DeleteDirectory(IndexModel model)
        { 
            return View(model);
        }

        [HttpPost, ActionName("DeleteDirectory")]
        public IActionResult DeleteDirectoryPost(IndexModel model)
        {
            // Retrieve path:
            var rootDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!));
            var pathDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!, model.Path ?? "."));
            if (GetPath(pathDir, rootDir) == null) return this.NotFound();

            // Delete directory:
            try
            {
                if (GetPath(pathDir, rootDir) != "" && pathDir.GetDirectories().Length == 0 && pathDir.GetFiles().Length == 0)
                {
                    pathDir.Delete();
                }
            }
            catch (DirectoryNotFoundException) { }

            // Return to the previous view:
            return this.Back(false);
        }

        [HttpGet]
        public IActionResult RenameFile(IndexModel model)
        { 
            return View(model);
        }

        [HttpPost, ActionName("RenameFile")]
        public IActionResult RenameFilePost(IndexModel model)
        { 
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteSelection(IndexModel model)
        { 
            return View(model);
        }

        [HttpPost, ActionName("DeleteSelection")]
        public IActionResult DeleteSelectionPost(IndexModel model, string[] selection)
        {
            // Retrieve path:
            var rootDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!));
            var pathDir = new DirectoryInfo(Path.Combine(env.WebRootPath, config["Content:Media"]!, model.Path ?? "."));
            if (GetPath(pathDir, rootDir) == null) return this.NotFound();

            foreach (var item in selection)
            {
                try
                {
                    foreach(var file in pathDir.GetFiles(item))
                    {
                        file.Delete(); 
                    }
                }
                catch (FileNotFoundException) { }
            }

            // Return to index view:
            Response.Headers["X-Sircl-History-Replace"] = Url.Action("Index", null, new { Path = model.Path });
            return IndexView(model);
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Returns the path segment between the given directory and the given root.
        /// </summary>
        private string? GetPath(DirectoryInfo directory, DirectoryInfo root)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (directory.FullName == root.FullName) return sb.ToString();
                if (sb.Length > 0) sb.Insert(0, '\\');
                sb.Insert(0, directory.Name);
                directory = directory.Parent!;
                if (directory == null) return null;
            }
        }

        #endregion
    }
}
