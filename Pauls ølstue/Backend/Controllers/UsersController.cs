﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Backend.Viewmodels;
using Data.Classes;
using Data.Interface;
using Model;

namespace Backend.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = new UsersViewmodel
            {
                Users = _userService.SearchUsers("", UserSort.RoomNrAsc, 5, 0).ToList(),
                Sort = UserSort.RoomNrAsc,
                UserTotal = _userService.UserCount(),
                Offset = 0,
                PageSize = 5,
                SearchText = ""
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string searchText, UserSort sort, int pageSize, int offSet)
        {


            var model = new UsersViewmodel
            {
                Users = _userService.SearchUsers(searchText, sort, pageSize, offSet).ToList(),
                Sort = sort,
                UserTotal = _userService.UserCount(),
                Offset = offSet,
                PageSize = pageSize,
                SearchText = searchText

            };
            return PartialView("_UserTable", model);
        }

        [HttpGet]
        public ActionResult Rediger(int id)
        {
            var model = new EditUserViewmodel
            {
                User = _userService.FindById(id),
                Roles = _userService.GetRoles().Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
            };
            return View("_EditUser", model);
        }

        [HttpPost]
        public bool Rediger(MyPageViewmodel model, string command, int userId)
        {
            var success = false;
            switch (command)
            {
                case "saveProfile":
                    _userService.SaveProfile(userId, model.User.Fornavn, model.User.Efternavn, model.User.Email, model.User.Role.Id);
                    success = true;
                    break;
                case "saveLogin":
                    _userService.SaveCredentials(model.User.VærelseNr, model.Password, userId);
                    success = true;
                    break;
                case "saveImage":
                    if (model.Image != null)
                    {
                        var file = new byte[model.Image.ContentLength];
                        model.Image.InputStream.Read(file, 0, model.Image.ContentLength);
                        _userService.SaveImage(file, userId);
                        success = true;
                    }
                    break;
            }
            return success;
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new EditUserViewmodel
            {
                Roles = _userService.GetRoles().Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(EditUserViewmodel model)
        {
            if (model.Image != null)
            {
                var file = new byte[model.Image.ContentLength];
                model.Image.InputStream.Read(file, 0, model.Image.ContentLength);
                model.User.Billede = file;
            }
            _userService.CreateUser(model.User, model.Password);
            return RedirectToAction("Index", "Users");
        }

        [HttpPost]
        public bool Delete(int userid)
        {
            return _userService.DeleteUser(userid);
        }
    }
}