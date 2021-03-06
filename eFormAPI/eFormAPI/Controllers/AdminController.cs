﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using eFormAPI.Common.API;
using eFormAPI.Common.Models;
using eFormAPI.Common.Models.User;
using eFormAPI.Web.Infrastructure.Consts;
using eFormAPI.Web.Infrastructure.Data;
using eFormAPI.Web.Infrastructure.Data.Entities;
using eFormAPI.Web.Infrastructure.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;

namespace eFormAPI.Web.Controllers
{
    [Authorize(Roles = EformRoles.Admin)]
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        private EformUserManager _eformUserManager;
        private EformRoleManager _eformRoleManager;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public EformUserManager UserManager
        {
            get => _eformUserManager ?? Request.GetOwinContext().GetUserManager<EformUserManager>();
            set => _eformUserManager = value;
        }

        public EformRoleManager RoleManager
        {
            get => _eformRoleManager ?? new EformRoleManager(new EformRoleStore(BaseDbContext.Create()));
            set => _eformRoleManager = value;
        }

        [HttpGet]
        [Route("user/{userId}")]
        public OperationDataResult<UserRegisterModel> GetUser(int userId)
        {
            try
            {
                var model = UserManager.Users
                    .Include(x => x.Roles)
                    .Select(userResult => new UserRegisterModel
                    {
                        Email = userResult.Email,
                        Id = userResult.Id,
                        FirstName = userResult.FirstName,
                        LastName = userResult.LastName,
                        UserName = userResult.UserName,
                        RoleId = userResult.Roles.FirstOrDefault().RoleId
                    }).FirstOrDefault(x => x.Id == userId);

                if (model?.RoleId != null)
                {
                    model.Role = RoleManager.FindById((int)model.RoleId).Name;
                }
                return new OperationDataResult<UserRegisterModel>(true, model);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                return new OperationDataResult<UserRegisterModel>(false, "Error when obtaining users");
            }
        }

        [HttpPost]
        [Route("get-users")]
        public OperationDataResult<UserInfoModelList> GetAllUsers(PaginationModel paginationModel)
        {
            try
            {
                var roles = RoleManager.Roles.ToList();
                var userList = new List<UserInfoViewModel>();
                var userResult = UserManager.Users
                    .Include(x => x.Roles)
                    .OrderBy(z => z.Id)
                    .Skip(paginationModel.Offset)
                    .Take(paginationModel.PageSize)
                    .ToList();

                userResult.ForEach(userItem =>
                {
                    var roleName =
                        roles.FirstOrDefault(x => x.Id == userItem.Roles.Select(y => y.RoleId).FirstOrDefault());
                    var modelItem = new UserInfoViewModel();
                    if (roleName != null)
                    {
                        modelItem.Role = roleName.Name;
                    }
                    modelItem.FirstName = userItem.FirstName;
                    modelItem.LastName = userItem.LastName;
                    modelItem.Email = userItem.Email;
                    modelItem.Id = userItem.Id;
                    modelItem.UserName = userItem.UserName;
                    userList.Add(modelItem);
                });
                var totalUsers = UserManager.Users.Count();
                return new OperationDataResult<UserInfoModelList>(true, new UserInfoModelList()
                {
                    TotalUsers = totalUsers,
                    UserList = userList
                });
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                return new OperationDataResult<UserInfoModelList>(false, "Error when obtaining users");
            }
        }

        [HttpPost]
        [Route("update-user")]
        public OperationResult UpdateUser(UserRegisterModel userRegisterModel)
        {
            try
            {
                var user = UserManager.FindById(userRegisterModel.Id);
                if (user == null)
                {
                    return new OperationResult(false, $"User {userRegisterModel.UserName} not found");
                }
                if (userRegisterModel.Role == null)
                {
                    return new OperationResult(false, "Role is required");
                }
                user.Email = userRegisterModel.Email;
                user.UserName = userRegisterModel.UserName;
                user.FirstName = userRegisterModel.FirstName;
                user.LastName = userRegisterModel.LastName;
                var result = UserManager.Update(user);
                if (!result.Succeeded)
                {
                    return new OperationResult(false, string.Join(" ", result.Errors));
                }
                // password
                if (userRegisterModel.Password != null)
                {
                    UserManager.RemovePassword(user.Id);
                    UserManager.AddPassword(user.Id, userRegisterModel.Password);
                }
                // change role
                UserManager.RemoveFromRoles(user.Id, EformRoles.Admin, EformRoles.User);
                UserManager.AddToRole(user.Id, userRegisterModel.Role);
                return new OperationResult(true, $"User {user.UserName} was updated");
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                return new OperationResult(false, "Error when updating user");
            }
        }

        [HttpPost]
        [Route("create-user")]
        public OperationResult CreateUser(UserRegisterModel userRegisterModel)
        {
            try
            {
                var userResult = UserManager.FindByName(userRegisterModel.UserName);
                if (userResult != null)
                {
                    return new OperationResult(false, $"User {userRegisterModel.UserName} already exist");
                }
                if (userRegisterModel.Role == null)
                {
                    return new OperationResult(false, "Role is required");
                }
                var user = new EformUser
                {
                    Email = userRegisterModel.Email,
                    UserName = userRegisterModel.UserName,
                    FirstName = userRegisterModel.FirstName,
                    LastName = userRegisterModel.LastName,
                };
                var result = UserManager.Create(user, userRegisterModel.Password);
                if (!result.Succeeded)
                {
                    return new OperationResult(false, string.Join(" ", result.Errors));
                }
                // change role
                UserManager.AddToRole(user.Id, userRegisterModel.Role.ToLower());
                return new OperationResult(true, $"User {user.UserName} was created");
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                return new OperationResult(false, "Error when creating user");
            }
        }

        [HttpGet]
        [Route("delete-user/{userId}")]
        public OperationResult DeleteUser(int userId)
        {
            try
            {
                if (userId == 1)
                {
                    return new OperationResult(false, "Can't delete primary admin user");
                }
                var user = UserManager.FindById(userId);
                if (user == null)
                {
                    return new OperationResult(false, $"User {userId} not found");
                }
                var result = UserManager.Delete(user);
                if (!result.Succeeded)
                {
                    return new OperationResult(false, string.Join(" ", result.Errors));
                }
                return new OperationResult(true, $"User {userId} was deleted");
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                return new OperationResult(false, "Error while deleting user");
            }
        }
    }
}