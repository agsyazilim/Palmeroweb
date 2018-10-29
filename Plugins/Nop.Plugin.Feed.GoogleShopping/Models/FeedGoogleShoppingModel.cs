using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Feed.GoogleShopping.Models
{
    public class FeedGoogleShoppingModel
    {
        public FeedGoogleShoppingModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCurrencies = new List<SelectListItem>();
            AvailableGoogleCategories = new List<SelectListItem>();
            GeneratedFiles = new List<GeneratedFileModel>();
            AvailableVendors = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Store")]
        public int StoreId { get; set; }

        public bool StoreId_OvverideForStore { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.ProductPictureSize")]
        public int ProductPictureSize { get; set; }
        public bool ProductPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Currency")]
        public int CurrencyId { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }
        public bool CurrencyId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.DefaultGoogleCategory")]
        public string DefaultGoogleCategory { get; set; }
        public IList<SelectListItem> AvailableGoogleCategories { get; set; }
        public bool DefaultGoogleCategory_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.PassShippingInfoWeight")]
        public bool PassShippingInfoWeight { get; set; }
        public bool PassShippingInfoWeight_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.PassShippingInfoDimensions")]
        public bool PassShippingInfoDimensions { get; set; }
        public bool PassShippingInfoDimensions_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.PricesConsiderPromotions")]
        public bool PricesConsiderPromotions { get; set; }
        public bool PricesConsiderPromotions_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Feed.GoogleShopping.StaticFilePath")]
        public IList<GeneratedFileModel> GeneratedFiles { get; set; }
        public int SearchVendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
        public string SearchProductName { get; set; }

        public class GeneratedFileModel : BaseNopModel
        {
            public string StoreName { get; set; }
            public string FileUrl { get; set; }
        }

        public class GoogleProductModel : BaseNopModel
        {
            public int ProductId { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.ProductName")]
            public string ProductName { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.GoogleCategory")]
            public string GoogleCategory { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.Gender")]
            public string Gender { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.AgeGroup")]
            public string AgeGroup { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.Color")]
            public string Color { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.Size")]
            public string GoogleSize { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.CustomGoods")]
            public bool CustomGoods { get; set; }
            public bool Cimri { get; set; }
            public bool GoogleShop { get; set; }
        }
        public class FeedGoogleProductModel : BaseNopModel
        {
            public int ProductId { get; set; }

            [NopResourceDisplayName("Plugins.Feed.GoogleShopping.Products.ProductName")]
            public string ProductName { get; set; }
            public bool Cimri { get; set; }
            public bool GoogleShop { get; set; }
            public string VendorName { get; set; }
            public decimal Price { get; set; }
        }
    }
}