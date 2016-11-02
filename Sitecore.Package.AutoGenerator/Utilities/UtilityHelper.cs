
namespace Sitecore.Package.AutoGenerator.Utilities
{
    using System;
    using System.IO;
    using Sitecore.Exceptions;
    using Sitecore.Globalization;
    using Sitecore.Package.AutoGenerator.Core.UI;
    using Sitecore.Shell.Applications.Install;
    using Sitecore.Shell.Applications.Install.Dialogs;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Sheer;

    public class UtilityHelper
    {
        public static void Upload(ClientPipelineArgs args, Edit fileEdit)
        {
            if (!args.IsPostBack)
            {
                UploadForm.Show(ApplicationContext.PackagePath, true);
                args.WaitForPostBack();
            }
            else
            {
                if (!args.Result.StartsWith("ok:", StringComparison.InvariantCulture))
                    return;
                string[] strArray = args.Result.Substring("ok:".Length).Split('|');
                if (strArray.Length < 1 || fileEdit == null)
                    return;
                fileEdit.Value = strArray[0];
            }
        }
    }
}
