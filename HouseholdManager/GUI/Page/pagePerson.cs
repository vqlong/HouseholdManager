﻿using DevExpress.XtraEditors;
using HouseholdManager.BUS;
using HouseholdManager.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace HouseholdManager.GUI
{
    /*
     * Khi insert, update, delete chỉ cập nhật lại hàng tương ứng trên GridView
     * Không load lại toàn bộ database
     * 
     */
    public partial class pagePerson : DevExpress.XtraEditors.XtraUserControl, INotifyPropertyChanged, IMode
    {
        public pagePerson(Account account)
        {
            InitializeComponent();           

            Initialize();

            LoadData();

            LoadEvent();

            Account = account;

        }

        Account _account;
        public Account Account
        {
            get => _account;
            set
            {
                _account = value;
                
                Help.SetPrivilege(AppSetting.Instance.PersonPrivilege, listPrivilege);
            }
        }

        BindingSource personBindingSource = new BindingSource();

        BindingSource householdBindingSource = new BindingSource();

        public IButtonControl AcceptButton { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 1 List&lt;Person&gt; dùng để binding cho ListPerson của pMain và pHousehold.
        /// </summary>
        public List<Person> ListPerson { get; set; }

        List<Household> _listHousehold;
        /// <summary>
        /// 1 List&lt;Household&gt; dùng để binding với ListHousehold của pHousehold.
        /// </summary>
        public List<Household> ListHousehold
        {
            get => _listHousehold;
            set
            {
                _listHousehold = value;

                householdBindingSource.DataSource = _listHousehold;
                householdBindingSource.ResetBindings(false);
                FormatHousehold();

            }
        }
   
        /// <summary>
        /// Dùng để binding với SelectedID của pHousehold.
        /// </summary>
        public int SelectedHouseholdID { get; set; }
        public int SelectedID { get; set; }

        private DisplayMode _mode;
        public DisplayMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                ChangeDisplayMode(_mode);
            }
        }

        private Font _fontHeader;
        public Font HeaderFont
        {
            get => _fontHeader;
            set
            {
                _fontHeader = value;
                Help.SetHeaderFont(new List<DataGridView> { dtgvHousehold, dtgvPerson }, _fontHeader);
            }
        }

        private Font _rowFont;
        public Font RowFont
        {
            get => _rowFont;
            set
            {
                _rowFont = value;
                Help.SetRowFont(new List<DataGridView> { dtgvHousehold, dtgvPerson }, _rowFont);
            }
        }

        private Font _buttonFont;
        public Font ButtonFont
        {
            get => _buttonFont;
            set
            {
                _buttonFont = value;
                Help.SetButtonFont(
                    new List<SimpleButton>
                    { btnSearch,
                        btnShow,
                        btnInsert,
                        btnUpdate,
                        btnDelete,
                        btnExportExcel,
                        btnImportExcel,
                        btnSelect,
                        btnCancel                       
                    },
                    _buttonFont); 
            }
        }

        private Font _labelFont;
        public Font LabelFont
        {
            get => _labelFont;
            set
            {
                _labelFont = value;

                Help.SetLabelFont(new List<Label> { labelStatistic }, _labelFont);
            }
        }

        private Color _textColor;
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Help.SetTextColor(
                    new List<object>
                    { btnSearch,
                        btnShow,
                        btnInsert,
                        btnUpdate,
                        btnDelete,
                        btnExportExcel,
                        btnImportExcel,
                        btnSelect,
                        btnCancel,
                        labelStatistic
                    },
                    _textColor);
            }
        }

        /// <summary>
        /// Danh sách các control binding cho pageAdmin để có thể cài đặt Enabled = true/false bởi admin.
        /// </summary>
        public List<Control> listPrivilege { get; set; }

        public void ChangeDisplayMode(DisplayMode mode)
        {
            if (mode == DisplayMode.Normal)
            {
                panelSelect.Visible = false;

                btnInsert.Enabled = true;
                Help.SetPrivilege(AppSetting.Instance.PersonPrivilege, listPrivilege);
                btnExportExcel.Enabled = true;
                btnImportExcel.Enabled = true;
                panelLabel.Visible = true;
                panelStatistic.Visible = true;
                txbSearch.Properties.NullText = "Quản lý nhân khẩu";
                txbSearch.Properties.NullValuePrompt = "Quản lý nhân khẩu";
                return;
            }

            btnInsert.Enabled = false;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            btnExportExcel.Enabled = false;
            btnImportExcel.Enabled = false;
            panelLabel.Visible = false;
            panelStatistic.Visible = false;

            panelSelect.Top = 317;
            panelSelect.Visible = true;

            txbSearch.Properties.NullText = "Lựa chọn 1 nhân khẩu trong danh sách dưới đây";
            txbSearch.Properties.NullValuePrompt = "Lựa chọn 1 nhân khẩu trong danh sách dưới đây";
        }

        void Initialize()
        {
            dtgvPerson.Text = "Bảng nhân khẩu";

            listPrivilege = new List<Control>
            {
                btnSearch,
                btnShow,
                btnInsert,
                btnUpdate,
                btnDelete,
                btnExportExcel,
                btnImportExcel,
                btnSelect,
                btnCancel, 
                dtgvPerson
            };

            //Khởi tạo DataSource cho các ComboBox
            var listGender = new List<Tuple<string, PersonGender>>()
            {
                new Tuple<string, PersonGender>( "Nam", PersonGender.Male ),
                new Tuple<string, PersonGender>( "Nữ", PersonGender.Female )
            };
            lkuGender.Properties.DataSource = listGender;
            lkuGender.Properties.DisplayMember = "Item1";
            lkuGender.Properties.ValueMember = "Item2";
            lkuGender.Properties.ShowFooter = false;
            lkuGender.Properties.ShowHeader = false;
            lkuGender.Properties.ShowLines = false;

            var listRelation = new List<Tuple<string, HouseholdRelation>>()
            {
               new Tuple<string, HouseholdRelation>( "Chủ hộ", HouseholdRelation.Owner ),
               new Tuple<string, HouseholdRelation>( "Vợ", HouseholdRelation.Wife ),
               new Tuple<string, HouseholdRelation>( "Chồng", HouseholdRelation.Husband ),
               new Tuple<string, HouseholdRelation>( "Con trai", HouseholdRelation.Son ),
               new Tuple<string, HouseholdRelation>( "Con gái", HouseholdRelation.Daughter ),
               new Tuple<string, HouseholdRelation>( "Cha", HouseholdRelation.Father ),
               new Tuple<string, HouseholdRelation>( "Mẹ", HouseholdRelation.Mother ),
               new Tuple<string, HouseholdRelation>( "Ông", HouseholdRelation.Grandfather ),
               new Tuple<string, HouseholdRelation>( "Bà", HouseholdRelation.Grandmother ),
               new Tuple<string, HouseholdRelation>( "Cháu trai", HouseholdRelation.Grandson ),
               new Tuple<string, HouseholdRelation>( "Cháu gái", HouseholdRelation.Granddaughter ),
               new Tuple<string, HouseholdRelation>( "Đối tượng tạm trú", HouseholdRelation.TemporaryResident ),
            };
            lkuRelation.Properties.DataSource = listRelation;
            lkuRelation.Properties.DisplayMember = "Item1";
            lkuRelation.Properties.ValueMember = "Item2";
            lkuRelation.Properties.ShowFooter = false;
            lkuRelation.Properties.ShowHeader = false;
            lkuRelation.Properties.ShowLines = false;

            Height = 700;

            //Khi mới khởi tạo pagePerson hiển thị các chức năng CRUD bình thường
            Mode = DisplayMode.Normal;

            //Kết quả form trả về khi các nút này được click
            btnCancel.DialogResult = DialogResult.Cancel;
            btnSelect.DialogResult = DialogResult.OK;

            AcceptButton = btnSearch;
          
        }

        void LoadData()
        {
            //Khởi tạo DataSource cho GridView
            dtgvPerson.DataSource = personBindingSource;
            dtgvHousehold.DataSource = householdBindingSource;

            ListPerson = PersonBUS.Instance.GetListPerson();
            personBindingSource.DataSource = ListPerson;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListPerson"));
           
            dtgvPerson.Columns[3].DefaultCellStyle.Format = "dd/MM/yyyy";

            dtgvPerson.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dtgvPerson.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dtgvPerson.Columns[0].HeaderText = "ID";
            dtgvPerson.Columns[1].HeaderText = "Họ tên";
            dtgvPerson.Columns[2].HeaderText = "Giới tính";
            dtgvPerson.Columns[3].HeaderText = "Ngày sinh";
            dtgvPerson.Columns[4].HeaderText = "CMND";
            dtgvPerson.Columns[5].HeaderText = "Quê quán";
            dtgvPerson.Columns[6].HeaderText = "ID Hộ";
            dtgvPerson.Columns[7].HeaderText = "Quan hệ";

            dtgvPerson.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dtgvPerson.Columns[0].Width = 40;
            dtgvPerson.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dtgvPerson.Columns[6].Width = 80;

            Help.SetHeaderFont(new List<DataGridView> { dtgvHousehold, dtgvPerson} , HeaderFont);

            Help.SetRowFont(new List<DataGridView> { dtgvHousehold, dtgvPerson }, RowFont); 

            //Load lại binding mỗi khi dtgvPerson.DataSource thay đổi
            LoadBinding();
        }

        void LoadBinding()
        {
            foreach (Control control in panelInfo.Controls)
            {
                if (!(control is Label)) control.DataBindings.Clear();
            }

            nmID.DataBindings.Add("Value", dtgvPerson.DataSource, "ID", false, DataSourceUpdateMode.Never);
            txbName.DataBindings.Add("Text", dtgvPerson.DataSource, "Name", false, DataSourceUpdateMode.Never);
            lkuGender.DataBindings.Add("EditValue", dtgvPerson.DataSource, "Gender", false, DataSourceUpdateMode.Never);
            dtDateOfBirth.DataBindings.Add("EditValue", dtgvPerson.DataSource, "DateOfBirth", false, DataSourceUpdateMode.Never);
            txbCmnd.DataBindings.Add("Text", dtgvPerson.DataSource, "CMND", false, DataSourceUpdateMode.Never);
            txbAddress.DataBindings.Add("Text", dtgvPerson.DataSource, "Address", false, DataSourceUpdateMode.Never);
            nmHouseholdID.DataBindings.Add("Value", dtgvPerson.DataSource, "HouseholdID", false, DataSourceUpdateMode.Never);
            lkuRelation.DataBindings.Add("EditValue", dtgvPerson.DataSource, "Relation", false, DataSourceUpdateMode.Never);
        }

        void LoadEvent()
        {
            fInsert.GetInstance().PersonInserted += (s, e) =>
            {               
                var person = (Person)(e as InsertedEventArgs).Inserted;

                //Nếu insert 1 người là chủ hộ thì Owner và Address của hộ khẩu cũng thay đổi theo => load lại hoàn toàn hộ khẩu đó
                //Hiển thị lại hộ khẩu vừa được insert thêm thành viên trên dtgvHousehold, notify cho các page khác biết
                var index = ListHousehold.FindIndex(household => household.ID == person.HouseholdID);
                householdBindingSource.RemoveAt(index);
                householdBindingSource.Insert(index, HouseholdBUS.Instance.GetHouseholdByID(person.HouseholdID));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListHousehold"));

                //Hiển thị lại nhân khẩu vừa insert lên dtgvPerson, notify cho các page khác biết
                personBindingSource.Add(person);
                //Nếu dtgvPerson.DataSource đang là personBindSource thì mới bôi đen hàng vừa thêm vào
                //Ngược lại (dtgvPerson.DataSource đang gán bởi hàm SearchPerson) thì thôi
                if (dtgvPerson.DataSource.Equals(personBindingSource))
                    dtgvPerson.CurrentCell = dtgvPerson.Rows[dtgvPerson.RowCount - 1].Cells[1];

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListPerson"));

                

                ////LoadData(); 
            };

            btnShow.Click += delegate { LoadData(); };

            btnInsert.Click += delegate { fInsert.GetInstance(InsertMode.Person).ShowDialog(); };

            btnDelete.Click += delegate { DeletePerson((int)nmID.Value); };

            btnUpdate.Click += delegate { ConfirmUpdate(); };

            btnSearch.Click += delegate { SearchPerson(txbSearch.Text); };

            btnImportExcel.Click += delegate { LoadFileExcel(); };

            btnExportExcel.Click += delegate { CreateFileExcel(); };
            
            dtgvPerson.CellClick += (s, e) => 
            {
                //Click vào cell ID để chọn toàn bộ GridView
                if (e.ColumnIndex == 0 && e.RowIndex == -1)
                {
                    dtgvPerson.SelectAll();

                    return;
                }
                
                //Click vào header cột nào sẽ sắp xếp theo cột đó
                var list = new List<Person>();

                if (e.ColumnIndex == 1 && e.RowIndex == -1) list = PersonBUS.Instance.GetListPerson("Name");
                if (e.ColumnIndex == 2 && e.RowIndex == -1) list = PersonBUS.Instance.GetListPerson("Gender");
                if (e.ColumnIndex == 3 && e.RowIndex == -1) list = PersonBUS.Instance.GetListPerson("DateOfBirth");
                if (e.ColumnIndex == 4 && e.RowIndex == -1) list = PersonBUS.Instance.GetListPerson("CMND");
                if (e.ColumnIndex == 5 && e.RowIndex == -1) list = PersonBUS.Instance.GetListPerson("Address");
                if (e.ColumnIndex == 6 && e.RowIndex == -1) list = PersonBUS.Instance.GetListPerson("HouseholdID");
                if (e.ColumnIndex == 7 && e.RowIndex == -1) list = PersonBUS.Instance.GetListPerson("Relation");

                if (list.Count > 0)
                {
                    ListPerson.Clear();

                    ListPerson.AddRange(list);

                    personBindingSource.ResetBindings(false);
                }

                
            };

            //Hiện thống kê về các hàng được chọn
            dtgvPerson.SelectionChanged += delegate { LoadStatistic(); };

            
            ////Hiện Description của enum thay vì tên enum
            ////(chậm khi GridView load nhiều hàng => chuyển sang dùng TypeConverter để nhanh hơn)
            //dtgvPerson.CellFormatting += (s, e) =>
            //{
            //    if (e.ColumnIndex != 2 && e.ColumnIndex != 7) return;

            //    FieldInfo info = e.Value.GetType().GetField(e.Value.ToString());
            //    DescriptionAttribute description = info.GetCustomAttribute<DescriptionAttribute>();
            //    e.Value = description.Description;
            //};
            

            //Highlight hộ khẩu khi có thành viên được chọn trên dtgvPerson
            dtgvPerson.CurrentCellChanged += delegate
            {
                //dtgvHousehold.ClearSelection();
                foreach (DataGridViewRow row in dtgvHousehold.Rows)
                {
                    if (row.Cells[0].Value.ToString() == nmHouseholdID.Text)
                        //row.Selected = true;
                        dtgvHousehold.CurrentCell = row.Cells[0];
                }
            };

            //Nhấn btnSelect (trong insert mode) sẽ gán giá trị cho SelectedID và thoát form
            btnSelect.Click += delegate { SelectedID = (int)nmID.Value; (Parent.Parent as Form).Close(); };

            //Nhấn btnCancel => thoát
            btnCancel.Click += delegate { (Parent.Parent as Form).Close(); };
        }

        void LoadStatistic()
        {
            //Tạo list chứa những hàng được chọn trên GridView
            var list = new List<Person>();
            foreach (DataGridViewRow row in dtgvPerson.SelectedRows)
            {
                list.Add(new Person((int)row.Cells[0].Value,
                                         row.Cells[1].Value.ToString(),
                           (PersonGender)row.Cells[2].Value,
                               (DateTime)row.Cells[3].Value,
                                         row.Cells[4].Value.ToString(),
                                         row.Cells[5].Value.ToString(),
                                    (int)row.Cells[6].Value,
                      (HouseholdRelation)row.Cells[7].Value));
            }

            //Thống kê dựa vào list
            nmTotalPerson.Value = list.Count();
            nmMan.Value = list.Count(person => person.Gender == PersonGender.Male);
            nmWoman.Value = nmTotalPerson.Value - nmMan.Value;
            nmTempResident.Value = list.Count(person => person.Relation == HouseholdRelation.TemporaryResident);
            nmPermanentResident.Value = nmTotalPerson.Value - nmTempResident.Value;
            nmChild.Value = list.Count(person => DateTime.Today.Year - person.DateOfBirth.Year < 10);
            nmTeenager.Value = list.Count(person => 10 <= DateTime.Today.Year - person.DateOfBirth.Year && DateTime.Today.Year - person.DateOfBirth.Year < 18);
            nmAdult.Value = list.Count(person => 18 <= DateTime.Today.Year - person.DateOfBirth.Year && DateTime.Today.Year - person.DateOfBirth.Year < 60);
            nmElder.Value = nmTotalPerson.Value - nmChild.Value - nmTeenager.Value - nmAdult.Value;
        }

        void DeletePerson(int id)
        {
            if (MessageBox.Show($"Bạn thực sự muốn xoá nhân khẩu này?\n" +
                                $"ID:              {nmID.Text}\n" +
                                $"Họ tên:      {txbName.Text}\n" +
                                $"Ngày sinh: {dtDateOfBirth.Text}", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (PersonBUS.Instance.DeletePerson(id))
            {
                MessageBox.Show("Xoá nhân khẩu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //Xoá nhân khẩu, reset lại dtgvPerson và báo cho những thằng đang binding với ListPerson bên ngoài
                var person = ListPerson.Find(p => p.ID == id);
                personBindingSource.Remove(person);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListPerson"));
                
                //dtgvHousehold: khi xoá bớt nhân khẩu chỉ cần update lại số lượng thành viên gia đình
                var household = ListHousehold.Find(h => h.ID == person.HouseholdID);
                household.MemberCount = ListPerson.Count(p => p.HouseholdID == household.ID);
                householdBindingSource.ResetBindings(false);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListHousehold"));

                ////LoadData();

                return;
            }

            MessageBox.Show("Xoá nhân khẩu thất bại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void ConfirmUpdate()
        {
            var result = Help.DialogBox("Bạn muốn chuyển hộ khẩu cho người này hay cập nhật các thông tin khác?\n" +
                                       $"Họ tên:      {txbName.Text}\n" +
                                       $"Ngày sinh: {dtDateOfBirth.Text}\n" +
                                       $"CMND:      {txbCmnd.Text}", "Thông báo", "Huỷ", "Cập nhật", "Chuyển");

            //Nếu chọn chuyển khẩu thì mở Form để chọn hộ khẩu muốn chuyển tới
            if (result == 1)
            {
                if(fSelect.Instance == null)
                {
                    MessageBox.Show("Form lựa chọn chưa khởi tạo.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (fSelect.Instance.ShowDialog() == DialogResult.Cancel) return;

                UpdatePerson((int)nmID.Value, txbName.Text, (int)lkuGender.EditValue, dtDateOfBirth.Text, txbCmnd.Text, txbAddress.Text, SelectedHouseholdID, (int)lkuRelation.EditValue);

                return;
            }

            //Nếu chọn cập nhật thì lấy luôn dữ liệu trên Form chính để cập nhật
            if (result == 2) UpdatePerson((int)nmID.Value, txbName.Text, (int)lkuGender.EditValue, dtDateOfBirth.Text, txbCmnd.Text, txbAddress.Text, (int)nmHouseholdID.Value, (int)lkuRelation.EditValue);
        }

        void UpdatePerson(int id, string name, int gender, string dateOfBirth, string cmnd, string address, int householdID, int relation)
        {

            if (PersonBUS.Instance.UpdatePerson(id, name, gender, dateOfBirth, cmnd, address, householdID, relation))
            {
                MessageBox.Show("Cập nhật nhân khẩu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //dtgvHousehold
                //Lấy ra person muốn update trên ListPerson (chưa update theo database)
                var person = ListPerson.Find(p => p.ID == id);
                //Lấy ra household cũ (nếu lựa chọn chuyển khẩu)
                var household = ListHousehold.Find(h => h.ID == person.HouseholdID);
                                              
                //dtgvPerson
                //Tìm index của nhân khẩu muốn cập nhật, xoá nó khỏi dtgvPerson và chèn lại nhân khẩu mới với thuộc tính được cập nhật
                var index = ListPerson.FindIndex(p => p.ID == id);
                personBindingSource.RemoveAt(index);
                personBindingSource.Insert(index, PersonBUS.Instance.GetPersonByID(id));               
                //Báo cho đồng bọn binding thay đổi data theo
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListPerson"));

                //Sau khi person update
                //update household mới (nếu lựa chọn chuyển khẩu)
                //Nếu update 1 người là chủ hộ thì Owner và Address của hộ khẩu cũng thay đổi theo => load lại hoàn toàn hộ khẩu đó
                var index2 = ListHousehold.FindIndex(h => h.ID == householdID);
                householdBindingSource.RemoveAt(index2);
                householdBindingSource.Insert(index2, HouseholdBUS.Instance.GetHouseholdByID(householdID));

                //Sau khi xoá đi - chèn lại, active lại cell này để dtgvPerson và dtgvHousehold highlight lại các hàng lựa chọn 
                if (dtgvPerson.DataSource.Equals(personBindingSource)) 
                    dtgvPerson.CurrentCell = dtgvPerson.Rows[index].Cells[1];

                //update household cũ, chỉ cần tính lại MemberCount
                household.MemberCount = ListPerson.Count(p => p.HouseholdID == household.ID);
                householdBindingSource.ResetBindings(false);                

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListHousehold"));
                ////LoadData();

                return;
            }

            MessageBox.Show("Cập nhật nhân khẩu thất bại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void SearchPerson(string input)
        {
            var list = new List<Person>(ListPerson); //this.ListPerson.GetRange(0, this.ListPerson.Count);

            input = input.ToUnsigned().ToLower();

            list.RemoveAll(person => person.Name.ToUnsigned().ToLower().Contains(input) == false
                                  && person.Address.ToUnsigned().ToLower().Contains(input) == false
                                  && person.DateOfBirth.ToString().ToUnsigned().ToLower().Contains(input) == false
                                  && person.Cmnd.Contains(input) == false);

            dtgvPerson.DataSource = list;

            //Load lại binding vì dtgvPerson.DataSource đã thay đổi
            LoadBinding();

        }

        void LoadFileExcel()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Excel file (*.xlsx)|*.xlsx";
            dialog.DefaultExt = ".xlsx";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CheckForIllegalCrossThreadCalls = false;
                btnExportExcel.Visible = false;
                progressPanel.Visible = true;
                progressExcel.Visible = true;

                //Mở 1 thread khác để Form chính không bị đơ
                Thread thread = new Thread(() => 
                {
                    var result = PersonBUS.Instance.LoadFileExcel(dialog.FileName, progressExcel, progressPanel);
                    
                    btnExportExcel.Visible = true;

                    progressPanel.WaitAnimationType = DevExpress.Utils.Animation.WaitingAnimatorType.Default;
                    progressPanel.ShowDescription = false;
                    progressPanel.Visible = false;

                    progressExcel.Visible = false;
                    progressExcel.Position = 0;

                    if (result)
                    {
                        MessageBox.Show("Nhập dữ liệu vào database thành công.\n" +
                                        "Nhấn nút [Xem] trong mục Quản lý nhân khẩu để hiển thị dữ liệu nhân khẩu mới.\n" +
                                        "Nhấn nút [Xem] trong mục Quản lý hộ khẩu để hiển thị dữ liệu hộ khẩu mới.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }

                    MessageBox.Show("Nhập dữ liệu thất bại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);;

                    CheckForIllegalCrossThreadCalls = true;
                });

                thread.IsBackground = true;
                thread.Priority = ThreadPriority.AboveNormal;
                thread.Start();

            }
                
        }

        void CreateFileExcel()
        {
            var list = new List<Person>();

            var resultDialog = Help.DialogBox("Bạn muốn xuất tất cả danh sách hay chỉ những hàng đã chọn?", "Thông báo", "Huỷ", "Đã chọn", "Tất cả");

            //Xuất tất cả
            if (resultDialog == 1) list = ListPerson;

            //Xuất những hàng bôi đen
            if(resultDialog == 2)
            {
                foreach (DataGridViewRow row in dtgvPerson.SelectedRows)
                {
                    list.Add(new Person((int)row.Cells[0].Value,
                                             row.Cells[1].Value.ToString(),
                               (PersonGender)row.Cells[2].Value,
                          Convert.ToDateTime(row.Cells[3].Value),
                                             row.Cells[4].Value.ToString(),
                                             row.Cells[5].Value.ToString(),
                                        (int)row.Cells[6].Value,
                          (HouseholdRelation)row.Cells[7].Value));
                }

                //Thứ tự trong SelectedRows ngược với thứ tự chọn trên GridView => đảo lại list để in ra file excel đúng thứ tự
                list.Reverse();
            }

            //Thoát
            if (resultDialog == 3) return;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "Excel file (*.xlsx)|*.xlsx";
            dialog.DefaultExt = ".xlsx";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CheckForIllegalCrossThreadCalls = false;
                btnImportExcel.Visible = false;
                btnExportExcel.Left = btnImportExcel.Left;
                progressPanel.Visible = true;
                progressExcel.Visible = true;

                //Mở 1 thread khác để Form chính không bị đơ
                Thread thread = new Thread(() =>
                {
                    var result = false;
                    result = PersonBUS.Instance.CreateFileExcel(dialog.FileName, list, progressExcel);

                    btnImportExcel.Visible = true;
                    btnExportExcel.Left = 197;
                    progressPanel.Visible = false;
                    progressExcel.Visible = false;
                    progressExcel.Position = 0;

                    CheckForIllegalCrossThreadCalls = true;

                    if (result)
                    {
                        if(MessageBox.Show("Xuất dữ liệu ra Excel thành công.\nNhấn [OK] để mở file.", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            Process process = new Process();
                            process.StartInfo.FileName = dialog.FileName;
                            process.StartInfo.UseShellExecute = true;
                            process.Start();
                        }

                        return;
                    }

                    MessageBox.Show("Xuất dữ liệu thất bại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error); ;

                    
                });

                thread.IsBackground = true;
                thread.Priority = ThreadPriority.AboveNormal;
                thread.Start();

            }
                
        }       

        /// <summary>
        /// Chỉnh sửa dtgvHousehold khi nó thay đổi DataSource.
        /// </summary>
        void FormatHousehold()
        {
            dtgvHousehold.Columns[0].HeaderText = "ID";
            dtgvHousehold.Columns[1].HeaderText = "Chủ hộ";
            dtgvHousehold.Columns[2].HeaderText = "Địa chỉ";
            dtgvHousehold.Columns[3].HeaderText = "Số thành viên";

            dtgvHousehold.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dtgvHousehold.Columns[0].Width = 40;

            dtgvHousehold.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dtgvHousehold.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }
}