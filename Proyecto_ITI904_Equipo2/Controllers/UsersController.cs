using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Proyecto_ITI904_Equipo2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_ITI904_Equipo2.Controllers
{
    public class UsersController : Controller
    {
        
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public UsersController()
        {
        }

        public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: Users
        public ActionResult Index()
        {
            LoadRolesToViewBag();

            return View(db.Users.ToList());
        }

        public ActionResult Ascender(string userId)
        {
            if (UserManager.IsInRole(userId, "Empleado"))
            {
                UserManager.RemoveFromRole(userId, "Empleado");
                UserManager.AddToRole(userId, "Admin");
            }

            if (UserManager.IsInRole(userId, "Cliente"))
            {
                UserManager.RemoveFromRole(userId, "Cliente");
                UserManager.AddToRole(userId, "Empleado");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Restringir(string userId)
        {
            if (UserManager.IsInRole(userId, "Empleado"))
            {
                UserManager.RemoveFromRole(userId, "Empleado");
                UserManager.AddToRole(userId, "Cliente");
            }

            if (UserManager.IsInRole(userId, "Admin"))
            {
                UserManager.RemoveFromRole(userId, "Admin");
                UserManager.AddToRole(userId, "Empleado");
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Users/SetPassword
        public ActionResult SetPassword(string userId)
        {
            var model = new SetUserPasswordModel() { UserId = userId};
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetUserPasswordModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    var result = await UserManager.ResetPasswordAsync(userId, , model.NewPassword);
            //    if (result.Succeeded)
            //    {
            //        return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
            //    }
            //    AddErrors(result);
            //}

            if (ModelState.IsValid)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(store);
                String userId = model.UserId;//"<YourLogicAssignsRequestedUserId>";
                String newPassword = model.NewPassword; //"<PasswordAsTypedByUser>";
                String hashedNewPassword = UserManager.PasswordHasher.HashPassword(newPassword);
                ApplicationUser cUser = await store.FindByIdAsync(userId);
                await store.SetPasswordHashAsync(cUser, hashedNewPassword);

                await store.UpdateAsync(cUser);

                return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
            }
            // Si llegamos a este punto, es que se ha producido un error, volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Users/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                user.FechaRegistro = DateTime.Now;
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await UserManager.IsInRoleAsync(user.Id, "Empleado"))
                    {
                        await UserManager.AddToRoleAsync(user.Id, "Empleado");

                        return RedirectToAction("Index", "Users");
                    }
                }
                AddErrors(result);
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        private void LoadRolesToViewBag()
        {
            var roleStore = new RoleStore<IdentityRole>(db);
            var roleMngr = new RoleManager<IdentityRole>(roleStore);
            List<IdentityRole> roles = roleMngr.Roles.ToList();
            ViewBag.Roles = roles;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
    }
}