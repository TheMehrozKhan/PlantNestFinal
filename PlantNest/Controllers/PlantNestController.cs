using PagedList;
using PlantNest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlantNest.Controllers
{
    public class PlantNestController : Controller
    {
        PlantNestEntities db = new PlantNestEntities();

        public ActionResult Index()
        {
            List<ProductViewModel> products = new List<ProductViewModel>();

            // Fetch products from the database
            var productList = db.tbl_product.ToList();

            foreach (var product in productList)
            {
                ProductViewModel viewModel = new ProductViewModel
                {
                    pro_id = product.pro_id,
                    pro_name = product.pro_name,
                    pro_price = product.pro_price,
                    pro_desc = product.pro_desc,
                    pro_images = product.pro_image.Split(',').ToList()
                };

                products.Add(viewModel);
            }

            return View(products);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult FAQs()
        {
            return View();
        }

        public ActionResult Term_of_Use()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(tbl_user uvm)
        {
            tbl_user us = db.tbl_user.Where(x => x.u_name == uvm.u_name && x.u_password == uvm.u_password).SingleOrDefault();
            if (us != null)
            {
                Session["u_id"] = us.u_id.ToString();
                Session["u_name"] = us.u_name;
                TempData["ToastMessage"] = "Hi, " + us.u_name + " You Successfully Logged In!";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = "Invalid Username or Password";
            }
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(tbl_user us)
        {
            
                tbl_user u = new tbl_user();
                u.u_name = us.u_name;
                u.u_email = us.u_email;
                u.u_password = us.u_password;
                u.u_contact = us.u_contact;
                db.tbl_user.Add(u);
                db.SaveChanges();
                return RedirectToAction("Login");
        }

        public ActionResult Profile(int? id)
        {
            int userId = Convert.ToInt32(Session["u_id"]);


            if (id == null)
            {
                tbl_user user = db.tbl_user.FirstOrDefault(u => u.u_id == userId);
                return View(user);
            }

            return View("ProfileNotFound");
        }

        [HttpGet]
        public ActionResult Edit_Profile(int id)
        {
            tbl_user user = db.tbl_user.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]

        public ActionResult Save_Edit(tbl_user user, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.Error = "Image Couldn't Uploaded Try Again";
            }
            else
            {
                tbl_user us = db.tbl_user.Find(user.u_id);
                us.u_name = user.u_name;
                if (imgfile != null)
                {
                    us.u_image = path;
                }
                db.SaveChanges();
                return RedirectToAction("Profile");
            }
            return View(user);
        }

        public ActionResult Search(int? id, int? page, string search)
        {
            int pageSize = 6;
            int pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            var productList = db.tbl_product.Where(x => x.pro_name.ToLower().Contains(search.ToLower()) || x.pro_desc.ToLower().Contains(search.ToLower())).OrderByDescending(x => x.pro_id).ToList();

            List<ProductViewModel> products = new List<ProductViewModel>();

            foreach (var product in productList)
            {
                ProductViewModel viewModel = new ProductViewModel
                {
                    pro_id = product.pro_id,
                    pro_name = product.pro_name,
                    pro_price = product.pro_price,
                    pro_desc = product.pro_desc,
                    pro_images = product.pro_image.Split(',').ToList()
                };

                products.Add(viewModel);
            }

            IPagedList<ProductViewModel> pro = products.ToPagedList(pageIndex, pageSize);

            int totalProductsFound = productList.Count;
            ViewBag.TotalProductsFound = totalProductsFound;
            return View(pro);
        }


        public string uploadimage(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();

            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try

                    {
                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);
                    }

                    catch (Exception ex)
                    {

                        path = "-1";

                    }
                }
                else
                {
                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");
                }

            }
            else

            {

                Response.Write("<script>alert('Please select a file'); </script>");

                path = "-1";

            }
            return path;
        }
    }
}