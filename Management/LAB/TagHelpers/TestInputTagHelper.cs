using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Management.TagHelpers
{
    [HtmlTargetElement("myinput")]
    public class TestInputTagHelper : TagHelper
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string TitleToolTip { get; set; }

        public string Valid { get; set; }
       
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.Attributes.SetAttribute("id", Id);
            output.Attributes.SetAttribute("name", Name);
            output.Attributes.SetAttribute("type", Type);
            output.Attributes.SetAttribute("class", "form-control" + " " + Valid);
            output.Attributes.SetAttribute("data-toggle", "tooltip");
            output.Attributes.SetAttribute("title", TitleToolTip);
            output.Attributes.SetAttribute("data-placement", "top");
        }          
    }
}