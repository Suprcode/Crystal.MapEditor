namespace Map_Editor
{
    partial class FrmSetDoor
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
            this.chkCoreDoor = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDoorIndex = new System.Windows.Forms.TextBox();
            this.txtDoorOffSet = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSetDoor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkCoreDoor
            // 
            this.chkCoreDoor.AutoSize = true;
            this.chkCoreDoor.Location = new System.Drawing.Point(24, 43);
            this.chkCoreDoor.Name = "chkCoreDoor";
            this.chkCoreDoor.Size = new System.Drawing.Size(132, 16);
            this.chkCoreDoor.TabIndex = 0;
            this.chkCoreDoor.Text = "实体门 entity door";
            this.chkCoreDoor.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(162, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "门索引 door index";
            // 
            // txtDoorIndex
            // 
            this.txtDoorIndex.Location = new System.Drawing.Point(284, 25);
            this.txtDoorIndex.Name = "txtDoorIndex";
            this.txtDoorIndex.Size = new System.Drawing.Size(50, 21);
            this.txtDoorIndex.TabIndex = 2;
            // 
            // txtDoorOffSet
            // 
            this.txtDoorOffSet.Location = new System.Drawing.Point(284, 52);
            this.txtDoorOffSet.Name = "txtDoorOffSet";
            this.txtDoorOffSet.Size = new System.Drawing.Size(50, 21);
            this.txtDoorOffSet.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(162, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "门偏移 door offSet";
            // 
            // btnSetDoor
            // 
            this.btnSetDoor.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSetDoor.Location = new System.Drawing.Point(140, 94);
            this.btnSetDoor.Name = "btnSetDoor";
            this.btnSetDoor.Size = new System.Drawing.Size(75, 23);
            this.btnSetDoor.TabIndex = 5;
            this.btnSetDoor.Text = "确定 ok";
            this.btnSetDoor.UseVisualStyleBackColor = true;
            this.btnSetDoor.Click += new System.EventHandler(this.btnSetDoor_Click);
            // 
            // FrmSetDoor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 145);
            this.Controls.Add(this.btnSetDoor);
            this.Controls.Add(this.txtDoorOffSet);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtDoorIndex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkCoreDoor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSetDoor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SetDoor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkCoreDoor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDoorIndex;
        private System.Windows.Forms.TextBox txtDoorOffSet;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSetDoor;
    }
}