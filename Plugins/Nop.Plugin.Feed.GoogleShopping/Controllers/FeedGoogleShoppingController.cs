using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Feed.GoogleShopping.Domain;
using Nop.Plugin.Feed.GoogleShopping.Models;
using Nop.Plugin.Feed.GoogleShopping.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Feed.GoogleShopping.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class FeedGoogleShoppingController : BasePluginController
    {
        #region Fields

        private readonly IGoogleService _googleService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IStoreService _storeService;
        private readonly GoogleShoppingSettings _googleShoppingSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IVendorService _vendorService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public FeedGoogleShoppingController(IGoogleService googleService,
            IProductService productService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPluginFinder pluginFinder,
            ILogger logger,
            IWebHelper webHelper,
            IStoreService storeService,
            ISettingService settingService,
            IPermissionService permissionService,
            IHostingEnvironment hostingEnvironment,
            IStoreContext storeContext, GoogleShoppingSettings googleShoppingSettings, IVendorService vendorService, IStaticCacheManager cacheManager, IWorkContext workContext)
        {
            this._googleService = googleService;
            this._productService = productService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._pluginFinder = pluginFinder;
            this._logger = logger;
            this._webHelper = webHelper;
            this._storeService = storeService;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._hostingEnvironment = hostingEnvironment;
            this._googleShoppingSettings = googleShoppingSettings;
            this._vendorService = vendorService;
            this._cacheManager = cacheManager;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new FeedGoogleShoppingModel();
            model.ProductPictureSize = _googleShoppingSettings.ProductPictureSize;
            model.PassShippingInfoWeight = _googleShoppingSettings.PassShippingInfoWeight;
            model.PassShippingInfoDimensions = _googleShoppingSettings.PassShippingInfoDimensions;
            model.PricesConsiderPromotions = _googleShoppingSettings.PricesConsiderPromotions;
            //stores
            model.StoreId = _googleShoppingSettings.StoreId;
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            //currencies
            model.CurrencyId = _googleShoppingSettings.CurrencyId;
            foreach (var c in _currencyService.GetAllCurrencies())
                model.AvailableCurrencies.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //Google categories
            model.DefaultGoogleCategory = _googleShoppingSettings.DefaultGoogleCategory;
            model.AvailableGoogleCategories.Add(new SelectListItem { Text = "Select a category", Value = "" });
            foreach (var gc in _googleService.GetTaxonomyList())
                model.AvailableGoogleCategories.Add(new SelectListItem { Text = gc, Value = gc });
            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(v);
            //file paths
            foreach (var store in _storeService.GetAllStores())
            {
                var localFilePath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "files\\exportimport", store.Id + "-cimri-" + _googleShoppingSettings.StaticFileName);
                var localFilePaths = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "files\\exportimport",
                    store.Id + "-google-" + _googleShoppingSettings.StaticFileName);
                if (System.IO.File.Exists(localFilePath))
                    model.GeneratedFiles.Add(new FeedGoogleShoppingModel.GeneratedFileModel
                    {
                        StoreName = store.Name,
                        FileUrl = $"{_webHelper.GetStoreLocation(false)}files/exportimport/{store.Id}-cimri-{_googleShoppingSettings.StaticFileName}"
                    });
                if (System.IO.File.Exists(localFilePaths))
                    model.GeneratedFiles.Add(new FeedGoogleShoppingModel.GeneratedFileModel
                    {
                        StoreName = store.Name,
                        FileUrl = $"{_webHelper.GetStoreLocation(false)}files/exportimport/{store.Id}-google-{_googleShoppingSettings.StaticFileName}"
                    });
            }

            return View("~/Plugins/Feed.GoogleShopping/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult Configure(FeedGoogleShoppingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Configure();
            }

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var googleShoppingSettings = _settingService.LoadSetting<GoogleShoppingSettings>(storeScope);

            //save settings
            googleShoppingSettings.ProductPictureSize = model.ProductPictureSize;
            googleShoppingSettings.PassShippingInfoWeight = model.PassShippingInfoWeight;
            googleShoppingSettings.PassShippingInfoDimensions = model.PassShippingInfoDimensions;
            googleShoppingSettings.PricesConsiderPromotions = model.PricesConsiderPromotions;
            googleShoppingSettings.CurrencyId = model.CurrencyId;
            googleShoppingSettings.DefaultGoogleCategory = model.DefaultGoogleCategory;
            googleShoppingSettings.StoreId = model.StoreId;

            //_settingService.SaveSetting(_googleShoppingSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(googleShoppingSettings, x => x.CurrencyId, model.CurrencyId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleShoppingSettings, x => x.DefaultGoogleCategory, model.DefaultGoogleCategory_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleShoppingSettings, x => x.PassShippingInfoDimensions, model.PassShippingInfoDimensions_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleShoppingSettings, x => x.PassShippingInfoWeight, model.PassShippingInfoWeight_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleShoppingSettings, x => x.PricesConsiderPromotions, model.PricesConsiderPromotions_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleShoppingSettings, x => x.ProductPictureSize, model.ProductPictureSize_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleShoppingSettings, x => x.StoreId,model.StoreId_OvverideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            //redisplay the form
            return Configure();
        }
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("generate")]
        public IActionResult GenerateFeed(FeedGoogleShoppingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("PromotionFeed.Froogle");
                if (pluginDescriptor == null)
                    throw new Exception("Cannot load the plugin");

                //plugin
                var plugin = pluginDescriptor.Instance() as GoogleShoppingService;
                if (plugin == null)
                    throw new Exception("Cannot load the plugin");

                var stores = new List<Store>();
                var storeById = _storeService.GetStoreById(_googleShoppingSettings.StoreId);
                if (storeById != null)
                    stores.Add(storeById);
                else
                    stores.AddRange(_storeService.GetAllStores());

                foreach (var store in stores)
                    plugin.GenerateStaticFile(store);

                SuccessNotification(_localizationService.GetResource("Plugins.Feed.GoogleShopping.SuccessResult"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                _logger.Error(exc.Message, exc);
            }

            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("generatecimri")]
        public IActionResult GenerateFeedCimri(FeedGoogleShoppingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("PromotionFeed.Froogle");
                if (pluginDescriptor == null)
                    throw new Exception("Cannot load the plugin");

                //plugin
                var plugin = pluginDescriptor.Instance() as GoogleShoppingService;
                if (plugin == null)
                    throw new Exception("Cannot load the plugin");

                var stores = new List<Store>();
                var storeById = _storeService.GetStoreById(_googleShoppingSettings.StoreId);
                if (storeById != null)
                    stores.Add(storeById);
                else
                    stores.AddRange(_storeService.GetAllStores());

                foreach (var store in stores)
                    plugin.GenerateStaticFileCimri(store);

                SuccessNotification(_localizationService.GetResource("Plugins.Feed.GoogleShopping.SuccessResult"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                _logger.Error(exc.Message, exc);
            }

            return Configure();
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GoogleProductListe(DataSourceRequest command, FeedGoogleShoppingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedKendoGridJson();
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.SearchProducts(pageIndex: command.Page - 1,
                pageSize: command.PageSize, showHidden: true, vendorId: model.SearchVendorId,
                keywords: model.SearchProductName);
            var productsModel = products
                .Select(x =>
                            {
                                var gModel = new FeedGoogleShoppingModel.FeedGoogleProductModel
                                {
                                    ProductId = x.Id,
                                    ProductName = x.Name,
                                    VendorName = _vendorService.GetVendorById(x.VendorId).Name,
                                    Price = x.Price

                                };
                                var googleProduct = _googleService.GetByProductId(x.Id);
                                if (googleProduct != null)
                                {
                                    gModel.Cimri = googleProduct.Cimri;
                                    gModel.GoogleShop = googleProduct.GoogleShop;

                                }

                                return gModel;
                            })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productsModel,
                Total = products.TotalCount
            };

            return Json(gridModel);
        }
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GoogleProductList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedKendoGridJson();

            var products = _productService.SearchProducts(pageIndex: command.Page - 1,
                pageSize: command.PageSize, showHidden: true);
            var productsModel = products
                .Select(x =>
                            {
                                var gModel = new FeedGoogleShoppingModel.GoogleProductModel
                                {
                                    ProductId = x.Id,
                                    ProductName = x.Name

                                };
                                var googleProduct = _googleService.GetByProductId(x.Id);
                                if (googleProduct != null)
                                {
                                    gModel.GoogleCategory = googleProduct.Taxonomy;
                                    gModel.Gender = googleProduct.Gender;
                                    gModel.AgeGroup = googleProduct.AgeGroup;
                                    gModel.Color = googleProduct.Color;
                                    gModel.GoogleSize = googleProduct.Size;
                                    gModel.CustomGoods = googleProduct.CustomGoods;
                                    gModel.Cimri = googleProduct.Cimri;
                                    gModel.GoogleShop = googleProduct.GoogleShop;
                                }

                                return gModel;
                            })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productsModel,
                Total = products.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GoogleProductUpdate(FeedGoogleShoppingModel.GoogleProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var googleProduct = _googleService.GetByProductId(model.ProductId);
            if (googleProduct != null)
            {

                googleProduct.Taxonomy = model.GoogleCategory;
                googleProduct.Gender = model.Gender;
                googleProduct.AgeGroup = model.AgeGroup;
                googleProduct.Color = model.Color;
                googleProduct.Size = model.GoogleSize;
                googleProduct.CustomGoods = model.CustomGoods;
                _googleService.UpdateGoogleProductRecord(googleProduct);
            }
            else
            {
                //insert
                googleProduct = new GoogleProductRecord
                {
                    ProductId = model.ProductId,
                    Taxonomy = model.GoogleCategory,
                    Gender = model.Gender,
                    AgeGroup = model.AgeGroup,
                    Color = model.Color,
                    Size = model.GoogleSize,
                    CustomGoods = model.CustomGoods
                };
                _googleService.InsertGoogleProductRecord(googleProduct);
            }

            return new NullJsonResult();
        }
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GoogleProductUpdateBool(IEnumerable<FeedGoogleShoppingModel.FeedGoogleProductModel> products)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");
            if (products != null)
            {
                foreach (var pModel in products)
                {
                    var googleProduct = _googleService.GetByProductId(pModel.ProductId);
                    if (googleProduct != null)
                    {
                        googleProduct.Cimri = pModel.Cimri;
                        googleProduct.GoogleShop = pModel.GoogleShop;
                        _googleService.UpdateGoogleProductRecord(googleProduct);
                    }
                    else
                    {
                        googleProduct = new GoogleProductRecord
                        {
                            ProductId = pModel.ProductId,
                            Cimri = pModel.Cimri,
                            GoogleShop = pModel.GoogleShop
                        };
                        _googleService.InsertGoogleProductRecord(googleProduct);
                    }
                }
            }
            return new NullJsonResult();
        }


        #endregion
    }
}