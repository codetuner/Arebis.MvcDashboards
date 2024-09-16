using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyMvcApp.Areas.MvcDashboardIdentity.Models.Users;
using MyMvcApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardIdentity.Controllers
{
    public class UsersController : BaseController
    {
        #region Construction

        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public UsersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        #endregion

        #region Claim Types

        static readonly Dictionary<string, string> ClaimTypeToName;
        static readonly Dictionary<string, string> ClaimTypeToAlias;

        static UsersController()
        {
            var claimTypes = typeof(ClaimTypes)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .OrderBy(f => f.Name)
                .Select(f => new string?[] { "@" + f.Name, (string?)f.GetValue(null) })
                .ToArray();

            ClaimTypeToName = new Dictionary<string, string>();
            ClaimTypeToAlias = new Dictionary<string, string>();
            foreach (var type in claimTypes)
            {
                ClaimTypeToName[type[0] ?? String.Empty] = type[1] ?? String.Empty;
                ClaimTypeToAlias[type[1] ?? String.Empty] = type[0] ?? String.Empty;
            }
        }

        static string ClaimsGet(Dictionary<string, string> claimsDict, string key)
        {
            if (claimsDict.TryGetValue(key, out string? value))
            {
                return value;
            }
            else
            {
                return key;
            }
        }

        #endregion

        #region Index

        public IActionResult Index(IndexModel model)
        {
            // Retrieve data:
            var query = context.Users.AsQueryable();
            if (!String.IsNullOrWhiteSpace(model.Query))
            {
                query = query.Where(d => d.NormalizedUserName!.Contains(model.Query) || d.NormalizedEmail!.Contains(model.Query));
            }
            if (!String.IsNullOrWhiteSpace(model.SelectedRoleName)) 
            {
                var roleId = context.Roles.SingleOrDefault(r => r.Name == model.SelectedRoleName)?.Id;
                if (roleId != null)
                {
                    // Retrieving all user ids having that role (not ideal, but how to do otherwise?):
                    var userIds = context.UserRoles.Where(ur => ur.RoleId == roleId).Select(ur => ur.UserId);
                    // Filter on user ids:
                    query = query.Where(d => userIds.Contains(d.Id));
                }
            }

            // Build model:
            var count = query
                .Count();
            model.MaxPage = (count + model.PageSize - 1) / model.PageSize;
            model.Items = query
                .OrderBy(model.Order ?? "UserName ASC")
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToArray();

            // Render view:
            model.RoleNames = context.Roles.Select(r => r.Name)
                .OrderBy(n => n)
                .Select(d => new SelectListItem() { Value = d, Text = d/*, Selected = (model.SelectedRoleName == d)*/ })
                .ToList();
            return View("Index", model);
        }

        public IActionResult DownloadList(string? q = null)
        {
            // Retrieve data:
            var query = context.Users.AsQueryable();
            var fullCount = query.Count();
            if (!String.IsNullOrWhiteSpace(q))
                query = query.Where(d => d.NormalizedUserName!.Contains(q) || d.NormalizedEmail!.Contains(q));

            // Build CSV:
            var sb = new StringBuilder();
            sb.AppendLine("UserId,UserName,Email,IsLockedOut");
            foreach (var line in query.OrderBy(u => u.UserName).Select(u => $"{u.Id},{u.UserName},{u.Email},{u.IsLockedout()}"))
                sb.AppendLine(line);
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return this.File(bytes, "text/csv; charset=utf-8", "Users.csv");
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> New()
        {
            return await Edit("0");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            // Retrieve data:
            var user = (await userManager.FindByIdAsync(id)) ?? new IdentityUser() { Id = "NEW", ConcurrencyStamp = "" };

            // Build model:
            var model = new EditModel
            {
                Item = user,
                CanHavePassword = userManager.SupportsUserPassword,
                CanHaveInitialPassword = userManager.SupportsUserPassword && (user?.PasswordHash == null),
                SupportsUserRoles = userManager.SupportsUserRole,
                SupportsUserClaims = userManager.SupportsUserClaim
            };
            if (model.SupportsUserRoles)
                model.UserRoleNames = context.Roles.Where(r => context.UserRoles.Where(ur => ur.UserId == id).Select(ur => ur.RoleId).Contains(r.Id)).Select(r => r.Name!).ToList();
            if (model.SupportsUserClaims)
                model.UserClaims = context.UserClaims.Where(c => c.UserId == id).ToList();

            // Render view:
            return EditView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(string id, EditModel model, bool apply = false)
        {
            if (id != model.Item.Id) throw new SecurityException();

            ModelState.ClearValidationState("Id");
            ModelState.MarkFieldSkipped("Id");
            if (ModelState.IsValid)
            {
                var result = await this.SaveUserAsync(model.Item, model.UserRoleNames, model.UserClaims, model.InitialPassword, model.RemovePasswordOnSave);
                if (result.Succeeded)
                {
                    Response.Headers["X-Sircl-Load"] = "#topMenu";
                    if (apply)
                    {
                        return this.ForwardToAction("Edit", null, new { model.Item.Id });
                    }
                    else
                    {
                        return Back(false);
                    }
                }
                else
                {
                    foreach (var resultError in result.Errors)
                    {
                        ModelState.AddModelError("", resultError.Description);
                    }
                }
            }

            Response.Headers["X-Sircl-History-Replace"] = Url.Action("Edit", new { id = model.Item.Id });
            return EditView(model);
        }

        [HttpPost]
        public IActionResult RemovePassword(string id, EditModel model)
        {
            if (id != model.Item.Id) throw new SecurityException();

            ModelState.Clear();

            model.RemovePasswordOnSave = true;
            model.CanHaveInitialPassword = true;
            model.HasChanges = true;

            return EditView(model);
        }

        [HttpPost]
        public IActionResult AddClaimDlgBody(EditModel model)
        {
            ModelState.Clear();

            return EditView(model, viewName: "Edit_Claims_AddClaimDlgBody");
        }

        [HttpPost]
        public IActionResult AddClaim(EditModel model)
        {
            ModelState.Clear();

            if (String.IsNullOrWhiteSpace(model.NewClaim?.ClaimType))
            {
                ModelState.AddModelError("NewClaim.ClaimType", "Value should not be empty.");
            }
            else
            {
                // Convert alias to real type name:
                model.NewClaim.ClaimType = ClaimsGet(ClaimTypeToName, model.NewClaim.ClaimType);

                // Add claim:
                model.UserClaims.Add(model.NewClaim);

                // Mark dirty:
                Response.Headers["X-Sircl-Form-Changed"] = "true";
            }

            // Return view:
            return EditView(model, viewName: "Edit_Claims");
        }

        [HttpPost]
        public IActionResult RemoveClaim(EditModel model, int claimIndex)
        {
            ModelState.Clear();

            // Remove claim:
            model.UserClaims.RemoveAt(claimIndex);

            // Mark dirty:
            Response.Headers["X-Sircl-Form-Changed"] = "true";

            // Return view:
            return EditView(model, viewName: "Edit_Claims");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(EditModel model)
        {
            if (model.Item.Id == null)
            {
                return Back();
            }
            else
            {
                IdentityResult result = await userManager.DeleteAsync(model.Item);

                if (result.Succeeded)
                {
                    Response.Headers["X-Sircl-Load"] = "#topMenu";
                    return Back(false);
                }

                return EditView(model, identityResult: result);
            }
        }

        private IActionResult EditView(EditModel model, IdentityResult? identityResult = null, string? viewName = null)
        {
            if (identityResult != null && !identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            model.SupportsUserRoles = userManager.SupportsUserRole;
            if (model.SupportsUserRoles)
                model.Roles = context.Roles.ToList();
            model.SupportsUserClaims = userManager.SupportsUserClaim;
            model.NewClaim = null;
            model.ClaimTypes = ClaimTypeToName.Keys.ToArray();

            return View(viewName ?? "Edit", model);
        }

        private async Task<IdentityResult> SaveUserAsync(IdentityUser user, List<string> userRoleNames, List<IdentityUserClaim<string>> userClaims, string? password, bool removePasswordOnSave)
        {
            IdentityResult result;
            IdentityUser storedUser;
            if (user.Id == "NEW")
            {
                // Create user object:
                user.Id = Guid.NewGuid().ToString();
                if (String.IsNullOrEmpty(password))
                {
                    result = await userManager.CreateAsync(user);
                }
                else
                {
                    result = await userManager.CreateAsync(user, password);
                }
                if (!result.Succeeded)
                {
                    user.Id = null!;
                    return result;
                }

                // Retrieve stored user:
                storedUser = await userManager.FindByNameAsync(user.UserName!)
                    ?? throw new NullReferenceException("Failed to create user.");
            }
            else
            {
                // Update user object:
                storedUser = await userManager.FindByIdAsync(user.Id)
                    ?? throw new NullReferenceException("No user found with given user.Id!");
                storedUser.UserName = user.UserName!;
                storedUser.Email = user.Email;
                storedUser.EmailConfirmed = user.EmailConfirmed;
                storedUser.PhoneNumber = user.PhoneNumber;
                storedUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                storedUser.LockoutEnabled = user.LockoutEnabled;
                storedUser.LockoutEnd = user.LockoutEnd;
                storedUser.TwoFactorEnabled = user.TwoFactorEnabled;
                storedUser.ConcurrencyStamp = user.ConcurrencyStamp;
                result = await userManager.UpdateAsync(storedUser);
                if (!result.Succeeded) return result;
                if (removePasswordOnSave)
                {
                    result = await userManager.RemovePasswordAsync(storedUser);
                    if (!result.Succeeded) return result;
                }
                if (!String.IsNullOrEmpty(password))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(storedUser);
                    result = await userManager.ResetPasswordAsync(storedUser, token, password);
                    if (!result.Succeeded) return result;
                }
            }

            // Update roles:
            if (userManager.SupportsUserRole)
            {
                // Remove claims without value:
                userClaims = userClaims.Where(c => !String.IsNullOrWhiteSpace(c.ClaimValue)).ToList();
                // Synchronise with stored claims:
                var storedUserRoleNames = context.Roles.Where(r => context.UserRoles.Where(ur => ur.UserId == storedUser!.Id).Select(ur => ur.RoleId).Contains(r.Id)).Select(r => r.Name!).ToList();
                var roleNamesToKeep = new List<string>();
                foreach (var name in userRoleNames)
                {
                    if (storedUserRoleNames.Contains(name))
                    {
                        roleNamesToKeep.Add(name);
                    }
                    else
                    {
                        result = await userManager.AddToRoleAsync(storedUser, name);
                        if (!result.Succeeded) return result;
                    }
                }
                foreach (var name in storedUserRoleNames.Except(roleNamesToKeep))
                {
                    result = await userManager.RemoveFromRoleAsync(storedUser, name);
                    if (!result.Succeeded) return result;
                }
            }

            // Update claims:
            if (userManager.SupportsUserClaim)
            {
                var storedClaims = context.UserClaims.Where(c => c.UserId == storedUser.Id).ToList();
                foreach (var sameclaim in storedClaims.Where(c => userClaims.Select(cs => cs.Id).Contains(c.Id)))
                {
                    var updatedclaim = userClaims.Single(c => c.Id == sameclaim.Id);
                    if (updatedclaim.ClaimValue != sameclaim.ClaimValue)
                    {
                        result = await userManager.RemoveClaimAsync(storedUser, new Claim(sameclaim.ClaimType!, sameclaim.ClaimValue!));
                        if (!result.Succeeded) return result;
                        result = await userManager.AddClaimAsync(storedUser, new Claim(updatedclaim.ClaimType!, updatedclaim.ClaimValue!));
                        if (!result.Succeeded) return result;
                    }
                }
                foreach (var oldclaim in storedClaims.Where(c => !userClaims.Select(cs => cs.Id).Contains(c.Id)))
                {
                    result = await userManager.RemoveClaimAsync(storedUser, new Claim(oldclaim.ClaimType!, oldclaim.ClaimValue!));
                    if (!result.Succeeded) return result;
                }
                foreach (var newclaim in userClaims.Where(c => !storedClaims.Select(cs => cs.Id).Contains(c.Id)))
                {
                    result = await userManager.AddClaimAsync(storedUser, new Claim(newclaim.ClaimType!, newclaim.ClaimValue!));
                    if (!result.Succeeded) return result;
                }
            }

            // Return success state:
            return result;
        }

        #endregion
    }
}
