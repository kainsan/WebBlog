using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PagedList.Core;
using WebBlog.Areas.Admin.Models;
using WebBlog.Extension;
using WebBlog.Helpers;
using WebBlog.Models;

namespace WebBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class AccountsController : Controller
    {
        private readonly dbBlogContext _context;

        public AccountsController(dbBlogContext context)
        {
            _context = context;
        }

        // GET: Admin/Accounts
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;

            var lsAccounts = _context.Accounts
                    .Include(a => a.Role)
                    .OrderByDescending(x => x.CreateDate);
            PagedList<Account> models = new PagedList<Account> (lsAccounts, pageNumber, pageSize);
            return View(models);
        }

        // GET: Admin/Login
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name ="Login")]
        public IActionResult Login(string returnurl =null)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if(taikhoanID != null) return RedirectToAction("Index", "Home", new {Area ="Admin"});
            ViewBag.ReturnUrl = returnurl;
            return View();
            
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "Login")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnurl = null)
        
        {
            try 
            {
                if (ModelState.IsValid)
                {
                    Account kh = _context.Accounts
                            .Include(p => p.Role)
                            .SingleOrDefault(p => p.Email.ToLower() == model.Email.ToLower().Trim());

                    if (kh == null)
                    {
                        ViewBag.Error = "Info not Valid";
                        return View(model);
                    }

                    string pass = new string(kh.Password);
                    
                    if (kh.Password.Trim() != pass)
                    {
                        ViewBag.Error = "Info pass not Valid";
                        return View(model);
                    }
                    //Dang nhap thanh cong

                    kh.LastLogin = DateTime.Now;
                    _context.Update(kh);
                    await _context.SaveChangesAsync();

                    var taikhoanID = HttpContext.Session.GetString("AccountId");
                    //Identify
                    //Save session maKH
                    HttpContext.Session.SetString("AccountId", kh.AccountId.ToString());

                    //Identify
                    var userClaims = new List<Claim>
                    {
                         new Claim(ClaimTypes.Name, kh.FullName),
                         new Claim(ClaimTypes.Email, kh.Email),
                         new Claim("AccountId",kh.AccountId.ToString()),
                         new Claim("RoleId",kh.RoleId.ToString()),
                         new Claim(ClaimTypes.Role,kh.Role.RoleName)
                    };

                    var grandmaIdentify = new ClaimsIdentity(userClaims, "User Identity");
                    var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentify });
                    await HttpContext.SignInAsync(userPrincipal);
                    return RedirectToAction("Index", "Home", new { Area ="Admin" });
                }
            }
            catch
            {
                return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            }

            return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
        }

        // GET: Admin/Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Admin/Accounts/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId");
            return View();
        }

        // POST: Admin/Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,FullName,Email,Phone,Password,Salt,Active,CreateDate,RoleId,LastLogin")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: Admin/Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // POST: Admin/Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,FullName,Email,Phone,Password,Salt,Active,CreateDate,RoleId,LastLogin")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: Admin/Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'dbBlogContext.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
          return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
