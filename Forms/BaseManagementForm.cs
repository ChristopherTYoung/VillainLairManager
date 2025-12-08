using System;
using System.Windows.Forms;
using VillainLairManager.Repositories;

namespace VillainLairManager.Forms
{
    public partial class BaseManagementForm : Form
    {
        private readonly ISecretBaseRepository _baseRepository;

        public BaseManagementForm(ISecretBaseRepository baseRepository)
        {
            _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Secret Base Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Base Management Form",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            this.Controls.Add(lblStub);
        }
    }
}
