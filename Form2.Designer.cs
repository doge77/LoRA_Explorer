namespace LoRA_Explorer {
    partial class SettingForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.ClearPromptWhenSelectNegativeCheckBox = new System.Windows.Forms.CheckBox();
            this.ClearPromptWhenSwitchItemCheckBox = new System.Windows.Forms.CheckBox();
            this.SaveChangesInstantlyCheckBox = new System.Windows.Forms.CheckBox();
            this.ClearDataButton = new System.Windows.Forms.Button();
            this.ItemWidthTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.ItemWidthTrackBarLabel = new System.Windows.Forms.Label();
            this.ItemHeightTrackBarLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ItemHeightTrackBar = new System.Windows.Forms.TrackBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.BackUpDataFilesButton = new System.Windows.Forms.Button();
            this.CopyPromptWhenSelectPromptCheckBox = new System.Windows.Forms.CheckBox();
            this.ShowGradeOnThumbnailCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ItemsPerPageTextBox = new System.Windows.Forms.NumericUpDown();
            this.ChangeLoraToLycoCheckBox = new System.Windows.Forms.CheckBox();
            this.ChangeJpgToPngCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.ItemWidthTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemHeightTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsPerPageTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ClearPromptWhenSelectNegativeCheckBox
            // 
            this.ClearPromptWhenSelectNegativeCheckBox.AutoSize = true;
            this.ClearPromptWhenSelectNegativeCheckBox.Location = new System.Drawing.Point(23, 85);
            this.ClearPromptWhenSelectNegativeCheckBox.Name = "ClearPromptWhenSelectNegativeCheckBox";
            this.ClearPromptWhenSelectNegativeCheckBox.Size = new System.Drawing.Size(308, 16);
            this.ClearPromptWhenSelectNegativeCheckBox.TabIndex = 7;
            this.ClearPromptWhenSelectNegativeCheckBox.Text = "네거티브 프롬프트 입력시 하단 프롬프트박스 지우기";
            this.ClearPromptWhenSelectNegativeCheckBox.UseVisualStyleBackColor = true;
            this.ClearPromptWhenSelectNegativeCheckBox.CheckedChanged += new System.EventHandler(this.ClearPromptWhenSelectNegativeCheckBox_CheckedChanged);
            // 
            // ClearPromptWhenSwitchItemCheckBox
            // 
            this.ClearPromptWhenSwitchItemCheckBox.AutoSize = true;
            this.ClearPromptWhenSwitchItemCheckBox.Location = new System.Drawing.Point(23, 63);
            this.ClearPromptWhenSwitchItemCheckBox.Name = "ClearPromptWhenSwitchItemCheckBox";
            this.ClearPromptWhenSwitchItemCheckBox.Size = new System.Drawing.Size(244, 16);
            this.ClearPromptWhenSwitchItemCheckBox.TabIndex = 6;
            this.ClearPromptWhenSwitchItemCheckBox.Text = "아이템 변경시 하단 프롬프트박스 지우기";
            this.ClearPromptWhenSwitchItemCheckBox.UseVisualStyleBackColor = true;
            this.ClearPromptWhenSwitchItemCheckBox.CheckedChanged += new System.EventHandler(this.ClearPromptWhenSwitchItemCheckBox_CheckedChanged);
            // 
            // SaveChangesInstantlyCheckBox
            // 
            this.SaveChangesInstantlyCheckBox.AutoSize = true;
            this.SaveChangesInstantlyCheckBox.Location = new System.Drawing.Point(23, 107);
            this.SaveChangesInstantlyCheckBox.Name = "SaveChangesInstantlyCheckBox";
            this.SaveChangesInstantlyCheckBox.Size = new System.Drawing.Size(268, 16);
            this.SaveChangesInstantlyCheckBox.TabIndex = 8;
            this.SaveChangesInstantlyCheckBox.Text = "프롬프트 꾸러미, data 파일 수정시 바로 저장";
            this.SaveChangesInstantlyCheckBox.UseVisualStyleBackColor = true;
            this.SaveChangesInstantlyCheckBox.CheckedChanged += new System.EventHandler(this.SaveChangesInstantlyCheckBox_CheckedChanged);
            // 
            // ClearDataButton
            // 
            this.ClearDataButton.BackColor = System.Drawing.Color.Silver;
            this.ClearDataButton.ForeColor = System.Drawing.Color.Black;
            this.ClearDataButton.Location = new System.Drawing.Point(23, 368);
            this.ClearDataButton.Name = "ClearDataButton";
            this.ClearDataButton.Size = new System.Drawing.Size(308, 39);
            this.ClearDataButton.TabIndex = 9;
            this.ClearDataButton.Text = "data 파일 일괄 삭제";
            this.ClearDataButton.UseVisualStyleBackColor = false;
            this.ClearDataButton.Click += new System.EventHandler(this.ClearDataButton_Click);
            // 
            // ItemWidthTrackBar
            // 
            this.ItemWidthTrackBar.AutoSize = false;
            this.ItemWidthTrackBar.LargeChange = 1;
            this.ItemWidthTrackBar.Location = new System.Drawing.Point(124, 226);
            this.ItemWidthTrackBar.Maximum = 30;
            this.ItemWidthTrackBar.Minimum = 10;
            this.ItemWidthTrackBar.Name = "ItemWidthTrackBar";
            this.ItemWidthTrackBar.Size = new System.Drawing.Size(167, 24);
            this.ItemWidthTrackBar.TabIndex = 9;
            this.ItemWidthTrackBar.TickFrequency = 10;
            this.ItemWidthTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.ItemWidthTrackBar.Value = 10;
            this.ItemWidthTrackBar.ValueChanged += new System.EventHandler(this.ItemWidthTrackBar_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 232);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "아이템 가로 크기";
            // 
            // ItemWidthTrackBarLabel
            // 
            this.ItemWidthTrackBarLabel.AutoSize = true;
            this.ItemWidthTrackBarLabel.Location = new System.Drawing.Point(297, 231);
            this.ItemWidthTrackBarLabel.Name = "ItemWidthTrackBarLabel";
            this.ItemWidthTrackBarLabel.Size = new System.Drawing.Size(23, 12);
            this.ItemWidthTrackBarLabel.TabIndex = 12;
            this.ItemWidthTrackBarLabel.Text = "100";
            // 
            // ItemHeightTrackBarLabel
            // 
            this.ItemHeightTrackBarLabel.AutoSize = true;
            this.ItemHeightTrackBarLabel.Location = new System.Drawing.Point(297, 258);
            this.ItemHeightTrackBarLabel.Name = "ItemHeightTrackBarLabel";
            this.ItemHeightTrackBarLabel.Size = new System.Drawing.Size(23, 12);
            this.ItemHeightTrackBarLabel.TabIndex = 15;
            this.ItemHeightTrackBarLabel.Text = "150";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 259);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "아이템 세로 크기";
            // 
            // ItemHeightTrackBar
            // 
            this.ItemHeightTrackBar.AutoSize = false;
            this.ItemHeightTrackBar.LargeChange = 1;
            this.ItemHeightTrackBar.Location = new System.Drawing.Point(124, 253);
            this.ItemHeightTrackBar.Maximum = 40;
            this.ItemHeightTrackBar.Minimum = 10;
            this.ItemHeightTrackBar.Name = "ItemHeightTrackBar";
            this.ItemHeightTrackBar.Size = new System.Drawing.Size(167, 24);
            this.ItemHeightTrackBar.TabIndex = 13;
            this.ItemHeightTrackBar.TickFrequency = 10;
            this.ItemHeightTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.ItemHeightTrackBar.Value = 15;
            this.ItemHeightTrackBar.ValueChanged += new System.EventHandler(this.ItemHeightTrackBar_ValueChanged);
            // 
            // BackUpDataFilesButton
            // 
            this.BackUpDataFilesButton.BackColor = System.Drawing.Color.White;
            this.BackUpDataFilesButton.ForeColor = System.Drawing.Color.Black;
            this.BackUpDataFilesButton.Location = new System.Drawing.Point(23, 318);
            this.BackUpDataFilesButton.Name = "BackUpDataFilesButton";
            this.BackUpDataFilesButton.Size = new System.Drawing.Size(308, 39);
            this.BackUpDataFilesButton.TabIndex = 16;
            this.BackUpDataFilesButton.Text = "data 파일 백업";
            this.BackUpDataFilesButton.UseVisualStyleBackColor = false;
            this.BackUpDataFilesButton.Click += new System.EventHandler(this.BackUpDataFilesButton_Click);
            // 
            // CopyPromptWhenSelectPromptCheckBox
            // 
            this.CopyPromptWhenSelectPromptCheckBox.AutoSize = true;
            this.CopyPromptWhenSelectPromptCheckBox.Location = new System.Drawing.Point(23, 129);
            this.CopyPromptWhenSelectPromptCheckBox.Name = "CopyPromptWhenSelectPromptCheckBox";
            this.CopyPromptWhenSelectPromptCheckBox.Size = new System.Drawing.Size(260, 16);
            this.CopyPromptWhenSelectPromptCheckBox.TabIndex = 17;
            this.CopyPromptWhenSelectPromptCheckBox.Text = "프롬프트 꾸러미 선택시 전체 프롬프트 복사";
            this.CopyPromptWhenSelectPromptCheckBox.UseVisualStyleBackColor = true;
            this.CopyPromptWhenSelectPromptCheckBox.CheckedChanged += new System.EventHandler(this.CopyPromptWhenSelectPromptCheckBox_CheckedChanged);
            // 
            // ShowGradeOnThumbnailCheckBox
            // 
            this.ShowGradeOnThumbnailCheckBox.AutoSize = true;
            this.ShowGradeOnThumbnailCheckBox.Location = new System.Drawing.Point(23, 151);
            this.ShowGradeOnThumbnailCheckBox.Name = "ShowGradeOnThumbnailCheckBox";
            this.ShowGradeOnThumbnailCheckBox.Size = new System.Drawing.Size(168, 16);
            this.ShowGradeOnThumbnailCheckBox.TabIndex = 18;
            this.ShowGradeOnThumbnailCheckBox.Text = "아이템 이미지에 별점 표시";
            this.ShowGradeOnThumbnailCheckBox.UseVisualStyleBackColor = true;
            this.ShowGradeOnThumbnailCheckBox.CheckedChanged += new System.EventHandler(this.ShowGradeOnThumbnailCheckBox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 289);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(157, 12);
            this.label2.TabIndex = 19;
            this.label2.Text = "페이지 당 아이템 표시 개수:";
            // 
            // ItemsPerPageTextBox
            // 
            this.ItemsPerPageTextBox.Location = new System.Drawing.Point(182, 285);
            this.ItemsPerPageTextBox.Name = "ItemsPerPageTextBox";
            this.ItemsPerPageTextBox.Size = new System.Drawing.Size(52, 21);
            this.ItemsPerPageTextBox.TabIndex = 21;
            this.ItemsPerPageTextBox.ValueChanged += new System.EventHandler(this.ItemsPerPageTextBox_ValueChanged);
            // 
            // ChangeLoraToLycoCheckBox
            // 
            this.ChangeLoraToLycoCheckBox.AutoSize = true;
            this.ChangeLoraToLycoCheckBox.Location = new System.Drawing.Point(23, 173);
            this.ChangeLoraToLycoCheckBox.Name = "ChangeLoraToLycoCheckBox";
            this.ChangeLoraToLycoCheckBox.Size = new System.Drawing.Size(177, 16);
            this.ChangeLoraToLycoCheckBox.TabIndex = 22;
            this.ChangeLoraToLycoCheckBox.Text = "\"<lora:\"를 \"<lyco:\"로 바꾸기";
            this.ChangeLoraToLycoCheckBox.UseVisualStyleBackColor = true;
            this.ChangeLoraToLycoCheckBox.CheckedChanged += new System.EventHandler(this.ChangeLoraToLycoCheckBox_CheckedChanged);
            // 
            // ChangeJpgToPngCheckBox
            // 
            this.ChangeJpgToPngCheckBox.AutoSize = true;
            this.ChangeJpgToPngCheckBox.Location = new System.Drawing.Point(23, 195);
            this.ChangeJpgToPngCheckBox.Name = "ChangeJpgToPngCheckBox";
            this.ChangeJpgToPngCheckBox.Size = new System.Drawing.Size(261, 16);
            this.ChangeJpgToPngCheckBox.TabIndex = 23;
            this.ChangeJpgToPngCheckBox.Text = "시비타이 정보 불러올 때 썸네일 png로 변환";
            this.ChangeJpgToPngCheckBox.UseVisualStyleBackColor = true;
            this.ChangeJpgToPngCheckBox.CheckedChanged += new System.EventHandler(this.ChangeJpgToPngCheckBox_CheckedChanged);
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 430);
            this.Controls.Add(this.ChangeJpgToPngCheckBox);
            this.Controls.Add(this.ChangeLoraToLycoCheckBox);
            this.Controls.Add(this.ItemsPerPageTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ShowGradeOnThumbnailCheckBox);
            this.Controls.Add(this.CopyPromptWhenSelectPromptCheckBox);
            this.Controls.Add(this.BackUpDataFilesButton);
            this.Controls.Add(this.ItemHeightTrackBarLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ItemHeightTrackBar);
            this.Controls.Add(this.ItemWidthTrackBarLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ItemWidthTrackBar);
            this.Controls.Add(this.ClearDataButton);
            this.Controls.Add(this.SaveChangesInstantlyCheckBox);
            this.Controls.Add(this.ClearPromptWhenSelectNegativeCheckBox);
            this.Controls.Add(this.ClearPromptWhenSwitchItemCheckBox);
            this.MaximumSize = new System.Drawing.Size(355, 430);
            this.MinimumSize = new System.Drawing.Size(355, 430);
            this.Name = "SettingForm";
            this.Text = "설정";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.ItemWidthTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemHeightTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsPerPageTextBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ClearPromptWhenSelectNegativeCheckBox;
        private System.Windows.Forms.CheckBox ClearPromptWhenSwitchItemCheckBox;
        private System.Windows.Forms.CheckBox SaveChangesInstantlyCheckBox;
        private System.Windows.Forms.Button ClearDataButton;
        private System.Windows.Forms.TrackBar ItemWidthTrackBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ItemWidthTrackBarLabel;
        private System.Windows.Forms.Label ItemHeightTrackBarLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar ItemHeightTrackBar;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button BackUpDataFilesButton;
        private System.Windows.Forms.CheckBox CopyPromptWhenSelectPromptCheckBox;
        private System.Windows.Forms.CheckBox ShowGradeOnThumbnailCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown ItemsPerPageTextBox;
        private System.Windows.Forms.CheckBox ChangeLoraToLycoCheckBox;
        private System.Windows.Forms.CheckBox ChangeJpgToPngCheckBox;
    }
}