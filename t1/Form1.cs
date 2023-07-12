using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Collections;

namespace t1
{

    public partial class Form1 : Form
    {
        private const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
        //private const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=baza.sql;User ID=DESKTOP-DTODAIB\\Ioana;Password=";
        //private const string connectionString = "Data Source=ServerSQL;Initial Catalog=baza ;User ID=Utilizator;Password=Parola";

        private const string apiUrl = "https://api-testing-dogu.freya.cloud/login";
        private const string apiKey = "f8ce74e00caf759759d311aad1e005a0";
        private string token;
        public Form1()
        {
            InitializeComponent();
            // ConectareBazaDate();

        }
        private void ConectareBazaDate()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    //string query = "SELECT * FROM Produs";


                    connection.Open();
                    MessageBox.Show("Conexiune reusita!");
                    string query = "SELECT * FROM Produs";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridView2.Invoke((MethodInvoker)delegate {
                        // Ștergeți conținutul existent din dataGridView2
                        dataGridView2.DataSource = null;
                        dataGridView2.Rows.Clear();
                        dataGridView2.Columns.Clear();

                        // Actualizați dataGridView2 cu rezultatele din DataTable
                        dataGridView2.DataSource = dataTable;
                    });
                }



                catch (Exception ex)
                {
                    MessageBox.Show("Eroare: " + ex.Message);
                }





            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.Visible = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            string username = textBox1.Text;
            string password = textBox2.Text;

            await Task.Run(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("apikey", apiKey);

                    var postData = new StringContent($"{{\"username\":\"{username}\",\"password\":\"{password}\"}}", Encoding.UTF8, "application/json");

                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(apiUrl, postData);
                        response.EnsureSuccessStatusCode();

                        string responseContent = await response.Content.ReadAsStringAsync();
                        JObject responseObject = JObject.Parse(responseContent);
                        token = responseObject["token"]?.ToString();

                        MessageBox.Show("Autentificare reușită!");
                        Invoke((MethodInvoker)delegate
                        {
                            tabControl1.Visible = true;
                        });
                        DisplayProductInformation(token);
                        ConectareBazaDate();

                    }
                    catch (HttpRequestException ex)
                    {
                        MessageBox.Show("Eroare la efectuarea cererii către server: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare neașteptată: " + ex.Message);
                    }
                }

            });


        }
        private async Task<List<Dictionary<string, object>>> GetProductRecords(string token)
        {
            string requestUrl = $"https://api-testing-dogu.freya.cloud/product/findMany?listOnly=true&productCategoryUid=5b693a50ff2c495290a741c5ecdf26d1&locationUid=cc33a3a158d14b34a80171caf35870e2&pageNo=0&top=100";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", apiKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                try
                {
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string responseContent = await response.Content.ReadAsStringAsync();

                    JObject responseObject = JObject.Parse(responseContent);
                    JArray recordsArray = responseObject["payload"]["records"] as JArray;

                    List<Dictionary<string, object>> productRecords = new List<Dictionary<string, object>>();

                    foreach (JObject record in recordsArray)
                    {
                        Dictionary<string, object> recordDictionary = record.ToObject<Dictionary<string, object>>();
                        productRecords.Add(recordDictionary);
                    }

                    return productRecords;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show("Eroare la efectuarea cererii către server: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare neașteptată: " + ex.Message);
                }
            }

            return null;
        }
        private async void DisplayProductInformation(string token)
        {
            List<Dictionary<string, object>> productRecords = await GetProductRecords(token);

            if (productRecords != null)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        foreach (var record in productRecords)
                        {
                            bool isAvailableOnPos = Convert.ToBoolean(record["isAvailableOnPos"]);
                            bool isForSale = Convert.ToBoolean(record["isForSale"]);
                            bool isDisabled = Convert.ToBoolean(record["isDisabled"]);

                            if (isAvailableOnPos && isForSale && !isDisabled)
                            {
                                string name = record["name"].ToString();
                                string categoryName = record["categoryName"].ToString();
                                string measureUnitName = record["measureUnitName"].ToString();
                                decimal unitPriceWithVat = Convert.ToDecimal(record["unitPriceWithVat"]);
                                string uid = record["uid"].ToString();
                                string vatName = record["vatName"].ToString();
                                string repositoryName = record["repositoryName"].ToString();

                                // Convert the vatName to a decimal value
                                decimal vatRate = GetVatRate(vatName);

                                // Save the product to the local database
                                SaveProductToDatabase(connection, name, categoryName, measureUnitName, unitPriceWithVat, uid, vatRate, repositoryName);
                            }
                        }

                        // Refresh the DataGridView with the updated data from the local database
                        ConectareBazaDate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la salvarea produselor în baza de date locală: " + ex.Message);
                    }
                }
            }
        }

        private decimal GetVatRate(string vatName)
        {
            // Example logic to extract the vat rate from the vatName
            if (vatName.Contains("T.V.A. 19%"))
            {
                return 0.19m;
            }
            else if (vatName.Contains("T.V.A. 20%"))
            {
                return 0.20m;
            }
            else
            {
                // Set a default vat rate if the vatName format is not recognized
                return 0.00m;
            }
        }

        private void SaveProductToDatabase(SqlConnection connection, string name, string categoryName, string measureUnitName, decimal unitPriceWithVat, string uid, decimal vatRate, string repositoryName)
        {
            string query = "INSERT INTO Produs (Name, CategoryName, MeasureUnitName, UnitPriceWithVat, UID, VatRate, RepositoryName) " +
                           "VALUES (@Name, @CategoryName, @MeasureUnitName, @UnitPriceWithVat, @UID, @VatRate, @RepositoryName)";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@CategoryName", categoryName);
            command.Parameters.AddWithValue("@MeasureUnitName", measureUnitName);
            command.Parameters.AddWithValue("@UnitPriceWithVat", unitPriceWithVat);
            command.Parameters.AddWithValue("@UID", uid);
            command.Parameters.AddWithValue("@VatRate", vatRate);
            command.Parameters.AddWithValue("@RepositoryName", repositoryName);

            command.ExecuteNonQuery();
        }

        public void SetToken(string token)
        {
            this.token = token;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string filterText = textBox3.Text.Trim();

            DataTable dataTable = dataGridView2.DataSource as DataTable;

            if (dataTable != null)
            {
                // Construiți expresia de filtrare
                string rowFilter = string.Format("RepositoryName LIKE '%{0}%'", filterText);

                // Setați filtrul pe DataView-ul asociat DataTable-ului
                dataTable.DefaultView.RowFilter = rowFilter;

                // Actualizați dataGridView2 cu rezultatele filtrării utilizând Invoke pentru a accesa controlul din firul principal
                dataGridView2.Invoke((MethodInvoker)delegate {
                    // Ștergeți conținutul existent din dataGridView2
                    dataGridView2.DataSource = null;
                    dataGridView2.Rows.Clear();
                    dataGridView2.Columns.Clear();

                    // Actualizați dataGridView2 cu rezultatul filtrării
                    dataGridView2.DataSource = dataTable.DefaultView;
                });
            }

        }
    }
}
