namespace WinFormsApp1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.TextBox txtSourceFile;
        private System.Windows.Forms.Button btnExecute;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnSelectFile = new Button();
            txtSourceFile = new TextBox();
            btnExecute = new Button();
            SuspendLayout();
            // 
            // btnSelectFile
            // 
            btnSelectFile.Location = new Point(35, 12);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(285, 27);
            btnSelectFile.TabIndex = 0;
            btnSelectFile.Text = "Выберите исходный файл";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += btnSelectFile_Click;
            // 
            // txtSourceFile
            // 
            txtSourceFile.Location = new Point(51, 45);
            txtSourceFile.Name = "txtSourceFile";
            txtSourceFile.Size = new Size(250, 23);
            txtSourceFile.TabIndex = 1;
            // 
            // btnExecute
            // 
            btnExecute.Location = new Point(113, 84);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(120, 23);
            btnExecute.TabIndex = 4;
            btnExecute.Text = "Выполнить";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(358, 122);
            Controls.Add(btnExecute);
            Controls.Add(txtSourceFile);
            Controls.Add(btnSelectFile);
            Name = "Form1";
            Text = "Загрузчик файлов";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
