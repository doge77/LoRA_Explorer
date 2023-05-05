using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LoRA_Explorer.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net;

namespace LoRA_Explorer {

    public enum SortMethod {
        /*
        ComboBox 인덱스랑 순서 일치시킬 것
        0 이름 (A → Z) NameAscending
        1 이름 (Z → A) NameDescending
        2 파일명 (A → Z) FileNameAscending
        3 파일명 (Z → A) FileNameDescending
        4 생성날짜 (예전 → 최신) CreateDateAscending
        5 생성날짜 (최신 → 예전) CreateDateDescending
        6 별점 GradeAscending
        7 별점 GradeDescending
        */
        NameAscending, NameDescending, FileNameAscending, FileNameDescending,
        CreateDateAscending, CreateDateDescending,
        GradeAscending, GradeDescending
    }

    public partial class MainForm : MetroFramework.Forms.MetroForm {
        public string rootDirectory;
        public string currentDirectory;
        public Item currentItem = null;
        public int currentPage = 0;
        public MainForm mainForm;
        LocalTreeView localTreeView;
        public ItemFlowLayout itemFlowLayout;
        ItemInfo itemInfo;

        public Dictionary<string, dynamic> settings = new Dictionary<string, dynamic>();

        public MainForm() {
            mainForm = this;

            LoadSettings();
            Console.WriteLine("loading settings...");
            foreach (KeyValuePair<string, dynamic> kvp in settings) {
                string key = kvp.Key;
                dynamic value = kvp.Value;
                Console.WriteLine("Key = {0}, Value = {1}", key, value);
            }
            InitializeComponent();
            DrawUI();

            toolTip1.SetToolTip(ReloadingButton, "새 루트 디렉토리를 지정하고 프로그램을 갱신합니다.");
            toolTip1.SetToolTip(UpdateCurrentRootButton, "현재 로드되어있는 루트 디렉토리와 하위 디렉토리 상에 새로 추가된 아이템이 있다면 그것을 찾아 아이템 목록에 추가합니다.\n경로의 추가 또는 삭제, 아이템의 중복 감지, 삭제된 아이템에 대해선 아무런 처리도 하지 않습니다.");
            toolTip1.SetToolTip(GetCivitaiInfoButton, "Civitai에 등재되어 있는 아이템일 경우, 아이템의 이름, 주소, 버전을 가져옵니다.\n또, 미리보기가 없을 경우 Civitai의 첫번째 샘플 이미지를 저장 후 갱신합니다.");

            localTreeView = new LocalTreeView();
            itemFlowLayout = new ItemFlowLayout(this);
            itemInfo = new ItemInfo(this);

            SearchCategory.SelectedIndex = 0;
            SortMethodCheckBox.SelectedIndex = 0;
            this.Show();

            Loading();

            SavePromptDataButton.BackColor = Color.White;
            SaveItemDataChangeButton.BackColor = Color.White;
        }
        private void DrawUI() {
            tableLayoutPanel1.Paint += new PaintEventHandler(this.DrawUpDownBorder);
            tableLayoutPanel10.Paint += new PaintEventHandler(this.DrawUpDownBorder);
        }
        private void DrawUpDownBorder(object sender, PaintEventArgs e) {
            ControlPaint.DrawBorder(e.Graphics, (sender as Control).ClientRectangle,
                Color.Black, 0, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 0, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid
            );
        }
        private void LoadSettings() {
            string path = "LoRA Explorer.settings";
            try {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines) {
                    string[] parts = line.Split('=');
                    string key = parts[0].Trim();
                    dynamic value = parts[1].Trim();
                    if (int.TryParse(value, out int intValue)) {
                        value = intValue;
                    } else if (bool.TryParse(value, out bool boolValue)) {
                        value = boolValue;
                    }
                    settings[key] = value;
                }
            } catch {
                string text = "clear_promptBox_when_changing_items = false\r\nclear_promptBox_when_select_negative_prompt = false\r\nsave_changes_immediately = false\r\ncopy_promptBox_when_select_prompt = false\r\nshow_star_grade_on_thumbnail = false\r\nitem_width = 100\r\nitem_height = 150\r\nitems_per_page = 50";
                File.WriteAllText(path, text);
                settings["clear_promptBox_when_changing_items"] = false;
                settings["clear_promptBox_when_select_negative_prompt"] = false;
                settings["save_changes_immediately"] = false;
                settings["copy_promptBox_when_select_prompt"] = false;
                settings["show_star_grade_on_thumbnail"] = false;
                settings["item_width"] = 100;
                settings["item_height"] = 150;
                settings["items_per_page"] = 50;
            }

            // 키 누락시 초기화
            if (!settings.ContainsKey("clear_promptBox_when_changing_items")) {
                settings["clear_promptBox_when_changing_items"] = false;
            }
            if (!settings.ContainsKey("clear_promptBox_when_select_negative_prompt")) {
                settings["clear_promptBox_when_select_negative_prompt"] = false;
            }
            if (!settings.ContainsKey("save_changes_immediately")) {
                settings["save_changes_immediately"] = false;
            }
            if (!settings.ContainsKey("copy_promptBox_when_select_prompt")) {
                settings["copy_promptBox_when_select_prompt"] = false;
            }
            if (!settings.ContainsKey("show_star_grade_on_thumbnail")) {
                settings["show_star_grade_on_thumbnail"] = false;
            }
            if (!settings.ContainsKey("item_width")) {
                settings["item_width"] = 100;
            }
            if (!settings.ContainsKey("item_height")) {
                settings["item_height"] = 150;
            }
            if (!settings.ContainsKey("items_per_page")) {
                settings["items_per_page"] = 50;
            }
        }
        private void OpenSettingFormButton_Click(object sender, EventArgs e) {
            SettingForm settingForm = new SettingForm(mainForm);
            settingForm.ShowDialog();
        }

        private bool Loading() {
            rootDirectory = localTreeView.SetRootDirectory(LocalTreeView);
            if (rootDirectory != null) {
                int itemCounter = 0;
                currentDirectory = rootDirectory;
                itemCounter = itemFlowLayout.GetAllItems(rootDirectory);

                List<string> pathes = new List<string>();
                foreach (Item item in itemFlowLayout.itemDict.Values) {
                    pathes.Add(item.modelPath);
                }
                pathes = pathes.Distinct().ToList();

                localTreeView.InitializeLocalTreeView(LocalTreeView, rootDirectory, pathes);

                itemFlowLayout.InitializeFlowLayout(SortMethodCheckBox.SelectedIndex);

                SetStatus($"로드됨: {itemCounter}개 아이템.");
                return true;
            } else {
                return false;
            }
        }

        // 트리 뷰
        public void LocalTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
            TreeView treeView = sender as TreeView;
            DirectoryInfo directoryInfo = e.Node.Tag as DirectoryInfo;
            string dir = e.Node.Tag as string;

            Console.WriteLine("경로 변경: " + dir);
            SetStatus("폴더 변경: " + dir);

            currentDirectory = dir;
            if (Directory.Exists(dir)) {
                itemFlowLayout.GetCurrentItems();
                itemFlowLayout.Update(0);
            } else {
                Console.WriteLine("경로를 찾을 수 없음");
                System.Windows.Forms.Application.Exit();
            }
        }

        // 버튼
        private void NextPageButton_Click(object sender, EventArgs e) {
            currentPage++;
            itemFlowLayout.Update(currentPage);
        }
        private void PreviousPageButton_Click(object sender, EventArgs e) {
            currentPage--;
            itemFlowLayout.Update(currentPage);
        }
        private void CopyButton_Click(object sender, EventArgs e) {
            copyPromptBox();
        }
        private void SearchButton_Click(object sender, EventArgs e) {
            if (itemFlowLayout.itemDict == null) {
                return;
            }
            TextBox searchTextBox = SearchTextBox;
            string searchText = searchTextBox.Text;

            itemFlowLayout.GetSearchedCurrentItems(searchText, SearchCategory.SelectedItem?.ToString());
            itemFlowLayout.Update(0);

            if (String.IsNullOrEmpty(searchText)) {
                SetStatus("검색 초기화");
            } else {
                SetStatus("검색: " + searchText);
            }
        }
        private void ReloadingButton_Click(object sender, EventArgs e) {
            string rootDirectoryBackUp = rootDirectory;

            bool loaded = Loading();
            if (!loaded) {
                rootDirectory = rootDirectoryBackUp;
            }
        }
        private void UpdateCurrentRootButton_Click(object sender, EventArgs e) {
            Dictionary<string, Item> dict = itemFlowLayout.itemDict;

            string[] modelExt = new[] { ".pt", ".safetensors", ".ckpt" };
            string[] files = Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories)
                .Where(file => modelExt.Contains(Path.GetExtension(file)))
                .ToArray();

            foreach (string file in files) {
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (dict.ContainsKey(fileName)) {
                    continue;
                }

                // modelName, modelPath, parentPath, size,
                string modelPath = Path.GetFullPath(file);
                string parentPath = Path.GetDirectoryName(file);
                dict[fileName] = new Item { modelName = fileName, modelPath = modelPath, parentPath = parentPath, itemHeight = mainForm.settings["item_height"], itemWidth = mainForm.settings["item_width"] };

                // imagePath
                string[] validImageExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                foreach (string ext in validImageExt) {
                    string imagePath = Path.Combine(parentPath, fileName + ext);
                    if (File.Exists(imagePath)) {
                        dict[fileName].imagePath = imagePath;
                        break;
                    }
                    imagePath = Path.Combine(parentPath, fileName + ".preview" + ext);
                    if (File.Exists(imagePath)) {
                        dict[fileName].imagePath = imagePath;
                        break;
                    }
                }

                // dataPath
                string dataPath = Path.Combine(parentPath, fileName + ".data");
                if (File.Exists(dataPath)) {
                    dict[fileName].dataPath = dataPath;
                }

                if (String.IsNullOrEmpty(dict[fileName].dataPath)) {
                    dict[fileName].CreateEmptyData();
                }
                dict[fileName].LoadJson();
                dict[fileName].CreatePictureBox();
                dict[fileName].pictureBox.Click += new EventHandler(mainForm.ItemClick);
                dict[fileName].CreateLabel();
                dict[fileName].CreateGradeGraphic();
                dict[fileName].gradePictureBox.Click += new EventHandler(mainForm.ItemClick);
                dict[fileName].CreateItemPanel();
                if (mainForm.settings["show_star_grade_on_thumbnail"] == false) {
                    dict[fileName].gradePictureBox.Image = null;
                }
            }

            itemFlowLayout.InitializeFlowLayout(SortMethodCheckBox.SelectedIndex);
        }
        private void ChangeFileNameButton_Click(object sender, EventArgs e) {
            Item item = currentItem;

            if (item == null) {
                return;
            }

            Form form = new Form();
            PictureBox picture = new PictureBox();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            string value = "";

            form.ClientSize = new Size(400, 180);
            form.Controls.AddRange(new Control[] { picture, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            form.Text = "파일명 변경";
            picture.Image = item.image;
            textBox.Text = item.modelName;
            buttonOk.Text = "확인";
            buttonCancel.Text = "취소";

            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            picture.SetBounds(10, 10, 100, 150);
            textBox.SetBounds(130, 10, 250, 90);
            textBox.Multiline = true;
            buttonOk.SetBounds(130, 120, 70, 20);
            buttonCancel.SetBounds(215, 120, 70, 20);

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;

            if (dialogResult == DialogResult.OK) {
                // value가 유효한지 확인
                char[] invalidChars = Path.GetInvalidFileNameChars();
                foreach (char c in value) {
                    if (invalidChars.Contains(c)) {
                        MessageBox.Show("파일명이 유효하지 않습니다.");
                        return;
                    }
                }

                string modelExt = Path.GetExtension(item.modelPath);
                string newName = value;
                // 모델파일
                string newModelPath = Path.Combine(item.parentPath, newName + modelExt);
                File.Move(item.modelPath, newModelPath);
                item.modelPath = newModelPath;
                // 데이터파일
                string newDataPath = Path.Combine(item.parentPath, newName + ".data");
                File.Move(item.dataPath, newDataPath);
                item.dataPath = newDataPath;
                // civitai 파일
                if (File.Exists(Path.Combine(item.parentPath, item.modelName + ".civitai.info"))) {
                    string newCivitaiPath = Path.Combine(item.parentPath, newName + ".civitai.info");
                    File.Move(Path.Combine(item.parentPath, item.modelName + ".civitai.info"), newCivitaiPath);
                }
                // 프리뷰파일
                if (item.imagePath != null) {
                    string previewExt = Path.GetExtension(item.imagePath);
                    string newPreviewPath = null;
                    if (Path.GetFileNameWithoutExtension(item.imagePath).EndsWith(".preview")) {
                        newPreviewPath = Path.Combine(item.parentPath, newName + ".preview" + previewExt);
                        File.Move(item.imagePath, newPreviewPath);
                    } else {
                        newPreviewPath = Path.Combine(item.parentPath, newName + previewExt);
                        File.Move(item.imagePath, newPreviewPath);
                    }
                    item.imagePath = newPreviewPath;
                }

                Console.WriteLine("파일 이름 변경: " + item.modelName + "to " + newName);
                SetStatus("파일 이름 변경: " + item.modelName + "to " + newName);

                item.modelName = newName;

                item.UpdateLabelText();
                itemInfo.Update();
            } else {
            }
        }
        private void ChangeFileDirectionButton_Click(object sender, EventArgs e) {
            Item item = currentItem;

            if (item == null) {
                return;
            }

            string newPath = null;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = item.modelName + " 경로 변경";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = item.parentPath;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                newPath = dialog.FileName;
            } else {
                return;
            }

            // 모델파일
            string modelExt = Path.GetExtension(item.modelPath);
            string newModelPath = Path.Combine(newPath, item.modelName + modelExt);
            File.Move(item.modelPath, newModelPath);
            item.modelPath = newModelPath;
            // 데이터파일
            string newDataPath = Path.Combine(newPath, item.modelName + ".data");
            File.Move(item.dataPath, newDataPath);
            item.dataPath = newDataPath;
            // civitai 파일
            if (File.Exists(Path.Combine(item.parentPath, item.modelName + ".civitai.info"))) {
                string newCivitaiPath = Path.Combine(newPath, item.modelName + ".civitai.info");
                File.Move(Path.Combine(item.parentPath, item.modelName + ".civitai.info"), newCivitaiPath);
            }
            // 프리뷰파일
            if (item.imagePath != null) {
                string previewExt = Path.GetExtension(item.imagePath);
                string newPreviewPath = null;
                if (Path.GetFileNameWithoutExtension(item.imagePath).EndsWith(".preview")) {
                    newPreviewPath = Path.Combine(newPath, item.modelName + ".preview" + previewExt);
                    File.Move(item.imagePath, newPreviewPath);
                } else {
                    newPreviewPath = Path.Combine(newPath, item.modelName + previewExt);
                    File.Move(item.imagePath, newPreviewPath);
                }
                item.imagePath = newPreviewPath;
            }

            item.parentPath = newPath;

            itemInfo.Update();

            Console.WriteLine("파일 이동: " + item.modelName + " -> " + newPath);
            SetStatus("파일 이동: " + item.modelName + " -> " + newPath);

            itemFlowLayout.GetCurrentItems();
            itemFlowLayout.Update(0);
        }
        private void DeleteItemButton_Click(object sender, EventArgs e) {
            if (currentItem == null) {
                return;
            }

            var confirmResult = MessageBox.Show("정말 파일을 삭제하시겠습니까? 이 작업은 " + currentItem.modelName + " 파일명을 가진 모델 파일, 프리뷰 파일, 데이터 파일, civitai.info 파일을 삭제합니다.", currentItem.modelName + "삭제", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes) {
                File.Delete(currentItem.modelPath);

                // preview붙은 이미지, 안 붙은 이미지 둘 다 삭제
                if (currentItem.imagePath != null) {
                    File.Delete(currentItem.imagePath);
                }
                if (File.Exists(Path.Combine(currentItem.parentPath, currentItem.modelName + ".preview.png"))) {
                    File.Delete(Path.Combine(currentItem.parentPath, currentItem.modelName + ".preview.png"));
                }
                if (File.Exists(Path.Combine(currentItem.parentPath, currentItem.modelName + ".png"))) {
                    File.Delete(Path.Combine(currentItem.parentPath, currentItem.modelName + ".png"));
                }

                File.Delete(currentItem.dataPath);
                if (File.Exists(Path.Combine(currentItem.parentPath, currentItem.modelName + ".civitai.info"))) {
                    File.Delete(Path.Combine(currentItem.parentPath, currentItem.modelName + ".civitai.info"));
                }

                Console.WriteLine("파일 삭제: " + currentItem.modelName);
                SetStatus("파일 삭제: " + currentItem.modelName);

                itemFlowLayout.itemDict.Remove(currentItem.modelName);
                currentItem = null;
                itemInfo.Clear();
                itemFlowLayout.InitializeFlowLayout(SortMethodCheckBox.SelectedIndex);
            } else {
            }
        }
        private void CopyFileNameButton_Click(object sender, EventArgs e) {
            Item item = currentItem;

            if (item == null) {
                return;
            }

            if (!String.IsNullOrEmpty(item.modelName)) {
                Clipboard.SetText(item.modelName);
                SetStatus("파일명 복사");
            }
        }
        private void OpenCurrentFolderButton_Click(object sender, EventArgs e) {
            Item item = currentItem;

            if (item == null) {
                return;
            }

            Process.Start("explorer.exe", $"/select,\"{item.modelPath}\"");
        }
        /* 지금은 안 쓰는 기능
        private void OpenCurrentImageButton_Click(object sender, EventArgs e) {
            Item item = currentItem;

            if (item == null) {
                return;
            }
            if (String.IsNullOrEmpty(item.imagePath)) {
                return;
            }

            Process.Start(item.imagePath);
        }
        private void OpenCivitaiInfoButton_Click(object sender, EventArgs e) {
            Item item = currentItem;
            if (item == null) {
                return;
            }

            string target = Path.Combine(item.parentPath, item.modelName + ".civitai.info");

            if (!File.Exists(target)) {
                return;
            }

            // Process.Start("notepad.exe", target); 메모장으로 열기

            // 웹으로 바로 열기
            JObject json = JObject.Parse(File.ReadAllText(target));
            if (json.ContainsKey("modelId")) {
                string modelID = (string)json["modelId"];

                Process.Start("https://civitai.com/models/" + modelID);
            }
        }
        */
        private void CopyDataButton_Click(object sender, EventArgs e) {
            if (currentItem == null) {
                return;
            }

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = $"{Path.GetFileName(currentItem.dataPath)} 파일을 복사할 모델을 선택하세요.";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                string targetModel = dialog.FileName;

                if (!(new List<string> { ".pt", ".safetensors", ".ckpt" }).Contains(Path.GetExtension(targetModel))) {
                    var confirmResult = MessageBox.Show($"{Path.GetFileName(targetModel)} 파일은 모델 파일이 아닙니다. 그래도 해당 파일명으로 data 파일을 복사하시겠습니까? 예기치 않은 문제가 발생할 수도 있습니다.", $"{Path.GetFileName(currentItem.dataPath)} 파일 복사", MessageBoxButtons.YesNo);
                    if (confirmResult == DialogResult.Yes) {
                    } else {
                        return;
                    }
                }

                string copiedDataPath = Path.Combine(Path.GetDirectoryName(targetModel), Path.GetFileNameWithoutExtension(targetModel) + ".data");

                File.Copy(currentItem.dataPath, copiedDataPath, true);

                itemFlowLayout.GetAllItems(rootDirectory);
                itemFlowLayout.InitializeFlowLayout(SortMethodCheckBox.SelectedIndex);

                SetStatus($".data 파일 복사: {currentItem.modelName}.data -> {Path.GetFileName(copiedDataPath)}");
            } else {
                return;
            }
        }
        private void OpenURLButton_Click(object sender, EventArgs e) {
            if (currentItem == null) {
                return;
            }
            if (String.IsNullOrEmpty(currentItem.data.url)) {
                return;
            }

            try {
                Process.Start(currentItem.data.url);
            } catch {
                return;
            }
        }

        // 키보드
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                e.Handled = true; // 키 이벤트 처리 완료
                SearchButton.PerformClick();
            } else if (e.KeyCode == Keys.Escape) {
                e.Handled = true; // 키 이벤트 처리 완료
                SearchTextBox.Text = "";
                SearchButton.PerformClick();
            }
        }
        private void PromptBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                e.Handled = true; // 키 이벤트 처리 완료
                PromptBox.Text = "";
            }
        }

        // 기타
        public void SetStatus(string text) {
            StatusLabel.Text = "마지막 동작: " + text;
        }
        private void MainForm_Resize(object sender, EventArgs e) {
            if (splitContainer4.SplitterDistance <= 265) {
                splitContainer4.SplitterDistance = 255;
            }
        }
        private void copyPromptBox() {
            if (!String.IsNullOrEmpty(PromptBox.Text)) {
                string copyTarget = PromptBox.Text;
                if (copyTarget.EndsWith(", ")) {
                    Clipboard.SetText(copyTarget);
                } else if (copyTarget.EndsWith(",")) {
                    Clipboard.SetText(copyTarget + " ");
                } else if (copyTarget.EndsWith(" ")) {
                    Clipboard.SetText(copyTarget.Substring(0, copyTarget.Length - 1) + ", ");
                } else {
                    Clipboard.SetText(copyTarget + ", ");
                }
                SetStatus("프롬프트 복사");
            } else {
                Clipboard.Clear();
            }
        }
        private void SearchCategory_SelectedIndexChanged(object sender, EventArgs e) {
            SearchTextBox.Focus(); // 이거 왜 하냐면 콤보박스 포커싱되면 배경 퍼래지는 거 보기 싫어서
        }
        private void SortMethodCheckBox_SelectedIndexChanged(object sender, EventArgs e) {
            itemFlowLayout.InitializeFlowLayout(SortMethodCheckBox.SelectedIndex);
            SearchTextBox.Focus(); // 이거 왜 하냐면 콤보박스 포커싱되면 배경 퍼래지는 거 보기 싫어서
        }
        private void SortItemsButton_Click(object sender, EventArgs e) {
            itemFlowLayout.InitializeFlowLayout(SortMethodCheckBox.SelectedIndex);
        }

        // 아이템, 프롬프트
        public void ItemClick(object sender, EventArgs e) {
            if (PromptSheet.EditingControl != null) {
                PromptSheet.EndEdit();
            }
            if (ItemAttributeGrid.EditingControl != null) {
                ItemAttributeGrid.EndEdit();
            }

            PictureBox pictureBox = sender as PictureBox;
            Item item = pictureBox.Tag as Item;

            currentItem = item;
            itemInfo.Update();
            itemInfo.UpdatePromptGrid();
            itemInfo.UpdateDataGrid();

            // settings
            if (settings["clear_promptBox_when_changing_items"]) {
                PromptBox.Text = "";
            }

            Console.WriteLine("아이템 로드: " + item.modelName);
            SetStatus("파일 로드: " + item.modelName);
        }
        public void PromptClick(object sender, EventArgs e) {
            Button button = sender as Button;

            string prompt = button.Text.Replace("\n", "\r\n");

            // settings
            if (settings["clear_promptBox_when_select_negative_prompt"]) {
                if ((string)button.Tag == "negative") {
                    PromptBox.Text = "";
                }
            }

            string result = "";
            if (PromptBox.Text.Length == 0) {
                result = prompt;
            } else if (PromptBox.Text.EndsWith(", ")) {
                result = PromptBox.Text + prompt;
                PromptBox.Text = result;
            } else if (PromptBox.Text.EndsWith(",")) {
                result = PromptBox.Text + " " + prompt;
            } else if (PromptBox.Text.EndsWith(" ")) {
                result = PromptBox.Text.Substring(0, PromptBox.Text.Length - 1) + ", " + prompt;
            } else {
                result = PromptBox.Text + ", " + prompt;
            }

            PromptBox.Text = result;

            if (settings["copy_promptBox_when_select_prompt"]) {
                copyPromptBox();
            }

            SetStatus("프롬프트 갱신");
        }

        // DataGridView Editing
        // settings
        private void SaveItemDataChangeButton_Click(object sender, EventArgs e) {
            SaveItemData();
        }
        private void SavePromptDataButton_Click(object sender, EventArgs e) {
            SavePromptData();
        }
        private void SaveItemData() {
            Item item = currentItem;

            if (item == null) {
                return;
            }

            string dataPath = item.dataPath;

            try {
                JObject json = JObject.Parse(File.ReadAllText(dataPath));

                // 특정 속성 json 데이터값이 존재하지 않으면 Empty 값으로 초기화
                foreach (DataGridViewRow row in ItemAttributeGrid.Rows) {
                    string key = row.Cells[0].Value?.ToString();
                    string value = row.Cells[1].Value?.ToString();
                    JToken testToken = null;
                    string target = null;

                    if (key == "이름") {
                        target = "name";
                    } else if (key == "태그") {
                        target = "tag";
                    } else if (key == "메모") {
                        target = "note";
                    } else if (key == "제작자") {
                        target = "creator";
                    } else if (key == "주소") {
                        target = "url";
                    } else if (key == "버전") {
                        target = "version";
                    } else if (key == "생성 날짜") {
                        target = "downloadDate";
                    } else if (key == "별점") {
                        target = "grade";
                    }

                    testToken = json.SelectToken(target);
                    if (testToken == null) {
                        json.Add(target, value);
                    } else {
                        testToken.Replace(value);
                    }

                }

                File.WriteAllText(dataPath, json.ToString());

                item.LoadJson();
                item.UpdateLabelText();
                if (settings["show_star_grade_on_thumbnail"]) {
                    item.UpdateGradeGraphic();
                }
                itemInfo.Update();
                Console.WriteLine("data 파일 저장: " + item.modelName);
                SetStatus("파일 수정: " + item.modelName + ".data");
            } catch {
                item.LoadJson();
                item.UpdateLabelText();
                itemInfo.Update();
                Console.WriteLine("data 파일 저장 실패: " + item.modelName);
                SetStatus("파일 수정 실패. JSON 서식 확인 바람: " + item.modelName + ".data");
            }
            SaveItemDataChangeButton.BackColor = Color.White;
        }
        private void SavePromptData() {
            Item item = currentItem;

            if (item == null) {
                return;
            }

            string dataPath = item.dataPath;

            try {
                JObject json = JObject.Parse(File.ReadAllText(dataPath));
                json.Remove("prompt");

                JObject prompt = new JObject();
                foreach (DataGridViewRow row in PromptSheet.Rows) {
                    int counter = 2;
                    string key = row.Cells[0].Value?.ToString();
                    string value = row.Cells[1].Value?.ToString();

                    if (string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value)) { // Value만 존재 -> Key는 "무제"로
                        if (prompt.ContainsKey("무제")) { // Key 중복처리
                            while (true) {
                                if (!prompt.ContainsKey($"무제 ({counter})")) {
                                    prompt.Add($"무제 ({counter})", value);
                                    row.Cells[0].Value = $"무제 ({counter})";
                                    break;
                                }
                                counter++;
                            }
                        } else {
                            prompt.Add("무제", value);
                            row.Cells[0].Value = "무제";
                        }
                    } else if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value)) { // key만 존재 -> value는 ""
                        if (prompt.ContainsKey(key)) { // Key 중복처리
                            while (true) {
                                if (!prompt.ContainsKey($"{key} ({counter})")) {
                                    prompt.Add($"{key} ({counter})", "");
                                    row.Cells[0].Value = $"{key} ({counter})";
                                    break;
                                }
                                counter++;
                            }
                        } else {
                            prompt.Add(key, "");
                        }
                    } else if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value)) { // Key, Value 모두 존재
                        if (prompt.ContainsKey(key)) { // Key 중복처리
                            while (true) {
                                if (!prompt.ContainsKey($"{key} ({counter})")) {
                                    prompt.Add($"{key} ({counter})", value);
                                    row.Cells[0].Value = $"{key} ({counter})";
                                    break;
                                }
                                counter++;
                            }
                        } else {
                            prompt.Add(key, value);
                        }
                    }
                    // key, value 모두 없으면 무시
                }

                json.Add("prompt", prompt);
                string newJson = JsonConvert.SerializeObject(json, Formatting.Indented);
                File.WriteAllText(dataPath, newJson);

                item.LoadJson();
                itemInfo.Update();
                Console.WriteLine("data 파일 저장: " + item.modelName);
                SetStatus("프롬프트 수정: " + item.modelName + ".data");
            } catch {
                item.LoadJson();
                itemInfo.Update();
                itemInfo.UpdatePromptGrid();
                Console.WriteLine("data 파일 저장 실패: " + item.modelName);
                SetStatus("프롬프트 수정 실패: " + item.modelName + ".data");
            }
            SavePromptDataButton.BackColor = Color.White;
        }
        public void PromptSheet_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
            SavePromptDataButton.BackColor = Color.Bisque;

            if (settings["save_changes_immediately"]) {
                SavePromptData();
            }
        }
        public void PromptSheet_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e) {
            SavePromptDataButton.BackColor = Color.Bisque;

            if (settings["save_changes_immediately"]) {
                SavePromptData();
            }
        }
        public void ItemAttributeGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
            SaveItemDataChangeButton.BackColor = Color.Bisque;

            if (settings["save_changes_immediately"]) {
                SaveItemData();
            }
        }


        // Star grading
        private void StarGrade_MouseMove(object sender, MouseEventArgs e) {
            PictureBox pic = sender as PictureBox;
            int picWidth = pic.Width;
            int mouseX = e.Location.X;

            if (mouseX < picWidth / 10 - 1) {
                pic.Image = Resources.star1;
            } else if (mouseX < picWidth * 2 / 10 - 1) {
                pic.Image = Resources.star2;
            } else if (mouseX < picWidth * 3 / 10) {
                pic.Image = Resources.star3;
            } else if (mouseX < picWidth * 4 / 10) {
                pic.Image = Resources.star4;
            } else if (mouseX < picWidth * 5 / 10 + 1) {
                pic.Image = Resources.star5;
            } else if (mouseX < picWidth * 6 / 10 + 1) {
                pic.Image = Resources.star6;
            } else if (mouseX < picWidth * 7 / 10 + 2) {
                pic.Image = Resources.star7;
            } else if (mouseX < picWidth * 8 / 10 + 3) {
                pic.Image = Resources.star8;
            } else if (mouseX < picWidth * 9 / 10 + 3) {
                pic.Image = Resources.star9;
            } else if (mouseX <= picWidth * 10 / 10) {
                pic.Image = Resources.star10;
            }
        }
        private void StarGrade_MouseClick(object sender, MouseEventArgs e) {
            if (currentItem == null) {
                return;
            }

            PictureBox pic = sender as PictureBox;
            int picWidth = pic.Width;
            int mouseX = e.Location.X;

            if (mouseX < picWidth / 10 - 1) {
                SetGrade(1);
            } else if (mouseX < picWidth * 2 / 10 - 1) {
                SetGrade(2);
            } else if (mouseX < picWidth * 3 / 10) {
                SetGrade(3);
            } else if (mouseX < picWidth * 4 / 10) {
                SetGrade(4);
            } else if (mouseX < picWidth * 5 / 10 + 1) {
                SetGrade(5);
            } else if (mouseX < picWidth * 6 / 10 + 1) {
                SetGrade(6);
            } else if (mouseX < picWidth * 7 / 10 + 2) {
                SetGrade(7);
            } else if (mouseX < picWidth * 8 / 10 + 3) {
                SetGrade(8);
            } else if (mouseX < picWidth * 9 / 10 + 3) {
                SetGrade(9);
            } else if (mouseX <= picWidth * 10 / 10) {
                SetGrade(10);
            }
        }
        private void StarGrade_MouseLeave(object sender, EventArgs e) {
            PictureBox pic = sender as PictureBox;
            if (currentItem == null) {
                pic.Image = Resources.star0;
            } else {
                int point = 0;
                if (int.TryParse(currentItem.data.grade, out point)) {
                    if (point > 0 && point < 11) {
                        FixStarGradeGraphic(point);
                    } else {
                        pic.Image = Resources.star0;
                    }
                } else {
                    pic.Image = Resources.star0;
                }
            }
        }
        private void SetGrade(int point) {
            currentItem.data.grade = point.ToString();
            itemInfo.UpdateDataGrid();
            SaveItemData();
            if (settings["show_star_grade_on_thumbnail"]) {
                currentItem.UpdateGradeGraphic();
            }
        }
        public void FixStarGradeGraphic(int point) {
            switch (point) {
                case 0:
                    StarGrade.Image = Resources.star0;
                    break;
                case 1:
                    StarGrade.Image = Resources.star1;
                    break;
                case 2:
                    StarGrade.Image = Resources.star2;
                    break;
                case 3:
                    StarGrade.Image = Resources.star3;
                    break;
                case 4:
                    StarGrade.Image = Resources.star4;
                    break;
                case 5:
                    StarGrade.Image = Resources.star5;
                    break;
                case 6:
                    StarGrade.Image = Resources.star6;
                    break;
                case 7:
                    StarGrade.Image = Resources.star7;
                    break;
                case 8:
                    StarGrade.Image = Resources.star8;
                    break;
                case 9:
                    StarGrade.Image = Resources.star9;
                    break;
                case 10:
                    StarGrade.Image = Resources.star10;
                    break;
            }
        }


        // Civitai API
        private async void GetCivitaiInfoButton_Click(object sender, EventArgs e) {
            if (currentItem == null) {
                return;
            }

            this.Enabled = false;

            try {
                string getInfoByHashAPI = "https://civitai.com/api/v1/model-versions/by-hash/";
                string modelPath = currentItem.modelPath;
                byte[] hash;

                using (SHA256 sha256 = SHA256.Create()) {
                    using (FileStream stream = File.OpenRead(modelPath)) {
                        hash = sha256.ComputeHash(stream);

                    }
                }
                string hashString = BitConverter.ToString(hash).Replace("-", "");
                Console.WriteLine($"{modelPath} 파일의 SHA256 해쉬값: {hashString}");

                using (HttpClient client = new HttpClient()) {
                    string url = getInfoByHashAPI + hashString;
                    HttpResponseMessage response = await client.GetAsync(url);
                    string jsonString = await response.Content.ReadAsStringAsync();

                    JObject json = JObject.Parse(jsonString);
                    if (json.ContainsKey("error")) {
                        Console.WriteLine("Civitai 정보 불러오기 실패");
                        SetStatus($"{currentItem.modelName}: Civitai 정보 불러오기 실패");
                    } else {
                        Console.WriteLine("Civitai 정보 불러오기 성공");
                        Console.WriteLine($"modelID: {json["modelId"]}");
                        Console.WriteLine($"url: {"https://civitai.com/models/" + json["modelId"]}");
                        Console.WriteLine($"name: {json["model"]["name"]}");
                        Console.WriteLine($"version: {json["name"]}");
                        Console.WriteLine($"type: {json["model"]["type"]}");

                        foreach (DataGridViewRow row in ItemAttributeGrid.Rows) {
                            string rowName = row.Cells[0].Value?.ToString();
                            if (rowName == "이름") {
                                row.Cells[1].Value = json["model"]["name"].ToString();
                            } else if (rowName == "주소") {
                                row.Cells[1].Value = "https://civitai.com/models/" + json["modelId"].ToString();
                            } else if (rowName == "버전") {
                                row.Cells[1].Value = json["name"].ToString();
                            }
                        }
                        if (currentItem.imagePath == null) {
                            string imageUrl = json["images"][0]["url"].ToString();
                            string extension = Path.GetExtension(imageUrl);
                            string savePath = Path.Combine(currentItem.parentPath, currentItem.modelName + extension);

                            WebClient webClient = new WebClient();
                            webClient.DownloadFile(imageUrl, savePath);
                            currentItem.imagePath = savePath;
                            currentItem.LoadImage();
                        }

                        // creator 정보는 modelId API로 불러와야 함
                        Console.WriteLine("Civitai 정보 모델 ID로 불러오기...");
                        string getInfosByModelID = "https://civitai.com/api/v1/models/";
                        using (HttpClient client2 = new HttpClient()) {
                            string url2 = getInfosByModelID + json["modelId"].ToString();
                            HttpResponseMessage response2 = await client2.GetAsync(url2);
                            string jsonString2 = await response2.Content.ReadAsStringAsync();

                            JObject json2 = JObject.Parse(jsonString2);
                            if (json2.ContainsKey("error")) {
                                Console.WriteLine("Civitai 정보 모델 ID로 불러오기 실패");
                            } else {
                                Console.WriteLine("Civitai 정보 모델 ID로 불러오기 성공");
                                Console.WriteLine($"type: {json2["creator"]["username"]}");

                                foreach (DataGridViewRow row in ItemAttributeGrid.Rows) {
                                    string rowName = row.Cells[0].Value?.ToString();
                                    if (rowName == "제작자") {
                                        row.Cells[1].Value = json2["creator"]["username"].ToString();
                                    }
                                }
                            }
                        }

                        SetStatus($"{currentItem.modelName}: Civitai 정보 불러오기 성공");
                    }
                }
            } catch {
                SetStatus($"{currentItem.modelName}: Civitai 정보 불러오기 실패");
            }

            this.Enabled = true;
        }


        // 사용 안 함
        // Lazy Loading
        /*
        private void ItemFlowLayout_Scroll(object sender, ScrollEventArgs e) {
            Virtualization();
        }
        private void ItemFlowLayout_MouseWheel(object sender, MouseEventArgs e) {
            Virtualization();
        }
        private void Virtualization() {
            int itemHeight = settings["item_height"] + 33; // 150일 경우 183
            int itemWidth = settings["item_width"] + 6;
            int itemNum = itemFlowLayout.itemDict.Values.Count();
            int flowLayoutWidth = ItemFlowLayout.ClientSize.Width;
            int flowLayoutHeight = ItemFlowLayout.ClientSize.Height; // 전체 사이즈가 아니라 스크롤 제외한 client size임

            Console.WriteLine("");
            Console.WriteLine($"아이템 개수: {itemNum.ToString()}");
            Console.WriteLine("아이템 세로크기: " + itemHeight.ToString());
            int maxNumOfItemsCanBePlacedInARow = flowLayoutWidth / itemWidth;
            Console.WriteLine("한 줄에 놓일 수 있는 최대 개수: " + maxNumOfItemsCanBePlacedInARow.ToString());
            int totalHeightOfItems = (int)(Math.Ceiling((double)itemNum / maxNumOfItemsCanBePlacedInARow) * itemHeight);
            Console.WriteLine("아이템 쌓았을 때 전체 세로길이: " + totalHeightOfItems.ToString());

            int currentScrollPosition = -ItemFlowLayout.AutoScrollPosition.Y; // 스크롤 내리면 마이너스 방향으로 감
            Console.WriteLine("현재 스크롤 y좌표: " + currentScrollPosition.ToString());
            int lastScrollPosition = currentScrollPosition + flowLayoutHeight;
            Console.WriteLine($"스크롤 최하단 y좌표: {lastScrollPosition.ToString()}");

            int initItemIndex = currentScrollPosition / itemHeight * maxNumOfItemsCanBePlacedInARow;
            Console.WriteLine($"표시되는 첫 번째 아이템 인덱스: {initItemIndex.ToString()}");
            int lastItemIndex = lastScrollPosition / itemHeight * maxNumOfItemsCanBePlacedInARow + (maxNumOfItemsCanBePlacedInARow - 1);
            Console.WriteLine($"표시되는 마지막 아이템 인덱스: {lastItemIndex.ToString()}");
        }
        */
    }

    public class Item {
        public string modelName { get; set; }
        public string parentPath { get; set; }
        public string modelPath { get; set; }
        public string imagePath { get; set; }
        public string dataPath { get; set; }
        public int itemWidth { get; set; }
        public int itemHeight { get; set; }
        public Bitmap image { get; set; }
        public PictureBox gradePictureBox { get; set; }
        public PictureBox pictureBox { get; set; }
        public Label label { get; set; }
        public Panel panel { get; set; }
        public Data data { get; set; }
        public class Data {
            public string name { get; set; }
            public string tag { get; set; }
            public string note { get; set; }
            public string creator { get; set; }
            public string url { get; set; }
            public string version { get; set; }
            public string downloadDate { get; set; }
            public string grade { get; set; }
            public JObject prompt { get; set; }
        }
        public void CreateEmptyData() {
            string path = this.parentPath;
            string fileName = this.modelName;
            var jprop = new JObject(
                new JProperty("!트리거", "<lora:__filename__:1>, ")
            );
            JObject jobj = new JObject(
                new JProperty("name", fileName),
                new JProperty("tag", ""),
                new JProperty("note", ""),
                new JProperty("creator", ""),
                new JProperty("url", ""),
                new JProperty("version", ""),
                new JProperty("downloadDate", DateTime.Now.ToString("yyyy-MM-dd")),
                new JProperty("grade", ""),
                new JProperty("prompt", jprop)
            );
            string resultPath = Path.Combine(path, fileName + ".data");
            File.WriteAllText(resultPath, jobj.ToString());
            this.dataPath = resultPath;
        }
        public void LoadJson() {
            if (this.dataPath == null) {
                return;
            }
            string json = File.ReadAllText(this.dataPath);
            var itemJson = JsonConvert.DeserializeObject<Item.Data>(json);
            this.data = itemJson;
        }
        public Bitmap LoadImageFromLocal(int imageWidth, int imageHeight) {
            string path = this.imagePath;
            if (path == null) {
                return null;
            }
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (BinaryReader reader = new BinaryReader(stream)) {
                    var memoryStream = new MemoryStream(reader.ReadBytes((int)stream.Length));
                    using (var image = new Bitmap(memoryStream)) {
                        // 이미지의 너비와 maxWidth, 높이와 maxHeight의 비율을 구하고, (boxWidth / image.Width)
                        // 가로, 세로 중 비율이 더 큰 놈을 자기 사이즈에 곱함.
                        int boxWidth = imageWidth;
                        int boxHeight = imageHeight;

                        int newWidth = 0;
                        int newHeight = 0;
                        float widthRatio = (float)boxWidth / (float)image.Width;
                        float heightRatio = (float)boxHeight / (float)image.Height;
                        float ratio = 0;

                        if (widthRatio >= heightRatio) {
                            ratio = widthRatio;
                        } else {
                            ratio = heightRatio;
                        }
                        newWidth = (int)(image.Width * ratio);
                        newHeight = (int)(image.Height * ratio);

                        Bitmap resizedImage = new Bitmap(newWidth, newHeight);
                        using (Graphics graphics = Graphics.FromImage(resizedImage)) {
                            graphics.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight),
                                new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                        }
                        image.Dispose();
                        return resizedImage;
                    }
                }
            }
        }
        public async void LoadImage() {
            var task = Task.Run(() => LoadImageFromLocal(itemWidth, itemHeight));

            this.image = await task;
            if (this.image == null) {
                this.image = Properties.Resources.anonymous;
            }
            this.pictureBox.Image = this.image;
        }
        public void CreatePictureBox() {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Size = new Size(itemWidth, itemHeight);

            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            pictureBox.Tag = this;
            this.pictureBox = pictureBox;

            // 비동기 로딩
            LoadImage();
        }
        public void CreateLabel() {
            string labelText = null;
            if (!String.IsNullOrEmpty(this.data.name)) {
                labelText = this.data.name;
            } else {
                labelText = this.modelName;
            }

            Label label = new Label();

            label.Size = new Size(itemWidth, 24);
            label.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            label.Location = new Point(0, itemHeight + 3);
            label.AutoSize = false;
            label.AutoEllipsis = true;
            label.Margin = new Padding(0);

            label.Text = labelText;
            this.label = label;
        }
        public void UpdateLabelText() {
            if (!String.IsNullOrEmpty(this.data.name)) {
                this.label.Text = this.data.name;
            } else {
                this.label.Text = this.modelName;
            }
        }
        public void CreateGradeGraphic() {
            PictureBox gradePictureBox = new PictureBox();
            gradePictureBox.Size = new Size(90, 16);
            gradePictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            gradePictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            gradePictureBox.Margin = new Padding(0);
            gradePictureBox.Location = new Point(3, itemHeight - 20);
            gradePictureBox.BackColor = Color.Transparent;
            gradePictureBox.Tag = this;
            this.gradePictureBox = gradePictureBox;
            UpdateGradeGraphic();
        }
        public void UpdateGradeGraphic() {
            int point = 0;
            if (int.TryParse(this.data.grade, out point)) {
                if (point > 0 && point < 11) {
                    switch (point) {
                        case 0:
                            this.gradePictureBox.Image = Resources.bottom0;
                            break;
                        case 1:
                            this.gradePictureBox.Image = Resources.bottom1;
                            break;
                        case 2:
                            this.gradePictureBox.Image = Resources.bottom2;
                            break;
                        case 3:
                            this.gradePictureBox.Image = Resources.bottom3;
                            break;
                        case 4:
                            this.gradePictureBox.Image = Resources.bottom4;
                            break;
                        case 5:
                            this.gradePictureBox.Image = Resources.bottom5;
                            break;
                        case 6:
                            this.gradePictureBox.Image = Resources.bottom6;
                            break;
                        case 7:
                            this.gradePictureBox.Image = Resources.bottom7;
                            break;
                        case 8:
                            this.gradePictureBox.Image = Resources.bottom8;
                            break;
                        case 9:
                            this.gradePictureBox.Image = Resources.bottom9;
                            break;
                        case 10:
                            this.gradePictureBox.Image = Resources.bottom10;
                            break;
                    }
                    return;
                }
            }
            this.gradePictureBox.Image = null;
        }
        public void CreateItemPanel() {
            Panel panel = new Panel();
            panel.Size = new Size(itemWidth, itemHeight + 27);
            panel.Controls.Add(this.pictureBox);
            panel.Controls.Add(this.label);
            panel.Controls.Add(this.gradePictureBox);
            this.gradePictureBox.BringToFront();
            this.gradePictureBox.Parent = this.pictureBox;
            this.panel = panel;
        }
        public void Resizing() {
            this.pictureBox.Size = new Size(itemWidth, itemHeight);
            this.label.Size = new Size(itemWidth, 24);
            this.label.Location = new Point(0, itemHeight + 3);
            this.panel.Size = new Size(itemWidth, itemHeight + 27);
        }
    }

    public class LocalTreeView {
        public string SetRootDirectory(TreeView tree) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "LoRA 폴더 선택";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                return dialog.FileName;
            } else {
                return null;
            }
        }
        public void InitializeLocalTreeView(TreeView tree, string rootPath, List<string> modelPathes) {
            // model의 경로를 반환, model이 존재하는 경로에 대해서만 노드를 생성하는 방식
            tree.Nodes.Clear();

            TreeNode rootNode = new TreeNode(Path.GetFileName(rootPath));
            rootNode.Tag = rootPath;

            foreach (string path in modelPathes) {
                string relativePath = path.Replace(rootPath, "");
                string[] folders = relativePath.Split('\\');

                TreeNode currentNode = rootNode;
                foreach (string folder in folders) {
                    if (!string.IsNullOrEmpty(folder)) {
                        bool nodeExists = false;
                        foreach (TreeNode childNode in currentNode.Nodes) {
                            if (childNode.Text == folder) { // 노드가 이미 존재할 경우
                                currentNode = childNode;
                                nodeExists = true;
                                break;
                            }
                        }
                        if (!nodeExists) {
                            // 새 노드 생성
                            string fullPath = Path.Combine(currentNode.Tag.ToString(), folder);
                            if (Directory.Exists(fullPath)) {
                                TreeNode newNode = new TreeNode(folder);
                                newNode.Tag = fullPath;
                                currentNode.Nodes.Add(newNode);
                                currentNode = newNode;
                            }
                        }
                    }
                }
            }

            tree.Nodes.Add(rootNode);
        }
        /* data 파일 존재 여부로 구현하는 방식.
        private void AddNodes(string directoryPath, TreeNodeCollection parentNodeCollection) {
            // Check if the directory contains a ".data" extension file.
            bool containsDataFile = Directory.GetFiles(directoryPath, "*.data").Any();

            if (containsDataFile) {
                // Add a new node to the parent node collection.
                string directoryName = new DirectoryInfo(directoryPath).Name;
                TreeNode newNode = parentNodeCollection.Add(directoryName);
                newNode.Tag = directoryPath;

                // Recursively add child nodes.
                foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath)) {
                    AddNodes(subDirectoryPath, newNode.Nodes);
                }
            } else {
                // Don't add a node for this directory if it doesn't contain a ".data" extension file.
                foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath)) {
                    AddNodes(subDirectoryPath, parentNodeCollection);
                }
            }
        }
        */
        /* 하위 폴더 전부 구현하는 방식.
        public TreeNode CreateDirectoryNode(DirectoryInfo dirInfo) {
            TreeNode dirNode = new TreeNode(dirInfo.Name);
            dirNode.Tag = dirInfo;
            foreach (DirectoryInfo dir in dirInfo.GetDirectories()) {
                dirNode.Nodes.Add(CreateDirectoryNode(dir));
            }
            return dirNode;
        }
        */
    }

    public class ItemFlowLayout {
        MainForm mainForm;
        public Dictionary<string, Item> itemDict;
        public List<Item> currentItems; // Visible 속성 토글이 아니라, 업데이트마다 flowLayout의 컨트롤을 갱신하는 방식

        public FlowLayoutPanel flowLayout;
        public Label pageLabel;
        public Button previousButton;
        public Button nextButton;

        public ItemFlowLayout(MainForm mainForm) {
            this.mainForm = mainForm;
            flowLayout = mainForm.Controls.Find("ItemFlowLayout", true).FirstOrDefault() as FlowLayoutPanel;
            pageLabel = mainForm.Controls.Find("CurrentPageLabel", true).FirstOrDefault() as Label;
            previousButton = mainForm.Controls.Find("PreviousPageButton", true).FirstOrDefault() as Button;
            nextButton = mainForm.Controls.Find("NextPageButton", true).FirstOrDefault() as Button;
        }

        public void Update(int page) {
            if (itemDict == null) {
                return;
            }

            Console.WriteLine("Flow layout 구성 시작");

            int itemsPerPage = mainForm.settings["items_per_page"];
            if (itemsPerPage == 0) {
                itemsPerPage = int.MaxValue;
            }
            int itemNum = currentItems.Count();
            if (page > (itemNum + itemsPerPage - 1) / itemsPerPage) {
                page = 0;
            }

            bool noMorePage = false;

            int startIndex = page * itemsPerPage;
            int endIndex = startIndex + itemsPerPage - 1;
            if (endIndex >= itemNum - 1) {
                endIndex = itemNum - 1;
                noMorePage = true;
            }

            // 페이지 이동 버튼 관리
            if (page <= 0) {
                previousButton.Enabled = false;
                previousButton.BackColor = Color.Gainsboro;
            } else {
                previousButton.Enabled = true;
                previousButton.BackColor = Color.White;
            }
            if (noMorePage) {
                nextButton.Enabled = false;
                nextButton.BackColor = Color.Gainsboro;
            } else {
                nextButton.Enabled = true;
                nextButton.BackColor = Color.White;
            }


            Paging(startIndex, endIndex);

            mainForm.currentPage = page;
            pageLabel.Text = $"{mainForm.currentPage + 1} page";

            Console.WriteLine("Flow layout 구성 완료");
        }
        private void Paging(int startIndex, int endIndex) {
            Console.WriteLine($"Paging: {startIndex} to {endIndex}");

            if (currentItems == null) {
                return;
            }

            flowLayout.SuspendLayout();

            flowLayout.Controls.Clear();

            for (int i = startIndex; i <= endIndex; i++) {
                if (flowLayout.InvokeRequired) { // UI 쓰레드가 아니면
                    flowLayout.BeginInvoke(new Action(() => { // UI 쓰레드 비동기 처리
                        flowLayout.Controls.Add(currentItems[i].panel);
                    }));
                } else {
                    flowLayout.Controls.Add(currentItems[i].panel); // 맞으면 동기 처리
                }
            }
            flowLayout.VerticalScroll.Value = 0;
            flowLayout.ResumeLayout();
        }

        public void GetCurrentItems() {
            if (itemDict == null) {
                return;
            }

            string path = mainForm.currentDirectory;

            List<Item> items = new List<Item>();
            foreach (Item item in itemDict.Values) {
                string relativePath = GetRelativePath(path, item.parentPath);
                if (relativePath != null) {
                    items.Add(item);
                }
            }
            currentItems = items;
        }
        public void GetSearchedCurrentItems(string searchText, string category) {
            GetCurrentItems(); // currentItems 초기화
            if (String.IsNullOrEmpty(searchText)) { return; }

            List<Item> items = new List<Item>();

            searchText = searchText.ToLower();
            switch (category) {
                case "전체":
                    foreach (Item item in currentItems) {
                        string modelName = item.modelName.ToLower() ?? "";
                        string name = item.data.name ?? "";
                        string tag = item.data.tag ?? "";
                        string creator = item.data.creator ?? "";

                        if (modelName.Contains(searchText) || name.Contains(searchText) || tag.Contains(searchText) || creator.Contains(searchText)) {
                            items.Add(item);
                        }
                    }
                    break;
                case "파일명":
                    foreach (Item item in currentItems) {
                        string modelName = item.modelName.ToLower() ?? "";

                        if (modelName.Contains(searchText)) {
                            items.Add(item);
                        }
                    }
                    break;
                case "이름":
                    foreach (Item item in currentItems) {
                        string name = item.data.name ?? "";

                        if (name.Contains(searchText)) {
                            items.Add(item);
                        }
                    }
                    break;
                case "태그":
                    foreach (Item item in currentItems) {
                        string tag = item.data.tag ?? "";

                        if (tag.Contains(searchText)) {
                            items.Add(item);
                        }
                    }
                    break;
                case "제작자":
                    foreach (Item item in currentItems) {
                        string creator = item.data.creator ?? "";

                        if (creator.Contains(searchText)) {
                            items.Add(item);
                        }
                    }
                    break;
            }

            currentItems = items;
        }

        /* Visible 속성 토글로 관리하는 방식
        public void UpdateVisibility(string path) {
            flowLayout.SuspendLayout();

            foreach (var item in itemDict.Values) {
                string itemPath = item.parentPath;
                string compator = GetRelativePath(path, itemPath);

                if (compator != null) {
                    item.panel.Visible = true;
                } else {
                    item.panel.Visible = false;
                }
            }

            flowLayout.ResumeLayout();
        }
        public void UpdateVisibilityBySearch(string searchText, string category) {
            flowLayout.SuspendLayout();

            if (String.IsNullOrEmpty(searchText)) {
                return;
            }

            searchText = searchText.ToLower();
            switch (category) {
                case "전체":
                    foreach (var item in itemDict.Values) {
                        // 이미 true(해당 경로 하에 있음)인 애들만
                        if (item.panel.Visible == true) {
                            string modelName = item.modelName.ToLower() ?? "";
                            string name = item.data.name ?? "";
                            string tag = item.data.tag ?? "";
                            string creator = item.data.creator ?? "";

                            if (modelName.Contains(searchText) || name.Contains(searchText) || tag.Contains(searchText) || creator.Contains(searchText)) {
                                continue;
                            } else {
                                item.panel.Visible = false;
                            }
                        }
                    }
                    break;
                case "파일명":
                    foreach (var item in itemDict.Values) {
                        // 이미 true(해당 경로 하에 있음)인 애들만
                        if (item.panel.Visible == true) {
                            string modelName = item.modelName.ToLower() ?? "";

                            if (modelName.Contains(searchText)) {
                                continue;
                            } else {
                                item.panel.Visible = false;
                            }
                        }
                    }
                    break;
                case "이름":
                    foreach (var item in itemDict.Values) {
                        // 이미 true(해당 경로 하에 있음)인 애들만
                        if (item.panel.Visible == true) {
                            string name = item.data.name ?? "";

                            if (name.Contains(searchText)) {
                                continue;
                            } else {
                                item.panel.Visible = false;
                            }
                        }
                    }
                    break;
                case "태그":
                    foreach (var item in itemDict.Values) {
                        // 이미 true(해당 경로 하에 있음)인 애들만
                        if (item.panel.Visible == true) {
                            string tag = item.data.tag ?? "";

                            if (tag.Contains(searchText)) {
                                continue;
                            } else {
                                item.panel.Visible = false;
                            }
                        }
                    }
                    break;
                case "제작자":
                    foreach (var item in itemDict.Values) {
                        // 이미 true(해당 경로 하에 있음)인 애들만
                        if (item.panel.Visible == true) {
                            string creator = item.data.creator ?? "";

                            if (creator.Contains(searchText)) {
                                continue;
                            } else {
                                item.panel.Visible = false;
                            }
                        }
                    }
                    break;
            }



            flowLayout.ResumeLayout();
        }
        */

        public int GetAllItems(string path) {
            Console.WriteLine("아이템 딕셔너리 생성 시작");

            string[] modelExt = new[] { ".pt", ".safetensors", ".ckpt" };

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(file => modelExt.Contains(Path.GetExtension(file)))
                .ToArray();
            Dictionary<string, Item> dict = new Dictionary<string, Item>();

            foreach (string file in files) {
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (dict.ContainsKey(fileName)) {
                    var confirmResult = MessageBox.Show("파일1 경로: " + dict[fileName].parentPath + "\n파일2 경로: " + Path.GetDirectoryName(file) + "\n중복된 파일명: " + fileName + "\n\n모델 파일의 이름은 중복될 수 없습니다. 프로그램을 종료합니다.", "중복된 모델명 감지", MessageBoxButtons.OK);
                    System.Windows.Forms.Application.Exit();
                }

                // modelName, modelPath, parentPath, size,
                string modelPath = Path.GetFullPath(file);
                string parentPath = Path.GetDirectoryName(file);
                dict[fileName] = new Item { modelName = fileName, modelPath = modelPath, parentPath = parentPath, itemHeight = mainForm.settings["item_height"], itemWidth = mainForm.settings["item_width"] };

                // imagePath
                string[] validImageExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                foreach (string ext in validImageExt) {
                    string imagePath = Path.Combine(parentPath, fileName + ext);
                    if (File.Exists(imagePath)) {
                        dict[fileName].imagePath = imagePath;
                        break;
                    }
                    imagePath = Path.Combine(parentPath, fileName + ".preview" + ext);
                    if (File.Exists(imagePath)) {
                        dict[fileName].imagePath = imagePath;
                        break;
                    }
                }

                // dataPath
                string dataPath = Path.Combine(parentPath, fileName + ".data");
                if (File.Exists(dataPath)) {
                    dict[fileName].dataPath = dataPath;
                }
            }

            foreach (Item item in dict.Values) {
                if (String.IsNullOrEmpty(item.dataPath)) {
                    item.CreateEmptyData();
                }
                item.LoadJson();
                item.CreatePictureBox();
                item.pictureBox.Click += new EventHandler(mainForm.ItemClick);
                item.CreateLabel();
                item.CreateGradeGraphic();
                item.gradePictureBox.Click += new EventHandler(mainForm.ItemClick);
                item.CreateItemPanel();
                if (mainForm.settings["show_star_grade_on_thumbnail"] == false) {
                    item.gradePictureBox.Image = null;
                }
                Console.WriteLine("아이템 생성: " + item.modelName);
            }

            itemDict = dict;
            Console.WriteLine("아이템 딕셔너리 생성 완료");
            return itemDict.Count();
        }
        public void SortItems(int methodIndex) {
            if (itemDict == null) {
                return;
            }

            Console.WriteLine("아이템 딕셔너리 정렬 시작");
            SortMethod method = (SortMethod)methodIndex;
            switch (method) {
                case SortMethod.NameAscending: // name이 비었으면 modelName으로 대신
                    itemDict = itemDict.Values.OrderBy(item => string.IsNullOrEmpty(item.data.name) ? item.modelName : item.data.name).ToDictionary(item => item.modelName, item => item);
                    break;
                case SortMethod.NameDescending:
                    itemDict = itemDict.Values.OrderByDescending(item => string.IsNullOrEmpty(item.data.name) ? item.modelName : item.data.name).ToDictionary(item => item.modelName, item => item);
                    break;
                case SortMethod.FileNameAscending:
                    itemDict = itemDict.Values.OrderBy(item => item.modelName).ToDictionary(item => item.modelName, item => item);
                    break;
                case SortMethod.FileNameDescending:
                    itemDict = itemDict.Values.OrderByDescending(item => item.modelName).ToDictionary(item => item.modelName, item => item);
                    break;
                case SortMethod.CreateDateAscending:
                    itemDict = itemDict.Values.OrderBy(item => {
                        DateTime downloadDate;
                        if (DateTime.TryParse(item.data.downloadDate, out downloadDate)) {
                            return downloadDate;
                        }
                        return DateTime.MaxValue;
                    }).ToDictionary(item => item.modelName, item => item);
                    break;
                case SortMethod.CreateDateDescending:
                    itemDict = itemDict.Values.OrderByDescending(item => {
                        DateTime downloadDate;
                        if (DateTime.TryParse(item.data.downloadDate, out downloadDate)) {
                            return downloadDate;
                        }
                        return DateTime.MinValue;
                    }).ToDictionary(item => item.modelName, item => item);
                    break;
                case SortMethod.GradeAscending:
                    itemDict = itemDict.Values.OrderBy(item => {
                        int point = 0;
                        if (int.TryParse(item.data.grade, out point)) {
                            if (point > 0 && point < 11) {
                                return point;
                            } else {
                                return 11;
                            }
                        }
                        return 11;
                    }).ToDictionary(item => item.modelName, item => item);
                    break;
                case SortMethod.GradeDescending:
                    itemDict = itemDict.Values.OrderByDescending(item => {
                        int point = 0;
                        if (int.TryParse(item.data.grade, out point)) {
                            if (point > 0 && point < 11) {
                                return point;
                            } else {
                                return 0;
                            }
                        }
                        return 0;
                    }).ToDictionary(item => item.modelName, item => item);
                    break;
            }

            mainForm.SetStatus("아이템 정렬.");
            Console.WriteLine("아이템 딕셔너리 정렬 완료");
        }
        public void InitializeFlowLayout(int methodIndex) {
            if (itemDict == null) {
                return;
            }

            SortItems(methodIndex);
            GetCurrentItems();
            Update(0);
        }
        public string GetRelativePath(string basePath, string targetPath) {
            if (basePath == targetPath) { // 경로 같음
                return "";
            } else if (targetPath.StartsWith(basePath)) { // 하위 경로임
                return targetPath.Substring(basePath.Length);
            } else { // 상위경로임(표시X)
                return null;
            }
        }
    }

    public class ItemInfo {
        public MainForm mainForm;
        public Item item;
        public Dictionary<string, string> itemPrompts;
        FlowLayoutPanel flowLayout;
        PictureBox pictureBox;
        PictureBox starGrade;
        FlowLayoutPanel promptFlowLayout;
        DataGridView dataGrid;
        DataGridView attributeGrid;
        Button savePromptButton;
        Button saveDataButton;

        public ItemInfo(MainForm mainForm) {
            this.mainForm = mainForm;
            this.item = mainForm.currentItem;
            this.flowLayout = mainForm.Controls.Find("CurrentItemFlowLayout", true).FirstOrDefault() as FlowLayoutPanel;
            this.pictureBox = mainForm.Controls.Find("CurrentItemPictureBox", true).FirstOrDefault() as PictureBox;
            this.starGrade = mainForm.Controls.Find("StarGrade", true).FirstOrDefault() as PictureBox;
            this.promptFlowLayout = mainForm.Controls.Find("PromptFlowLayout", true).FirstOrDefault() as FlowLayoutPanel;
            this.dataGrid = mainForm.Controls.Find("PromptSheet", true).FirstOrDefault() as DataGridView;
            this.attributeGrid = mainForm.Controls.Find("ItemAttributeGrid", true).FirstOrDefault() as DataGridView;
            this.savePromptButton = mainForm.Controls.Find("SavePromptDataButton", true).FirstOrDefault() as Button;
            this.saveDataButton = mainForm.Controls.Find("SaveItemDataChangeButton", true).FirstOrDefault() as Button;
        }
        public void Clear() {
            flowLayout.Controls.Clear();
            pictureBox.Image = null;
            promptFlowLayout.Controls.Clear();
            starGrade.Image = Resources.star0;

            // DataGridView 삭제 전 CellValueChanged 이벤트 핸들러 삭제
            dataGrid.CellValueChanged -= mainForm.PromptSheet_CellValueChanged;
            dataGrid.RowsRemoved -= mainForm.PromptSheet_RowsRemoved;
            attributeGrid.CellValueChanged -= mainForm.ItemAttributeGrid_CellValueChanged;

            dataGrid.Rows.Clear();
            attributeGrid.Rows.Clear();

            dataGrid.CellValueChanged += mainForm.PromptSheet_CellValueChanged;
            dataGrid.RowsRemoved += mainForm.PromptSheet_RowsRemoved;
            attributeGrid.CellValueChanged += mainForm.ItemAttributeGrid_CellValueChanged;
        }
        public void Update() {
            item = mainForm.currentItem;

            if (item == null) {
                Clear();
                return;
            }
            UpdateUpperContiner();

            JObject prompts = item.data.prompt;
            string json = prompts.ToString();
            itemPrompts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            UpdatePrompt();

            savePromptButton.BackColor = Color.White;
            saveDataButton.BackColor = Color.White;
        }
        public void UpdateUpperContiner() {
            flowLayout.Controls.Clear();
            if (item.imagePath == null) {
                pictureBox.Image = Properties.Resources.anonymous;
            } else {
                pictureBox.Image = item.LoadImageFromLocal(100, 150);
            }
            // pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            // pictureBox.BorderStyle = BorderStyle.FixedSingle;
            int labelWidth = flowLayout.Width - 20;
            Padding labelPadding = new Padding(0, 4, 0, 0);

            // 이름 라벨
            Label nameLabel = new Label();
            nameLabel.AutoEllipsis = true;
            nameLabel.Text = "이름: " + item.data.name;
            nameLabel.Width = labelWidth;
            nameLabel.Padding = labelPadding;
            if (!String.IsNullOrEmpty(item.data.version)) {
                nameLabel.Text += $" ({item.data.version})";
            }
            flowLayout.Controls.Add(nameLabel);

            // 태그 라벨
            if (!string.IsNullOrEmpty(item.data.tag)) {
                Label tagLabel = new Label();
                tagLabel.AutoEllipsis = true;
                tagLabel.Text = "태그: " + item.data.tag;
                tagLabel.Width = labelWidth;
                tagLabel.Padding = labelPadding;
                flowLayout.Controls.Add(tagLabel);
            }

            // 제작자 라벨
            if (!String.IsNullOrEmpty(item.data.creator)) {
                Label creatorLabel = new Label();
                creatorLabel.AutoEllipsis = true;
                creatorLabel.Text = "제작자: " + item.data.creator;
                creatorLabel.Width = labelWidth;
                creatorLabel.Padding = labelPadding;
                flowLayout.Controls.Add(creatorLabel);
            }

            // 메모 라벨
            Label memoLabel = new Label();
            memoLabel.Text = "[메모]\n" + item.data.note;
            memoLabel.AutoSize = true;
            memoLabel.Padding = labelPadding;
            flowLayout.Controls.Add(memoLabel);

            // 별점
            int point = 0;
            if (int.TryParse(item.data.grade, out point)) {
                if (point > 0 && point < 11) {
                    mainForm.FixStarGradeGraphic(point);
                } else {
                    starGrade.Image = Resources.star0;
                }
            } else {
                starGrade.Image = Resources.star0;
            }
        }
        public void UpdatePrompt() {
            promptFlowLayout.Controls.Clear();
            const int groupBoxHeight = 70;

            // !로 시작하면 핵심 프롬
            // ~로 시작하면 네거티브 프롬
            // !로 시작하는 핵심 프롬을 가장 위로, dict 멤버들을 순회하며 핵심 프롬 딕셔너리와 일반프롬(네거 포함) 딕셔너리를 생성한 후, 다시 딕셔너리를 순회하며 버튼을 생성하는 식
            Dictionary<string, string> keyPrompt = new Dictionary<string, string>();
            Dictionary<string, string> commonPrompt = new Dictionary<string, string>();
            Dictionary<string, string> negativePrompt = new Dictionary<string, string>();

            foreach (var prompt in itemPrompts) {
                string key = prompt.Key.Replace("__filename__", item.modelName);
                string value = prompt.Value.Replace("__filename__", item.modelName); ;

                if (key.StartsWith("!")) {
                    keyPrompt.Add(key.Substring(1), value);
                } else if (key.StartsWith("~")) {
                    negativePrompt.Add(key.Substring(1), value);
                } else {
                    commonPrompt.Add(key, value);
                }
            }

            // 키 프롬프트 ~ Bisque
            foreach (var prompt in keyPrompt) {
                GroupBox groupBox = new GroupBox();
                groupBox.Text = prompt.Key;
                groupBox.Height = groupBoxHeight;
                groupBox.Width = promptFlowLayout.Width - 30;
                groupBox.Padding = new Padding(0, 0, 0, 0);

                // groupBox는 레이아웃용이라 테두리만 클릭으로 인식함. Button으로 채워주고 버튼에 클릭이벤트
                Button button = new Button();
                button.Text = prompt.Value;
                button.BackColor = Color.Bisque;
                button.Dock = DockStyle.Fill;
                button.TextAlign = ContentAlignment.TopLeft;
                button.Padding = new Padding(5);

                button.Tag = "key";

                groupBox.Controls.Add(button);
                button.Click += new EventHandler(mainForm.PromptClick);

                promptFlowLayout.Controls.Add(groupBox);
            }
            // 일반 프롬프트 ~ White
            foreach (var prompt in commonPrompt) {
                GroupBox groupBox = new GroupBox();
                groupBox.Text = prompt.Key;
                groupBox.Height = groupBoxHeight;
                groupBox.Width = promptFlowLayout.Width - 30;
                groupBox.Padding = new Padding(0, 0, 0, 0);

                // groupBox는 레이아웃용이라 테두리만 클릭으로 인식함. Button으로 채워주고 버튼에 클릭이벤트
                Button button = new Button();
                button.Text = prompt.Value;
                button.BackColor = Color.White;
                button.Dock = DockStyle.Fill;
                button.TextAlign = ContentAlignment.TopLeft;
                button.Padding = new Padding(5);

                button.Tag = "common";

                groupBox.Controls.Add(button);
                button.Click += new EventHandler(mainForm.PromptClick);

                promptFlowLayout.Controls.Add(groupBox);
            }
            // 네거티브 프롬프트 ~ YellowGreen
            foreach (var prompt in negativePrompt) {
                GroupBox groupBox = new GroupBox();
                groupBox.Text = prompt.Key;
                groupBox.Height = groupBoxHeight;
                groupBox.Width = promptFlowLayout.Width - 30;
                groupBox.Padding = new Padding(0, 0, 0, 0);

                // groupBox는 레이아웃용이라 테두리만 클릭으로 인식함. Button으로 채워주고 버튼에 클릭이벤트
                Button button = new Button();
                button.Text = prompt.Value;
                button.BackColor = Color.YellowGreen;
                button.Dock = DockStyle.Fill;
                button.TextAlign = ContentAlignment.TopLeft;
                button.Padding = new Padding(5);

                button.Tag = "negative";

                groupBox.Controls.Add(button);
                button.Click += new EventHandler(mainForm.PromptClick);

                promptFlowLayout.Controls.Add(groupBox);
            }
        }

        public void UpdatePromptGrid() {
            // DataGridView 삭제 전 이벤트 핸들러 삭제 -> Rows.Clear 메서드가 RowsRemoved 이벤트 발생시킴.
            dataGrid.CellValueChanged -= mainForm.PromptSheet_CellValueChanged;
            dataGrid.RowsRemoved -= mainForm.PromptSheet_RowsRemoved;

            dataGrid.Rows.Clear();
            foreach (var p in itemPrompts) {
                dataGrid.Rows.Add(p.Key, p.Value);
            }

            dataGrid.CellValueChanged += mainForm.PromptSheet_CellValueChanged;
            dataGrid.RowsRemoved += mainForm.PromptSheet_RowsRemoved;
        }
        public void UpdateDataGrid() {
            // DataGridView 삭제 전 CellValueChanged 이벤트 핸들러 삭제
            attributeGrid.CellValueChanged -= mainForm.ItemAttributeGrid_CellValueChanged;

            attributeGrid.Rows.Clear();
            attributeGrid.Rows.Add("이름", item.data.name);
            attributeGrid.Rows.Add("태그", item.data.tag);
            attributeGrid.Rows.Add("메모", item.data.note);
            attributeGrid.Rows.Add("제작자", item.data.creator);
            attributeGrid.Rows.Add("주소", item.data.url);
            attributeGrid.Rows.Add("버전", item.data.version);
            attributeGrid.Rows.Add("생성 날짜", item.data.downloadDate);
            attributeGrid.Rows.Add("별점", item.data.grade);

            attributeGrid.CellValueChanged += mainForm.ItemAttributeGrid_CellValueChanged;
        }
    }

    public class DangerArea {
        public void DeleteAllDataFiles(string path) {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string file in files) {
                string fileExt = Path.GetExtension(file);
                if (Path.GetExtension(file) == ".data") {
                    File.Delete(file);
                    Console.WriteLine("파일 삭제: " + file);
                }
            }

            System.Windows.Forms.Application.Exit();
        }
    }
}
