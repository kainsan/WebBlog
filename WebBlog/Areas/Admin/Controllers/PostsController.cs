using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PagedList.Core;
using WebBlog.Helpers;
using WebBlog.Models;

namespace WebBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PostsController : Controller
    {
        private readonly dbBlogContext _context;

        public PostsController(dbBlogContext context)
        {
            _context = context;
        }

        // GET: Admin/Posts
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;
            var lsPosts = _context.Posts
                    .Include(p => p.Account).Include(p => p.Cat)
                    .OrderByDescending(x => x.PostId);
            PagedList<Post> models = new PagedList<Post>(lsPosts, pageNumber, pageSize);
            return View(models);
        }

        // GET: Admin/Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Admin/Posts/Create
        public IActionResult Create()
        {
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dangnhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountID");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            ViewData["Danhmuc"] = new SelectList(_context.Categories, "CatID", "CatName");
            return View();
        }

        // POST: Admin/Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountId,Tags,CatId,IsHot,IsNewsfeed")] Post post,Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            //Check role
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dangnhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountID");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
            if (account == null) return NotFound();

            if (ModelState.IsValid)
            {
                post.AccountId = account.AccountId;
                post.Author = account.FullName;
                post.Alias = Utilities.SEOUrl(post.Title);
                //default ID
                if (post.CatId == null) post.CatId = 1;
                post.CreateDate = DateTime.Now;
                

                if(fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string Newname = Utilities.SEOUrl(post.Title)+extension;
                    post.Thumb = await Utilities.UploadFile(fThumb, @"new\", Newname.ToLower());
                }
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
    
            ViewData["Danhmuc"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // GET: Admin/Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", post.AccountId);
            ViewData["Danhmuc"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // POST: Admin/Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountId,Tags,CatId,IsHot,IsNewsfeed")] Post post, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            if (!User.Identity.IsAuthenticated) Response.Redirect("/dangnhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountID");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
            if (account == null) return NotFound();

            if(account.RoleId < 3)
            { //truong hop co tinh an cap bai
            if (post.AccountId != account.AccountId) return RedirectToAction(nameof(Index));
             }
            

            if (ModelState.IsValid)
            {
                try
                {
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string Newname = Utilities.SEOUrl(post.Title) + extension;
                        post.Thumb = await Utilities.UploadFile(fThumb, @"new\", Newname.ToLower());
                    }
                    post.AccountId = account.AccountId;
                    post.Author = account.FullName;
                    post.Alias = Utilities.SEOUrl(post.Title);

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
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
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", post.AccountId);
            ViewData["Danhmuc"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // GET: Admin/Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'dbBlogContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
