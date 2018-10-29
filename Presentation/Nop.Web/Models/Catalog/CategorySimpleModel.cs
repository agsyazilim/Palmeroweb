using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public class CategorySimpleModel : BaseNopEntityModel
    {
        public CategorySimpleModel()
        {
            SubCategories = new List<CategorySimpleModel>();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public bool IncludeInTopMenu { get; set; }
        public PictureModel PictureModel { get; set; }
        public bool ShowCategoryPictureMenu { get; set; }
        public bool MegaMenu { get; set; }
        public List<CategorySimpleModel> SubCategories { get; set; }
    }
}