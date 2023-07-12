using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LoRA_Explorer {
    public partial class SettingForm : MetroFramework.Forms.MetroForm {
        MainForm mainForm;
        Dictionary<string, dynamic> settings;
        bool isSizeChanged = false;
        bool isShowGradeOptionChanged = false;
        bool isItemsPerPageOptionChanged = false;

        public SettingForm(MainForm mainForm) {
            this.mainForm = mainForm;
            this.settings = mainForm.settings;

            InitializeComponent();
            toolTip1.SetToolTip(ClearPromptWhenSwitchItemCheckBox, "다른 아이템을 클릭했을 때 하단 프롬프트 창의 내용을 지웁니다.");
            toolTip1.SetToolTip(ClearPromptWhenSelectNegativeCheckBox, "네거티브 프롬프트 꾸러미를 클릭했을 때 하단 프롬프트 창의 내용을 지웁니다.");
            toolTip1.SetToolTip(SaveChangesInstantlyCheckBox, "셀이 편집되는 즉시 자동으로 .data 파일이 갱신됩니다.");
            toolTip1.SetToolTip(CopyPromptWhenSelectPromptCheckBox, "프롬프트 꾸러미를 클릭했을 때 자동으로 하단 프롬프트 창의 내용을 복사합니다.");
            toolTip1.SetToolTip(ItemsPerPageTextBox, "0 입력시 개수 제한 없음");
            toolTip1.SetToolTip(ChangeJpgToPngCheckBox, "Civitai 정보 불러오기 기능을 이용할 때 썸네일 이미지를 내려받은 후 png 포맷으로 변환합니다.");

            if (settings["clear_promptBox_when_changing_items"] == true) {
                ClearPromptWhenSwitchItemCheckBox.Checked = true;
            }
            if (settings["clear_promptBox_when_select_negative_prompt"] == true) {
                ClearPromptWhenSelectNegativeCheckBox.Checked = true;
            }
            if (settings["save_changes_immediately"] == true) {
                SaveChangesInstantlyCheckBox.Checked = true;
            }
            if (settings["copy_promptBox_when_select_prompt"] == true) {
                CopyPromptWhenSelectPromptCheckBox.Checked = true;
            }
            if (settings["show_star_grade_on_thumbnail"] == true) {
                ShowGradeOnThumbnailCheckBox.Checked = true;
            }
            if (settings["change_lora_to_lyco"] == true) {
                ChangeLoraToLycoCheckBox.Checked = true;
            }
            if (settings["change_jpg_to_png"] == true) {
                ChangeJpgToPngCheckBox.Checked = true;
            }
            int curWidth = settings["item_width"] / 10;
            if (curWidth >= ItemWidthTrackBar.Minimum && curWidth <= ItemWidthTrackBar.Maximum) {
                ItemWidthTrackBar.Value = curWidth;
            }
            int curHeight = settings["item_height"] / 10;
            if (curHeight >= ItemHeightTrackBar.Minimum && curHeight <= ItemHeightTrackBar.Maximum) {
                ItemHeightTrackBar.Value = curHeight;
            }
            ItemsPerPageTextBox.Value = (decimal)settings["items_per_page"];


            isSizeChanged = false; // 위에서 초기화하는 걸로도 true됨 참고
            isShowGradeOptionChanged = false;
            isItemsPerPageOptionChanged = false;
        }

        private void ItemWidthTrackBar_ValueChanged(object sender, EventArgs e) {
            int curVal = ItemWidthTrackBar.Value * 10;
            ItemWidthTrackBarLabel.Text = curVal.ToString();
            settings["item_width"] = curVal;
            isSizeChanged = true;
            Console.WriteLine($"setting changed: item_width to {curVal}");
        }

        private void ItemHeightTrackBar_ValueChanged(object sender, EventArgs e) {
            int curVal = ItemHeightTrackBar.Value * 10;
            ItemHeightTrackBarLabel.Text = curVal.ToString();
            settings["item_height"] = curVal;
            isSizeChanged = true;
            Console.WriteLine($"setting changed: item_height to {curVal}");
        }


        private void ItemsPerPageTextBox_ValueChanged(object sender, EventArgs e) {
            settings["items_per_page"] = (int)ItemsPerPageTextBox.Value;
            isItemsPerPageOptionChanged = true;
            Console.WriteLine($"setting changed: items_per_page to {ItemsPerPageTextBox.Value}");
        }

        private void ClearPromptWhenSwitchItemCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (ClearPromptWhenSwitchItemCheckBox.Checked == true) {
                settings["clear_promptBox_when_changing_items"] = true;
            } else if (ClearPromptWhenSwitchItemCheckBox.Checked == false) {
                settings["clear_promptBox_when_changing_items"] = false;
            }
            Console.WriteLine($"setting changed: clear_promptBox_when_changing_items to {ClearPromptWhenSwitchItemCheckBox.Checked}");
        }

        private void ClearPromptWhenSelectNegativeCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (ClearPromptWhenSelectNegativeCheckBox.Checked == true) {
                settings["clear_promptBox_when_select_negative_prompt"] = true;
            } else if (ClearPromptWhenSelectNegativeCheckBox.Checked == false) {
                settings["clear_promptBox_when_select_negative_prompt"] = false;
            }
            Console.WriteLine($"setting changed: clear_promptBox_when_select_negative_prompt to {ClearPromptWhenSelectNegativeCheckBox.Checked}");
        }

        private void SaveChangesInstantlyCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (SaveChangesInstantlyCheckBox.Checked == true) {
                settings["save_changes_immediately"] = true;
            } else if (SaveChangesInstantlyCheckBox.Checked == false) {
                settings["save_changes_immediately"] = false;
            }
            Console.WriteLine($"setting changed: save_changes_immediately to {SaveChangesInstantlyCheckBox.Checked}");
        }

        private void CopyPromptWhenSelectPromptCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (CopyPromptWhenSelectPromptCheckBox.Checked == true) {
                settings["copy_promptBox_when_select_prompt"] = true;
            } else if (CopyPromptWhenSelectPromptCheckBox.Checked == false) {
                settings["copy_promptBox_when_select_prompt"] = false;
            }
            Console.WriteLine($"setting changed: copy_promptBox_when_select_prompt to {CopyPromptWhenSelectPromptCheckBox.Checked}");
        }

        private void ShowGradeOnThumbnailCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (ShowGradeOnThumbnailCheckBox.Checked == true) {
                settings["show_star_grade_on_thumbnail"] = true;
            } else if (ShowGradeOnThumbnailCheckBox.Checked == false) {
                settings["show_star_grade_on_thumbnail"] = false;
            }
            isShowGradeOptionChanged = true;
            Console.WriteLine($"setting changed: show_star_grade_on_thumbnail to {ShowGradeOnThumbnailCheckBox.Checked}");
        }
        private void ChangeLoraToLycoCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (ChangeLoraToLycoCheckBox.Checked == true) {
                settings["change_lora_to_lyco"] = true;
            } else if (ChangeLoraToLycoCheckBox.Checked == false) {
                settings["change_lora_to_lyco"] = false;
            }
            Console.WriteLine($"setting changed: change_lora_to_lyco to {ChangeLoraToLycoCheckBox.Checked}");
        }
        private void ChangeJpgToPngCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (ChangeJpgToPngCheckBox.Checked == true) {
                settings["change_jpg_to_png"] = true;
            } else if (ChangeJpgToPngCheckBox.Checked == false) {
                settings["change_jpg_to_png"] = false;
            }
            Console.WriteLine($"setting changed: change_jpg_to_png to {ChangeJpgToPngCheckBox.Checked}");
        }


        // data 파일 일괄삭제
        private void ClearDataButton_Click(object sender, EventArgs e) {
            if (mainForm.itemFlowLayout.itemDict == null) {
                return;
            }

            var confirmResult = MessageBox.Show("정말 모든 data 파일을 삭제하고 프로그램을 종료합니까? 삭제된 data 파일은 복구할 수 없으며, 만약 달리 data 확장자를 사용하는 외부 파일이 현재 경로 하에 위치해있다면 해당 파일까지 삭제됩니다.",
            "모든 data 파일 제거", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes) {
                DangerArea dangerArea = new DangerArea();
                dangerArea.DeleteAllDataFiles(mainForm.itemFlowLayout.itemDict);
            } else {
            }
        }


        private void SettingForm_FormClosed(object sender, FormClosedEventArgs e) {
            Console.WriteLine("loading settings...");
            foreach (KeyValuePair<string, dynamic> kvp in settings) {
                string key = kvp.Key;
                dynamic value = kvp.Value;
                Console.WriteLine("Key = {0}, Value = {1}", key, value);
            }

            string text = "clear_promptBox_when_changing_items = ";
            if (settings.ContainsKey("clear_promptBox_when_changing_items")) {
                text += settings["clear_promptBox_when_changing_items"].ToString();
            } else {
                text += "false";
            }
            text += "\nclear_promptBox_when_select_negative_prompt = ";
            if (settings.ContainsKey("clear_promptBox_when_select_negative_prompt")) {
                text += settings["clear_promptBox_when_select_negative_prompt"].ToString();
            } else {
                text += "false";
            }
            text += "\nsave_changes_immediately = ";
            if (settings.ContainsKey("save_changes_immediately")) {
                text += settings["save_changes_immediately"].ToString();
            } else {
                text += "false";
            }
            text += "\ncopy_promptBox_when_select_prompt = ";
            if (settings.ContainsKey("copy_promptBox_when_select_prompt")) {
                text += settings["copy_promptBox_when_select_prompt"].ToString();
            } else {
                text += "false";
            }
            text += "\nshow_star_grade_on_thumbnail = ";
            if (settings.ContainsKey("show_star_grade_on_thumbnail")) {
                text += settings["show_star_grade_on_thumbnail"].ToString();
            } else {
                text += "false";
            }
            text += "\nchange_lora_to_lyco = ";
            if (settings.ContainsKey("change_lora_to_lyco")) {
                text += settings["change_lora_to_lyco"].ToString();
            } else {
                text += "false";
            }
            text += "\nchange_jpg_to_png = ";
            if (settings.ContainsKey("change_jpg_to_png")) {
                text += settings["change_jpg_to_png"].ToString();
            } else {
                text += "false";
            }
            text += "\nitem_width = ";
            if (settings.ContainsKey("item_width")) {
                text += settings["item_width"].ToString();
            } else {
                text += "100";
            }
            text += "\nitem_height = ";
            if (settings.ContainsKey("item_height")) {
                text += settings["item_height"].ToString();
            } else {
                text += "150";
            }
            text += "\nitems_per_page = ";
            if (settings.ContainsKey("items_per_page")) {
                text += settings["items_per_page"].ToString();
            } else {
                text += "50";
            }

            string path = "LoRA Explorer.settings";
            File.WriteAllText(path, text);

            if (isSizeChanged) {
                ItemFlowLayout itemFlowLayout = mainForm.itemFlowLayout;
                itemFlowLayout.flowLayout.SuspendLayout();
                foreach (Item item in itemFlowLayout.itemDict.Values) {
                    item.itemHeight = settings["item_height"];
                    item.itemWidth = settings["item_width"];

                    item.Resizing();
                    item.LoadImage();
                    Console.WriteLine("resized: " + item.modelName);
                }
                itemFlowLayout.flowLayout.ResumeLayout();

                isSizeChanged = false;
            }
            if (isShowGradeOptionChanged) {
                ItemFlowLayout itemFlowLayout = mainForm.itemFlowLayout;
                if (ShowGradeOnThumbnailCheckBox.Checked == true) {
                    foreach (Item item in itemFlowLayout.itemDict.Values) {
                        item.UpdateGradeGraphic();
                    }
                } else {
                    foreach (Item item in itemFlowLayout.itemDict.Values) {
                        item.gradePictureBox.Image = null;
                    }
                }
                isShowGradeOptionChanged = false;
            }
            if (isItemsPerPageOptionChanged) {
                ItemFlowLayout itemFlowLayout = mainForm.itemFlowLayout;
                itemFlowLayout.Update(mainForm.currentPage);
            }
        }

        private void BackUpDataFilesButton_Click(object sender, EventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "data 파일들을 백업할 위치를 선택하세요.";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                string copyPath = dialog.FileName;
                string rootPath = mainForm.rootDirectory;

                ItemFlowLayout itemFlowLayout = mainForm.itemFlowLayout;
                foreach (Item item in itemFlowLayout.itemDict.Values) {
                    string filePath = item.dataPath;

                    string relativePath = filePath.Replace(rootPath, "");
                    string copyFilePath = copyPath + relativePath;

                    string tmpPath = Path.GetDirectoryName(copyFilePath);
                    if (!Directory.Exists(tmpPath)) {
                        Directory.CreateDirectory(tmpPath);
                    }

                    File.Copy(filePath, copyFilePath, true);
                }
                // mainForm.SetStatus("data 파일 백업."); 이거 때문에 MainForm으로 포커스 바껴서 설정창 안 닫힌 줄 모르고 메인폼 동작 안 돼서 렉걸린 줄 알게되더라
            } else {
                return;
            }
            this.Activate();
        }


    }
}
