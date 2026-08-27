namespace GenerateMigrationScript
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBajajCall = new System.Windows.Forms.CheckBox();
            this.GenMig = new System.Windows.Forms.CheckBox();
            this.btnGenCommand = new System.Windows.Forms.Button();
            this.txtSqlScript = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 100);
            this.label1.TabIndex = 0;
            this.label1.Text = "ConnectionString";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConnectionString.Location = new System.Drawing.Point(88, 0);
            this.txtConnectionString.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(544, 100);
            this.txtConnectionString.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnBajajCall);
            this.panel1.Controls.Add(this.GenMig);
            this.panel1.Controls.Add(this.btnGenCommand);
            this.panel1.Controls.Add(this.txtConnectionString);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(632, 100);
            this.panel1.TabIndex = 4;
            // 
            // btnBajajCall
            // 
            this.btnBajajCall.AutoSize = true;
            this.btnBajajCall.Location = new System.Drawing.Point(9, 77);
            this.btnBajajCall.Margin = new System.Windows.Forms.Padding(2);
            this.btnBajajCall.Name = "btnBajajCall";
            this.btnBajajCall.Size = new System.Drawing.Size(75, 17);
            this.btnBajajCall.TabIndex = 5;
            this.btnBajajCall.Text = "Bajaj Data";
            this.btnBajajCall.UseVisualStyleBackColor = true;
            // 
            // GenMig
            // 
            this.GenMig.AutoSize = true;
            this.GenMig.Location = new System.Drawing.Point(9, 57);
            this.GenMig.Margin = new System.Windows.Forms.Padding(2);
            this.GenMig.Name = "GenMig";
            this.GenMig.Size = new System.Drawing.Size(63, 17);
            this.GenMig.TabIndex = 4;
            this.GenMig.Text = "GenMig";
            this.GenMig.UseVisualStyleBackColor = true;
            // 
            // btnGenCommand
            // 
            this.btnGenCommand.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnGenCommand.Location = new System.Drawing.Point(558, 0);
            this.btnGenCommand.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnGenCommand.Name = "btnGenCommand";
            this.btnGenCommand.Size = new System.Drawing.Size(74, 100);
            this.btnGenCommand.TabIndex = 3;
            this.btnGenCommand.Text = "Gen Script";
            this.btnGenCommand.UseVisualStyleBackColor = true;
            this.btnGenCommand.Click += new System.EventHandler(this.btnGenCommand_Click_1);
            // 
            // txtSqlScript
            // 
            this.txtSqlScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSqlScript.Location = new System.Drawing.Point(0, 100);
            this.txtSqlScript.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSqlScript.Multiline = true;
            this.txtSqlScript.Name = "txtSqlScript";
            this.txtSqlScript.ReadOnly = true;
            this.txtSqlScript.Size = new System.Drawing.Size(632, 204);
            this.txtSqlScript.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 304);
            this.Controls.Add(this.txtSqlScript);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Form1";
            this.Text = "Sql Migration Script generator";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnGenCommand;
        private System.Windows.Forms.TextBox txtSqlScript;
        private System.Windows.Forms.CheckBox GenMig;
        private System.Windows.Forms.CheckBox btnBajajCall;
    }
}

