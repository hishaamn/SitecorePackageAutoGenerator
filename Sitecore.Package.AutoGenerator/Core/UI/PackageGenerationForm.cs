
namespace Sitecore.Package.AutoGenerator.Core.UI
{
    using System;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.IO;
    using Sitecore.Jobs;
    using Sitecore.Package.AutoGenerator.Core.Service;
    using Sitecore.Package.AutoGenerator.Utilities;
    using Sitecore.Shell.Framework;
    using Sitecore.StringExtensions;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Pages;
    using Sitecore.Web.UI.Sheer;

    public class PackageGenerationForm : WizardForm
    {
        protected Scrollbox FileList;

        protected Edit PackageFile;
        protected Edit PackageName;
        protected Edit Version;
        protected Edit Author;
        protected Edit Publisher;

        protected Literal ErrorPackageRequired;
        protected Literal ErrorPackageName;

        private const string TextJobHandle = "JobHandle";

        private string JobHandle
        {
            get
            {
                return StringUtil.GetString(this.ServerProperties[TextJobHandle]);
            }
            set
            {
                this.ServerProperties[TextJobHandle] = value;
            }
        }

        private bool Cancelling
        {
            set
            {
                Context.ClientPage.ServerProperties["__cancelling"] = value;
            }
        }

        public string ResultFile
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["ResultFile"]);
            }
            set
            {
                Context.ClientPage.ServerProperties["ResultFile"] = value;
            }
        }

        private string OriginalNextButtonHeader
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["next-header"]);
            }
            set
            {
                Context.ClientPage.ServerProperties["next-header"] = value;
            }
        }

        private bool Successful
        {
            get
            {
                object obj = this.ServerProperties["Successful"];
                if (!(obj is bool))
                    return true;
                return (bool)obj;
            }
            set
            {
                this.ServerProperties["Successful"] = value;
            }
        }

        public void AskOverwrite(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                Context.ClientPage.ClientResponse.Confirm(Translate.Text("File exists. Do you wish to overwrite?"));
                args.WaitForPostBack();
            }
            else
            {
                if (!args.HasResult || args.Result != "yes")
                {
                    return;
                }

                Context.ClientPage.ClientResponse.SetDialogValue(args.Result);
                Context.ClientPage.ServerProperties["__NameConfirmed"] = true;
                this.Next();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Context.ClientPage.IsEvent)
            {
                this.OriginalNextButtonHeader = this.NextButton.Header;

                this.ErrorPackageRequired.Visible = false;
                this.ErrorPackageName.Visible = false;
            }

            base.OnLoad(e);
        }

        protected override void OnCancel(object sender, EventArgs formEventArgs)
        {
            this.Cancel();
        }

        public new void Cancel()
        {
            if (this.Pages.IndexOf(this.Active) == this.Pages.Count - 1)
            {
                this.EndWizard();
            }
            else
            {
                this.Cancelling = true;
                Context.ClientPage.Start(this, "Confirmation");
            }
        }

        protected override void EndWizard()
        {
            Windows.Close();
        }

        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            if (page == "LoadInformation" && newpage == "Generator")
            {
                if (this.PackageFile.Value.IsNullOrEmpty() || this.PackageName.Value.IsNullOrEmpty())
                {
                    this.ErrorPackageRequired.Visible = this.PackageFile.Value.IsNullOrEmpty();

                    this.ErrorPackageName.Visible = this.PackageName.Value.IsNullOrEmpty();

                    return false;
                }
            }

            base.ActivePageChanging(page, ref newpage);
            return true;

        }

        protected override void ActivePageChanged(string page, string oldPage)
        {
            base.ActivePageChanged(page, oldPage);
            this.NextButton.Header = this.OriginalNextButtonHeader;

            if (page == "Generator")
            {
                this.BackButton.Disabled = true;
                this.NextButton.Disabled = true;
                this.CancelButton.Disabled = true;
                Context.ClientPage.SendMessage(this, "pgenerator:startGeneration");
            }
            if (page == "Ready")
            {
                this.NextButton.Header = Translate.Text("Generate Package");
            }

            if (page == "LastPage")
            {
                this.BackButton.Disabled = true;
            }

            if (this.Successful)
            {
                return;
            }

            this.CancelButton.Header = Translate.Text("Close");

            this.Successful = true;
        }

        [HandleMessage("pgenerator:startGeneration")]
        protected void StartGeneration(Message message)
        {
            var packageGeneratorFolder = Settings.GetSetting("PackageGeneratorFolder");

            var filename = FileUtil.MakePath(Settings.PackagePath, string.Format(@"\{0}\{1}", packageGeneratorFolder, this.PackageFile.Value));

            this.ResultFile = MainUtil.MapPath(string.Format("{0}/{1}/{2}.zip", Settings.PackagePath, packageGeneratorFolder, this.PackageName.Value));

            if (FileUtil.IsFile(filename))
            {
                this.StartTask(filename);
            }
            else
            {
                Context.ClientPage.ClientResponse.Alert("Package not found");
                this.Active = "Ready";
                this.BackButton.Disabled = true;
            }
        }

        [HandleMessage("pgenerator:upload", true)]
        protected void Upload(ClientPipelineArgs args)
        {
            UtilityHelper.Upload(args, this.PackageFile);
        }

        [HandleMessage("buildpackage:download")]
        protected void DownloadPackage(Message message)
        {
            string resultFile = this.ResultFile;
            if (resultFile.Length > 0)
                Context.ClientPage.ClientResponse.Download(resultFile);
            else
                Context.ClientPage.ClientResponse.Alert("Could not download package");
        }

        private void StartTask(string packageFile)
        {
            var parameters = new object[] { packageFile, this.PackageName.Value, this.Version.Value, this.Author.Value, this.Publisher.Value, "csv" };

            var generator = new CustomPackageGenerator(parameters);

            var options = new JobOptions("Package Generation", "[Custom] Package Generator", Context.Site.Name, generator, "GeneratePackage");

            var job = JobManager.Start(options);

            this.JobHandle = job.Handle.ToString();

            SheerResponse.Timer("CheckStatus", 5);
        }

        public void CheckStatus()
        {
            try
            {
                var job = JobManager.GetJob(Handle.Parse(this.JobHandle));

                if (job != null)
                {
                    var status = job.Status;
                    var state = status.State;

                    if (status == null)
                    {
                        throw new Exception("The generating process was unexpectedly interrupted.");
                    }

                    if (state == JobState.Running)
                    {
                        this.NextButton.Disabled = true;
                        this.BackButton.Disabled = false;
                        this.CancelButton.Disabled = false;
                    }
                    else if (state == JobState.Finished)
                    {
                        this.Active = "LastPage";
                        this.BackButton.Disabled = true;

                        return;
                    }

                    SheerResponse.Timer("CheckStatus", 5);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message, this);
            }
        }
    }
}
