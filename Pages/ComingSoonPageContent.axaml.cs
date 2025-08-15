using Avalonia.Controls;

namespace AstroGathering.Pages
{
    public partial class ComingSoonPageContent : UserControl
    {
        public ComingSoonPageContent()
        {
            InitializeComponent();
        }

        public ComingSoonPageContent(string pageName, string description) : this()
        {
            SetPageContent(pageName, description);
        }

        private void SetPageContent(string pageName, string description)
        {
            if (PageTitle != null)
                PageTitle.Text = $"{pageName} Coming Soon";
                
            if (PageDescription != null)
                PageDescription.Text = description;
                
            if (PageIcon != null)
            {
                PageIcon.Text = pageName switch
                {
                    "Upload" => "📸",
                    "Gallery" => "🖼️",
                    "Admin" => "⚡",
                    "Help" => "❓",
                    _ => "🚀"
                };
            }
        }
    }
}
