namespace Map_Editor
{
    partial class FrmSetAnimation
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
            this.btnSetAnimation = new System.Windows.Forms.Button();
            this.txtAnimationTick = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAnimationFrame = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkBlend = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnSetAnimation
            // 
            this.btnSetAnimation.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSetAnimation.Location = new System.Drawing.Point(99, 86);
            this.btnSetAnimation.Name = "btnSetAnimation";
            this.btnSetAnimation.Size = new System.Drawing.Size(75, 23);
            this.btnSetAnimation.TabIndex = 11;
            this.btnSetAnimation.Text = "确定 ok";
            this.btnSetAnimation.UseVisualStyleBackColor = true;
            this.btnSetAnimation.Click += new System.EventHandler(this.btnSetAnimation_Click);
            // 
            // txtAnimationTick
            // 
            this.txtAnimationTick.Location = new System.Drawing.Point(264, 49);
            this.txtAnimationTick.Name = "txtAnimationTick";
            this.txtAnimationTick.Size = new System.Drawing.Size(50, 21);
            this.txtAnimationTick.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(97, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "动画间隔 Animation interval ";
            // 
            // txtAnimationFrame
            // 
            this.txtAnimationFrame.Location = new System.Drawing.Point(264, 22);
            this.txtAnimationFrame.Name = "txtAnimationFrame";
            this.txtAnimationFrame.Size = new System.Drawing.Size(50, 21);
            this.txtAnimationFrame.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(97, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "动画帧数 Animation frames ";
            // 
            // chkBlend
            // 
            this.chkBlend.AutoSize = true;
            this.chkBlend.Location = new System.Drawing.Point(12, 37);
            this.chkBlend.Name = "chkBlend";
            this.chkBlend.Size = new System.Drawing.Size(84, 16);
            this.chkBlend.TabIndex = 6;
            this.chkBlend.Text = "混合 blend";
            this.chkBlend.UseVisualStyleBackColor = true;
            // 
            // FrmSetAnimation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 135);
            this.Controls.Add(this.btnSetAnimation);
            this.Controls.Add(this.txtAnimationTick);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtAnimationFrame);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkBlend);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSetAnimation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmSetAnimation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSetAnimation;
        private System.Windows.Forms.TextBox txtAnimationTick;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAnimationFrame;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkBlend;
    }
}