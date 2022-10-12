﻿using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace HouseholdManager.DAO
{
    public class DataProvider
    {
        private static readonly DataProvider instance = new DataProvider();

        public static DataProvider Instance => instance;

        private DataProvider() { }

        private string connectionString = "Data Source = localdb.db; foreign keys=true";

        /// <summary>
        /// Kiểm tra kết nối.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối.</param>
        /// <returns>
        /// true, nếu thành công, đồng thời gán chuỗi kết nối này cho biến connectionString của DataProvider.
        /// <br>nếu thất bại, thông báo lỗi và trả về false.</br>
        /// </returns>
        public bool TestConnection(string connectionString)
        {

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand("SELECT 1", connection);

                    if ((System.Int64)command.ExecuteScalar() == 1)
                    {
                        this.connectionString = connectionString;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {

                MessageBox.Show("Kết nối thất bại!\n" + e.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Trả về 1 bảng kết quả khi chạy câu truy vấn.
        /// </summary>
        /// <param name="query">Câu truy vấn.</param>
        /// <param name="parameter">Mảng các giá trị truyền cho các parameter @xyz... </param>
        /// <returns></returns>
        public DataTable ExecuteQuery(string query, object[] parameter = null)
        {
            DataTable data = new DataTable();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(query, connection);

                if (parameter != null)
                {
                    //cắt nhỏ câu truy vấn để lấy ra phần parameter dạng @abcxyz
                    string[] paraList = query.Split(' ');
                    int i = 0;
                    foreach (string item in paraList)
                    {
                        //nếu chứa '@' => nó là parameter
                        if (item.Contains('@'))
                        {
                            //xoá dấu "," sau đít tham số
                            string para = item.Replace(",", "");
                            //thêm vào danh sách tên parameter và giá trị tương ứng được truyền cho nó, nếu nó null thì truyền vào DBNull.Value
                            command.Parameters.AddWithValue(para, parameter[i] ?? DBNull.Value);
                            //chỉ tăng i mỗi khi tìm được tên 1 parameter
                            i++;
                        }
                    }

                }

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                adapter.Fill(data);

            }

            return data;
        }

        /// <summary>
        /// Trả về số số dòng áp dụng thành công khi chạy câu truy vấn.
        /// </summary>
        /// <param name="query">Câu truy vấn.</param>
        /// <param name="parameter">Mảng các giá trị truyền cho các parameter của store procedure
        /// (nếu câu truy vấn là thực thi 1 store procedure).</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string query, object[] parameter = null)
        {
            //tạo biến chứa số dòng áp dụng thành công khi chạy câu truy vấn
            int data = 0;

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(query, connection);

                if (parameter != null)
                {
                    //cắt nhỏ câu truy vấn để lấy ra phần parameter dạng @abcxyz
                    string[] paraList = query.Split(' ');
                    int i = 0;
                    foreach (string item in paraList)
                    {
                        //nếu chứa '@' => nó là parameter
                        if (item.Contains('@'))
                        {
                            //Xoá dấu "," sau đít tham số
                            string para = item.Replace(",", "");
                            //thêm vào danh sách tên parameter và giá trị tương ứng được truyền cho nó, nếu nó null thì truyền vào DBNull.Value
                            command.Parameters.AddWithValue(para, parameter[i] ?? DBNull.Value);
                            //chỉ tăng i mỗi khi tìm được tên 1 parameter
                            i++;
                        }
                    }

                }
                data = command.ExecuteNonQuery();
                //try
                //{
                //    data = command.ExecuteNonQuery();
                //}
                //catch(SQLiteException sqle)
                //{
                //    MessageBox.Show(sqle.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}

            }

            return data;
        }

        /// <summary>
        /// Trả về cột đầu tiên của dòng đầu tiên trong bảng kết quả.
        /// </summary>
        /// <param name="query">Câu truy vấn.</param>
        /// <param name="parameter">Mảng các giá trị truyền cho các parameter của store procedure
        /// (nếu câu truy vấn là thực thi 1 store procedure).</param>
        /// <returns></returns>
        public object ExecuteScalar(string query, object[] parameter = null)
        {
            //tạo biến chứa kết quả trả về
            object data = 0;

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(query, connection);

                if (parameter != null)
                {
                    //cắt nhỏ câu truy vấn để lấy ra phần parameter dạng @abcxyz
                    string[] paraList = query.Split(' ');
                    int i = 0;
                    foreach (string item in paraList)
                    {
                        //nếu chứa '@' => nó là parameter
                        if (item.Contains('@'))
                        {
                            //Xoá dấu "," sau đít tham số
                            string para = item.Replace(",", "");
                            //thêm vào danh sách tên parameter và giá trị tương ứng được truyền cho nó, nếu nó null thì truyền vào DBNull.Value
                            command.Parameters.AddWithValue(para, parameter[i] ?? DBNull.Value);
                            //chỉ tăng i mỗi khi tìm được tên 1 parameter
                            i++;
                        }
                    }

                }

                data = command.ExecuteScalar();

            }

            return data;
        }

    }
}
