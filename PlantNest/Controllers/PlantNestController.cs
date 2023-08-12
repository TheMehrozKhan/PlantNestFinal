using PagedList;
using PlantNest.Models;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlantNest.Controllers
{
    public class PlantNestController : Controller
    {
        PlantNestEntities1 db = new PlantNestEntities1();

        public ActionResult Index()
        {
            List<ProductViewModel> products = new List<ProductViewModel>();

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

        public ActionResult CategoryPage(int? page)
        {
            int pageSize = 8;
            int pageIndex = 1;

            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            var list = db.tbl_category.Where(x => x.cat_status == 1).OrderByDescending(x => x.cat_id).ToList();
            IPagedList<tbl_category> cateList = list.ToPagedList(pageIndex, pageSize);

            return View(cateList);
        }


        public ActionResult DisplayAdd(int? id, int? page, string search)
        {
            int pageSize = 6;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            var categoryName = string.Empty;
            if (id != null)
            {
                var category = db.tbl_category.SingleOrDefault(c => c.cat_id == id);
                if (category != null)
                {
                    categoryName = category.cat_name;
                }
            }

            var list = db.tbl_product.Where(x => x.cat_id_fk == id).OrderByDescending(x => x.pro_id).ToList();
            IPagedList<tbl_product> pro = list.ToPagedList(pageIndex, pageSize);


            ViewBag.CategoryName = categoryName;
            return View(pro);
        }

        public ActionResult ProductDetails(int? id)
        {
            ProductDetailsViewModel pr = new ProductDetailsViewModel();

            tbl_product p = db.tbl_product.Where(x => x.pro_id == id).SingleOrDefault();
            pr.pro_id = p.pro_id;
            pr.pro_name = p.pro_name;
            pr.pro_price = p.pro_price;
            pr.pro_desc = p.pro_desc;

            tbl_category cat = db.tbl_category.Where(x => x.cat_id == p.cat_id_fk).SingleOrDefault();
            pr.cat_name = cat.cat_name;

            tbl_admin us = db.tbl_admin.Where(x => x.ad_id == p.ad_id_fk).SingleOrDefault();
            pr.u_id = us.ad_id;
            pr.u_name = us.ad_name;

            pr.ImageUrls = p.pro_image.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return View(pr);
        }

        public ActionResult Add_To_Cart(int? id)
        {
            // Check if the user is logged in
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            tbl_product p = db.tbl_product.Where(x => x.pro_id == id).SingleOrDefault();
            if (p == null)
            {
                return RedirectToAction("Index");
            }

            List<Cart> cartItems = Session["cart"] as List<Cart>;

            if (cartItems == null)
            {
                cartItems = new List<Cart>();
            }

            Cart existingItem = cartItems.FirstOrDefault(item => item.pro_id == id);

            if (existingItem != null)
            {
                existingItem.o_qty++;
                existingItem.o_bill = existingItem.pro_price * existingItem.o_qty;
            }
            else
            {
                Cart ca = new Cart();
                ca.pro_id = p.pro_id;
                ca.pro_name = p.pro_name;
                ca.pro_price = p.pro_price;
                ca.o_qty = 1;
                ca.o_bill = ca.pro_price * ca.o_qty;
                ca.pro_image = p.pro_image;

                cartItems.Add(ca);
            }

            int cartCount = cartItems.Count;
            ViewBag.CartCount = cartCount;

            Session["cart"] = cartItems;

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Remove_From_Cart(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Add_To_Cart");
            }

            List<Cart> cartItems = Session["cart"] as List<Cart>;

            if (cartItems != null)
            {

                Cart itemToRemove = cartItems.FirstOrDefault(item => item.pro_id == id);

                // Remove the item from the cart
                if (itemToRemove != null)
                {
                    cartItems.Remove(itemToRemove);
                }

                int cartCount = cartItems.Count;
                ViewBag.CartCount = cartCount;


                Session["cart"] = cartItems;
            }

            return RedirectToAction("View_Cart");
        }

        public ActionResult View_Cart()
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            List<Cart> cartItems = Session["cart"] as List<Cart>;

            if (cartItems == null)
            {
                cartItems = new List<Cart>();
            }

            ViewBag.CartCount = cartItems.Count;

            return View(cartItems);
        }

        public ActionResult Checkout()
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            List<Cart> cartItems = Session["cart"] as List<Cart>;

            if (cartItems == null || cartItems.Count == 0)
            {
                return RedirectToAction("Index");
            }

            int totalBill = Convert.ToInt32(cartItems.Sum(item => item.o_bill));

            ViewBag.TotalBill = totalBill;

            return View();
        }

        [HttpPost]
        public ActionResult PlaceOrder(CheckoutViewModel model)
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["u_id"]);

            List<Cart> cartItems = Session["cart"] as List<Cart>;

            if (cartItems == null || cartItems.Count == 0)
            {
                return RedirectToAction("View_Cart");
            }

            int totalBill = Convert.ToInt32(cartItems.Sum(item => item.o_bill));

            tbl_invoice invoice = new tbl_invoice();
            invoice.in_fk_user = userId;
            invoice.in_date = DateTime.Now;
            invoice.in_totalbill = totalBill;

            db.tbl_invoice.Add(invoice);
            db.SaveChanges();

            int invoiceId = invoice.in_id;

            foreach (var item in cartItems)
            {
                tbl_order order = new tbl_order();
                order.o_fk_pro = item.pro_id;
                order.o_date = DateTime.Now;
                order.o_totalbill = totalBill;
                order.o_fk_invoice = invoiceId;
                order.o_qty = item.o_qty;
                order.o_unitprice = item.pro_price;
                order.o_bill = item.o_bill;

                order.u_credit_card_number = model.CreditCardNumber;
                order.u_ccv = model.CCV;
                order.u_expiry_date = DateTime.ParseExact(model.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                order.u_pin = model.PIN;

                db.tbl_order.Add(order);
            }

            db.SaveChanges();
            Session["cart"] = null;
            return RedirectToAction("Index", "User");
        }

        public ActionResult Add_Wishlist(int? id)
        {
            // Check if the user is logged in
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            tbl_product p = db.tbl_product.Where(x => x.pro_id == id).SingleOrDefault();
            if (p == null)
            {
                return RedirectToAction("Index");
            }

            List<Wishlist> userItems = Session["wishlist"] as List<Wishlist>;

            if (userItems == null)
            {
                userItems = new List<Wishlist>();
            }

            Wishlist existingItem = userItems.FirstOrDefault(item => item.pro_id == id);

            if (existingItem != null)
            {
                existingItem.o_qty++;
                existingItem.o_bill = existingItem.pro_price * existingItem.o_qty;
            }
            else
            {
                Wishlist ca = new Wishlist();
                ca.pro_id = p.pro_id;
                ca.pro_name = p.pro_name;
                ca.pro_price = p.pro_price;
                ca.o_qty = 1;
                ca.o_bill = ca.pro_price * ca.o_qty;
                ca.pro_image = p.pro_image;

                userItems.Add(ca);
            }

            int cartCount = userItems.Count;
            ViewBag.CartCount = cartCount;

            Session["wishlist"] = userItems;

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult View_Wishlist()
        {
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            List<Wishlist> userItems = Session["Wishlist"] as List<Wishlist>;

            if (userItems == null)
            {
                userItems = new List<Wishlist>();
            }

            ViewBag.WishlistCount = userItems.Count;

            return View(userItems);
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