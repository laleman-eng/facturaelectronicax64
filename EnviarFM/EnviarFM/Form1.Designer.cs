namespace EnviarFM
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TipoDoc = new System.Windows.Forms.ComboBox();
            this.txSerie = new System.Windows.Forms.TextBox();
            this.txFolio = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(105, 239);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Enviar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(269, 239);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Salir";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(74, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Tipo documento";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(74, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Serie";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(74, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Folio";
            // 
            // TipoDoc
            // 
            this.TipoDoc.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.TipoDoc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TipoDoc.FormattingEnabled = true;
            this.TipoDoc.Items.AddRange(new object[] {
            "01 - Factura",
            "01A - Factura de Anticipo",
            "03 - Boleta Venta",
            "07 - Nota de Credito",
            "08 - Nota de Debito"});
            this.TipoDoc.Location = new System.Drawing.Point(191, 67);
            this.TipoDoc.Name = "TipoDoc";
            this.TipoDoc.Size = new System.Drawing.Size(195, 21);
            this.TipoDoc.TabIndex = 5;
            // 
            // txSerie
            // 
            this.txSerie.Location = new System.Drawing.Point(191, 98);
            this.txSerie.Name = "txSerie";
            this.txSerie.Size = new System.Drawing.Size(100, 20);
            this.txSerie.TabIndex = 6;
            // 
            // txFolio
            // 
            this.txFolio.Location = new System.Drawing.Point(191, 130);
            this.txFolio.Name = "txFolio";
            this.txFolio.Size = new System.Drawing.Size(100, 20);
            this.txFolio.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 282);
            this.Controls.Add(this.txFolio);
            this.Controls.Add(this.txSerie);
            this.Controls.Add(this.TipoDoc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Enviar a Factura Movil";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox TipoDoc;
        private System.Windows.Forms.TextBox txSerie;
        private System.Windows.Forms.TextBox txFolio;
    }
}

