namespace SmzdmExcelAddin
{
    partial class SmzdmUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Launch = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Launch
            // 
            this.Launch.Location = new System.Drawing.Point(59, 165);
            this.Launch.Margin = new System.Windows.Forms.Padding(6);
            this.Launch.Name = "Launch";
            this.Launch.Size = new System.Drawing.Size(266, 78);
            this.Launch.TabIndex = 0;
            this.Launch.Text = "Launch";
            this.Launch.UseVisualStyleBackColor = true;
            this.Launch.Click += new System.EventHandler(this.LaunchButton_Clicked);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(59, 564);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(585, 29);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "C:\\Users\\jiatwang\\Documents\\GitHub\\smzdm\\WebBrowser\\bin\\Release\\SmzdmBot.exe";
            //this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(54, 522);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "exe file path";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(59, 660);
            this.textBox3.Margin = new System.Windows.Forms.Padding(6);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(583, 29);
            this.textBox3.TabIndex = 5;
            this.textBox3.Text = "C:\\Users\\jiatwang\\Documents\\GitHub\\smzdm_output.txt";
            //this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(59, 310);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(266, 78);
            this.button1.TabIndex = 6;
            this.button1.Text = "Reload";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ReLoadButton_Clicked);
            // 
            // SmzdmUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Launch);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "SmzdmUserControl";
            this.Size = new System.Drawing.Size(715, 884);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Launch;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button button1;
    }
}
